
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
