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

namespace Azos.Security.Tokens
{
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
    /// The returned token is guaranteed to be in non-expired state.
    /// You may want to call Validate() to run additional state checks.
    /// Null is returned if token is not found or has been tampered with.
    /// Compare to `GetAsync` which performs mandatory state validation
    /// </summary>
    /// <remarks>
    /// 'Unsafe' refers to state validation of tokens using `token.Validate()` - `Validate()` has not been called on 'unsafe' tokens.
    /// All client-based tokens are always validated for integrity and tampering by design before doing the state validation via `Validate()`
    /// </remarks>
    Task<TToken> GetUnsafeAsync<TToken>(string token) where TToken : RingToken;

    /// <summary>
    /// Returns a token content object from the string token representation.
    /// The returned token is guaranteed to be in non-expired state having already passed the `Validate()` check
    /// so callers do not need to call `Validate()` again.
    /// Null is returned if token is not found or has been tampered with or has validation errors.
    /// You should prefer calling this method over `GetUnsafeAsync()` unless needed (e.g. when you need to get state validation error)
    /// </summary>
    Task<TToken> GetAsync<TToken>(string token) where TToken : RingToken;

    /// <summary>
    /// Adds a token content to the ring, returning string token representation.
    /// The token instance must be in non-expired valid state.
    /// The server-side stateful implementations would save the token to the backend store,
    /// whereas stateless client-side implementations would just encode token using ICryptoMessageAlgo.Protect (or similar cryptographic mechanism)
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
