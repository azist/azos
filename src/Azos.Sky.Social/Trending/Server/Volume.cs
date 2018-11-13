using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Instrumentation;
using Azos.Conf;
using MessageType = Azos.Log.MessageType;

namespace Azos.Sky.Social.Trending.Server
{
  /// <summary>
  /// Denotes the levels of time granularity - the level of detail
  /// </summary>
  public enum VolumeDetalizationLevel
  {
    /// <summary>
    /// YYYY MM
    /// </summary>
    Quarter,
    /// <summary>
    /// YYYY MM
    /// </summary>
    Monthly,
    /// <summary>
    /// YYYY MM
    /// </summary>
    Weekly,
    /// <summary>
    /// YYYY MM DD
    /// </summary>
    Daily,
    /// <summary>
    /// YYYY MM DD HH
    /// </summary>
    Hourly,
    /// <summary>
    /// YYYY MM DD HH F
    /// </summary>
    Fractional
  }

  /// <summary>
  /// Represents an entity that stores trending data per the specified detalization level.
  /// This class is NOT thread-safe
  /// </summary>
  public abstract class Volume : ServiceWithInstrumentationBase<TrendingSystemService>
  {
    #region CONSTS
      public const int MIN_HISTORY_LENGTH_DAYS = 30;
      public const int MIN_OLD_DATA_PURGE_FREQUENCY_MINUTES = 15;
      public const int FRACTIONAL_FREQUENCY_MINUTES = 5;
    #endregion

    #region Static/.ctor
      /// <summary>
      /// Rounds the specified UTC date by discarding the extra date/time components per supplied detalization level
      /// </summary>
      public static DateTime RoundDatePerDetalization(DateTime utcDate, VolumeDetalizationLevel level)
      {
        switch (level)
        {
          case VolumeDetalizationLevel.Quarter:
              var mi = utcDate.Month - 1;
               mi -= (mi % 3);
              var month = mi + 1;
              return new DateTime(utcDate.Year, month, day: 1, hour: 12, minute: 0, second: 0, kind: DateTimeKind.Utc );

          case VolumeDetalizationLevel.Monthly:
              return new DateTime(utcDate.Year, utcDate.Month, day: 1, hour: 12, minute: 0, second: 0, kind: DateTimeKind.Utc );

          case VolumeDetalizationLevel.Weekly:
              var dt =  DayOfWeek.Sunday - utcDate.DayOfWeek;
              DateTime result = utcDate.AddDays(dt);
              return new DateTime(result.Year, result.Month, result.Day, hour: 12, minute: 0, second: 0, kind: DateTimeKind.Utc );

          case VolumeDetalizationLevel.Daily:
              return new DateTime(utcDate.Year, utcDate.Month, utcDate.Day, hour: 12, minute: 0, second: 0, kind: DateTimeKind.Utc );

          case VolumeDetalizationLevel.Hourly:
              return new DateTime(utcDate.Year, utcDate.Month, utcDate.Day, utcDate.Hour, minute: 0, second: 0, kind: DateTimeKind.Utc );

          case VolumeDetalizationLevel.Fractional:
              var minute = utcDate.Minute;
              minute -= (minute % FRACTIONAL_FREQUENCY_MINUTES);
              return new DateTime(utcDate.Year, utcDate.Month, utcDate.Day, utcDate.Hour, minute: minute, second: 0, kind: DateTimeKind.Utc );

          default:
              throw new SocialException(StringConsts.TS_VOLUME_UNKNOWN_DETALIZATION_ERROR.Args(level));
        }
      }

      /// <summary>
      /// Maps the detalization level to minutes
      /// </summary>
      public static int MapDetalizationToMinutes(VolumeDetalizationLevel level)
      {
        switch(level)
        {
          case VolumeDetalizationLevel.Quarter:    return 24*60*30*3;
          case VolumeDetalizationLevel.Monthly:    return 24*60*30;
          case VolumeDetalizationLevel.Weekly:     return 24*60*7;
          case VolumeDetalizationLevel.Daily:      return 24*60;
          case VolumeDetalizationLevel.Hourly:     return 60;
          case VolumeDetalizationLevel.Fractional: return 5;

          default:
              throw new SocialException(StringConsts.TS_VOLUME_UNKNOWN_DETALIZATION_ERROR.Args(level));
        }
      }


      protected Volume(TrendingSystemService director) : base(director)
      {
      }
    #endregion

