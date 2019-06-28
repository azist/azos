/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Security
{
  /// <summary>
  /// Represents credentials which point to an entity by its Uri
  /// </summary>
  [Serializable]
  public class EntityUriCredentials : Credentials
  {
    public EntityUriCredentials(string uri) => Uri = uri.NonBlank(nameof(uri));

    /// <summary>
    /// Entity URi which can be used with secman.LookupEntity(uri) method
    /// </summary>
    public string Uri { get;}
    public override void Forget(){ }
    public override string ToString() => "{0}(`{1}`)".Args(GetType().Name, Uri);
  }
}
