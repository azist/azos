using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Instrumentation;
using Azos.Serialization.JSON;

namespace Azos.Security.Services
{
  /// <summary>
  /// Decorates entities that host IOAuthManager implementation.
  /// By Convention application which support OAuth should implement this interface using composition pattern off their ISecurityManager,
  /// therefore OAuth-consuming applications shall reconsider the type of their ISecurityManager implementation as it will also be used for OAuth flow
  /// (revise what types of Credentials are accepted as some credential types might need to be restricted for OAuth).
  /// </summary>
  public interface IOAuthManagerHost
  {
    IOAuthManager OAuthManager { get; }
  }

  /// <summary>
  /// Describes entity which manages IAM/IDP services, such as token rings and underlying data stores
  /// </summary>
  public interface IOAuthManager : IApplicationComponent
  {

    /// <summary>
    /// Returns security manager responsible for authentication and authorization of clients(applications) which
    /// request access to the system on behalf of the user.
    /// The returned `User` object represents a requesting client party/application along with its rights, such as ability
    /// to execute "implicit" OAuth flows etc.
    /// </summary>
    ISecurityManager ClientSecurity {  get; }

    /// <summary>
    /// Returns a special kind of data manager/store which manages tokens/temp keys issued at different stages of various
    /// flows such as OAuth token grant, refresh token etc.
    /// </summary>
    ITokenRing TokenRing { get; }
  }

  public interface IOAuthManagerImplementation : IOAuthManager, IDaemon, IInstrumentable
  {
  }


  /// <summary>
  /// Manages tokens issued at different stages of various flows such as OAuth token grant, refresh token etc.
  /// </summary>
  public interface ITokenRing : IApplicationComponent
  {
    /// <summary>
    /// Generates new instance of RingToken-derivative of the specified type, generating new Token.Value and setting header fields (create date etc..)
    /// </summary>
    TToken GenerateNewToken<TToken>() where TToken : RingToken;

    /// <summary>
    /// Creates system-internal AuthenticationToken which represents a subject (a target user) impersonated by other tokens in a TokenRing
    /// </summary>
    /// <param name="content">String representation obtained by a complementary call to MapSubjectAuthenticationTokenToContent</param>
    /// <returns>System-internal AuthenticationToken representing a subject</returns>
    AuthenticationToken MapSubjectAuthenticationTokenFromContent(string content);

    /// <summary>
    /// Represents system-internal subject AuthenticationToken as a string content suitable for storage on a TokenRing
    /// </summary>
    /// <param name="token">System-internal AuthenticationToken which represents a subject (a target user) </param>
    /// <returns>String representation which can be converted back to AuthenticationToken by a complementary call to MapSubjectAuthenticationTokenFromContent</returns>
    string MapSubjectAuthenticationTokenToContent(AuthenticationToken token);

    /// <summary>
    /// Maps external Access Token string into system-internal AuthenticationToken as understood by the Application SecurityManager.
    /// This method is purposely synchronous as it handles high volume of API calls and is expected to be
    /// implemented efficiently with in-Memory caching technique.
    /// </summary>
    /// <param name="accessToken">External auth token value as the one issued in OAuth flow</param>
    /// <returns>Internal AuthenticationToken or null if not found (deny access)</returns>
    AuthenticationToken? MapAccessToken(string accessToken);

    /// <summary>
    /// Retrieves an existing client access code token or null if it is not found or expired
    /// </summary>
    Task<ClientAccessCodeToken> LookupClientAccessCodeAsync(string accessCode);

    Task<AccessToken> IssueAccessToken(User userClient, User targetUser);

    /////////// <summary>
    /////////// Creates JWT JSON data map representation of AccessToken
    /////////// </summary>
    ////////JsonDataMap MakeJWT(AccessToken accessToken);


    /// <summary>
    /// Instantly invalidates all tokens issued for the specified client.
    /// The caller must have admin grant to succeed
    /// </summary>
    /// <remarks>
    /// This is used to invalidate all tokens when the specified client gets banned in the external security manager referenced by  IOAuthManager.ClientSecurity.
    /// This method does not ban the future client-related access, do ban the user in ClientSecurity first as
    /// this method just resets tokens in a ring
    /// </remarks>
    Task InvalidateClient(string clientID);

    /// <summary>
    /// Instantly invalidates ALL tokens issued for specified target subject AuthenticationToken.
    /// The caller must have admin grant to succeed
    /// </summary>
    /// <remarks>
    /// This is used to invalidate all tokens when the specified user (represented by a system-internal AuthenticationToken) gets security panic, such as
    /// suspected account breach/password change.
    /// This method does not ban the future user-related access, just invalidates all of the token grants
    /// </remarks>
    Task InvalidateSubject(AuthenticationToken token);

    /// <summary>
    /// Instantly invalidates the specified access token
    /// </summary>
    Task InvalidateAccessToken(string accessToken);
  }

  public interface ITokenRingImplementation : ITokenRing, IDaemon, IInstrumentable
  {
  }

}
