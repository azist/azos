/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;

namespace Azos.Security
{
  /// <summary>
  /// Checks that user is authenticated with Administrator status (user.Status=Admin) and has the specified access level.
  /// The validation fails for plain User statuses regardless of their ACL level.
  /// </summary>
  public sealed class SystemAdministratorPermission : TypedPermission
  {
    public SystemAdministratorPermission(int level) : base(level) { }

    protected override bool DoCheckAccessLevel(IApplication app, ISession session, AccessLevel access)
      => session.User.Status >= UserStatus.Administrator &&
         base.DoCheckAccessLevel(app, session, access);
  }

}
