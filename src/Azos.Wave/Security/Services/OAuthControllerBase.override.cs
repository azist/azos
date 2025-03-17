/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Wave.Mvc;
using Azos.Security.Tokens;
using Azos.Serialization.JSON;
using Azos.Wave;
using Azos.Data;
using System.Net;
using Azos.Conf;
using Microsoft.AspNetCore.Http;

namespace Azos.Security.Services
{
  partial class OAuthControllerBase
  {
    public const string CONFIG_WEB_SECTION = "web";

    /// <summary>
    /// Shortcut accessor to OAuth.Options["web"]
    /// </summary>
    protected IConfigSectionNode WebOptions => OAuth.Options[CONFIG_WEB_SECTION];

    protected T GateError<T>(T response) => gate(response, false);
    protected T GateUser<T>(T response) => gate(response, true);
    private T gate<T>(T response, bool isInvalidUser)
    {
      var varName = isInvalidUser ? OAuth.GateVarInvalidUser : OAuth.GateVarErrors;
      if (varName.IsNullOrWhiteSpace()) return response;
      WorkContext.IncreaseGateVar(varName);
      return response;
    }

    /// <summary>
    /// Override to allocate a custom derived type for login flow context
    /// </summary>
    protected virtual LoginFlow MakeLoginFlow() => new LoginFlow();

    /// <summary>
    /// Override to extract SSO session cookie from the request. The value (if any) is set on `loginFlow.SsoSessionId`
    /// Default implementation uses Request Cookie named OAuth.SsoSessionName if it is set.
    /// You must set loginFlow.SsoSessionId to null if there is no such session id provided or OAuth.ssoSessionName is turned off (null or whitespace)
    /// </summary>
    protected virtual void TryExtractSsoSessionId(LoginFlow loginFlow)
    {
      var ssoCookieName = OAuth.SsoSessionName;//copy
      if (ssoCookieName.IsNullOrWhiteSpace()) return;
      var cookie = WorkContext.Request.Cookies[ssoCookieName];
      if (cookie.IsNullOrWhiteSpace()) return;

      loginFlow.SsoSessionId = cookie;
    }

    /// <summary>
    /// Sets SSO token. Default implementation sets browser cookie
    /// </summary>
    protected virtual void SetSsoSessionId(LoginFlow loginFlow, string ssoSessionName)
    {
      var copt = new CookieOptions()
      {
        HttpOnly = true,
        Secure = true ^ WebOptions.Of("cookie-not-secure").ValueAsBool(false),
        Expires = App.TimeSource.UTCNow.AddMinutes(WebOptions.Of("cookie-expire-in-minutes").ValueAsDouble(2 * 24 * 60))
      };

      WorkContext.Response.AppendCookie(ssoSessionName, loginFlow.SsoSessionId, copt);
    }

    /// <summary>
    /// Erases SSO token. Default implementation un-sets cookie
    /// </summary>
    protected virtual void DeleteSsoSessionId(string ssoSessionName)
    {
      var copt = new CookieOptions()
      {
        HttpOnly = true,
        Secure = true ^ WebOptions.Of("cookie-not-secure").ValueAsBool(false),
        Expires = App.TimeSource.UTCNow.AddDays(-2)
      };

      WorkContext.Response.AppendCookie(ssoSessionName, "", copt);
    }

    /// <summary>
    /// Override to return a 403 (unauthorized result) of user logout
    /// </summary>
    protected virtual object ReturnSsoLogout403(LoginFlow flow)
    {
      return new Http403Forbidden();
    }


    /// <summary>
    /// Tries to get SSO subject user by examining the supplied idSsoSession.
    /// Set sso user to NULL if the SSO session id is invalid/or user revoked etc..
    /// </summary>
    protected async virtual Task TryGetSsoSubjectAsync(LoginFlow loginFlow)
    {
      var session = await OAuth.TokenRing.GetAsync<SsoSessionToken>(loginFlow.SsoSessionId).ConfigureAwait(false);
      if (session == null) return;

      if (!SysAuthToken.TryParse(session.SysAuthToken, out var sysToken)) return;

      var ssoSubject =  await App.SecurityManager.AuthenticateAsync(sysToken).ConfigureAwait(false);
      if (!ssoSubject.IsAuthenticated) return; //sys auth token may have expired

      loginFlow.SsoSubjectUser = ssoSubject;
    }

    protected async virtual Task SetSsoSubjectSessionAsync(LoginFlow loginFlow, DateTime lastLoginUtc, User ssoSubjectUser)
    {
      loginFlow.SsoWasJustSet = true;
      loginFlow.SsoSubjectLastLoginUtc = lastLoginUtc;
      loginFlow.SsoSubjectUser = ssoSubjectUser;
      var session = OAuth.TokenRing.GenerateNew<SsoSessionToken>();
      session.SysAuthToken = loginFlow.SsoSubjectUser.AuthToken.ToString();
      loginFlow.SsoSessionId = await OAuth.TokenRing.PutAsync(session).ConfigureAwait(false);
    }


