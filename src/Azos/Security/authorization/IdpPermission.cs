/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Security
{
  /// <summary>
  /// Identity Provider (IDP) access permission
  /// </summary>
  public sealed class IdpPermission : TypedPermission
  {
    public IdpPermission() : base(AccessLevel.VIEW) { }

    public IdpPermission(int level) : base(level) { }

    public override string Description => nameof(IdpPermission);
  }
}
