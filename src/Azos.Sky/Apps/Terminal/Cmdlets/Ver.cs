/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Text;
using System.Reflection;

using Azos.Conf;
using Azos.Sky;

namespace Azos.Apps.Terminal.Cmdlets
{
  public sealed class Ver : Cmdlet
  {
    public Ver(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
    {
    }

    public override string Execute()
    {
      var result = new StringBuilder(0xff);
      result.AppendLine("Server Version/Build information:");
      result.AppendLine(" App:       [{0}] {1}".Args(App.AppId.IsZero ? "#" : App.AppId.Value,  App.Name));
      result.AppendLine(" Azos Core: " + BuildInformation.ForFramework);
      result.AppendLine(" Azos Sky:  " + new BuildInformation( typeof(Azos.Sky.SkySystem).Assembly ));

      string host  = "n/a";
      try
      {
        host = new BuildInformation(Assembly.GetEntryAssembly()).ToString();
      }
      catch{ }

      result.AppendLine(" Host:      " + host);

      return result.ToString();
    }

    public override string GetHelp()
    {
      return "Returns version/build information";
    }
  }
}
