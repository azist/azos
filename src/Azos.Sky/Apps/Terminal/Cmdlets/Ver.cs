/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Text;
using System.Reflection;

using Azos.Conf;

namespace Azos.Sky.Apps.Terminal.Cmdlets
{

    public class Ver : Cmdlet
    {
        public Ver(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
        {

        }

        public override string Execute()
        {
            var result = new StringBuilder(0xff);
            result.AppendLine("Server Version/Build information:");
            result.AppendLine(" App:       " + App.Name);
            result.AppendLine(" Azos Core: " + BuildInformation.ForFramework);
            result.AppendLine(" Azos Sky:  " + new BuildInformation( typeof(Azos.Sky.SkySystem).Assembly ));
            result.AppendLine(" Host:      " + new BuildInformation( Assembly.GetEntryAssembly() ));

            return result.ToString();
        }

        public override string GetHelp()
        {
            return "Returns version/build information";
        }
    }

}
