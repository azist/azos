/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Client;
using Azos.Collections;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Log;
using Azos.Serialization.JSON;
using Azos.Web;

namespace Azos.Sky.Chronicle.Feed
{
  /// <summary>
  /// Represents a pull source: such as shard address and channel
  /// </summary>
  public sealed class Source : ApplicationComponent<PullAgentDaemon>, INamed
  {
    public const string CONFIG_SOURCE_SECTION = "source";
    public const int MAX_NAME_LEN = 48;

    public const int FETCH_BY_MIN = 8;
    public const int FETCH_BY_MAX = 500;
    public const int FETCH_BY_DEFAULT = 32;

    public Source(PullAgentDaemon director, IConfigSectionNode cfg) : base(director)
    {
      ConfigAttribute.Apply(this, cfg);
      m_Name.NonBlankMax(MAX_NAME_LEN, nameof(Name));
      m_UplinkAddress.NonBlank(nameof(UplinkAddress));
      m_SinkName.NonBlankMax(MAX_NAME_LEN, nameof(Name));
      m_Channel.HasRequiredValue(nameof(Name));
    }

    protected override void Destructor()
    {
      base.Destructor();
    }

    [Config] private string m_Name;
    [Config] private string m_UplinkAddress;
    [Config] private Atom m_Channel;
    [Config] private string m_SinkName;
    [Config] private int? m_SpecificShard;
    private bool m_CheckpointChanged;

    private DateTime m_LastFetchUtc;
    private bool m_LastFetchHadData;

    private int m_FetchBy = FETCH_BY_DEFAULT;

    private DateTime m_CheckpointUtc;

    internal void SetCheckpointUtc(DateTime utc)
    {
      m_CheckpointUtc = utc;
      m_CheckpointChanged = true;
    }


    public override string ComponentLogTopic => CoreConsts.LOG_TOPIC;


    [Config(Default = FETCH_BY_DEFAULT), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public int FetchBy
    {
      get => m_FetchBy;
      set => m_FetchBy.KeepBetween(FETCH_BY_MIN, FETCH_BY_MAX);
    }

    public string   Name => m_Name;

    /// <summary>
    /// The remote address of the source node where the source is pulling from
    /// </summary>
    public string UplinkAddress => m_UplinkAddress;

    /// <summary>
    /// Name of sink where this source is written into
    /// </summary>
    public string   SinkName => m_SinkName;

    public Atom     Channel => m_Channel;
    public DateTime CheckpointUtc => m_CheckpointUtc;

    /// <summary>
    /// True when source has fetched data since last checkpoint and needs to be written to log
    /// </summary>
    public bool CheckpointChanged => m_CheckpointChanged;

    /// <summary>
    /// Resets dirty has fetched flag
    /// </summary>
    public void ResetCheckpointChanged() => m_CheckpointChanged = false;

    /// <summary>
    /// When fetched for the last time
    /// </summary>
    public DateTime LastFetchUtc => m_LastFetchUtc;

    /// <summary>
    /// True if data was fetched with the last call to uplink
    /// </summary>
    public bool LastFetchHadData => m_LastFetchHadData;

    /// <summary>
    /// If set tells the multiplexing server source to return data from the specified shard
    /// </summary>
    public int? SpecificShard => m_SpecificShard;


    public async Task<Message[]> PullAsync(HttpService uplink)
    {
      var filter = new LogChronicleFilter
      {
        Channel = this.Channel,
        PagingCount = FetchBy,
        CrossShard = false,
        DemandAllShards = false,
        SpecificShard = this.SpecificShard,
        TimeRange = new Time.DateRange(m_CheckpointUtc, null)
      };
      var response = await uplink.Call(UplinkAddress,
                                       nameof(ILogChronicle),
                                       new ShardKey(0u),
                                       (http, ct) => http.Client.PostAndGetJsonMapAsync("filter", new { filter = filter })).ConfigureAwait(false);

      var result = response.UnwrapPayloadArray()
             .OfType<JsonDataMap>()
             .Select(imap => JsonReader.ToDoc<Message>(imap))
             .OrderBy(msg => msg.UTCTimeStamp)
             .ToArray();

      m_LastFetchHadData = result.Length > 0;
      m_LastFetchUtc = App.TimeSource.UTCNow;
      return result;
    }
  }
}
