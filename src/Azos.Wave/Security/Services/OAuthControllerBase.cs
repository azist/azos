/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Data;
using Azos.Wave.Mvc;
using Azos.Security.Tokens;
using System.Collections.Generic;
using Azos.Serialization.JSON;

namespace Azos.Security.Services
{
  /// <summary>
  /// Provides a base for OAuth flow controllers.
  /// Derive from this class to implement OAuth controller customized for your system.
  /// This class depends on Azos.Security.Services.IOAuthModule present in app chassis
  /// </summary>
  [NoCache]
  public abstract class OAuthControllerBase : Controller
  {
    //https://www.digitalocean.com/community/tutorials/an-introduction-to-oauth-2
    //https://medium.com/@darutk/diagrams-and-movies-of-all-the-oauth-2-0-flows-194f3c3ade85
    //https://developer.okta.com/blog/2019/05/01/is-the-oauth-implicit-flow-dead
    //https://www.oauth.com/oauth2-servers/access-tokens/authorization-code-request/

    protected OAuthControllerBase() : base() { }
    protected OAuthControllerBase(IOAuthModule oauth) : base()
    => m_OAuth = oauth.NonNull(nameof(oauth));

    [InjectModule] private IOAuthModule m_OAuth;

    /// <summary> References IOAuthModule dependency </summary>
    protected IOAuthModule OAuth => m_OAuth;


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
    /// Represents the entry point of OAuth flow
    /// </summary>
    /// <param name="client_id">Client Id issued by your IDP during Client/App registration</param>
    /// <param name="response_type">code | token (for implicit flow which returns token)</param>
    /// <param name="scope">e.g. "read" Specifies the level of access that the application is requesting</param>
    /// <param name="redirect_uri">Where the service redirects the user-agent after an authorization code is granted</param>
    /// <param name="state">Arbitrary String specific to client application</param>
    /// <returns>
    /// <code>
    /// HTTP/1.1 302 Found
    /// Location: {Redirect URI}
    ///   ?code={Authorization Code}  // - Always included
    ///   &state={Arbitrary String}     // - Included if the authorization
    ///                                 //   request included 'state'.
    /// </code>
    /// </returns>
    [ActionOnGet(Name = "authorize")]
    [ActionOnGet(Name = "authorization")]
    public async Task<object> Authorize_GET(string response_type, string scope, string client_id, string redirect_uri, string state)
    {
      //only Support CODE
      if (!response_type.EqualsOrdIgnoreCase("code"))
        return new Http401Unauthorized("Unsupported capability");//we can not redirect because redirect_uri has not been checked yet for inclusion in client ACL

      //the only SCOPE
      if (!OAuth.CheckScope(scope))
        return new Http401Unauthorized("Unsupported scope");//we can not redirect because redirect_uri has not been checked yet for inclusion in client ACL

      if (client_id.IsNullOrWhiteSpace() ||
          redirect_uri.IsNullOrWhiteSpace())
        return new Http401Unauthorized("Malformed request");//we can not redirect because redirect_uri has not been checked yet for inclusion in client ACL

      //1. Lookup client app, just by client_id (w/o password)
      var clcred = new EntityUriCredentials(client_id);
      var cluser = await OAuth.ClientSecurity.AuthenticateAsync(clcred);
      if (!cluser.IsAuthenticated) return GateError(new Http401Unauthorized("Unknown client"));//we don't have ACL yet, hence can't check redirect_uri

      //2. Check client ACL for allowed redirect URIs
      var redirectPermission = new OAuthClientAppPermission(redirect_uri);//this call comes from front channel, hence we don't check for address
      var uriAllowed = await redirectPermission.CheckAsync(App, cluser);
      if (!uriAllowed) return GateError(new Http403Forbidden("Unauthorized redirect Uri"));

      //3. Generate result, such as JSON or Login Form
      var startedUtc = App.TimeSource.UTCNow.ToSecondsSinceUnixEpochStart();
      return RespondWithAuthorizeResult(startedUtc, cluser, response_type, scope, client_id, redirect_uri, state, error: null);
    }

    protected virtual object RespondWithAuthorizeResult(long sdUtc, User clientUser, string response_type, string scope, string client_id, string redirect_uri, string state, string error)
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

      return MakeAuthorizeResult(clientUser, roundtrip, error);
    }

    protected virtual object MakeAuthorizeResult(User clientUser, string roundtrip, string error)
    {
      if (WorkContext.RequestedJson)
        return new { OK = error.IsNullOrEmpty(), roundtrip, error };

      return new Wave.Templatization.StockContent.OAuthLogin(clientUser, roundtrip, error);
    }


