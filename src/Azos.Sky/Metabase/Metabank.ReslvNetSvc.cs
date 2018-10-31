/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

using Azos.Conf;
using Azos.Log;
using Azos.Glue;

namespace Azos.Sky.Metabase{ public sealed partial class Metabank{


              /// <summary>
              /// Holds data that describes network service peer
              /// </summary>
              public struct NetSvcPeer
              {
                public const string BASE_MATCH = "*";

                public NetSvcPeer(string addr, string port, string group)
                {
                  Address = addr;
                  Port = port;
                  Group = group;
                }
                public readonly string Address;
                public readonly string Port;
                public readonly string Group;

                /// <summary>
                /// Overrides this value with another field by field, where '*' represents the base value, i.e.
                ///  if address is "123.12" gets overridden by address "*.2", then result will be "123.12.2". Same rule applies field-by-field for address,port and group
                /// </summary>
                /// <param name="other">Another peer to override this one with</param>
                /// <returns>New peer with field-by-field overridden data</returns>
                public NetSvcPeer Override(NetSvcPeer other)
                {
                  if (other.Blank) return this;
                  var addr = Address;
                  var port = Port;
                  var group = Group;

                  if (other.Address.IsNotNullOrWhiteSpace())
                    addr = other.Address.Replace(BASE_MATCH, addr).Trim();

                  if (other.Port.IsNotNullOrWhiteSpace())
                    port = other.Port.Replace(BASE_MATCH, port).Trim();

                  if (other.Group.IsNotNullOrWhiteSpace())
                    group = other.Group.Replace(BASE_MATCH, group).Trim();

                  return new NetSvcPeer(addr, port, group);
                }

                public bool Blank
                {
                  get
                  {
                    return Address.IsNullOrWhiteSpace()&&
                           Port.IsNullOrWhiteSpace()&&
                           Group.IsNullOrWhiteSpace();
                  }
                }
              }




    /// <summary>
    /// Resolves logical service name on the specified remote host into physical Glue.Node connection string suitable
    /// for making remote calls from this machine
    /// </summary>
    /// <param name="host">Metabase host name i.e. 'World/Us/East/CLE/A/I/wmed0001' of the destination to make a call to</param>
    /// <param name="net">Network name i.e. 'internoc'</param>
    /// <param name="svc">Service name i.e. 'zgov'</param>
    /// <param name="binding">Optional preferred binding name i.e. 'sync'</param>
    /// <param name="fromHost">Optional metabase host name for the host that calls will be made from. If null, then local host as determined at boot will be used</param>
    /// <returns>Glue Node instance with remote address visible to the calling party</returns>
    public Node ResolveNetworkService(string host, string net, string svc, string binding = null, string fromHost = null)
    {
      return new Node(ResolveNetworkServiceToConnectString(host, net, svc, binding, fromHost));
    }


    /// <summary>
    /// Resolves logical service name on the specified remote host into physical Glue.Node connection string suitable
    /// for making remote calls from this machine
    /// </summary>
    /// <param name="host">Metabase host name i.e. 'World/Us/East/CLE/A/I/wmed0001' of the destination to make a call to</param>
    /// <param name="net">Network name i.e. 'internoc'</param>
    /// <param name="svc">Service name i.e. 'zgov'</param>
    /// <param name="binding">Optional preferred binding name i.e. 'sync'</param>
    /// <param name="fromHost">Optional metabase host name for the host that calls will be made from. If null, then local host as determined at boot will be used</param>
    /// <returns>Glue Node as connection string string instance with remote address visible to the calling party</returns>
    public string ResolveNetworkServiceToConnectString(string host, string net, string svc, string binding = null, string fromHost = null)
    {
        try
        {
          return resolveNetworkServiceToConnectString(host, net, svc, binding, fromHost);
        }
        catch(Exception error)
        {
          throw new MetabaseException(StringConsts.METABASE_NETWORK_SVC_RESOLVE_ERROR.Args(host, net, svc, error.ToMessageWithType()), error);
        }

    }


    private ConcurrentDictionary<string, string> m_NetResolverCache  = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);


