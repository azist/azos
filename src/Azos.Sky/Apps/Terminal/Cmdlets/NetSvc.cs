using System;
using System.Diagnostics;

using Azos.Conf;

namespace Azos.Sky.Apps.Terminal.Cmdlets
{


    public class NetSvc : Cmdlet
    {
        public const string CONFIG_HOST_ATTR = "host";
        public const string CONFIG_NET_ATTR = "net";
        public const string CONFIG_SVC_ATTR = "svc";
        public const string CONFIG_BINDING_ATTR = "binding";
        public const string CONFIG_FROM_ATTR = "from";

        public NetSvc(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
        {

        }

        public override string Execute()
        {
            var watch = Stopwatch.StartNew();

            var host = m_Args.AttrByName(CONFIG_HOST_ATTR).ValueAsString();
            var net  = m_Args.AttrByName(CONFIG_NET_ATTR).ValueAsString();
            var svc = m_Args.AttrByName(CONFIG_SVC_ATTR).ValueAsString();
            var binding = m_Args.AttrByName(CONFIG_BINDING_ATTR).ValueAsString(null);
            var from = m_Args.AttrByName(CONFIG_FROM_ATTR).ValueAsString(null);

            string node;
            try
            {
               node = SkySystem.Metabase.ResolveNetworkServiceToConnectString(host, net, svc, binding, from);
            }
            catch(Exception error)
            {
               return "ERROR: "+error.ToMessageWithType();
            }

            return
@"Resolved
Host: {0}
Net:  {1}
Service: {2}
Binding: {3}
From:   {4}

into Glue node
 {5}
Elapsed: {6} ms.".Args(host, net, svc, binding, from, node, watch.ElapsedMilliseconds );
        }

        public override string GetHelp()
        {
            return
@"Resolves network service call into Glue node.
           Parameters:
            <f color=yellow>host=path<f color=gray> - metabase path of target host
            <f color=yellow>net=name<f color=gray> - network name
            <f color=yellow>svc=name<f color=gray> - service name
            <f color=yellow>binding=name<f color=gray> - optional, Glue binding
            <f color=yellow>from=path<f color=gray> - optional, metabase path to call-issuing host
";
        }


    }

}
