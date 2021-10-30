/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Security;
using System;

namespace Azos.Security.Services
{
  /// <summary>
  /// Coordinates possibly complex multi-step login flows of login, such as
  /// SSO-based long-term logins possibly with 2FA, client id/app scope (re)confirmation
  /// user identity refresh via PIN (short code) etc.
  /// You derive your own class from this one, as the reference gets passed between OAuth
  /// controller methods relevant to log-in (aka user sign-on)
  /// </summary>
  public class LoginFlow
  {
    /// <summary>
    /// Client/relying party/application id
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Response type (e.g. `code`) requested by the client
    /// </summary>
    public string ClientResponseType { get; set; }

    /// <summary>
    /// User representing client application/relying party
    /// </summary>
    public User ClientUser { get; set; }

    /// <summary>
    /// Requested OAuth scope
    /// </summary>
    public string ClientScope { get; set; }

    /// <summary>
    /// Requested Redirect URI
    /// </summary>
    public string ClientRedirectUri {  get; set; }

    /// <summary>
    /// Client-requested state to be passed back with redirect URI
    /// </summary>
    public string ClientState { get; set; }

    /// <summary>
    /// Set from request if present. (e.g. extracted from session cookie) or NULL if no SSO context is present
    /// </summary>
    public string SsoSessionId {  get; set; }

    public bool HasSsoSessionId => SsoSessionId.IsNotNullOrWhiteSpace();

    /// <summary>
    /// When set, indicates that there was a long-term session token used for SSO (single-sign-on)
    /// </summary>
    public User SsoSubjectUser { get; set; }

    /// <summary>
    /// When set, tells the system when the SSO user logged-in for the last time
    /// </summary>
    public DateTime? SsoSubjectLastLoginUtc {  get; set;}

    /// <summary>
    /// True if there is a valid authenticated SsoSubjectUser
    /// </summary>
    public bool IsValidSsoUser => SsoSubjectUser != null && SsoSubjectUser.IsAuthenticated;


    /// <summary>
    /// If true, the state is finite success - no more extra/next steps are needed
    /// </summary>
    public bool FiniteStateSuccess{  get; set; }

  }
}
