/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Text;

using Azos.Conf;

namespace Azos.Apps.Terminal.Cmdlets
{
  public class Mbc : SkyAppCmdlet
  {
    public Mbc(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
    {
    }

    public override string Execute()
    {

      var result = new StringBuilder();
      App.Metabase.DumpCacheStatus(result);

      return result.ToString();
    }

    public override string GetHelp()
    {
        return "Dumps status of metabase cache";
    }
  }
}
