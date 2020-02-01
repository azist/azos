/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Linq;
using System.Text;

using Azos.Conf;

namespace Azos.Apps.Terminal.Cmdlets
{

  public class Help : Cmdlet
  {
    public const string CONFIG_CMD_ATTR = "cmd";

    public Help(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
    {
    }

    public override string Execute()
    {
        var cmdlets = m_Terminal.Cmdlets;

        var cmdName = m_Args.AttrByName(CONFIG_CMD_ATTR).Value;

        if (cmdName.IsNotNullOrWhiteSpace())
          cmdlets = cmdlets.Where( cmd => cmd.Name.EqualsIgnoreCase(cmdName) );

        var result = new StringBuilder(1024);
        result.AppendLine(AppRemoteTerminal.MARKUP_PRAGMA);
        result.AppendLine("<push>");

        result.AppendLine("<f color=white> Remote Terminal Help<f color=gray> ");
        result.AppendLine();
        result.AppendLine(" Commands get sent to server after ';' is typed at the end of line.");
        result.AppendLine(" Use '<command> /?' for help.");
        result.AppendLine(" Use 'exit;' to disconnect.");
        result.AppendLine(" List of server cmdlets: ");
        result.AppendLine();

        foreach(var cmdlet in cmdlets.OrderBy(c=>c.Name))
        {
            var name = cmdlet.Name.ToLower();
            var help = Sky.SysConsts.UNKNOWN_ENTITY;

            using(var inst = Activator.CreateInstance(cmdlet, m_Terminal, m_Args) as Cmdlet)
            {
                try
                {
                    help = inst.GetHelp();
                }
                catch(Exception error)
                {
                    help = "Error getting help: " + error.ToMessageWithType();
                }
            }

            result.AppendLine( "<f color=white><j width=8 dir=right text=\"{0}\"><f color=gray> - {1}".Args( name, help) );
        }

        result.AppendLine("<pop>");
        return result.ToString();
    }

    public override string GetHelp()
    {
        return "Provides help on commandlets; help{cmd=name}";
    }
  }

}
