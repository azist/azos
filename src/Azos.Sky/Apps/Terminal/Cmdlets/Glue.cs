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
    public class Glue : Cmdlet
    {
        public const string CONFIG_BINDING_ATTR = "binding";
        private const string VAL = "|<f color=yellow>{0}<f color=gray>\n" ;

        public Glue(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
        {

        }

        public override string Execute()
        {
            var bname = m_Args.AttrByName(CONFIG_BINDING_ATTR).Value;

            var glue = App.Glue;

            var attr = m_Args.AttrByName("client-log-level");
            if (attr.Exists) glue.ClientLogLevel = attr.ValueAsEnum<Log.MessageType>(glue.ClientLogLevel);

            attr = m_Args.AttrByName("server-log-level");
            if (attr.Exists) glue.ServerLogLevel = attr.ValueAsEnum<Log.MessageType>(glue.ServerLogLevel);

            attr = m_Args.AttrByName("server-instance-lock-timeout-ms");
            if (attr.Exists) glue.ServerInstanceLockTimeoutMs = attr.ValueAsInt(glue.ServerInstanceLockTimeoutMs);

            attr = m_Args.AttrByName("default-timeout-ms");
            if (attr.Exists) glue.DefaultTimeoutMs = attr.ValueAsInt(glue.DefaultTimeoutMs);

            attr = m_Args.AttrByName("default-dispatch-timeout-ms");
            if (attr.Exists) glue.DefaultDispatchTimeoutMs = attr.ValueAsInt(glue.DefaultDispatchTimeoutMs);






            var sb = new StringBuilder(1024);
            sb.AppendLine(AppRemoteTerminal.MARKUP_PRAGMA);
            sb.AppendLine("<push><f color=gray>");
            sb.AppendLine("Glue Status");
            sb.AppendLine("----------------------------------------------------------------------------");
            sb.AppendFormat("Glue                      "+VAL, glue.GetType().FullName );
            sb.AppendFormat("Bindings                  "+VAL, glue.Bindings.Count );
            sb.AppendFormat("Client Log Level          "+VAL, glue.ClientLogLevel );
            sb.AppendFormat("Server Log Level          "+VAL, glue.ServerLogLevel );
            sb.AppendFormat("Client Msg Inspectors     "+VAL, glue.ClientMsgInspectors.Count );
            sb.AppendFormat("Server Msg Inspectors     "+VAL, glue.ServerMsgInspectors.Count );
            sb.AppendFormat("Dflt.Timeout ms.          "+VAL, glue.DefaultTimeoutMs );
            sb.AppendFormat("Dflt.Dispatch Timeout ms. "+VAL, glue.DefaultDispatchTimeoutMs );
            sb.AppendFormat("Localized Time            "+VAL, glue.LocalizedTime );
            sb.AppendFormat("Time Location             "+VAL, glue.TimeLocation );
            sb.AppendFormat("Providers                 "+VAL, glue.Providers.Count );
            sb.AppendFormat("Servers                   "+VAL, glue.Servers.Count );
            sb.AppendFormat("Srv Inst Lock Timeout ms. "+VAL, glue.ServerInstanceLockTimeoutMs );


            sb.AppendLine();
            if (bname.IsNullOrWhiteSpace())
            {
              sb.AppendLine("Bindings");
              sb.AppendLine("Name       Type                      CI SI    CTr  STr      CDump    SDump");
              sb.AppendLine("----------------------------------------------------------------------------");
              foreach(var binding in App.Glue.Bindings)
                sb.AppendFormat("{0,-10} {1,-25} {2,1}  {3,1}  {4,4}   {5,4}  {6,8} {7,8}   \n",
                                  binding.Name,
                                  binding.GetType().Name,
                                  binding.InstrumentClientTransportStat ? ON : OFF,
                                  binding.InstrumentServerTransportStat ? ON : OFF,
                                  binding.ClientTransports.Count(),
                                  binding.ServerTransports.Count(),
                                  binding.ClientDump,
                                  binding.ServerDump
                                  );
            }
            else
            {
              var binding = glue.Bindings[bname];
              if (binding==null)
                sb.AppendFormat("Binding {0} not present in glue stack\n", bname);
              else
              {
                sb.AppendLine("Binding Status");
                sb.AppendLine("----------------------------------------------------------------------------");
                sb.AppendFormat("Name                      "+VAL, binding.Name );
                sb.AppendFormat("Type                      "+VAL, binding.GetType().FullName );
                sb.AppendFormat("Provider                  "+VAL, binding.Provider==null?"":binding.Provider.GetType().FullName );
                sb.AppendFormat("Client Dump               "+VAL, binding.ClientDump );
                sb.AppendFormat("Server Dump               "+VAL, binding.ServerDump );
                sb.AppendFormat("Cl Tr Cnt Wait Threshold  "+VAL, binding.ClientTransportCountWaitThreshold);
                sb.AppendFormat("Cl Tr Ex Acq Timeout ms.  "+VAL, binding.ClientTransportExistingAcquisitionTimeoutMs);
                sb.AppendFormat("Cl Tr Idle Timeout ms.    "+VAL, binding.ClientTransportIdleTimeoutMs);
                sb.AppendFormat("Cl Tr Max Count           "+VAL, binding.ClientTransportMaxCount);
                sb.AppendFormat("Cl Tr Max Ex Acq Tmout ms."+VAL, binding.ClientTransportMaxExistingAcquisitionTimeoutMs);
                sb.AppendFormat("Client Transport Count    "+VAL, binding.ClientTransports.Count());
                sb.AppendFormat("Server Transport Count    "+VAL, binding.ServerTransports.Count());
                sb.AppendFormat("Server Tr Idle Timeout ms."+VAL, binding.ServerTransportIdleTimeoutMs);
                sb.AppendFormat("Localized Time            "+VAL, binding.LocalizedTime );
                sb.AppendFormat("Time Location             "+VAL, binding.TimeLocation );

                sb.AppendLine();
                sb.AppendLine("Client Transports");
                sb.AppendLine("Node                           IdlSec      RBy      SBy   Err    RMsg   SMsg");
                sb.AppendLine("----------------------------------------------------------------------------");
                foreach(var tran in binding.ClientTransports)
                  sb.AppendFormat("{0,-30} {1,6} {2,8} {3,8} {4,5}  {5,5}  {6,5}\n",
                                    tran.Node,
                                    tran.IdleAgeMs / 1000,
                                    tran.StatBytesReceived,
                                    tran.StatBytesSent,
                                    tran.StatErrors,
                                    tran.StatMsgReceived,
                                    tran.StatMsgSent
                                    );
                sb.AppendLine();
                sb.AppendLine("Server Transports");
                sb.AppendLine("Node                           IdlSec      RBy      SBy   Err    RMsg   SMsg");
                sb.AppendLine("----------------------------------------------------------------------------");
                foreach(var tran in binding.ServerTransports)
                  sb.AppendFormat("{0,-30} {1,6} {2,8} {3,8} {4,5}  {5,5}  {6,5}\n",
                                    tran.Node,
                                    tran.IdleAgeMs / 1000,
                                    tran.StatBytesReceived,
                                    tran.StatBytesSent,
                                    tran.StatErrors,
                                    tran.StatMsgReceived,
                                    tran.StatMsgSent
                                    );


              }
            }

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
@"Dumps Glue status:
           Pass <f color=yellow>binding<f color=gray>=string to query the particular instance
           Parameters:
            <f color=yellow>binding=string<f color=gray> - name of the binding of interest
            <f color=yellow>client-log-level=MessageType<f color=gray> - client side log level
            <f color=yellow>server-log-level=MessageType<f color=gray> - server side log level
            <f color=yellow>server-instance-lock-timeout-ms=int<f color=gray> - acquisition interval for
                        non-thread-safe servers
            <f color=yellow>default-timeout-ms=int<f color=gray> - default call timeout
            <f color=yellow>default-dispatch-timeout-ms=int<f color=gray> - default call dispatch timeout

";
        }


        private IEnumerable<Binding> bindings(string bname)
        {
          return App.Glue.Bindings.Where(b=>bname.IsNullOrWhiteSpace() || b.Name.EqualsIgnoreCase(bname));
        }

    }

}
