/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;
using Azos.Log;
using Azos.Apps.Terminal;

namespace Azos.Apps.HostGovernor.Cmdlets
{
  public class Install : SkyAppCmdlet
  {
    public const string CONFIG_FORCE_ATTR = "force";

    public Install(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
    {

    }

    public override string Execute()
    {
        var list = new List<string>();
        var force =  m_Args.AttrByName(CONFIG_FORCE_ATTR).ValueAsBool(false);

        if (force)
        App.Log.Write( new Message
          {
              Type = MessageType.Warning,
              Topic = Sky.SysConsts.LOG_TOPIC_APP_MANAGEMENT,
              From = "{0}.Force".Args(GetType().FullName),
              Text = "Installation with force=true initiated"
          });

        var anew = App.Singletons
                      .Get<HostGovernorService>().NonNull(nameof(HostGovernorService))
                      .CheckAndPerformLocalSoftwareInstallation(list, force);

        var progress = list.Aggregate(new StringBuilder(), (sb, s) => sb.AppendLine(s)).ToString();

        App.Log.Write( new Message
          {
              Type = MessageType.Warning,
              Topic = Sky.SysConsts.LOG_TOPIC_APP_MANAGEMENT,
              From = "{0}.Force".Args(GetType().FullName),
              Text = "Installation finished. Installed anew: " + anew,
              Parameters = progress
          });

        return progress;
    }

    public override string GetHelp()
    {
        return
@"Initiates check and installation of local software.
        Parameters:
            <f color=yellow>force=bool<f color=gray> - force reinstall
";
    }
  }
}
