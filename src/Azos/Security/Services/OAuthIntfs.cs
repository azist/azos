using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Instrumentation;

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
    /// Maps external auth token into system-internal AuthenticationToken as understood by the SecurityManager.
    /// This method is purposely synchronous as it handles high volume of API calls and is expected to be
    /// implemented efficiently with in-Memory caching technique.
    /// </summary>
    /// <param name="externalToken">External auth token as the one issued in OAuth flow</param>
    /// <returns>Internal AuthenticationToken or null if not found</returns>
    AuthenticationToken? MapExternalToken(string externalToken);

    /// <summary>
    /// Retrieves an existing client access code token or null if it is not found or expired
    /// </summary>
    Task<ClientAccessCodeToken> LookupClientAccessCodeAsync(string accessCode);

   // Task<ClientToken> CreateAccessToken(User userClient, User appUser);
  }

  public interface ITokenRingImplementation : ITokenRing, IDaemon, IInstrumentable
  {
  }

}
