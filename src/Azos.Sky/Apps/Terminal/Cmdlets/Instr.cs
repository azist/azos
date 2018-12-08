/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;
using Azos.Glue;

namespace Azos.Sky.Apps.Terminal.Cmdlets
{

    public enum InstrType { View, Glue, ClientGlue, ServerGlue, Metabase, Mb = Metabase }

    public class Instr : Cmdlet
    {

        public const string CONFIG_TYPE_ATTR = "type";
        public const string CONFIG_ON_ATTR = "on";
        public const string CONFIG_BINDING_ATTR = "binding";

        public Instr(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
        {
        }

        public override string Execute()
        {
            var type = m_Args.AttrByName(CONFIG_TYPE_ATTR).ValueAsEnum<InstrType>(InstrType.View);
            var on = m_Args.AttrByName(CONFIG_ON_ATTR).ValueAsBool();
            var bname = m_Args.AttrByName(CONFIG_BINDING_ATTR).Value;
            switch(type)
            {
                case InstrType.ClientGlue:
                {
                    foreach(var binding in bindings(bname))
                     binding.InstrumentClientTransportStat = on;

                    break;
                }

                case InstrType.ServerGlue:
                {
                    foreach(var binding in bindings(bname))
                     binding.InstrumentServerTransportStat = on;

                    break;
                }

                case InstrType.Glue:
                {
                    foreach(var binding in bindings(bname))
                    {
                     binding.InstrumentClientTransportStat = on;
                     binding.InstrumentServerTransportStat = on;
                    }
                    break;
                }

                case InstrType.Metabase:
                {
                    if (SkySystem.IsMetabase)
                    {
                      var mb = SkySystem.Metabase;
                      mb.InstrumentationEnabled = on;
                    }
                    break;
                }

                default:
                {
                    break;//don't change anything
                }
            }

            var sb = new StringBuilder(1024);
            sb.AppendLine(AppRemoteTerminal.MARKUP_PRAGMA);
            sb.AppendLine("<push><f color=gray>");
            sb.AppendLine("Instrumentation Status");
            sb.AppendLine("----------------------");
            sb.AppendFormat("Enabled: <f color=yellow>{0}<f color=gray>  Overflown: <f color=yellow>{1}<f color=gray>  RecordCount: <f color=yellow>{2}<f color=gray>  MaxRecordCount: <f color=yellow>{3}<f color=gray>\n",
                               App.Instrumentation.Enabled,
                               App.Instrumentation.Overflown,
                               App.Instrumentation.RecordCount,
                               App.Instrumentation.MaxRecordCount);
            sb.AppendFormat("Interval: <f color=yellow>{0}<f color=gray>ms. OS Interval: <f color=yellow>{1}<f color=gray>ms.\n",
                                 App.Instrumentation.ProcessingIntervalMS,
                                 App.Instrumentation.OSInstrumentationIntervalMS);
            sb.AppendLine();
            sb.AppendLine("Glue Bindings Instrumentation");
            sb.AppendLine("-----------------------------");
            foreach(var binding in App.Glue.Bindings)
              sb.AppendFormat("{0,-10} {1,-25}  Client: {2,6}  Server: {3,6}\n",
                                binding.Name,
                                binding.GetType().Name,
                                binding.InstrumentClientTransportStat ? ON : OFF,
                                binding.InstrumentServerTransportStat ? ON : OFF);
            sb.AppendLine();
            var mon = SkySystem.IsMetabase && SkySystem.Metabase.InstrumentationEnabled;

            sb.AppendLine();
            sb.AppendLine("Metabase Instrumentation");
            sb.AppendLine("-----------------------------");
            sb.AppendFormat("Enabled: {0}\n", mon ? ON : OFF);

            sb.AppendLine();
            sb.AppendLine("<f color=white>NOTE:<f color=darkgray> For management use CMAN instead as it allows to set more parameters");

            sb.AppendLine("<pop>");

            return sb.ToString();
        }


        private const string ON = "<f color=yellow>X<f color=gray>";
        private const string OFF = "<f color=darkGRay>-<f color=gray>";

        public override string GetHelp()
        {
            return
@"Enables/disables instrumentation:
           Pass <f color=yellow>type<f color=gray>={<f color=darkRed>View<f color=gray>|<f color=darkRed>Glue<f color=gray>|<f color=darkRed>ClientGlue<f color=gray>|<f color=darkRed>ServerGlue<f color=gray>|
                <f color=darkRed>Metabase<f color=gray>} to specify what to control.
           Parameters:
            <f color=yellow>on=bool<f color=gray> - turn on/off
            <f color=yellow>binding=string<f color=gray> - optional name of binding to apply changes to
";
        }


        private IEnumerable<Binding> bindings(string bname)
        {
          return App.Glue.Bindings.Where(b=>bname.IsNullOrWhiteSpace() || b.Name.EqualsIgnoreCase(bname));
        }

    }

}
