using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Azos.Apps;
using Azos.Conf;

namespace Azos.Client
{
  public class HttpService : ServiceBase<HttpEndpoint, HttpTransport>
  {
    public HttpService(IApplicationComponent director, IConfigSectionNode conf) : base(director, conf)
    {
    }

    protected override void Destructor()
    {
      m_Transports.ForEach( kvp => this.DontLeak( () => kvp.Value.Dispose()) );
      base.Destructor();
    }

    private object m_TransportLock = new object();
    private volatile Dictionary<EndpointAssignment, HttpTransport> m_Transports = new Dictionary<EndpointAssignment, HttpTransport>();

    protected override IEnumerable<EndpointAssignment> DoGetEndpointsForCall(string remoteAddress, string contract, object shardKey, string network, string binding)
    {
      var shard = (int)Data.ShardingUtils.ObjectToShardingID(shardKey) & CoreConsts.ABS_HASH_MASK;

      //todo add Caching for speed
      var shards = m_Endpoints.Where( ep =>
         ep.RemoteAddress.EqualsIgnoreCase(remoteAddress) &&
         ep.Contract.EqualsIgnoreCase(contract) &&
         ep.Binding.EqualsIgnoreCase(binding) &&
         ep.Network.EqualsIgnoreCase(network)
      ).GroupBy( ep => ep.Shard ).ToArray();

      var result = shards[shards.Length % shard].OrderBy( ep => ep.ShardOrder);
      return result.Select( ep => new EndpointAssignment(ep, network, binding, remoteAddress, contract));
    }

    protected override HttpTransport DoAcquireTransport(EndpointAssignment assignment, bool reserve)
    {
      if (reserve)
      {
        return new HttpTransport(assignment);
      }

      if (m_Transports.TryGetValue(assignment, out var transport)) return transport;
      lock(m_TransportLock)
      {
        if (m_Transports.TryGetValue(assignment, out transport)) return transport;

        transport = new HttpTransport(assignment);
        var dict = new Dictionary<EndpointAssignment, HttpTransport>(m_Transports);
        dict[assignment] = transport;
        Thread.MemoryBarrier();
        m_Transports = dict;
      }

      return transport;
    }

    protected override void DoReleaseTransport(HttpTransport endpoint)
    {
      //do nothing
    }

  }
}
