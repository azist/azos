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
using Azos.Serialization.JSON;
using Azos.Wave;

namespace Azos.Security.Services
{
  /// <summary>
  /// Provides a base for OAuth flow controllers.
  /// Derive from this class to implement OAuth controller customized for your system.
  /// This class depends on Azos.Security.Services.IOAuthModule present in app chassis
  /// </summary>
  [NoCache]
  public abstract partial class OAuthControllerBase : Controller
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
    [ApiEndpointDoc(
      Title = "OAuth Authorize GET",
      Description = "Provides entry point for OAuth `code` flow by getting a login page for the specified application",
      RequestQueryParameters = new []{
        "client_id: Client Id issued by your IDP during Client/App registration",
        "response_type: code | token (for implicit flow which returns token). Only `code` flow is supported as `token` flow is security flawed",
        "scope: e.g. `read` Specifies the level of access that the application is requesting",
        "redirect_uri: Where the service redirects the user-agent after an authorization code is granted",
        "state: Arbitrary String specific to client application"
      },
      ResponseContent = "200 with either an HTML login form or JSON if it was requested via `Accept` header. 401 for bad requested parameters"
    )]
    [ActionOnGet(Name = "authorize")]
    [ActionOnGet(Name = "authorization")]    //note: param naming here and below is dictated by OAuth specification
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
      var cluser = await OAuth.ClientSecurity.AuthenticateAsync(clcred).ConfigureAwait(false);
      if (!cluser.IsAuthenticated) return GateError(new Http401Unauthorized("Unknown client"));//we don't have ACL yet, hence can't check redirect_uri

      //2. Check client ACL for allowed redirect URIs
      var redirectPermission = new OAuthClientAppPermission(redirect_uri);//this call comes from front channel, hence we don't check for address
      var uriAllowed = await redirectPermission.CheckAsync(OAuth.ClientSecurity, cluser).ConfigureAwait(false);
      if (!uriAllowed) return GateError(new Http403Forbidden("Unauthorized redirect Uri"));

      //3. Establish a login flow instance of appropriate type (factory method)
      var loginFlow = MakeLoginFlow();
      loginFlow.ClientId = client_id;
      loginFlow.ClientResponseType = response_type;
      loginFlow.ClientUser = cluser;
      loginFlow.ClientScope = scope;
      loginFlow.ClientRedirectUri = redirect_uri;
      loginFlow.ClientState = state;

      //4. SSO: See if the subject user is already logged-in (SSO is turned on)
      TryExtractSsoSessionId(loginFlow);
      if (loginFlow.HasSsoSessionId)
      {
        await TryGetSsoSubjectAsync(loginFlow).ConfigureAwait(false);
        if (loginFlow.IsValidSsoUser)
        {
          await AdvanceLoginFlowStateAsync(loginFlow).ConfigureAwait(false);
          if (loginFlow.FiniteStateSuccess)//all set, there is nothing else to do with login, so shirt-circuit to OAuth redirect
          {
            //SSO success ------------------
            // 5A. Generate ClientAccessCodeToken
            var result = await GenerateSuccessfulClientAccessCodeTokenRedirectAsync(loginFlow.SsoSubjectUser,
                                                                                    client_id,
                                                                                    state,
                                                                                    redirect_uri).ConfigureAwait(false);
            return result;
          }
        }//ssoSubjectUser
      }//idSsoSession

