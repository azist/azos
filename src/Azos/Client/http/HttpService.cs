using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Client
{
  public class HttpService : ServiceBase<HttpEndpoint, HttpTransport>
  {
    public HttpService(IApplicationComponent director, IConfigSectionNode conf) : base(director, conf)
    {
    }

    protected override IEnumerable<EndpointAssignment> DoGetEndpointsForCall(string remoteAddress, string contract, object shardKey, string network, string binding)
    {
      var shard = (int)Data.ShardingUtils.ObjectToShardingID(shardKey) & CoreConsts.ABS_HASH_MASK;

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
      throw new NotImplementedException();
    }

    protected override void DoReleaseTransport(HttpTransport endpoint)
    {
      throw new NotImplementedException();
    }

  }
}
