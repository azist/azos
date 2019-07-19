/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;

namespace Azos.Security.Services
{
  /// <summary>
  /// Describes a module which manages IAM/IDP services, such as token rings and underlying data stores.
  /// OAuth is a module because not all applications need it, consequently it is not a hard-coded dependency, rather
  /// a mix-in module mounted by app chassis when needed
  /// </summary>
  public interface IOAuthModule : IModule
  {
    /// <summary>
    /// Variable name for gating bad OAuth request (such as bad client ID, invalid redirect Uri or backchannel IP, etc.)
    /// </summary>
    string GateVarErrors { get; set; }

    /// <summary>
    /// Variable name for gating invalid user credentials
    /// </summary>
    string GateVarInvalidUser { get; set; }


    /// <summary>
    /// Imposes a maximum age of roundtrip state which is generated on flow start and checked at user credentials POST.
    /// Value is in seconds
    /// </summary>
    int MaxAuthorizeRoundtripAgeSec {  get; set;}

    /// <summary>
    /// Returns true if the specified scope specification is supported. The string may contain multiple
    /// scopes delimited by spaces or commas
    /// </summary>
    bool CheckScope(string scope);

    /// <summary>
    /// Returns security manager responsible for authentication and authorization of clients(applications) which
    /// request access to the system on behalf of the user. This security manager is expected to understand the
    /// `EntityUriCredentials` used for pseudo-authentication/lookup and `IDPasswordCredentials`.
    /// The returned `User` object represents a requesting client party/application along with its rights, such as:
    /// allowed redirect URIs, back-channel IPs, ability to execute "implicit" OAuth flows etc.
    /// </summary>
    ISecurityManager ClientSecurity { get; }

    /// <summary>
    /// Returns a token ring used to store tokens/temp keys issued at different stages of various
    /// flows such as OAuth token grant, refresh tokens etc.
    /// </summary>
    Tokens.ITokenRing TokenRing { get; }
  }

  /// <summary>
  /// Denotes an entity implementing IOAuthModule
  /// </summary>
  public interface IOAuthModuleImplementation : IOAuthModule, IModuleImplementation
  {
  }

}