    #region Fields
      private int                      m_HistoryLengthDays = MIN_HISTORY_LENGTH_DAYS;
      private VolumeDetalizationLevel  m_DetalizationLevel = VolumeDetalizationLevel.Daily;
      private int                      m_OldDataPurgeFrequencyMin = MIN_OLD_DATA_PURGE_FREQUENCY_MINUTES;
      private DateTime                 m_LastDeleteDate = DateTime.MinValue;
    #endregion

    #region Properties
      /// <summary>
      /// Defines a point beyond which the data will be discarded
      /// </summary>
      [Config(Default = MIN_HISTORY_LENGTH_DAYS)]
      public int HistoryLengthDays
      {
        get { return m_HistoryLengthDays; }
        set
        {
          m_HistoryLengthDays = value < MIN_HISTORY_LENGTH_DAYS ? MIN_HISTORY_LENGTH_DAYS : value;
        }
      }

      [Config(Default = VolumeDetalizationLevel.Daily)]
      public VolumeDetalizationLevel DetalizationLevel
      {
        get
        {
          return m_DetalizationLevel;
        }
        set
        {
          CheckServiceInactive();
          m_DetalizationLevel = value;
        }
      }

      [Config(Default = MIN_OLD_DATA_PURGE_FREQUENCY_MINUTES)]
      [ExternalParameter]
      public int OldDataPurgeFrequencyMin
      {
        get
        {
          return m_OldDataPurgeFrequencyMin;
        }
        set
        {
          CheckServiceInactive();
          m_OldDataPurgeFrequencyMin =
            value < MIN_OLD_DATA_PURGE_FREQUENCY_MINUTES ? MIN_OLD_DATA_PURGE_FREQUENCY_MINUTES : value;
        }
      }

      public TrendingSystemHost TrendingHost {get { return this.ComponentDirector.TrendingHost; }}

    #endregion

    #region Public
      /// <summary>
      /// Saves gauge in volume
      /// This method does not leak
      /// This method is NOT thread-safe
      /// </summary>
      public void WriteGauge(SocialTrendingGauge gauge)
      {
        try
        {
          if (!Running) return;
          DoWriteGauge(gauge);
        }
        catch (Exception e)
        {
          Log(MessageType.Error, "WriteGauge", e.ToMessageWithType(), e);
        }
      }

      /// <summary>
      /// Load trending in volume
      /// This method does not leak
      /// </summary>
      public List<TrendingEntity> GetTreding(TrendingQuery query)
      {
        try
        {
          if (!Running) return new List<TrendingEntity>();
          return DoGetTreding(query);
        }
        catch (Exception e)
        {
          Log(MessageType.Error, "GetTreding", e.ToMessageWithType(), e);
        }
        return new List<TrendingEntity>();
      }

      /// <summary>
      /// Delete trending in volume
      /// </summary>
      public void DeleteOldData(DateTime utcNow)
      {
        try
        {
          if (!Running) return;
          var deletePoint = utcNow.AddDays(-m_HistoryLengthDays);
          DoDeleteOldData(deletePoint);
        }
        catch (Exception e)
        {
          Log(MessageType.Error, GetType().Name+".DeleteOldData", e.ToMessageWithType(), e);
        }
      }
    #endregion

    #region Protected

      protected override void DoAcceptManagerVisit(object manager, DateTime managerNow)
      {
        base.DoAcceptManagerVisit(manager, managerNow);
        var oldDate = IntUtils.ChangeByRndPct(OldDataPurgeFrequencyMin, 0.25f);
        if ((managerNow - m_LastDeleteDate).TotalMinutes > oldDate )
        {
          m_LastDeleteDate = managerNow;
          DeleteOldData(managerNow);
        }

      }

      protected abstract void DoDeleteOldData(DateTime deletePoint);
      protected abstract void DoWriteGauge(SocialTrendingGauge gauge);
      protected abstract List<TrendingEntity> DoGetTreding(TrendingQuery query);

      protected void Log(MessageType type, string from, string text, Exception error = null, Guid? related = null)
      {
        var msg = new Azos.Log.Message
        {
          Type = type,
          Topic = SysConsts.LOG_TOPIC_APP_MANAGEMENT,
          From = "{0}.{1}".Args(GetType().FullName, from),
          Text = text,
          Exception = error
        };

        if (related.HasValue) msg.RelatedTo = related.Value;

        App.Log.Write( msg );
      }

    #endregion
  }

}