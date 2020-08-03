/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Security;

namespace Azos.Sky.Security.Permissions.Chronicle
{
  /// <summary>
  /// Controls whether users can access chronicle functionality
  /// </summary>
  public sealed class ChroniclePermission : TypedPermission
  {
    public ChroniclePermission() : base(AccessLevel.VIEW) { }

    public ChroniclePermission(int level) : base(level) { }

    public override string Description => StringConsts.PERMISSION_DESCRIPTION_ChroniclePermission;
  }
}
