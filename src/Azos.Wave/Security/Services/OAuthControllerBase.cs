using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azos.Data;
using Azos.Wave.Mvc;

namespace Azos.Security.Services
{
  /// <summary>
  /// Provides a base for OAuth flow controllers.
  /// Derive from this class to implement OAuth controller customized for your system
  /// </summary>
  [NoCache]
  public abstract class OAuthControllerBase : Controller
  {
    //https://www.digitalocean.com/community/tutorials/an-introduction-to-oauth-2
    //https://medium.com/@darutk/diagrams-and-movies-of-all-the-oauth-2-0-flows-194f3c3ade85
    //https://developer.okta.com/blog/2019/05/01/is-the-oauth-implicit-flow-dead

    /// <summary> Returns IOAuthManager module off the App.SecMan </summary>
    protected IOAuthManager OAuth => (App.SecurityManager as IOAuthManagerHost).NonNull("not IOAuthManagerHost make sure App.Secman is configured to use IOAuthManagerHost").OAuthManager;

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
      if (!scope.EqualsOrdIgnoreCase("openid connect"))//todo use Constant
        return new Http401Unauthorized("Unsupported capability");//we can not redirect because redirect_uri has not been checked yet for inclusion in client ACL

      if (client_id.IsNullOrWhiteSpace() ||
          redirect_uri.IsNullOrWhiteSpace())
        return new Http401Unauthorized("Malformed request");//we can not redirect because redirect_uri has not been checked yet for inclusion in client ACL

      //1. Lookup client app, just by client_id (w/o password)
      var clcred = new EntityUriCredentials(client_id);
      var cluser = await OAuth.ClientSecurity.AuthenticateAsync(clcred);
      if (!cluser.IsAuthenticated) return new Http401Unauthorized("Unknown client");//we don't have ACL yet, hence can't check redirect_uri
                                                                                    //todo <------------------- ATTACK THREAT: GATE the caller

      //2. Check client ACL for allowed redirect URIs
      var redirectPermission = new OAuthClientAppPermission(redirect_uri);
      var uriAllowed = await redirectPermission.CheckAsync(App, cluser);
      if (!uriAllowed) return new Http403Forbidden("Unauthorized URI");//todo <------------------- ATTACK THREAT: GATE the caller

      //3. Generate result, such as JSON or Login Form
      return MakeAuthorizeResult(cluser, response_type, scope, client_id, redirect_uri, state, error: null);
    }

    protected virtual object MakeAuthorizeResult(User clientUser, string response_type, string scope, string client_id, string redirect_uri, string state, string error)
    {
      //Pack all requested content(session) into cryptographically encoded message
      var flow = new { tp = response_type, scp = scope, id = client_id, uri = redirect_uri, st = state, utc = App.TimeSource.UTCNow };
      var roundtrip = App.SecurityManager.PublicProtectAsString(flow);

      //todo Sett HTTP status code per error
      return new { OK=true, roundtrip };

      //todo if request non json, return UI
    }

    [ActionOnPost(Name = "authorize")]
    [ActionOnPost(Name = "authorization")]
    public async virtual Task<object> Authorize_POST(string roundtrip, string id, string pwd)
    {
      var flow = App.SecurityManager.PublicUnprotectMap(roundtrip);
      if (flow == null) return new Http401Unauthorized("Bad Request");//we don't have ACL yet, hence can't check redirect_uri
                                                                      //todo <------------------- ATTACK THREAT: GATE the caller

      //1. check token age
      var age = App.TimeSource.UTCNow - flow["utc"].AsDateTime(DateTime.MinValue);
      if (age.TotalSeconds > 600) return new Http401Unauthorized("Bad Request");//todo MOVE to setting/constant default

      //2. Lookup client app, just by client_id (w/o password)
      var clcred = new EntityUriCredentials(flow["id"].AsString());
      var cluser = await OAuth.ClientSecurity.AuthenticateAsync(clcred);
      if (!cluser.IsAuthenticated) return new Http401Unauthorized("Unknown client");//we don't have ACL yet, hence can't check redirect_uri
                                                                                    //todo <------------------- ATTACK THREAT: GATE the caller

      //3. Check user credentials
      var subjcred = new IDPasswordCredentials(id, pwd);
      var subject = await App.SecurityManager.AuthenticateAsync(subjcred);
      if (!subject.IsAuthenticated)
        return MakeAuthorizeResult(cluser, flow["tp"].AsString(), flow["scp"].AsString(), flow["id"].AsString(), flow["uri"].AsString(), flow["st"].AsString(), "Invalid credentials");

      //success ------------------

     // 4. Generate Accesscode token
      var accessCode = "aaaa";//OAuth.TokenRing.IssueClientAccessToken(clientid, subject.AuthToken, session.redirect_uri, session.state);
      //5. Redirect to URI

      var redirect = "{0}?token={1}&state={2}".Args(flow["uri"].AsString(), accessCode, flow["st"].AsString());//todo Encode URI
      return new Redirect(redirect);
    }

