/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Azos.Apps;
using Azos.Conf;
using Azos.Text;

namespace Azos.Client
{
  /// <summary>
  /// Implements a remote Http(s) service client
  /// </summary>
  public class HttpService : ServiceBase<HttpEndpoint, HttpTransport>, IHttpService
  {
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
    private volatile Dictionary<EndpointAssignment.Request, EndpointAssignment[][]> m_EPCache = new Dictionary<EndpointAssignment.Request, EndpointAssignment[][]>();

    protected override void EndpointsHaveChanged()
    {
      m_EPCache = new Dictionary<EndpointAssignment.Request, EndpointAssignment[][]>();//clear cache after endpoints change
      Thread.MemoryBarrier();
    }

    protected override IEnumerable<IEnumerable<EndpointAssignment>> DoGetEndpointsForAllShards(string remoteAddress, string contract, Atom network, Atom binding)
     => DoGetEndpointsForAllShardsArray(remoteAddress, contract, network, binding);

    protected virtual EndpointAssignment[][] DoGetEndpointsForAllShardsArray(string remoteAddress, string contract, Atom network, Atom binding)
    {
      var key = new EndpointAssignment.Request(remoteAddress, contract, binding, network);
      if (!m_EPCache.TryGetValue(key, out var shards))
      {
        shards = m_Endpoints.Where(ep =>
                    remoteAddress.MatchPattern(ep.RemoteAddress) &&
                    contract.MatchPattern(ep.Contract) &&
                    ep.Binding == binding &&
                    ep.Network == network
                ).GroupBy(ep => ep.Shard)
                 .OrderBy(g => g.Key)
                 .Select(g => g.OrderBy(ep => ep.ShardOrder)
                               .Select(ep => new EndpointAssignment(ep, remoteAddress, contract)).ToArray())
                 .ToArray();

        if (shards.Length == 0) return null;

        var dict = new Dictionary<EndpointAssignment.Request, EndpointAssignment[][]>(m_EPCache);
        dict[key] = shards;
        Thread.MemoryBarrier();
        m_EPCache = dict;//atomic
      }

      return shards;
    }

    protected override IEnumerable<EndpointAssignment> DoGetEndpointsForCall(string remoteAddress, string contract, ShardKey shardKey, Atom network, Atom binding)
    {
      var shard = ((int)shardKey.GetDistributedStableHash()) & CoreConsts.ABS_HASH_MASK;

      var shards = DoGetEndpointsForAllShardsArray(remoteAddress, contract, network, binding);
      if (shards==null || shards.Length==0) return Enumerable.Empty<EndpointAssignment>();

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
