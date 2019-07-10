/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
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
    /// request access to the system on behalf of the user. This security manager is expected to understand the
    /// `EntityUriCredentials` used for pseudo-authentication/lookup and `IDPasswordCredentials`.
    /// The returned `User` object represents a requesting client party/application along with its rights, such as ability
    /// to execute "implicit" OAuth flows etc.
    /// </summary>
    ISecurityManager ClientSecurity {  get; }

    /// <summary>
    /// Returns a special kind of data store which manages tokens/temp keys issued at different stages of various
    /// flows such as OAuth token grant, refresh tokens etc.
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
    /// Generates new token initializing appropriate fields (e.g. Store token rings need to generate unique ID)
    /// </summary>
    TToken GenerateNew<TToken>() where TToken : RingToken;


    /// <summary>
    /// Returns the token content object from the token string representation.
    /// The returned token is ensured to be in non-expired state.
    /// You may want to call Validate() to run additional state checks.
    /// Null is returned if token is not found or has been tampered with.
    /// Compare to `GetAsync` which performs mandatory state validation
    /// </summary>
    /// <remarks>
    /// 'Unsafe' refers to state validation of tokens using `token.Validate()` - `Validate()` has not been called on 'unsafe' tokens.
    /// All client-based tokens are always validated for integrity and tampering by design before state validation as provided by `Validate()`
    /// </remarks>
    Task<TToken> GetUnsafeAsync<TToken>(string token) where TToken : RingToken;

    /// <summary>
    /// Returns a token content object from the string token representation.
    /// The returned token is ensured to be in non-expired state and passed `Validate()` check
    /// so callers do not need to call Validate() again.
    /// Null is returned if token is not found or has been tampered with or has validation errors.
    /// You should prefer calling this method over `GetUnsafeAsync()` unless needed (e.g. when you need to get state validation error)
    /// </summary>
    Task<TToken> GetAsync<TToken>(string token) where TToken : RingToken;

    /// <summary>
    /// Adds a token content to the ring, returning string token representation.
    /// The token instance must be in non-expired valid state.
    /// The server-side stateful implementations would save the token to the backend store,
    /// whereas stateless client-side implementations would just encode token using ICryptoMessageAlgo.Protect (or similar)
    /// </summary>
    Task<string> PutAsync(RingToken token);

    /// <summary>
    /// Applies items affected by the specified selector to the blacklist, e.g. this call may effectively invalidate all tokens
    /// for specific client id. The implementation details depend on the ring. The selector tree consists of `name=value` pairs, optionally joint with `and`, `or`, example:
    /// `or{clientId='78hshd78fhsd7fh' clientId='UIHIUH888'}`
    /// </summary>
    Task Blacklist(IConfigSectionNode selector);
  }

  public interface ITokenRingImplementation : ITokenRing, IDaemon, IInstrumentable
  {
  }

}
