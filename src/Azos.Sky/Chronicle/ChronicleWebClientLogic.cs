/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Client;
using Azos.Conf;
using Azos.Log;
using Azos.Serialization.JSON;
using Azos.Web;


namespace Azos.Sky.Chronicle
{
  /// <summary>
  /// Provides client for consuming ILogChronicle and  IInstrumentationChronicle remote services.
  /// Multiplexes reading from multiple shards when `CrossShard` filter param is passed
  /// </summary>
  public sealed class ChronicleWebClientLogic : ModuleBase, ILogChronicleLogic, IInstrumentationChronicleLogic
  {
    public const string CONFIG_SERVICE_SECTION = "service";

    public ChronicleWebClientLogic(IApplication application) : base(application) { }
    public ChronicleWebClientLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Server);
      base.Destructor();
    }

    private HttpService m_Server;


    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.INSTRUMENTATION_TOPIC;


    /// <summary>
    /// Logical service address of logger
    /// </summary>
    [Config]
    public string LogServiceAddress{  get; set; }

    /// <summary>
    /// Logical service address of instrumentation
    /// </summary>
    [Config]
    public string InstrumentationServiceAddress { get; set; }


    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      DisposeAndNull(ref m_Server);
      if (node == null) return;

      var nServer = node[CONFIG_SERVICE_SECTION];
      m_Server = FactoryUtils.MakeDirectedComponent<HttpService>(this,
                                                                 nServer,
                                                                 typeof(HttpService),
                                                                 new object[] { nServer });
    }

    protected override bool DoApplicationAfterInit()
    {
      m_Server.NonNull("Not configured Server of config section `{0}`".Args(CONFIG_SERVICE_SECTION));
      LogServiceAddress.NonBlank(nameof(LogServiceAddress));
      InstrumentationServiceAddress.NonBlank(nameof(InstrumentationServiceAddress));

      return base.DoApplicationAfterInit();
    }

    private int m_CallClock;
    private ShardKey nextShard() =>  new ShardKey((uint)System.Threading.Interlocked.Increment(ref m_CallClock));

    public async Task WriteAsync(LogBatch data)
    {
      var response = await m_Server.Call(LogServiceAddress,
                                          nameof(ILogChronicle),
                                          nextShard(),
                                          (http, ct) => http.Client.PostAndGetJsonMapAsync("batch", new {batch = data})).ConfigureAwait(false);
      response.UnwrapChangeResult();
    }


    public async Task<IEnumerable<Message>> GetAsync(LogChronicleFilter filter)
     => await (filter.NonNull(nameof(filter)).CrossShard ? getCrossShard(filter)
                                                         : getOneShard(filter)).ConfigureAwait(false);

    //20230515 DKh #861
    public async Task<IEnumerable<Fact>> GetFactsAsync(LogChronicleFactFilter filter)
         => await (filter.NonNull(nameof(filter)).LogFilter.NonNull(nameof(filter.LogFilter)).CrossShard ? getFactsCrossShard(filter)
                                                             : getFactsOneShard(filter)).ConfigureAwait(false);

    private async Task<IEnumerable<Message>> getCrossShard(LogChronicleFilter filter)
    {
      filter.CrossShard = false; //stop recursion, each shard should return just its own data
      var shards = m_Server.GetEndpointsForAllShards(LogServiceAddress, nameof(ILogChronicle));

      async Task<(int shard, JsonDataMap data)> callBody(IHttpTransport http)
      {
        var data = await http.Client.PostAndGetJsonMapAsync("filter", new { filter = filter }).ConfigureAwait(false);
        return (http.Assignment.Endpoint.Shard, data);
      }

      var calls = shards.Select(shard => shard.Call((http, ct) => callBody(http)));

      var responses = await Task.WhenAll(calls.Select( async call => {
        try
        {
          return await call.ConfigureAwait(false);
        }
        catch(Exception error)
        {
          WriteLog(MessageType.Error, nameof(getCrossShard), "Shard fetch error: " + error.ToMessageWithType(), error);

          if (filter.DemandAllShards) throw;

          return (0, null);
        }
      })).ConfigureAwait(false);

      var result = responses.Where(one => one.data != null)
                            .SelectMany(response => response.data.UnwrapPayloadArray()
                                                                 .OfType<JsonDataMap>()
                                                                 .Select(imap =>
                                                                 {
                                                                    var msg =  JsonReader.ToDoc<Message>(imap);
                                                                    msg.SrcDataShard = response.shard;
                                                                    return msg;
                                                                 }))
                            .OrderBy(m => m.UTCTimeStamp)
                            .ToArray();

      return result;
    }

    private async Task<IEnumerable<Message>> getOneShard(LogChronicleFilter filter)
    {
      JsonDataMap response;

      int? srcDataShard = null;

      if (!filter.SpecificShard.HasValue)
      {
        response = await m_Server.Call(LogServiceAddress,
                                       nameof(ILogChronicle),
                                       new ShardKey(0u),
                                       (http, ct) => http.Client.PostAndGetJsonMapAsync("filter", new {filter = filter})).ConfigureAwait(false);
      }
      else
      {
        var sidx = filter.SpecificShard.Value;
        var shards = m_Server.GetEndpointsForAllShards(LogServiceAddress, nameof(ILogChronicle)).ToArray();
        if (sidx < 0 || sidx >= shards.Length ) return Enumerable.Empty<Message>();
        var shard = shards[sidx];
        srcDataShard = shard.FirstOrDefault().Endpoint?.Shard;
        response = await shard.Call((http, ct) => http.Client.PostAndGetJsonMapAsync("filter", new { filter = filter })).ConfigureAwait(false);
      }

      var result = response.UnwrapPayloadArray()
              .OfType<JsonDataMap>()
              .Select(imap =>
              {
                var result = JsonReader.ToDoc<Message>(imap);
                result.SrcDataShard = srcDataShard;
                return result;
              });

      return result;
    }

    private async Task<IEnumerable<Fact>> getFactsCrossShard(LogChronicleFactFilter filter)
    {
      filter.LogFilter.CrossShard = false; //stop recursion, each shard should return just its own data
      var shards = m_Server.GetEndpointsForAllShards(LogServiceAddress, nameof(ILogChronicle));

      var calls = shards.Select(shard => shard.Call((http, ct) => http.Client.PostAndGetJsonMapAsync("filter-facts", new { filter = filter })));

      var responses = await Task.WhenAll(calls.Select(async call => {
        try
        {
          return await call.ConfigureAwait(false);
        }
        catch (Exception error)
        {
          WriteLog(MessageType.Error, nameof(getCrossShard), "Shard fetch error: " + error.ToMessageWithType(), error);

          if (filter.LogFilter.DemandAllShards) throw;

          return null;
        }
      })).ConfigureAwait(false);

      var result = responses.SelectMany(response => response.UnwrapPayloadArray()
                                                            .OfType<JsonDataMap>()
                                                            .Select(imap => JsonReader.ToDoc<Fact>(imap)))
                            .OrderBy(m => m.UtcTimestamp)
                            .ToArray();

      return result;
    }

    private async Task<IEnumerable<Fact>> getFactsOneShard(LogChronicleFactFilter filter)
    {
      JsonDataMap response;

      if (!filter.LogFilter.SpecificShard.HasValue)
      {
        response = await m_Server.Call(LogServiceAddress,
                                       nameof(ILogChronicle),
                                       new ShardKey(0u),
                                       (http, ct) => http.Client.PostAndGetJsonMapAsync("filter-facts", new { filter = filter })).ConfigureAwait(false);
      }
      else
      {
        var sidx = filter.LogFilter.SpecificShard.Value;
        var shards = m_Server.GetEndpointsForAllShards(LogServiceAddress, nameof(ILogChronicle)).ToArray();
        if (sidx < 0 || sidx >= shards.Length) return Enumerable.Empty<Fact>();
        response = await shards[sidx].Call((http, ct) => http.Client.PostAndGetJsonMapAsync("filter-facts", new { filter = filter })).ConfigureAwait(false);
      }

      var result = response.UnwrapPayloadArray()
              .OfType<JsonDataMap>()
              .Select(imap => JsonReader.ToDoc<Fact>(imap));

      return result;
    }

    public async Task WriteAsync(InstrumentationBatch data)
    {
      var response = await m_Server.Call(InstrumentationServiceAddress,
                                         nameof(IInstrumentationChronicle),
                                         nextShard(),
                                         (http, ct) => http.Client.PostAndGetJsonMapAsync("batch", new {batch = data})).ConfigureAwait(false);
      response.UnwrapChangeResult();
    }

    public async Task<IEnumerable<JsonDataMap>> GetAsync(InstrumentationChronicleFilter filter)
    {
      var response = await m_Server.Call(InstrumentationServiceAddress,
                                           nameof(IInstrumentationChronicle),
                                           new ShardKey(0u),
                                           (http, ct) => http.Client.PostAndGetJsonMapAsync("filter", new {filter = filter})).ConfigureAwait(false);

      var result = response.UnwrapPayloadArray()
                           .OfType<JsonDataMap>();

      return result;
    }

  }
}
