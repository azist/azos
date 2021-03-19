/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using Azos.Data;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Security
{
  /// <summary>
  /// Base data for specifying extra information for authentication requests
  /// (not to be confuse with authorization), such as providing extra data regarding
  /// the nature of the authentication operation. The ISecurityManager and its subordinate
  /// services may elect to interpret the supplied context object in various ways,
  /// for example: generate sys tokens with longer lifespan for OAuth requestors
  /// </summary>
  [BixJsonHandler]
  public abstract class AuthenticationRequestContext : TypedDoc/*must be easily serializable hence DataDoc*/
  {
    /// <summary>
    /// Provides a short description for the Authentication request, e.g. "API OAuth"
    /// </summary>
    [Field]
    public string RequestDescription { get; set; }

    protected override void AddJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, Dictionary<string, object> jsonMap, string name, object value)
    {
      if (def?.Order == 0)
      {
        BixJsonHandler.EmitJsonBixDiscriminator(this, jsonMap);
      }

      base.AddJsonSerializerField(def, options, jsonMap, name, value);
    }
  }


  /// <summary>
  /// Denotes requests related to OAuth subject (target impersonated user) authentication
  /// </summary>
  [Bix("5ADDB87A-FB26-4843-97C9-8A549594DB36")]
  public class OAuthSubjectAuthenticationRequestContext : AuthenticationRequestContext
  {
    [Field]
    public long? SysAuthTokenValiditySpanSec { get; set; }
  }
}
