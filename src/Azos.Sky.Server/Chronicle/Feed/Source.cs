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
using Azos.Collections;
using Azos.Conf;
using Azos.Data;
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
    private const int PULL_LOG_CAPACITY = 1024;

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

    private DateTime m_CheckpointUtc;
    private bool m_CheckpointChanged;
    private Dictionary<GDID, DateTime> m_PullLog = new Dictionary<GDID, DateTime>();
    private bool m_LastFetchHadData;
    private DateTime m_LastFetchUtc;
    private int m_FetchBy = FETCH_BY_DEFAULT;

    private int m_ConsecutivePullCount;


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

    /// <summary>
    /// How many pulls where called consecutively
    /// </summary>
    public int ConsecutivePullCount => m_ConsecutivePullCount;

    /// <summary>
    /// Resets to zero
    /// </summary>
    public void ResetConsecutivePullCount() => m_ConsecutivePullCount = 0;


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
             .Select (map => JsonReader.ToDoc<Message>(map))
             .Where  (msg => !m_PullLog.ContainsKey(msg.Gdid))
             .OrderBy(msg => msg.UTCTimeStamp)
             .ToArray();

      m_ConsecutivePullCount++;
      m_LastFetchHadData = result.Length > 0;
      m_LastFetchUtc = App.TimeSource.UTCNow;
      return result;
    }

    public void InitPullStateAsOfCheckpointUtc(DateTime utc)
    {
      m_CheckpointUtc = utc;
      m_CheckpointChanged = false;
      m_ConsecutivePullCount = 0;
      m_LastFetchUtc = default(DateTime);
      m_LastFetchHadData = false;
      m_PullLog.Clear();
    }

    public void SetCheckpointUtc(Message[] batch)
    {
      if (batch == null || batch.Length == 0) return;

      //Trim pull log - delete Older data over capacity
      while (m_PullLog.Count > PULL_LOG_CAPACITY)
      {
        var oldest = m_PullLog.MinBy(one => one.Value);
        m_PullLog.Remove(oldest.Key);
      }

      for(var i = 0; i < batch.Length; i++)
      {
        var msg = batch[i];
        m_PullLog[msg.Gdid] = msg.UTCTimeStamp;
        if (msg.UTCTimeStamp > m_CheckpointUtc)
        {
          m_CheckpointUtc = msg.UTCTimeStamp;
          m_CheckpointChanged = true;
        }
      }
    }//SetCheckpointUtc
  }
}
