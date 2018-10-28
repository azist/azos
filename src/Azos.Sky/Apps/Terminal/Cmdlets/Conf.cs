/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
