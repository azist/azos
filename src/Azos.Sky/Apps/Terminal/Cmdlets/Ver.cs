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