    /// <summary>
    /// Obtains the TOKEN based on the {Authorization Code} received in authorize step
    /// </summary>
    /// <param name="client_id">Client Id issued by your IDP during Client/App registration, if not supplied will try to parse out of [Authorization] header</param>
    /// <param name="client_secret">Client Secret as issued by the IDP during registration, if not supplied will try to parse out of [Authorization] header</param>
    /// <param name="grant_type">authorization_code</param>
    /// <param name="code">{Authorization Code} received as a part of authorize step response</param>
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
      if (!cluser.IsAuthenticated) return ReturnError("invalid_client", "Client denied", code: 401); //todo <------------------- ATTACK THREAT: GATE the caller

      //2. Validate the supplied client access code (token), that it exists (was issued and not expired), and it was issued for THIS client
      ClientAccessTokenBase clientToken;
      if (isAccessToken)
      {
        clientToken = await OAuth.TokenRing.GetAsync<ClientAccessCodeToken>(code);
      }
      else//refresh token
      {
        clientToken = await OAuth.TokenRing.GetAsync<ClientRefreshCodeToken>(refresh_token);
      }

      if (clientToken == null)
        return ReturnError("invalid_grant", "Invalid grant", code: 403);//todo <------------------- ATTACK THREAT: GATE the caller

      if (clientToken.Validate() != null)//one token used in place of another
        return ReturnError("invalid_request", "Invalid client spec");//todo <------------------- ATTACK THREAT: GATE the caller

      //check that client_id supplied now matches the original one that was supplied during client access code issuance
      if (!clcred.ID.EqualsOrdSenseCase(clientToken.ClientId))
        return ReturnError("invalid_grant", "Invalid grant", code: 403);//todo <------------------- ATTACK THREAT: GATE the caller




      //3. Check that caller IP is in the list of allowed caller IP submasks IPv4
      //todo --------------------------------
      //todo <------------------- ATTACK THREAT: GATE the caller

      //4. Check that the requested redirect_uri is indeed in the list of permitted URIs for this client
      var redirectPermission = new OAuthClientAppPermission(redirect_uri);
      var uriAllowed = await redirectPermission.CheckAsync(App, cluser);
      if (!uriAllowed) return ReturnError("invalid_grant", "Invalid grant", code: 403);//todo <------------------- ATTACK THREAT: GATE the caller

      //5. Fetch target user
      var auth = new AuthenticationToken("REALM what?", clientToken.SubjectAuthenticationToken);
      var targetUser = await App.SecurityManager.AuthenticateAsync(auth);
      if (!targetUser.IsAuthenticated)
        return ReturnError("invalid_grant", "Invalid grant", code: 403);//no need for gate

      //6. Issue the API access token for this access code
      var accessToken = OAuth.TokenRing.GenerateNew<AccessToken>();
      accessToken.ClientId = "aaaaa";//cluser;
      accessToken.SubjectAuthenticationToken = "todo ";//targetUser.AuthToken.Data;

      var token = await OAuth.TokenRing.PutAsync(accessToken);

      var json = new // https://www.oauth.com/oauth2-servers/access-tokens/access-token-response/
      {
        access_token = token,
        token_type = "bearer",
        //scope = "access",
        //refresh_token = "optional", <----- create ONE
        expires_in =(int)(accessToken.ExpireUtc - accessToken.IssueUtc).Value.TotalSeconds
      };


      //No cache is set on whole controller
      return new JsonResult(json, Serialization.JSON.JsonWritingOptions.PrettyPrint);
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
      return new JsonResult(json, Serialization.JSON.JsonWritingOptions.PrettyPrint);
    }

    [ActionOnGet(Name = "userinfo")]
    public object UserInfo()
    {
      return null;
    }
  }
}
