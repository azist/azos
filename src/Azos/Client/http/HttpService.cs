/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Client
{
  /// <summary>
  /// Implements a remote Http(s) service
  /// </summary>
  public class HttpService : ServiceBase<HttpEndpoint, HttpTransport>, IHttpService
  {
    private struct cacheKey : IEquatable<cacheKey>
    {
      public cacheKey(string addr, string contr, string bin, string net)
      {
        RemoteAddress = addr; Contract = contr; Binding = bin; Network = net;
      }
      public readonly string RemoteAddress;
      public readonly string Contract;
      public readonly string Binding;
      public readonly string Network;

      public override int GetHashCode() => (RemoteAddress ?? "").GetHashCodeOrdIgnoreCase() ^ (Network ?? "").GetHashCodeOrdIgnoreCase();
      public override bool Equals(object obj) => obj is cacheKey ck ? this.Equals(ck) : false;

      public bool Equals(cacheKey other)
       => this.RemoteAddress.EqualsOrdIgnoreCase(other.RemoteAddress) &&
          this.Contract.EqualsOrdIgnoreCase(other.Contract) &&
          this.Binding.EqualsOrdIgnoreCase(other.Binding) &&
          this.Network.EqualsOrdIgnoreCase(other.Network);
    }


    public HttpService(IApplicationComponent director, IConfigSectionNode conf) : base(director, conf)
    {
      Web.WebSettings.RequireInitilizedServicePointManager(App);
    }

    protected override void Destructor()
    {
      m_Transports.ForEach( kvp => this.DontLeak( () => kvp.Value.Dispose()) );
      base.Destructor();
    }

    private object m_TransportLock = new object();
    private volatile Dictionary<EndpointAssignment, HttpTransport> m_Transports = new Dictionary<EndpointAssignment, HttpTransport>();

    private volatile Dictionary<cacheKey, EndpointAssignment[][]> m_EPCache = new Dictionary<cacheKey, EndpointAssignment[][]>();

    protected override void EndpointsHaveChanged()
    {
      m_EPCache = new Dictionary<cacheKey, EndpointAssignment[][]>();//clear cache after endpoints change
    }

    protected override IEnumerable<EndpointAssignment> DoGetEndpointsForCall(string remoteAddress, string contract, object shardKey, string network, string binding)
    {
      var shard = (int)Data.ShardingUtils.ObjectToShardingID(shardKey) & CoreConsts.ABS_HASH_MASK;

      var key = new cacheKey(remoteAddress, contract, binding, network);
      if (!m_EPCache.TryGetValue(key, out var shards))
      {
        shards = m_Endpoints.Where(ep =>
              ep.RemoteAddress.EqualsIgnoreCase(remoteAddress) &&
              ep.Contract.EqualsIgnoreCase(contract) &&
              ep.Binding.EqualsIgnoreCase(binding) &&
              ep.Network.EqualsIgnoreCase(network)
            ).GroupBy(ep => ep.Shard)
             .OrderBy(g => g.Key)
             .Select(g => g.OrderBy(ep => ep.ShardOrder).Select( ep => new EndpointAssignment(ep, network, binding, remoteAddress, contract)).ToArray())
             .ToArray();

        if (shards.Length==0) return Enumerable.Empty<EndpointAssignment>();

        var dict = new Dictionary<cacheKey, EndpointAssignment[][]>();
        dict[key] = shards;
        Thread.MemoryBarrier();
        m_EPCache = dict;//atomic
      }


      var result = shards[shard % shards.Length];
      return result;
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
