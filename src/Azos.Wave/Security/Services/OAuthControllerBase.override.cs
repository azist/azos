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

namespace Azos.Security.Services
{
  partial class OAuthControllerBase
  {

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
    /// Override to extract SSO session cookie from the request.
    /// Default implementation uses Request Cookie named OAuth.SsoSessionName if it is set.
    /// You must return null if there is no such session id provided or OAuth.ssoSessionName is turned off (null or whitespace)
    /// </summary>
    /// <returns>Null or SSO Session ID as supplied by the caller</returns>
    protected virtual string TryExtractSsoSessionId()
    {
      var ssoCookieName = OAuth.SsoSessionName;//copy
      if (ssoCookieName.IsNullOrWhiteSpace()) return null;
      var cookie = WorkContext.Request.Cookies[ssoCookieName];
      if (cookie == null) return null;
      var result = cookie.Value;
      if (result.IsNullOrWhiteSpace()) return null;
      return result;
    }

    /// <summary>
    /// Tries to get SSO subject user by examining the supplied idSsoSession.
    /// The session identified by the ID must match the supplied client user and scope.
    /// Returns NULL if the SSO session id is invalid/or user revoked etc..
    /// </summary>
    protected async virtual Task<(User subject, bool hasClient, bool hasScope)> TryGetSsoSubjectAsync(string idSsoSession, User clientUser, string scope)
    {
      var session = await OAuth.TokenRing.GetAsync<SsoSessionToken>(idSsoSession).ConfigureAwait(false);
      if (session == null) return (null, false, false);

      if (!SysAuthToken.TryParse(session.SysAuthToken, out var sysToken)) return (null, false, false);

      var ssoSubject =  await App.SecurityManager.AuthenticateAsync(sysToken).ConfigureAwait(false);
      if (!ssoSubject.IsAuthenticated) return (null, false, false); //sys auth token may have expired

      //(hasClient, hasScope) = virtual InspectRights(user, clientUser, scope);//override to check your db table etc...
      //return (subject, hasClient, hasScope)


      return (ssoSubject, false, false);//for now
    }

    protected virtual object RespondWithAuthorizeResult(long sdUtc, User clientUser, User ssoSubjectUser, string response_type, string scope, string client_id, string redirect_uri, string state, string error)
    {
      //Pack all requested content(session) into cryptographically encoded message aka "roundtrip"
      var flow = new {
        sd = sdUtc,
        iss = App.TimeSource.UTCNow.ToSecondsSinceUnixEpochStart(),
        tp = response_type,
        scp = scope,
        id = client_id,
        uri = redirect_uri,
        st = state
      };
      var roundtrip = App.SecurityManager.PublicProtectAsString(flow);

      if (error!=null)
      {
        WorkContext.Response.StatusCode = WebConsts.STATUS_403;
        WorkContext.Response.StatusDescription = error;
      }

      return MakeAuthorizeResult(clientUser, ssoSubjectUser, scope, roundtrip, error);
    }

    /// <summary>
    /// Override to provide a authorize result which is by default either a stock login form or
    /// JSON object
    /// </summary>
    protected virtual object MakeAuthorizeResult(User clientUser, User ssoSubject, string scope, string roundtrip, string error)
    {
      if (WorkContext.RequestedJson)
        return new { OK = error.IsNullOrEmpty(), roundtrip, error };

//20211029 DKh
//todo: use SSOSUBJECT to return a different page with authorization of CLIENTUSER and SCOPE

      //default always returns stock log-in page
      return new Wave.Templatization.StockContent.OAuthLogin(clientUser, roundtrip, error);
    }


    protected virtual async Task<object> GenerateSuccessfulClientAccessCodeTokenRedirectAsync(User subject, string clid, string state, string uri)
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

      return new Redirect(redirect);
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

    protected object ReturnError(string error, string error_description, string error_uri = null, int code = 400)
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
      return new JsonResult(json, JsonWritingOptions.PrettyPrint);
    }

  }
}