    private string resolveNetworkServiceToConnectString(string host, string net, string svc, string binding = null, string fromHost = null)
    {
      if (host.IsNullOrWhiteSpace() ||
          net.IsNullOrWhiteSpace() ||
          svc.IsNullOrWhiteSpace())
         throw new MetabaseException(StringConsts.ARGUMENT_ERROR + "Metabase.ResolveNetSvc(host|net|svc==null|empty)");

      if (fromHost.IsNullOrWhiteSpace()) fromHost = SkySystem.HostName;

      if (fromHost.IsNullOrWhiteSpace())
         throw new MetabaseException(StringConsts.ARGUMENT_ERROR + "Metabase.ResolveNetSvc(fromHost==null|empty & SkySystem is not avail)");

      var sb = new StringBuilder();
      sb.Append(host); sb.Append(';');
      sb.Append(net); sb.Append(';');
      sb.Append(svc); sb.Append(';');
      sb.Append(binding.IsNullOrWhiteSpace() ? SysConsts.NULL : binding); sb.Append(';');
      sb.Append(fromHost);

      var cacheKey = sb.ToString();

      string result = null;
      if (m_NetResolverCache.TryGetValue(cacheKey, out result)) return result;


      var bindingNode = GetNetworkSvcBindingConfNode(net, svc, binding);
      binding = bindingNode.Name;
      var dfltAddress = bindingNode.AttrByName(CONFIG_ADDRESS_ATTR).Value;
      var dfltPort    = bindingNode.AttrByName(CONFIG_PORT_ATTR).Value;


      var fromh = CatalogReg.NavigateHost(fromHost);
      var toh = CatalogReg.NavigateHost(host);

      var scope = GetNetworkScope(net);
      var nsvc = GetNetworkSvcConfNode(net, svc);

      if (scope==NetworkScope.NOC || scope==NetworkScope.NOCGroup)
       if (!fromh.NOC.IsLogicallyTheSame(toh.NOC))
        throw new MetabaseException(StringConsts.METABASE_NET_SVC_RESOLVER_TARGET_NOC_INACCESSIBLE_ERROR.Args(svc, fromh.RegionPath, toh.RegionPath, net, scope));



      var toPeer = new NetSvcPeer(dfltAddress, dfltPort, null);
      foreach(var segment in toh.SectionsOnPath)
      {
        var level = segment.MatchNetworkRoute(net, svc, binding, fromh.RegionPath);
        toPeer = toPeer.Override(level);
      }


      if (scope== NetworkScope.Group || scope==NetworkScope.NOCGroup)
       if (toPeer.Group.IsNotNullOrWhiteSpace())
       {
        var fromPeer = new NetSvcPeer(dfltAddress, dfltPort, null);
        foreach(var segment in fromh.SectionsOnPath)
        {
            var level = segment.MatchNetworkRoute(net, svc, binding, fromh.RegionPath);
            fromPeer = fromPeer.Override(level);
        }

        if (!INVSTRCMP.Equals( fromPeer.Group, toPeer.Group))
          throw new MetabaseException(StringConsts.METABASE_NET_SVC_RESOLVER_TARGET_GROUP_INACCESSIBLE_ERROR.Args(svc, fromh.RegionPath, toh.RegionPath, net, scope));
       }

      if (toh.Dynamic)
      {
        var dynPeer = resolveDynamicHostAddress(host, net, svc, fromh,  toh, toPeer);
        result = "{0}://{1}:{2}".Args(binding, dynPeer.Address, dynPeer.Port);
        //TODO no cache for dynamic host? or use cahce_Put...
      }
      else
      {
        result = "{0}://{1}:{2}".Args(binding, toPeer.Address, toPeer.Port);
        m_NetResolverCache.TryAdd(cacheKey, result);//put in cache ONLY for non-dynamic hosts
      }

      return result;
    }

            private NetSvcPeer resolveDynamicHostAddress(string fullHostName, string net, string svc, SectionHost fromh, SectionHost toh, NetSvcPeer toPeer)
            {
              Contracts.HostInfo hinfo = null;

              SectionZone zone = toh.ParentZone;
              while(zone!=null)
              {
                  var hzgovs = zone.ZoneGovernorHosts.Where( h => !h.Dynamic );//Where for safeguard check, as dynamic host can not be zonegov, but in case someone ignores AMM error
                  foreach (var hzgov in hzgovs)
                  {
                    try
                    {
                      using (var cl = Contracts.ServiceClientHub.New<Contracts.IZoneHostRegistryClient>(hzgov))
                      {
                        cl.TimeoutMs = this.m_ResolveDynamicHostNetSvcTimeoutMs;
                        hinfo = cl.GetSubordinateHost(fullHostName);
                        break;
                      }
                    }
                    catch(Exception error)
                    {
                      //todo Perf counter
                      log(MessageType.Error,
                                "resolveDynamicHostAddress()",
                                "Error resolving net svc on dynamic host '{0}' while contacting zgov on '{1}': {2}".Args(fullHostName, hzgov.RegionPath, error.ToMessageWithType()),
                                error);
                    }
                  }//foreach
                  zone = zone.ParentZone;  //loop only WITHIN the NOC
              }//while

              if (hinfo == null)
                throw new MetabaseException(StringConsts.METABASE_NET_SVC_RESOLVER_DYN_HOST_UNKNOWN_ERROR.Args(svc, fromh.RegionPath, toh.RegionPath, net));

              var pattern = toPeer.Address + "*";
              foreach(var nic in hinfo.NetInfo.Adapters)
              {
                foreach(var addr in nic.Addresses.Where(a => a.Unicast))
                {
                  if (Parsing.Utils.MatchPattern(addr.Name, pattern))
                   return new NetSvcPeer(addr.Name, toPeer.Port, toPeer.Group );
                }
              }

              throw new MetabaseException(StringConsts.METABASE_NET_SVC_RESOLVER_DYN_HOST_NO_ADDR_MATCH_ERROR.Args(svc, fromh.RegionPath, toh.RegionPath, net, toPeer.Address));
            }








    /// <summary>
    /// Returns conf node for named binding per network service, or if binding name is blank then returns default network binding.
    /// If net service does not have default binding specified then takes fist available binding.
    /// Throws if no match could be made
    /// </summary>
    public IConfigSectionNode GetNetworkSvcBindingConfNode(string net, string svc, string binding = null)
    {
      try
      {
        var bindingNodes = GetNetworkSvcBindingNodes(net, svc);
        if (binding.IsNullOrWhiteSpace())
        {
          //get default
          var nsNode = GetNetworkSvcConfNode(net, svc);
          binding = nsNode.AttrByName(CONFIG_DEFAULT_BINDING_ATTR).Value;
          if (binding.IsNullOrWhiteSpace())
           return bindingNodes.First();
        }
        return bindingNodes.First(n=>n.IsSameName(binding));
      }
      catch(Exception error)
      {
        throw new MetabaseException(StringConsts.METABASE_NETWORK_GET_BINDING_NODE_ERROR.Args(net, svc, binding, error.ToMessageWithType()), error);
      }
    }

}}