    [ActionOnPost(Name = "authorize")]
    [ActionOnPost(Name = "authorization")]
    public async virtual Task<object> Authorize_POST(string roundtrip, string id, string pwd)
    {
      var flow = App.SecurityManager.PublicUnprotectMap(roundtrip);
      if (flow == null) return GateError(new Http401Unauthorized("Bad Request X1"));//we don't have ACL yet, hence can't check redirect_uri

      //1. check token age
      var utcNow = App.TimeSource.UTCNow;
      var age = utcNow - flow["sd"].AsLong(0).FromSecondsSinceUnixEpochStart();
      if (age.TotalSeconds > OAuth.MaxAuthorizeRoundtripAgeSec) return GateError(new Http401Unauthorized("Bad Request X2"));

      //2. Lookup client app, just by client_id (w/o password)
      var clid = flow["id"].AsString();
      var clcred = new EntityUriCredentials(clid);
      var cluser = await OAuth.ClientSecurity.AuthenticateAsync(clcred);
      if (!cluser.IsAuthenticated) return GateError(new Http401Unauthorized("Unknown client"));//we don't have ACL yet, hence can't check redirect_uri

      //3. Check client ACL for allowed redirect URIs
      var redirectPermission = new OAuthClientAppPermission(flow["uri"].AsString());//this call comes from front channel, hence we don't check for address
      var uriAllowed = await redirectPermission.CheckAsync(App, cluser);
      if (!uriAllowed) return GateError(new Http403Forbidden("Unauthorized redirect Uri"));

      //4. Check user credentials for the subject
      var subjcred = new IDPasswordCredentials(id, pwd);
      var subject = await App.SecurityManager.AuthenticateAsync(subjcred);
      if (!subject.IsAuthenticated)
      {
        await Task.Delay(1000);//this call resulting in error is guaranteed to take at least 1 second to complete, throttling down the hack attempts
        var redo = RespondWithAuthorizeResult(flow["sd"].AsLong(),
                                       cluser,
                                       flow["tp"].AsString(),
                                       flow["scp"].AsString(),
                                       clid,
                                       flow["uri"].AsString(),
                                       flow["st"].AsString(),
                                       "Bad login");//!!! DO NOT disclose any more details

        return GateUser(redo);
      }

      //success ------------------

     // 5. Generate ClientAccessCodeToken
      var acToken = OAuth.TokenRing.GenerateNew<ClientAccessCodeToken>();
      acToken.ClientId = clid;
      acToken.State = flow["st"].AsString();
      acToken.RedirectURI = flow["uri"].AsString();
      acToken.SubjectSysAuthToken = subject.AuthToken.ToString();
      var accessCode = await OAuth.TokenRing.PutAsync(acToken);

      //6. Redirect to URI
      var redirect = new UriQueryBuilder(flow["uri"].AsString())
      {
        {"code", accessCode},
        {"state", flow["st"].AsString()}
      }.ToString();

      return new Redirect(redirect);
    }

