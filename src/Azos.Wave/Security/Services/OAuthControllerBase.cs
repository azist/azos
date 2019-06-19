using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
    protected IOAuthManager OAuth => (App.SecurityManager as IOAuthManagerHost).NonNull("not IOAuthManagerHost").OAuthManager;

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
    public object Authorize_GET(string client_id, string response_type, string scope, string redirect_uri, string state)
    {
      return null;
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
    public async Task<object> Token_POST(string client_id, string client_secret, string grant_type, string code, string redirect_uri)
    {
      //todo check grant_type - and possibly pivot

      var clcred = client_id.IsNotNullOrWhiteSpace() ? new IDPasswordCredentials(client_id, client_secret)
                                                     : IDPasswordCredentials.FromBasicAuth(WorkContext.Request.Headers[WebConsts.HTTP_HDR_AUTHORIZATION]);

      if (clcred==null || clcred.ID.IsNullOrWhiteSpace())
      {
        //todo report client invalid credentials error (how do we return errors? to redirect? or http status code?)
      }

      //1. Check client/app credentials and get app's permissions
      var cluser = await OAuth.ClientSecurity.AuthenticateAsync(clcred);
      if (!cluser.IsAuthenticated)
      {
        //todo report client invalid credentials error (how do we return errors? to redirect? or http status code?)
      }

      //2. Check that the requested redirect_uri is indeed in the list of permitted URIs for this client
      //todo check redirect URI is in the list of permitted redirect URIS for cluster


      //3. Validate the supplied client access code (token), that it exists (was issued and not expired), and it was issued for THIS client
      var catoken = await OAuth.TokenRing.LookupClientAccessCodeAsync(code);
      if (catoken == null)
      {
        //check that client_id supplied now matches the original one that was supplied during client access code issuance
        if (!clcred.ID.EqualsOrdSenseCase(catoken.ClientId))
        {
          //todo report client invalid client access code error (how do we return errors? to redirect? or http status code?)
        }
      }

      //4. Fetch target user
      var auth = OAuth.TokenRing.TargetAuthenticationTokenFromContent(catoken.TargetAuthenticationToken);
      var targetUser = App.SecurityManager.Authenticate(auth);
      if (!targetUser.IsAuthenticated)
      {
        //todo Access denied, already revoked
      }

      //5. Issue the API access token for this access code
      var token = await OAuth.TokenRing.IssueAccessToken(cluser, targetUser);

      var jwt = OAuth.TokenRing.MakeJWT(token);

      return new JSONResult(jwt, Serialization.JSON.JsonWritingOptions.PrettyPrint);//todo: Where is base64 encoding?
    }

    [ActionOnGet(Name = "userinfo")]
    public object UserInfo()
    {
      return null;
    }
  }
}
