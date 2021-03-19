/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Data;

namespace Azos.Security
{
  /// <summary>
  /// Base data for specifying extra information for authentication requests
  /// (not to be confuse with authorization), such as providing extra data regarding
  /// the nature of the authentication operation. The ISecurityManager and its subordinate
  /// services may elect to interpret the supplied context object in various ways,
  /// for example: generate sys tokens with longer lifespan for OAuth requestors
  /// </summary>
  public abstract class AuthenticationRequestContext : TypedDoc/*must be easily serializable hence DataDoc*/
  {
    /// <summary>
    /// Provides a short description for the Authentication request, e.g. "API OAuth"
    /// </summary>
    [Field]
    public string RequestDescription { get; set; }
  }


  /// <summary>
  /// Denotes requests related to OAuth subject (target impersonated user) authentication
  /// </summary>
  public class OAuthSubjectAuthenticationRequestContext : AuthenticationRequestContext
  {
    [Field]
    public long? SysAuthTokenValiditySpanSec { get; set; }
  }
}
