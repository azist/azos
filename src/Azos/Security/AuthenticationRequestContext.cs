/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Security
{
  /// <summary>
  /// Provides optional extra information for authentication requests
  /// (not to be confused with authorization), such as providing extra data regarding
  /// the nature of the authentication operation. The ISecurityManager and its subordinate
  /// services may elect to interpret the supplied context object in various ways,
  /// for example: generate sys tokens with longer lifespan for OAuth request-ors
  /// </summary>
  public sealed class AuthenticationRequestContext : TypedDoc/*must be easily serializable hence DataDoc*/
  {
    public const string INTENT_OAUTH = "oauth";

    /// <summary>
    /// Provides an intent code for the call, e.g. OAuth
    /// </summary>
    [Field]
    public string Intent { get; set; }

    /// <summary>
    /// Provides a short description for the Authentication request, e.g. "API OAuth"
    /// </summary>
    [Field]
    public string Description { get; set; }

    /// <summary>
    /// When set, asks the provider to make a sysAuth token with the specified life span.
    /// The provider may deny the request based on its policies and implementation and
    /// provide a token with its default life span
    /// </summary>
    [Field]
    public long? SysAuthTokenValiditySpanSec { get; set; }

    /// <summary>
    /// Provides optional(may be null) context parameter bag.
    /// Warning: when using Data property, make sure that dictionary values are all json-serializable because
    /// this instance may need to be marshaled across process boundaries
    /// </summary>
    [Field]
    public JsonDataMap Data { get; set; }
  }

}
