/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Text;

using Azos.Conf;

namespace Azos.Sky.Apps.Terminal.Cmdlets
{
  public class Who : Cmdlet
  {
    public Who(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args) { }

    public override string Execute()
    {
      var result = new StringBuilder(0xff);

      var registry = AppRemoteTerminalRegistry.Instance(App).All;

      foreach (var t in registry)
        result.AppendFormat("{0}-{1}-{2}-{3}\n", t.Name, t.Who, t.WhenConnected, t.WhenInteracted);

      return result.ToString();
    }

    public override string GetHelp() { return "Displays remote terminal sessions"; }
  }
}
