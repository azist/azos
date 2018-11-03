
using Azos.Security;

namespace Azos.Sky.Security.Permissions.Admin
{
  /// <summary>
  /// Controls whether users can access remote terminals
  /// </summary>
  public sealed class RemoteTerminalOperatorPermission : TypedPermission
  {
      public RemoteTerminalOperatorPermission() : base(AccessLevel.VIEW) { }

      public override string Description
      {
          get { return StringConsts.PERMISSION_DESCRIPTION_RemoteTerminalOperatorPermission; }
      }
  }
}