    /// <summary>
    /// Advances login flow state to the next level.
    /// You may override this and accompanying "RespondWithAuthorizeResult/MakeAuthorizeResult"/>
    /// method to build a complex login flows which return different views, such as 2FA etc.
    /// </summary>
    protected virtual Task AdvanceLoginFlowStateAsync(LoginFlow loginFlow)
    {
      if (loginFlow.IsValidSsoUser) loginFlow.FiniteStateSuccess = true;
      if (loginFlow.IsValidSubjectUser) loginFlow.FiniteStateSuccess = true;
      return Task.CompletedTask;
    }


    protected virtual object RespondWithAuthorizeResult(long sdUtc, LoginFlow loginFlow, string error)
    {
      //Pack all requested content(session) into cryptographically encoded message aka "roundtrip"
      var flow = new {
        sd  = sdUtc,
        iss = App.TimeSource.UTCNow.ToSecondsSinceUnixEpochStart(),
        tp  = loginFlow.ClientResponseType,
        scp = loginFlow.ClientScope,
        id  = loginFlow.ClientId,
        uri = loginFlow.ClientRedirectUri,
        st  = loginFlow.ClientState
      };
      var roundtrip = App.SecurityManager.PublicProtectAsString(flow);

      if (error!=null)
      {
        WorkContext.Response.StatusCode = WebConsts.STATUS_403;
        WorkContext.Response.StatusDescription = error;
      }

      return MakeAuthorizeResult(loginFlow, roundtrip, error);
    }

    /// <summary>
    /// Override to provide a authorize result which is by default either a stock login form or
    /// JSON object
    /// </summary>
    protected virtual object MakeAuthorizeResult(LoginFlow loginFlow, string roundtrip, string error)
    {
      if (WorkContext.RequestedJson)
        return new { OK = error.IsNullOrEmpty(), roundtrip, error };

      //stock log-in page
      return new Wave.Templatization.StockContent.OAuthLogin(loginFlow.ClientUser, roundtrip, error);
    }


    protected virtual async Task<object> GenerateSuccessfulClientAccessCodeTokenRedirectAsync(User subject, string clid, string state, string uri, bool usePageRedirect = false)
    {
      var acToken = OAuth.TokenRing.GenerateNew<ClientAccessCodeToken>();
      acToken.ClientId = clid;
      acToken.State = state;
      acToken.RedirectURI = uri;
      acToken.SubjectSysAuthToken = subject.AuthToken.ToString();
      var accessCode = await OAuth.TokenRing.PutAsync(acToken).ConfigureAwait(false);

      //6. Redirect to URI
      var redirect = new UriQueryBuilder(uri)
      {
        {"code", accessCode},
        {"state", state}
      }.ToString();

      if (usePageRedirect)
      {
        var suppressAutoRedirect = WebOptions.Of("suppress-auto-sso-redirect").ValueAsBool(false);
        return new Wave.Templatization.StockContent.OAuthSsoRedirect(redirect, suppressAutoRedirect);
      }
      else
        return new Redirect(redirect);
    }

    /// <summary>
    /// Override to add extra information to <see cref="AuthenticationRequestContext"/>.
    /// Default implementation does nothing
    /// </summary>
    protected virtual void ConfigureAuthenticationRequestContext(LoginFlow loginFlow, AuthenticationRequestContext context)
    {
    }


    /// <summary>
    /// Override to add extra claims to id_token JWT
    /// </summary>
    protected virtual void AddExtraClaimsToIDToken(User clientUser, User subjectUser, AccessToken accessToken, JsonDataMap jwtClaims)
    {
    }

    /// <summary>
    /// Override to add extra field to response body (rarely needed)
    /// </summary>
    protected virtual void AddExtraFieldsToResponseBody(User clientUser, User subjectUser, AccessToken accessToken, JsonDataMap responseBody)
    {
    }

    protected object ReturnError(string error, string error_description, string error_uri = null, int code = 400, string trace = null)
    {
      if (error.IsNullOrWhiteSpace())
        error_uri = "invalid_request";

      if (error_description.IsNullOrWhiteSpace())
        error_description = "Could not be processed";

      if (error_uri.IsNullOrWhiteSpace())
       error_uri = "https://en.wikipedia.org/wiki/OAuth";

      var json = new // https://www.oauth.com/oauth2-servers/access-tokens/access-token-response/
      {
        error,
        error_description,
        error_uri
      };
      WorkContext.Response.StatusCode = code;
      WorkContext.Response.StatusDescription = "Bad request";

      if (trace.IsNotNullOrWhiteSpace())
      {
        WorkContext.Log(Log.MessageType.Error,
                        $"OAuth error trace",
                        $"{this.GetType().Name}.{nameof(ReturnError)}",
                        pars: new
                        {
                          e = error,
                          d = error_description,
                          uri = error_uri,
                          http = code,
                          trace = trace
                        }.ToJson(JsonWritingOptions.CompactASCII));
      }

      return new JsonResult(json, JsonWritingOptions.PrettyPrint);
    }

  }
}
