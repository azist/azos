/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Client;
using Azos.Conf;
using Azos.Data;
using Azos.Data.Idgen;
using Azos.Instrumentation;
using Azos.Log;
using Azos.Serialization.JSON;
using Azos.Sky.Identification;
using Azos.Sky.Security.Permissions.EventHub;
using Azos.Web;

namespace Azos.Sky.EventHub
{
  /// <summary>
  /// Provides client implementation for IEventProducerLogic and IEventConsumerLogic
  /// </summary>
  public sealed class EventHubClientLogic : ModuleBase, IEventProducerLogic, IEventConsumerLogic
  {
    public const string CONFIG_SERVICE_SECTION = "service";
    public const int FETCH_BY_MAX = 255;
    public const int FETCH_BY_DEFAULT = 8;

    public EventHubClientLogic(IApplication application) : base(application) { }
    public EventHubClientLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Server);
      base.Destructor();
    }


    [Inject] IGdidProviderModule m_Gdid;

    private int m_FecthBy = FETCH_BY_DEFAULT;
    private HttpService m_Server;


    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.QUEUE_TOPIC;


    /// <summary>
    /// Logical service address for this event queue Origin
    /// </summary>
    [Config]
    public string OriginServiceAddress { get; set; }

    /// <summary>
    /// Logical cluster region/zone id
    /// </summary>
    [Config]
    public Atom Origin { get; set; }


    /// <summary>
    /// Shard count
    /// </summary>
    public int PartitionCount
    {
      get
      {
        return m_Server.GetEndpointsForAllShards(OriginServiceAddress, nameof(IEventConsumer)).Count();
      }
    }

    /// <summary>
    /// Specifies how many events per queue are fetched if the caller did not specify the count
    /// </summary>
    [Config(Default = FETCH_BY_DEFAULT), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_QUEUE)]
    public int FetchBy
    {
      get => m_FecthBy;
      set => m_FecthBy = value.KeepBetween(0, FETCH_BY_MAX);
    }

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
      Origin.IsTrue(v => !v.IsZero && v.IsValid, "Origin");
      OriginServiceAddress.NonBlank(nameof(OriginServiceAddress));

      return base.DoApplicationAfterInit();
    }

    public Event MakeNew(Atom contentType, byte[] content, string headers = null)
    {
      contentType.IsTrue(v => v.IsZero || v.IsValid);
      content.NonNull(nameof(content)).IsTrue( v => content.Length < Event.MAX_CONTENT_LENGTH, "content.len < MAX_LEN");
      if (headers != null) headers.IsTrue(v => v.Length < Event.MAX_HEADERS_LENGTH, "header.len < MAX_LEN");

      var gdid = m_Gdid.Provider.GenerateOneGdid(SysConsts.GDID_NS_EVENTHUB, SysConsts.GDID_SEQ_EVENTHUB);

      var result = new Event
      {
        Gdid = gdid,
        Origin = this.Origin,
        CheckpointUtc = 0, //not written to disk yet as of right now
        CreateUtc = App.TimeSource.UTCNow.ToUnsignedMillisecondsSinceUnixEpochStart(),
        Headers = headers,
        ContentType = contentType,
        Content = content
      };

      return result;
    }

    private static int mapDataLossMode(int totalCount, DataLossMode lossMode, string opName)
    {
      int need(int v)
        => v <= totalCount ? v
                           : throw new EventHubException("Operation `{0}` requires {1} nodes in cohort, but cluster only has {2} available".Args(opName, v, totalCount));

      switch (lossMode)
      {
        case DataLossMode.High: return need(1);
        case DataLossMode.Mid: return need(2);
        case DataLossMode.Low: return need(3);
        case DataLossMode.Minimum: return need(Math.Max(3, totalCount));
        case DataLossMode.MinimumPossible: return need(Math.Max(1, totalCount));
        default: return need(3);
      }
    }

    /// <summary>
    /// Posts event across all servers per shard
    /// </summary>
    public async Task<WriteResult> PostAsync(Route route, ShardKey partition, Event evt, DataLossMode lossMode = DataLossMode.Default)
    {
      route.IsTrue(v => v.Assigned, "assigned Route");

      //Validate Event object before post
      var ve = evt.NonNull(nameof(evt)).Validate();
      if (ve != null) throw ve;

      EventProducerPermission.Instance.Check(App.SecurityManager);

      //Capture the checkpoint time as of writing
      evt.CheckpointUtc = App.TimeSource.UTCNow.ToUnsignedMillisecondsSinceUnixEpochStart();

      var all = m_Server.GetEndpointsForCall(OriginServiceAddress,
                                             nameof(IEventProducer),
                                             partition)
                        .Where(ep => ep.Endpoint.IsAvailable);

      var allCount = all.Count();

      var takeLimit = mapDataLossMode(allCount, lossMode, nameof(PostAsync));

      var calls = all.Take(takeLimit).Select(
        one => one.CallOne(
          (http, ct) => http.Client
                            .PostAndGetJsonMapAsync("event",
                                                     new
                                                     {
                                                       ns = route.Namespace,
                                                       queue = route.Queue,
                                                       evt
                                                     }
                                                   )
        )
      );

      var responses = await Task.WhenAll(calls.Select(async call => {
        try
        {
          var got = await call.ConfigureAwait(false);
          return got.UnwrapChangeResult();
        }
        catch (Exception error)
        {
          WriteLog(MessageType.Warning, nameof(PostAsync), "Post error: " + error.ToMessageWithType(), error);
          return default(ChangeResult);
        }
      })).ConfigureAwait(false);


      return new WriteResult(responses, allCount);
    }

    #warning see #515 - needs to be rewritten more optimaly
    public async Task<IEnumerable<Event>> FetchAsync(Route route, int partition,  ulong checkpoint, int skip, int count, DataLossMode lossMode = DataLossMode.Default)
    {
      route.IsTrue(v => v.Assigned, "assigned Route");
      if (count <= 0) count = FetchBy;
      count = count.KeepBetween(1, FETCH_BY_MAX);

      EventConsumerPermission.Instance.Check(App.SecurityManager);

      var all = m_Server.GetEndpointsForCall(OriginServiceAddress,
                                             nameof(IEventConsumer),
                                             new ShardKey((ulong)partition))
                        .Where(ep => ep.Endpoint.IsAvailable);

      var allCount = all.Count();
      var takeLimit = mapDataLossMode(allCount, lossMode, nameof(FetchAsync));
      //var first = all.First();

      //Read from all in cohort
      var calls = all.Take(takeLimit).Select(
        one => one.CallOne(
          (http, ct) => http.Client
                            .PostAndGetJsonMapAsync("feed",
                                                     new
                                                     {
                                                       ns = route.Namespace,
                                                       queue = route.Queue,
                                                       checkpoint = checkpoint,
                                                       skip,
                                                       count = count,
                                                       //onlyid = one == first
                                                     }
                                                   )
        )
      );

      var responses = await Task.WhenAll(calls.Select(async call => {
        try
        {
          return await call.ConfigureAwait(false);
        }
        catch (Exception error)
        {
          WriteLog(MessageType.Warning, nameof(FetchAsync), "Fetch error: " + error.ToMessageWithType(), error);
          return null;
        }
      })).ConfigureAwait(false);


      var result = responses.SelectMany(response => response.UnwrapPayloadArray()
                                                            .OfType<JsonDataMap>()
                                                            .Select(map => JsonReader.ToDoc<Event>(map)))
                            .DistinctBy(e => e.Gdid)
                            .OrderBy(e => e.CheckpointUtc)
                            .ToArray();

      return result;
    }

    public async Task<ulong> GetCheckpointAsync(Route route, int partition, string idConsumer, DataLossMode lossMode = DataLossMode.Default)
    {
      route.IsTrue(v => v.Assigned, "assigned Route");
      idConsumer.NonBlank(nameof(idConsumer));

      EventConsumerPermission.Instance.Check(App.SecurityManager);

      var all = m_Server.GetEndpointsForCall(OriginServiceAddress,
                                            nameof(IEventConsumer),
                                            new ShardKey((ulong)partition))
                        .Where(ep => ep.Endpoint.IsAvailable);

      var allCount = all.Count();
      var takeLimit = mapDataLossMode(allCount, lossMode, nameof(FetchAsync));

      var uri = new UriQueryBuilder("checkpoint")
               .Add("ns", route.Namespace)
               .Add("queue", route.Queue)
               .Add("consumer", idConsumer).ToString();

      //Read from all in cohort
      var calls = all.Take(takeLimit).Select(
        one => one.CallOne(
          (http, ct) => http.Client
                            .CallAndGetJsonMapAsync(uri, HttpMethod.Get, null)
        )
      );

      var responses = await Task.WhenAll(calls.Select(async call => {
        try
        {
          var map = await call.ConfigureAwait(false);
          return map.UnwrapPayloadObject().AsULong(handling: ConvertErrorHandling.Throw);
        }
        catch (Exception error)
        {
          WriteLog(MessageType.Warning, nameof(GetCheckpointAsync), "GetCheckpointAsync error: " + error.ToMessageWithType(), error);
          return 0ul;
        }
      })).ConfigureAwait(false);


      var result = responses.Max();//take the "latest"="maximum"
      return result;
    }

    public async Task<WriteResult> SetCheckpointAsync(Route route, int partition, string idConsumer, ulong checkpoint, DataLossMode lossMode = DataLossMode.Default)
    {
      route.IsTrue(v => v.Assigned, "assigned Route");

      idConsumer.NonBlank(nameof(idConsumer));

      EventProducerPermission.Instance.Check(App.SecurityManager);

      var all = m_Server.GetEndpointsForCall(OriginServiceAddress,
                                            nameof(IEventProducer),
                                            new ShardKey((ulong)partition))
                       .Where(ep => ep.Endpoint.IsAvailable);

      var allCount = all.Count();

      var takeLimit = mapDataLossMode(allCount, lossMode, nameof(PostAsync));

      var calls = all.Take(takeLimit).Select(
        one => one.CallOne(
          (http, ct) => http.Client
                            .PostAndGetJsonMapAsync("checkpoint",
                                                     new
                                                     {
                                                       ns = route.Namespace,
                                                       queue = route.Queue,
                                                       consumer = idConsumer,
                                                       checkpoint = checkpoint
                                                     }
                                                   )
        )
      );

      var responses = await Task.WhenAll(calls.Select(async call => {
        try
        {
          var got = await call.ConfigureAwait(false);
          got.ExpectOK();
          return new ChangeResult(ChangeResult.ChangeType.Updated, 1, null, null);
        }
        catch (Exception error)
        {
          WriteLog(MessageType.Warning, nameof(SetCheckpointAsync), "SetCheckpointAsync error: " + error.ToMessageWithType(), error);
          return new ChangeResult();//undefined
        }
      })).ConfigureAwait(false);


      return new WriteResult(responses, allCount);

    }
  }
}
