using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Time;

using Azos.Sky.WebMessaging;

namespace Azos.Sky.Social.Trending.Server
{
  /// <summary>
  /// Implements ITrendingSystem contract
  /// </summary>
  public sealed class TrendingSystemService : DaemonWithInstrumentation<object>, ITrendingSystem
  {
    #region CONSTS
      public const string CONFIG_VOLUME_SECTION = "volume";
      public const string CONFIG_TRENDING_HOST_SECTION = "trending-host";
    #endregion

    #region STATIC/.ctor
      private static object s_Lock = new object();
      private static volatile TrendingSystemService s_Instance;

      internal static TrendingSystemService Instance
      {
        get
        {
          var instance = s_Instance;
          if (instance == null)
            throw new WebMessagingException(StringConsts.TS_INSTANCE_NOT_ALLOCATED_ERROR.Args(typeof(TrendingSystemService).Name));
          return instance;
        }
      }

      /// <summary>
      /// Selects the most appropriate detalization level based on the supplied number of samples
      /// </summary>
      public static VolumeDetalizationLevel CalcDetalizationLevelForRange(DateTime startDate, DateTime endDate, int sampleCount)
      {
        var d1 = endDate < startDate ? endDate : startDate;
        var d2 = endDate >= startDate ? endDate : startDate;
        var span = d2 - d1;

        if (span.TotalMinutes < 5 * sampleCount) return VolumeDetalizationLevel.Fractional;
        if (span.TotalHours < sampleCount)  return VolumeDetalizationLevel.Hourly;
        if (span.TotalDays < sampleCount) return VolumeDetalizationLevel.Daily;
        if (span.TotalDays < 7 * sampleCount) return VolumeDetalizationLevel.Weekly;
        if (span.TotalDays < 30 * sampleCount) return VolumeDetalizationLevel.Monthly;

        return VolumeDetalizationLevel.Quarter;
      }

      public TrendingSystemService(object director) : base(director)
      {
        lock (s_Lock)
        {
          if (s_Instance != null)
            throw new WebMessagingException(StringConsts.TS_INSTANCE_ALREADY_ALLOCATED_ERROR.Args(GetType().Name));

          m_Volumes = new Registry<Volume>();

          s_Instance = this;
        }
      }

      protected override void Destructor()
      {
        lock (s_Lock)
        {
          base.Destructor();
          deleteVolumes();
          s_Instance = null;
        }
      }

    #endregion

    #region Fields

      private Registry<Volume> m_Volumes;
      private Event m_ManagerEvent;
      private TrendingSystemHost m_TrendingHost;

    #endregion;

    #region Properties
      public override bool InstrumentationEnabled { get; set; }

      public TrendingSystemHost TrendingHost { get { return m_TrendingHost; } }

    #endregion

    #region Public

      public void Send(SocialTrendingGauge[] gauges)
      {
        if (gauges == null) return;
        foreach (var gauge in gauges)
        {
          if (!TrendingHost.HasEntity(gauge.Entity))
          {
            // TODO Log error "unsupported entity" with throttling (1 error per 1 minute)
            continue;
          }

          foreach (var volume in m_Volumes)
          {
            volume.WriteGauge(gauge);
          }
        }
      }

      public IEnumerable<TrendingEntity> GetTrending(TrendingQuery query)
      {
        var volume = findVolume(query.StartDate, query.EndDate, query.SampleCount);
        return volume != null ? volume.GetTreding(query) : Enumerable.Empty<TrendingEntity>();
      }

    #endregion

    #region Protected

      protected override void DoConfigure(IConfigSectionNode node)
      {
        base.DoConfigure(node);
        deleteVolumes();
        if (node == null || !node.Exists) return;

        var nHost = node[CONFIG_TRENDING_HOST_SECTION];
        m_TrendingHost = FactoryUtils.Make<TrendingSystemHost>(nHost, args: new object[]{ this, nHost });

        foreach (var cnode in node.Children.Where(cn => cn.IsSameName(CONFIG_VOLUME_SECTION)))
        {
          var volume = FactoryUtils.MakeAndConfigure<Volume>(cnode, args: new object[] { this });
          if (!m_Volumes.Register(volume))
            throw new WebMessagingException(StringConsts.TS_SERVICE_DUPLICATE_VOLUMES_ERROR.Args(GetType().Name, volume.Name));
        }
      }

      protected override void DoStart()
      {
        if (m_Volumes.Count==0)
          throw new WebMessagingException(StringConsts.TS_SERVICE_NO_VOLUMES_ERROR.Args(GetType().Name));

        if (m_TrendingHost == null)
          throw new WebMessagingException(StringConsts.TS_SERVICE_NO_TRENDING_HOST_ERROR.Args(GetType().Name));

        m_Volumes.ForEach(v => v.Start());

        m_ManagerEvent = new Event(App.EventTimer,
          interval: TimeSpan.FromMilliseconds(3759),
          body: _ => this.AcceptManagerVisit(this, _.LocalizedTime),
          bodyAsyncModel: EventBodyAsyncModel.AsyncTask
          );
        base.DoStart();
      }

      protected override void DoSignalStop()
      {
        base.DoSignalStop();
        DisposeAndNull(ref m_ManagerEvent);
        m_Volumes.ForEach(v => v.SignalStop());
      }

      protected override void DoWaitForCompleteStop()
      {
        base.DoWaitForCompleteStop();

        m_Volumes.ForEach(v => v.WaitForCompleteStop());
      }

      protected override void DoAcceptManagerVisit(object manager, DateTime managerNow)
      {
        base.DoAcceptManagerVisit(manager, managerNow);
        managerNow = App.TimeSource.UTCNow;
        m_Volumes.ForEach(v => v.AcceptManagerVisit(this, managerNow));
      }

    #endregion

    #region pvt

      private void deleteVolumes()
      {
        m_Volumes.ForEach(v => v.Dispose());
        m_Volumes.Clear();
      }

      private Volume findVolume(DateTime startDate, DateTime endDate, int sampleCount)
      {
        var level = CalcDetalizationLevelForRange(startDate, endDate, sampleCount);
        return m_Volumes.FirstOrDefault(v => v.DetalizationLevel == level);
      }

    #endregion
  }
}