    /// <summary>
    /// Obtains the TOKEN based on the {Authorization Code} received in authorize step
    /// </summary>
    /// <param name="client_id">Client Id issued by your IDP during Client/App registration, if not supplied will try to parse out of [Authorization] header</param>
    /// <param name="client_secret">Client Secret as issued by the IDP during registration, if not supplied will try to parse out of [Authorization] header</param>
    /// <param name="grant_type">authorization_code</param>
    /// <param name="code">{Authorization Code} received as a part of authorize step response</param>
    /// <param name="refresh_token">{Refresh Token} or refresh token gotten previously</param>
    /// <param name="redirect_uri">Where the service redirects the user-agent after an token is issued</param>
    /// <returns>
    /// <code>
    /// HTTP/1.1 200 OK
    ///   Content-Type: application/json;charset=UTF-8
    ///   Cache-Control: no-store
    /// Pragma: no-cache
    /// {
    ///   "access_token": "{Access Token}",    // - Always included
    ///   "token_type": "{Token Type}",        // - Always included
    ///   "expires_in": {Lifetime In Seconds}  // - Optional
    ///   "refresh_token": "{Refresh Token}",  // - Optional
    ///   "scope": "{Scopes}"                  // - Mandatory if the granted
    ///                                        //   scopes differ from the
    ///                                        //   requested ones.
    /// }
    /// </code>
    /// </returns>
    [ActionOnPost(Name = "token")]
    public async Task<object> Token_POST(string client_id, string client_secret,
                                         string grant_type,
                                         string code, string refresh_token,//either accessCode or RefreshToken are used
                                         string redirect_uri)
    {
      var isAccessToken = grant_type.EqualsOrdSenseCase("authorization_code");
      var isRefreshToken = grant_type.EqualsOrdSenseCase("refresh_token");

      if (!isAccessToken && !isRefreshToken)
        return ReturnError("unsupported_grant_type", "Unsupported grant_type");

      var clcred = client_id.IsNotNullOrWhiteSpace() ? new IDPasswordCredentials(client_id, client_secret)
                                                     : IDPasswordCredentials.FromBasicAuth(WorkContext.Request.Headers[WebConsts.HTTP_HDR_AUTHORIZATION]);

      if (
          clcred==null ||
          clcred.ID.IsNullOrWhiteSpace() ||
          (isAccessToken && code.IsNullOrWhiteSpace()) ||
          (isRefreshToken && refresh_token.IsNullOrWhiteSpace())
         ) return ReturnError("invalid_request", "Invalid client spec");

      //1. Check client/app credentials and get app's permissions
      var cluser = await OAuth.ClientSecurity.AuthenticateAsync(clcred);
      if (!cluser.IsAuthenticated) return GateError(ReturnError("invalid_client", "Client denied", code: 401));

      //2. Validate the supplied client access code (token), that it exists (was issued and not expired), and it was issued for THIS client
      ClientAccessTokenBase clientToken;
      if (isAccessToken)
      {
        clientToken = await OAuth.TokenRing.GetAsync<ClientAccessCodeToken>(code);

        //The access token is one-time use only:
        if (clientToken!=null)
          await OAuth.TokenRing.DeleteAsync(code);
      }
      else//refresh token
      {
        clientToken = await OAuth.TokenRing.GetAsync<ClientRefreshCodeToken>(refresh_token);
      }

      if (clientToken == null)
        return GateError(ReturnError("invalid_grant", "Invalid grant", code: 403));

      //check that client_id supplied now matches the original one that was supplied during client access code issuance
      if (!clcred.ID.EqualsOrdSenseCase(clientToken.ClientId))
        return GateError(ReturnError("invalid_grant", "Invalid grant", code: 403));


      //3. Check that the requested redirect_uri is indeed in the list of permitted URIs for this client
      var redirectPermission = new OAuthClientAppPermission(redirect_uri, WorkContext.EffectiveCallerIPEndPoint.Address.ToString());
      var uriAllowed = await redirectPermission.CheckAsync(App, cluser);
      if (!uriAllowed) return GateError(ReturnError("invalid_grant", "Invalid grant", code: 403));

      //4. Fetch subject/target user
      var auth = SysAuthToken.Parse(clientToken.SubjectSysAuthToken);
      var targetUser = await App.SecurityManager.AuthenticateAsync(auth);
      if (!targetUser.IsAuthenticated)
        return ReturnError("invalid_grant", "Invalid grant", code: 403);//no need for gate, the token just got denied

      //5. Issue the API access token for this access code
      var accessToken = OAuth.TokenRing.GenerateNew<AccessToken>();
      accessToken.ClientId = clcred.ID;
      accessToken.SubjectSysAuthToken = targetUser.AuthToken.ToString();

      var token = await OAuth.TokenRing.PutAsync(accessToken);

      //6. Optionally issue a refresh token
      string refreshToken = null;
      if (OAuth.RefreshTokenLifespanSec > 0)
      {
        //https://www.oauth.com/oauth2-servers/access-tokens/refreshing-access-tokens/
        var refreshTokenData = OAuth.TokenRing.GenerateNew<ClientRefreshCodeToken>(OAuth.RefreshTokenLifespanSec);
        refreshTokenData.ClientId = clcred.ID;
        refreshTokenData.SubjectSysAuthToken = targetUser.AuthToken.ToString();
        refreshToken = await OAuth.TokenRing.PutAsync(refreshTokenData);
      }

      // https://openid.net/specs/openid-connect-basic-1_0-28.html#id.token.validation
      // https://tools.ietf.org/html/rfc7519#section-4.1.4
      var id_token = new JsonDataMap
      {
        {"iss", accessToken.IssuedBy},
        {"aud", clcred.ID},
        {"exp", accessToken.ExpireUtc},//in seconds
        {"iat", accessToken.IssueUtc}, //in seconds
        {"sub", targetUser.Name},
        {"name", targetUser.Description},
      };

      AddExtraClaimsToIDToken(cluser, targetUser, accessToken, id_token);

      var jwt_id_token = App.SecurityManager.PublicProtectJWTPayload(id_token);

      var result = new JsonDataMap // https://www.oauth.com/oauth2-servers/access-tokens/access-token-response/
      {
        {"id_token", jwt_id_token}, // Canonical JWT format hdr.payload.hash
        {"access_token", token},
        {"token_type", "bearer"},
        {"expires_in", (int)(accessToken.ExpireUtcTimestamp - accessToken.IssueUtcTimestamp).Value.TotalSeconds}
      };

      if (refreshToken != null) result["refresh_token"] = refreshToken;

      AddExtraFieldsToResponseBody(cluser, targetUser, accessToken, result);

      //No cache is set on whole controller
      //for clarity
      WorkContext.Response.SetNoCacheHeaders(force: true);
      return new JsonResult(result, JsonWritingOptions.PrettyPrint);
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

    //https://openid.net/specs/openid-connect-basic-1_0-28.html#userinfo
    [ActionOnGet(Name = "userinfo")]
    [AuthenticatedUserPermission]
    public object UserInfo()
    {
      var user = WorkContext?.Session?.User;
      return new { sub = user.Name, name = user.Description};
    }
  }
}