      //5B. Generate result, such as JSON or Login Form
      var startedUtc = App.TimeSource.UTCNow.ToSecondsSinceUnixEpochStart();
      return RespondWithAuthorizeResult(startedUtc, loginFlow, error: null);
    }

    [ApiEndpointDoc(
      Title = "OAuth Authorize POST",
      Description = "Provides OAuth flow continuation taking Id/Password and returning client access code on success",
      RequestBody = "Login vector as: {roundtrip, id, pwd}",
      ResponseContent = "302 with client access code or 401 for bad requested parameters. 403 for unauthorized client URI"
    )]
    [ActionOnPost(Name = "authorize")]
    [ActionOnPost(Name = "authorization")]
    public async virtual Task<object> Authorize_POST(string roundtrip, string id, string pwd, bool stay = false)
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
      var cluser = await OAuth.ClientSecurity.AuthenticateAsync(clcred).ConfigureAwait(false);
      if (!cluser.IsAuthenticated) return GateError(new Http401Unauthorized("Unknown client"));//we don't have ACL yet, hence can't check redirect_uri

      //3. Check client ACL for allowed redirect URIs
      var redirectPermission = new OAuthClientAppPermission(flow["uri"].AsString());//this call comes from front channel, hence we don't check for address
      var uriAllowed = await redirectPermission.CheckAsync(OAuth.ClientSecurity, cluser).ConfigureAwait(false);
      if (!uriAllowed) return GateError(new Http403Forbidden("Unauthorized redirect Uri"));

      //4. Establish a login flow instance of appropriate type (factory method)
      var loginFlow = MakeLoginFlow();
      loginFlow.ClientId = clid;
      loginFlow.ClientResponseType = flow["tp"].AsString();
      loginFlow.ClientUser         = cluser;
      loginFlow.ClientScope        = flow["scp"].AsString();
      loginFlow.ClientRedirectUri  = flow["uri"].AsString();
      loginFlow.ClientState        = flow["st"].AsString();

      //5. Check user credentials for the subject
      var subjcred = new IDPasswordCredentials(id, pwd);
      var oauthCtx = new AuthenticationRequestContext
      {
        Intent = AuthenticationRequestContext.INTENT_OAUTH,
        Description = "OAuth"
      };

      if (OAuth.AccessTokenLifespanSec > 0)
      {
        oauthCtx.SysAuthTokenValiditySpanSec = OAuth.AccessTokenLifespanSec + 60;//+1 min for login
      }

      ConfigureAuthenticationRequestContext(loginFlow, oauthCtx);


      var subject = await App.SecurityManager.AuthenticateAsync(subjcred, oauthCtx).ConfigureAwait(false);
      loginFlow.SubjectUser = subject;
      loginFlow.SubjectUserWasJustSet = true;
      if (!subject.IsAuthenticated)  //e.g. 2FA will report as NOT(yet)Authenticated
      {
        //this call resulting in error is guaranteed to take at least 0.5 second to complete, throttling down the hack attempts
        await Task.Delay(Ambient.Random.NextScaledRandomInteger(500, 1500)).ConfigureAwait(false);

        //What is next, user is bad, e.g. process 2FA request
        await AdvanceLoginFlowStateAsync(loginFlow).ConfigureAwait(false);

        var redo = RespondWithAuthorizeResult(flow["sd"].AsLong(), loginFlow, "Bad login");//!!! DO NOT disclose any more details
        return GateUser(redo);
      }

      //success ------------------
      // 6. SSO
      var ssoSessionName = OAuth.SsoSessionName;//make copy
      if (stay && ssoSessionName.IsNotNullOrWhiteSpace())//if STAY logged-in was checked
      {
        await SetSsoSubjectSessionAsync(loginFlow, utcNow, subject).ConfigureAwait(false);
        //e.g. sets a cookie
        SetSsoSessionId(loginFlow, ssoSessionName);
      }

      //7. Advance the flow to determine whats next
      await AdvanceLoginFlowStateAsync(loginFlow).ConfigureAwait(false);

      if (loginFlow.FiniteStateSuccess)
      {
        // 8A. Generate ClientAccessCodeToken
        var result = await GenerateSuccessfulClientAccessCodeTokenRedirectAsync(subject,
                                                                                loginFlow.ClientId,
                                                                                loginFlow.ClientState,
                                                                                loginFlow.ClientRedirectUri,
                                                                                usePageRedirect: loginFlow.SsoWasJustSet).ConfigureAwait(false);
        return result;
      }

      //8B. Generate result, such as JSON or next step in the flow Form
      return RespondWithAuthorizeResult(flow["sd"].AsLong(), loginFlow, error: null);
    }


    [ApiEndpointDoc(
      Title = "OAuth Authorize Cancel",
      Description = "Provides ability to redirect back to the calling app without an authorization token effectively canceling the OAuth flow",
      RequestQueryParameters = new[]{"roundtrip: the roundtrip content produced by authorization/get"},
      ResponseContent = "302 without client access code or 401 for bad requested parameters. 403 for unauthorized client URI"
    )]
    [ActionOnGet(Name = "cancel")]
    public async virtual Task<object> Cancel_Get(string roundtrip)
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
      var cluser = await OAuth.ClientSecurity.AuthenticateAsync(clcred).ConfigureAwait(false);
      if (!cluser.IsAuthenticated) return GateError(new Http401Unauthorized("Unknown client"));//we don't have ACL yet, hence can't check redirect_uri

      //3. Check client ACL for allowed redirect URIs
      var uri = flow["uri"].AsString();
      var redirectPermission = new OAuthClientAppPermission(uri);//this call comes from front channel, hence we don't check for address
      var uriAllowed = await redirectPermission.CheckAsync(OAuth.ClientSecurity, cluser).ConfigureAwait(false);
      if (!uriAllowed) return GateError(new Http403Forbidden("Unauthorized redirect Uri"));

      return new Redirect(uri);
    }

    [ApiEndpointDoc(
      Title = "OAuth SSO Logout",
      Description = "Provides ability to logout user from single-sign-on IDP",
      RequestQueryParameters = new[]{
        "client_id: Client Id issued by your IDP during Client/App registration",
        "redirect_uri: Where the service redirects the user-agent after logout. Must be authorized for client_id",
        "state: Arbitrary String specific to client application"
      },
      ResponseContent = "302 redirect or 401 for bad requested parameters or logout is not supported. 403 for unauthenticated caller or unauthorized client URI"
    )]
    [ActionOnGet(Name = "sso-logout")]
    public async virtual Task<object> SsoLogout_Get(string client_id, string redirect_uri, string state)
    {
      var sso = OAuth.SsoSessionName;
      if (sso.IsNullOrWhiteSpace()) return new Http401Unauthorized("Logout not supported");

      if (client_id.IsNullOrWhiteSpace() || redirect_uri.IsNullOrWhiteSpace())
        return new Http401Unauthorized("Malformed request");//we can not redirect because redirect_uri has not been checked yet for inclusion in client ACL

      //1. Lookup client app, just by client_id (w/o password)
      var clcred = new EntityUriCredentials(client_id);
      var cluser = await OAuth.ClientSecurity.AuthenticateAsync(clcred).ConfigureAwait(false);
      if (!cluser.IsAuthenticated) return GateError(new Http401Unauthorized("Unknown client"));//we don't have ACL yet, hence can't check redirect_uri

      //2. Check client ACL for allowed redirect URIs
      var redirectPermission = new OAuthClientAppPermission(redirect_uri);//this call comes from front channel, hence we don't check for address
      var uriAllowed = await redirectPermission.CheckAsync(OAuth.ClientSecurity, cluser).ConfigureAwait(false);
      if (!uriAllowed) return GateError(new Http403Forbidden("Unauthorized redirect Uri"));


      //3. Establish a login flow instance of appropriate type (factory method)
      var loginFlow = MakeLoginFlow();
      loginFlow.ClientId = client_id;
      loginFlow.ClientResponseType = "code";
      loginFlow.ClientUser = cluser;
      loginFlow.ClientScope = "openidc";
      loginFlow.ClientRedirectUri = redirect_uri;
      loginFlow.ClientState = state;

      //4. SSO: See if the subject user is already logged-in and if so check it and log him out
      TryExtractSsoSessionId(loginFlow);
      if (loginFlow.HasSsoSessionId)
      {
        await TryGetSsoSubjectAsync(loginFlow).ConfigureAwait(false);
        if (loginFlow.IsValidSsoUser)
        {
          DeleteSsoSessionId(sso);

          //4A. Redirect to URI
          var redirect = new UriQueryBuilder(redirect_uri)
          {
            {"state", state}
          }.ToString();

          var suppressAutoRedirect = WebOptions.Of("suppress-auto-sso-redirect").ValueAsBool(false);
          return new Wave.Templatization.StockContent.OAuthSsoRedirect(redirect, suppressAutoRedirect);
        }//ssoSubjectUser
      }//idSsoSession

      return new Http403Forbidden();
    }


    /// <summary>
    /// Obtains the TOKEN based on the {Authorization Code} received in authorize step
    /// </summary>
    /// <param name="client_id">Client Id issued by your IDP during Client/App registration, if not supplied will try to parse out of [Authorization] header</param>
    /// <param name="client_secret">Client Secret as issued by the IDP during registration, if not supplied will try to parse out of [Authorization] header</param>
    /// <param name="grant_type">`authorization_code` or `refresh_token` (if enabled)</param>
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
    ///   "id_token": "{JWT OIDConnect}",      // - Always included
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
    [ApiEndpointDoc(
      Title = "OAuth Token POST",
      Description = "Obtains the TOKEN based either on the `Authorization Code`received in authorize step or `Refresh Token` received with previous call",
      RequestBody = "Data object supplied as JSOn body or request parameters (see)",
      RequestQueryParameters = new []{
        "client_id: Client Id issued by your IDP during Client/App registration, if not supplied will try to parse out of [Authorization] header",
        "client_secret: Client Secret as issued by the IDP during registration, if not supplied will try to parse out of [Authorization] header",
        "grant_type: `authorization_code` or `refresh_token` (if enabled)",
        "code: {Authorization Code} received as a part of authorize step response",
        "refresh_token: {Refresh Token} or refresh token gotten previously",
        "redirect_uri: Where the service redirects the user-agent after an token is issued"},
      ResponseContent = "200 with client access code and OIDC JWT or 401 for bad requested parameters. 403 for unauthorized client URI"
    )]
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
          clcred == null ||
          clcred.ID.IsNullOrWhiteSpace() ||
          (isAccessToken && code.IsNullOrWhiteSpace()) ||
          (isRefreshToken && refresh_token.IsNullOrWhiteSpace())
         ) return ReturnError("invalid_request", "Invalid client spec");

      //1. Check client/app credentials and get app's permissions
      var cluser = await OAuth.ClientSecurity.AuthenticateAsync(clcred).ConfigureAwait(false);
      if (!cluser.IsAuthenticated) return GateError(ReturnError("invalid_client", "Client denied", code: 401));

      //2. Validate the supplied client access code (token), that it exists (was issued and not expired), and it was issued for THIS client
      ClientAccessTokenBase clientToken;
      if (isAccessToken)
      {
        clientToken = await OAuth.TokenRing.GetAsync<ClientAccessCodeToken>(code).ConfigureAwait(false);

        //The access token is one-time use only:
        if (clientToken != null)
          await OAuth.TokenRing.DeleteAsync(code).ConfigureAwait(false);
      }
      else//refresh token
      {
        clientToken = await OAuth.TokenRing.GetAsync<ClientRefreshCodeToken>(refresh_token).ConfigureAwait(false);
      }

      if (clientToken == null)
        return GateError(ReturnError("invalid_grant", "Invalid grant", code: 403));

      //check that client_id supplied now matches the original one that was supplied during client access code issuance
      if (!clcred.ID.EqualsOrdSenseCase(clientToken.ClientId))
        return GateError(ReturnError("invalid_grant", "Invalid grant", code: 403));


      //3. Check that the requested redirect_uri is indeed in the list of permitted URIs for this client
      var redirectPermission = new OAuthClientAppPermission(redirect_uri, WorkContext.EffectiveCallerIPEndPoint.Address.ToString());
      var uriAllowed = await redirectPermission.CheckAsync(OAuth.ClientSecurity, cluser).ConfigureAwait(false);
      if (!uriAllowed) return GateError(ReturnError("invalid_grant", "Invalid grant", code: 403));

      //4. Fetch subject/target user
      var auth = SysAuthToken.Parse(clientToken.SubjectSysAuthToken);
      var targetUser = await App.SecurityManager.AuthenticateAsync(auth).ConfigureAwait(false); //re-authenticate the user using the original token.
      if (!targetUser.IsAuthenticated)                                    //the returned AuthToken must be logically the same as the one granted on login above
        return ReturnError("invalid_grant", "Invalid grant", code: 403);//no need for gate, the token just got denied

      //5. Issue the API access token for this access code
      var accessToken = OAuth.TokenRing.GenerateNew<AccessToken>(OAuth.AccessTokenLifespanSec);
      accessToken.ClientId = clcred.ID;
      accessToken.SubjectSysAuthToken = targetUser.AuthToken.ToString();//pointing to the subject user

      var token = await OAuth.TokenRing.PutAsync(accessToken).ConfigureAwait(false);

      //6. Optionally issue a refresh token
      string refreshToken = null;
      if (OAuth.RefreshTokenLifespanSec > 0)
      {
        //https://www.oauth.com/oauth2-servers/access-tokens/refreshing-access-tokens/
        var refreshTokenData = OAuth.TokenRing.GenerateNew<ClientRefreshCodeToken>(OAuth.RefreshTokenLifespanSec);
        refreshTokenData.ClientId = clcred.ID;
        refreshTokenData.SubjectSysAuthToken = targetUser.AuthToken.ToString();
        refreshToken = await OAuth.TokenRing.PutAsync(refreshTokenData).ConfigureAwait(false);
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

    //https://openid.net/specs/openid-connect-basic-1_0-28.html#userinfo
    [ActionOnGet(Name = "userinfo")]
    [AuthenticatedUserPermission]
    public object UserInfo()
    {
      var user = WorkContext?.Session?.User;

      if (user==null) return new Http403Forbidden("No user");

      var id_token = new JsonDataMap
      {
        {"iat", App.TimeSource.UTCNow.ToSecondsSinceUnixEpochStart()},
        {"sub", user.Name},
        {"name", user.Description},
      };

      AddExtraClaimsToIDToken(null, user, null, id_token);

      return new JsonResult(id_token, JsonWritingOptions.PrettyPrintRowsAsMap);
    }
  }
}
