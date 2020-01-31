/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Text;

using Azos.Apps;
using Azos.Conf;
using Azos.Glue;

namespace Azos.Apps.Terminal.Cmdlets
{
  /// <summary>
  /// Execute commands
  /// </summary>
  public class Exec: Cmdlet
  {
    public const string CONFIG_TO_ATTR = "to";

    public Exec(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args) { }

    public override string Execute()
    {
      var to = m_Args.AttrByName(CONFIG_TO_ATTR).ValueAsString();
      var sb = new StringBuilder();

      foreach (var arg in m_Args.Children)
        sb.Append(m_Terminal.Execute(arg));

        if (to.IsNullOrWhiteSpace())
          return sb.ToString();

      m_Terminal.Vars[to] = sb.ToString();
      return "OK";
    }

    public override string GetHelp()
    {
      return @"Executes commands";
    }
  }
}