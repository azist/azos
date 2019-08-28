/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Security
{
  /// <summary>
  /// Represents credentials based on a bearer scheme token supplied as: `Authorization: Bearer (token)`
  /// </summary>
  [Serializable]
  public sealed class BearerCredentials : Credentials
  {
    public BearerCredentials(string token) => Token = token.NonBlank(nameof(token));

    public string Token { get; }
    public override void Forget(){ }
    public override string ToString() => "{0}({1})".Args(GetType().Name, Token);
  }
}
