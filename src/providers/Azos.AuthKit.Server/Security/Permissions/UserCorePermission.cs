/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.AuthKit.Server;

using System.Security;

namespace Azos.Security.Authkit
{
  /// <summary>
  /// Defines access levels for core AuthKit user accounts
  /// </summary>
  public enum UserCoreAccessLevel
  {
    Denied = AccessLevel.DENIED,
    View = AccessLevel.VIEW,
    Change = AccessLevel.VIEW_CHANGE,
    Delete = AccessLevel.VIEW_CHANGE_DELETE
  }

  /// <summary>
  /// Grants the assignee an ability to access core AuthKit user account functionality, possibly making changes depending on access level
  /// </summary>
  public sealed class UserCorePermission : TypedPermission
  {
    public UserCorePermission() : base(AccessLevel.VIEW) { }
    public UserCorePermission(UserCoreAccessLevel level) : base((int)level) { }

    public override string Description
      => $"Grants the asignee an ability to access user account functionality " +
         $"{(Level > AccessLevel.VIEW ? "and making changes" : "in read only mode")}";
  }
}
