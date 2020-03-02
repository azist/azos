/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Security;

namespace Azos.Apps.Terminal.Cmdlets
{
  [SystemAdministratorPermission(AccessLevel.ADVANCED)]
  public class Conf : Cmdlet
  {
    public const string CONFIG_PATH_ATTR = "path";

    public Conf(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
    {
    }

    public override string Execute()
    {
      var path = m_Args.AttrByName(CONFIG_PATH_ATTR).Value;

      var cfg = App.ConfigRoot;

      if (path.IsNotNullOrWhiteSpace())
        cfg = cfg.NavigateSection(path);

      var result = cfg.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint);

      return result;
    }

    public override string GetHelp()
    {
      return
@"Fetches current process configuration tree.
      Parameters:
      <f color=yellow>path=string<f color=gray> - optional section path within config tree
 <f color=magenta>Example:
   <f color=green> conf{ path='/log/' }

   <f color=cyan> NOTE: 
    Requires  <f color=red>SystemAdministratorPermission(AccessLevel.ADVANCED)<f color=cyan> grant 
";
    }
  }
}
