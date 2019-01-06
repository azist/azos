/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Security;

namespace Azos.Sky.Security.Permissions.Admin
{
  /// <summary>
  /// Controls whether users can access remote terminal of application context
  /// </summary>
  public sealed class AppRemoteTerminalPermission : TypedPermission
  {
      public AppRemoteTerminalPermission() : base(AccessLevel.VIEW) { }

      public override string Description
      {
          get { return StringConsts.PERMISSION_DESCRIPTION_AppRemoteTerminalPermission; }
      }
  }
}
