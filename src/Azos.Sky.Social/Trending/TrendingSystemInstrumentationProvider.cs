using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Collections;
using Azos.Instrumentation;

using Azos.Sky.Contracts;
using Azos.Sky.Coordination;


namespace Azos.Sky.Social.Trending
{
  /// <summary>
  /// Uploads trending data to the to the trending system server.
  /// This provider only processes SocialTrendingGauge instances and ignores others.
  /// Use this provider with CompositeProvider when configuring application setting instrumentation
  /// </summary>
  public sealed class TrendingSystemInstrumentationProvider : InstrumentationProvider
  {
    #region CONSTS
      public const int HIGH_PASS_FILTER_COUNTER_MASK = 0x0ffff;
      public const int HIGH_PASS_FILTER_TIME_WINDOW_MINUTES_MIN = 1;
      public const int HIGH_PASS_FILTER_TIME_WINDOW_MINUTES_DEFAULT = 60;
      public const int HIGH_PASS_FILTER_BYPASS_THRESHOLD_COUNT_DEFAULT = 10;
    #endregion

            //we store COUNTER_MASK (65K) GDID latches per entity, this way we can roughly filter-out traffic which happens not-frequently
            // suppose there are 1,000,000,000 trending users / 0xffff = 15,258 collisions per 1 filter cell
            private class entityFilter : Dictionary<ulong, DateTime>, INamed
            {
              public entityFilter(string name) : base(HIGH_PASS_FILTER_COUNTER_MASK) { m_Name = name;}
              private string m_Name;
              public string Name { get { return m_Name;} }
            }


    public TrendingSystemInstrumentationProvider(InstrumentationService director) : base(director)
    {
    }

    #region Fields
      private string m_HostsetName;
      private HostSet m_Hostset;

      private Registry<entityFilter> m_HighPassFilter = new Registry<entityFilter>();
      private bool m_HighPassFilterEnabled;
      private int m_HighPassFilterTimeWindowMinutes = HIGH_PASS_FILTER_TIME_WINDOW_MINUTES_DEFAULT;
      private int m_HighPassFilterBypassThresholdCount = HIGH_PASS_FILTER_BYPASS_THRESHOLD_COUNT_DEFAULT;
    #endregion

    #region Properties
      /// <summary>
      /// Provides the name of hostset where the data will be sent
      /// </summary>
      [Config(path:"$hostset|$host-set|$hostset-name")]
      public string HostsetName
      {
        get { return m_HostsetName; }
        set
        {
          CheckDaemonInactive();
          m_HostsetName = value;
        }
      }

      public HostSet HostSet
      {
        get
        {
          if(m_Hostset == null) m_Hostset = SkySystem.ProcessManager.HostSets[m_HostsetName];
          return m_Hostset;
        }
      }

      /// <summary>
      /// Enables high-pass-filter that filters-out datums that do not happen frequently enough
      /// </summary>
      [ExternalParameter]
      [Config]
      public bool HighPassFilterEnabled
      {
        get { return m_HighPassFilterEnabled;  }
        set { m_HighPassFilterEnabled = value; }
      }

      /// <summary>
      /// Specifies the time window size in minutes within which some activity has to take place, otherwise the slot get discarded
      /// </summary>
      [ExternalParameter]
      [Config(Default = HIGH_PASS_FILTER_TIME_WINDOW_MINUTES_DEFAULT)]
      public int HighPassFilterTimeWindowMinutes
      {
        get { return m_HighPassFilterTimeWindowMinutes; }
        set
        {
          m_HighPassFilterTimeWindowMinutes = value < HIGH_PASS_FILTER_TIME_WINDOW_MINUTES_MIN ? m_HighPassFilterTimeWindowMinutes : value;
        }
      }

      /// <summary>
      /// Specifies the threshold value above which the data bypasses filter
      /// </summary>
      [ExternalParameter]
      [Config(Default = HIGH_PASS_FILTER_BYPASS_THRESHOLD_COUNT_DEFAULT)]
      public int HighPassFilterBypassThresholdCount
      {
        get { return m_HighPassFilterBypassThresholdCount; }
        set
        {
          m_HighPassFilterBypassThresholdCount = value > 0 ? value : 0;
        }
      }
    #endregion

    #region Protected
    protected override void DoStart()
    {
      base.DoStart();
      m_Hostset = SkySystem.ProcessManager.HostSets[m_HostsetName];
      m_HighPassFilter.Clear();
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();
      m_HighPassFilter.Clear();
    }


    protected override object BeforeBatch()
    {
      return new List<SocialTrendingGauge>();
    }

    protected override void AfterBatch(object batchContext)
    {
      var datumList = batchContext as List<SocialTrendingGauge>;
      if (datumList != null)
        send(datumList.ToArray());
    }

    protected override void Write(Datum aggregatedDatum, object batchContext, object typeContext)
    {
      var astg = aggregatedDatum as SocialTrendingGauge;
      if (astg==null) return;

      if (!highPassFilter(astg)) return;//todo Instrument - the datum is not frequent enough and gets filtered-out

      var datumList = batchContext as List<SocialTrendingGauge>;
      if (datumList != null)
      {
        datumList.Add(astg);
        if (datumList.Count>100)
        {
          send(datumList.ToArray());
          datumList.Clear();
        }
      }
      else
        send(astg);
    }
    #endregion

    #region pvt
    private bool highPassFilter(SocialTrendingGauge sample)
    {
      if (!m_HighPassFilterEnabled) return true;
      var entityName = sample.Entity;
      if (entityName.IsNullOrWhiteSpace()) entityName = "*";

      var key = sample.G_Entity.Counter & HIGH_PASS_FILTER_COUNTER_MASK;
      DateTime lastDate;
      var now = App.TimeSource.UTCNow;

      //If the sample happened more than threshold then bypass filtering by time altogether
      var pass = sample.Value >= m_HighPassFilterBypassThresholdCount;

      var filter = m_HighPassFilter.GetOrRegister(entityName, n => new entityFilter(n), entityName);

      //only include those GDIDS that had some traffic in the past m_HighPassFilterTimeWindowMinutes
      if (!pass && filter.TryGetValue(key, out lastDate))
        pass = (now-lastDate).TotalMinutes <= m_HighPassFilterTimeWindowMinutes;

      filter[key] = now;

      return pass;
    }

    private void send(params SocialTrendingGauge[] data)
    {
      if (data==null || data.Length==0) return;

      var hostPair = HostSet.AssignHost(App.TimeSource.UTCNow.Ticks);
      ServiceClientHub.CallWithRetry<ITrendingSystemClient>
      (
        cl => cl.Send( data ),
        hostPair.Select(host => host.RegionPath)
      );
    }
    #endregion
  }
}
