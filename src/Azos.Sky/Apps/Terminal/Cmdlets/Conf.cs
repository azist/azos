
using Azos.Conf;

namespace Azos.Sky.Apps.Terminal.Cmdlets
{

    public class Conf : Cmdlet
    {
        public Conf(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
        {

        }

        public override string Execute()
        {
            var conf = new LaconicConfiguration();
            conf.CreateFromNode( App.ConfigRoot );
            return conf.SaveToString();
        }

        public override string GetHelp()
        {
            return "Fetches current configuration tree";
        }
    }

}
