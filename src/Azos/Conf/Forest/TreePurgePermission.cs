/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Security.Config
{
  /// <summary>
  /// Grants the ability to purge all tree data. Requires ADVANCED(1000) grant level
  /// </summary>
  public sealed class TreePurgePermission : TypedPermission
  {
    public static readonly TreePurgePermission Instance = new TreePurgePermission();

    public TreePurgePermission() : base(AccessLevel.ADVANCED){ }

    public override string Description
      => $"Grants the ability to purge all tree data. Requires ADVANCED(1000) grant level";
  }
}
