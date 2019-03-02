/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps;
using Azos.Conf;
using Azos.Collections;
using Azos.Time;
using Azos.Instrumentation;

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Common type for entities that own sinks: LogDaemon and CompositeSink is a good example
  /// </summary>
  public interface ISinkOwner : IApplicationComponent
  {
    LogDaemonBase LogDaemon {  get; }
    IOrderedRegistry<Sink> Sinks {  get; }
  }

  /// <summary>
  /// Provides internal plumbing for sink registration - regular code should never call this
  /// </summary>
  internal interface ISinkOwnerRegistration : ISinkOwner
  {
    void Register(Sink sink);
    void Unregister(Sink sink);
  }

  /// <summary>
  /// Delegate for message filtering
  /// </summary>
  public delegate bool MessageFilterHandler(Sink sink, Message msg);

  /// <summary>
  /// Represents logging message destination - an abstract entity that messages are written to by LogService.
  /// Destinations must be efficient as they block logger thread. They provide failover mechanism when
  ///  processing can not be completed. Once failed, the processing can try to be resumed after configurable interval.
  /// Destinations also provide optional SLA on the time it takes to perform actual message write - once exceeded destination is considered to have failed.
  /// Basic efficient filtering is provided for times, dates and levels. Complex C# expression-based filtering is also supported
  /// </summary>
  public abstract class Sink : DaemonWithInstrumentation<ISinkOwner>, IConfigurable, INamed, IOrdered
  {
    #region Local Classes

        public class LevelsList : List<Tuple<MessageType,MessageType>> {}

    #endregion

    #region CONSTS
        public const string CONFIG_FAILOVER_ATTR = "failover";
        public const string CONFIG_GENERATE_FAILOVER_MSG_ATTR = "generate-failover-msg";
        public const string CONFIG_ONLY_FAILURES_ATTR = "only-failures";
        public const string CONFIG_MIN_LEVEL_ATTR = "min-level";
        public const string CONFIG_MAX_LEVEL_ATTR = "max-level";
        public const string CONFIG_LEVELS_ATTR = "levels";
        public const string CONFIG_DAYS_OF_WEEK_ATTR = "days-of-week";
        public const string CONFIG_START_DATE_ATTR = "start-date";
        public const string CONFIG_END_DATE_ATTR = "end-date";
        public const string CONFIG_START_TIME_ATTR = "start-time";
        public const string CONFIG_END_TIME_ATTR = "end-time";
        public const string CONFIG_FILTER_ATTR = "filter";
        public const string CONFIG_TEST_ON_START_ATTR = "test-on-start";

        public const string CONFIG_MAX_PROCESSING_TIME_MS_ATTR = "max-processing-time-ms";
        public const int CONFIG_MAX_PROCESSING_TIME_MS_MIN_VALUE = 25;

        public const string CONFIG_RESTART_PROCESSING_AFTER_MS_ATTR = "restart-processing-after-ms";
        public const int CONFIG_RESTART_PROCESSING_AFTER_MS_DEFAULT = 60000;

        /// <summary>
        /// Defines how much smoothing the processing time filter does - the lower the number the more smoothing is done.
        /// Smoothing makes MaxProcessingTimeMs detection insensitive to some seldom delays that may happen every now and then
        /// while destination performs actual write into its sink
        /// </summary>
        public const float PROCESSING_TIME_EMA_FILTER = 0.0007f;

    #endregion

    #region .ctor

      protected Sink(ISinkOwner owner) : this(owner, null, 0)
      {
      }

      protected Sink(ISinkOwner owner, string name, int order) : base(owner)
      {
        m_Levels = new LevelsList();
        Name = name.IsNullOrWhiteSpace() ? "{0}.{1}".Args(GetType().Name, FID.Generate().ID.ToString("X")) : name;
        m_Order = order;
        ((ISinkOwnerRegistration)owner).Register(this);
      }

      internal Sink(ISinkOwner owner, bool _) : base(owner)
      {
        m_Levels = new LevelsList();
        //this overload purposely does not do registration with owner
      }

      protected override void Destructor()
      {
        ((ISinkOwnerRegistration)ComponentDirector).Unregister(this);
        base.Destructor();
      }
    #endregion

    #region Pvt/Protected Fields


        private Exception m_LastError;
        [Config]protected int m_Order;

        private DateTime? m_LastErrorTimestamp;

        private MessageFilterExpression m_Filter;

        private System.Diagnostics.Stopwatch m_StopWatch = new System.Diagnostics.Stopwatch();

        private MessageType? m_MinLevel;
        private MessageType? m_MaxLevel;
        private LevelsList   m_Levels;
        private DaysOfWeek? m_DaysOfWeek;
        private DateTime? m_StartDate;
        private DateTime? m_EndDate;
        private TimeSpan? m_StartTime;
        private TimeSpan? m_EndTime;

        private bool m_GenerateFailoverMessages;
        private bool m_OnlyFailures;
        private string m_Failover;
        private bool m_TestOnStart;
        private ASCII8 m_Channel;

        private int? m_MaxProcessingTimeMs;
        private float m_AverageProcessingTimeMs;
        private int m_RestartProcessingAfterMs = CONFIG_RESTART_PROCESSING_AFTER_MS_DEFAULT;


    #endregion


    #region Properties

        /// <summary>
        /// Provides log-wide sink processing order
        /// </summary>
        public int Order => m_Order;

        public override string ComponentLogTopic => CoreConsts.LOG_TOPIC;

        /// <summary>
        /// Implements IInstrumentable
        /// </summary>
        [Config]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
        public override bool InstrumentationEnabled {  get; set; }


        /// <summary>
        /// Gets/sets filter expression for this destination.
        /// Filter expressions get dynamically compiled into filter assembly,
        /// consequently it is not a good practice to create too many different filters.
        /// Filters are heavyweight, and it is advisable to use them ONLY WHEN regular destination filtering (using Min/Max levels, dates and times) can not be used
        ///  to achieve the desired result
        /// </summary>
        public MessageFilterExpression Filter
        {
          get { return m_Filter; }
          set { m_Filter = value; }
        }

        /// <summary>
        /// References message filtering method or null
        /// </summary>
        public MessageFilterHandler FilterMethod
        {
          get;
          set;
        }

        /// <summary>
        /// Returns last error that this destination has encountered
        /// </summary>
        public Exception LastError
        {
          get { return m_LastError; }
        }

        /// <summary>
        /// Returns last error timestamp (if any)in localized time
        /// </summary>
        private DateTime? LastErrorTimestamp
        {
          get { return m_LastErrorTimestamp; }
        }

        /// <summary>
        /// Imposes a minimum log level constraint
        /// </summary>
        [Config("$" + CONFIG_MIN_LEVEL_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public MessageType? MinLevel
        {
          get { return m_MinLevel; }
          set { m_MinLevel = value;}
        }

        /// <summary>
        /// Imposes a maximum log level constraint
        /// </summary>
        [Config("$" + CONFIG_MAX_LEVEL_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public MessageType? MaxLevel
        {
          get { return m_MaxLevel; }
          set { m_MaxLevel = value;}
        }

        public LevelsList Levels
        {
          get { return m_Levels; }
          set { m_Levels = value ?? new LevelsList(); }
        }

        /// <summary>
        /// Imposes a filter on days when this destination handles messages
        /// </summary>
        [Config("$" + CONFIG_DAYS_OF_WEEK_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public DaysOfWeek? DaysOfWeek
        {
          get { return m_DaysOfWeek; }
          set { m_DaysOfWeek = value;}
        }

        /// <summary>
        /// Imposes a filter that specifies the starting date and time
        /// after which this destination will start processing log messages
        /// </summary>
        [Config("$" + CONFIG_START_DATE_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public DateTime? StartDate
        {
          get { return m_StartDate; }
          set { m_StartDate = value;}
        }

        /// <summary>
        /// Imposes a filter that specifies the ending date and time
        /// before which this destination will be processing log messages
        /// </summary>
        [Config("$" + CONFIG_END_DATE_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public DateTime? EndDate
        {
          get { return m_EndDate; }
          set { m_EndDate = value;}
        }

        /// <summary>
        /// Imposes a filter that specifies the starting time of the day
        /// after which this destination will start processing log messages
        /// </summary>
        [Config("$" + CONFIG_START_TIME_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public TimeSpan? StartTime
        {
          get { return m_StartTime; }
          set { m_StartTime = value;}
        }

        /// <summary>
        /// Imposes a filter that specifies the ending time of the day
        /// before which this destination will be processing log messages
        /// </summary>
        [Config("$" + CONFIG_END_TIME_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public TimeSpan? EndTime
        {
          get { return m_EndTime; }
          set { m_EndTime = value;}
        }

        /// <summary>
        /// Indicates whether this destination should only process failures - messages that crashed other destinations.
        /// When set to true regular messages (dispatched by Send(msg)) are ignored
        /// </summary>
        [Config("$" + CONFIG_ONLY_FAILURES_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public bool OnlyFailures
        {
          get { return m_OnlyFailures; }
          set { m_OnlyFailures = value; }
        }


        /// <summary>
        /// Determines whether additional co-related error message should be generated when this destination fails or when it is
        ///  used as failover by some other destination. When this property is true an additional error message gets written into failover destination that
        ///   describes what message caused failure (error is co-related to original) at what destination. False by default.
        /// </summary>
        [Config("$" + CONFIG_GENERATE_FAILOVER_MSG_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public bool GenerateFailoverMessages
        {
          get { return m_GenerateFailoverMessages; }
          set { m_GenerateFailoverMessages = value;}
        }



        /// <summary>
        /// Sets destination name used for failover of this one
        /// </summary>
        [Config("$" + CONFIG_FAILOVER_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public string Failover
        {
          get { return m_Failover ?? string.Empty; }
          set { m_Failover = value; }
        }


        /// <summary>
        /// Indicates whether this destination should try to test the underlying sink on startup.
        /// For example DB-based destinations will try to connect to server upon log service launch when this property is true
        /// </summary>
        [Config("$" + CONFIG_TEST_ON_START_ATTR)]
        public bool TestOnStart
        {
          get { return m_TestOnStart; }
          set { m_TestOnStart = value;}
        }


        /// <summary>
        /// Imposes a time limit on internal message processing (writing into actual sink) by this destination.
        /// If this limit is exceeded, this destination fails and processing is re-tried to be resumed after RestartProcessingAfterMs interval.
        /// The minimum value for this property is 25 ms as lower values compromise timer accuracy
        /// </summary>
        [Config("$" + CONFIG_MAX_PROCESSING_TIME_MS_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public int? MaxProcessingTimeMs
        {
          get { return m_MaxProcessingTimeMs; }
          set
          {
           if (value.HasValue)
           {
            m_MaxProcessingTimeMs = value > CONFIG_MAX_PROCESSING_TIME_MS_MIN_VALUE ? value : CONFIG_MAX_PROCESSING_TIME_MS_MIN_VALUE;
            m_AverageProcessingTimeMs = 0f;
           }
           else
            m_MaxProcessingTimeMs = null;
          }
        }

        [Config]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public ASCII8 Channel
        {
          get { return m_Channel; }
          set { m_Channel = value; }
        }

        /// <summary>
        /// Returns average time it takes destination implementation to write the log message to actual sink.
        /// This property is only computed when MaxProcessingTimeMs limit is imposed, otherwise it returns 0f
        /// </summary>
        public float AverageProcessingTimeMs
        {
          get { return m_AverageProcessingTimeMs; }
        }


        /// <summary>
        /// Specifies how much time must pass before processing will be tried to resume after failure.
        /// The default value is 60000 ms
        /// </summary>
        [Config("$" + CONFIG_RESTART_PROCESSING_AFTER_MS_ATTR)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
        public int RestartProcessingAfterMs
        {
          get { return m_RestartProcessingAfterMs; }
          set { m_RestartProcessingAfterMs = value; }
        }

    #endregion


    #region Public

        /// <summary>
        /// Sends the message into destination doing filter checks first.
        /// </summary>
        public void Send(Message msg)
        {
          if (!Running) return;
          if (m_OnlyFailures) return;

          SendRegularAndFailures(msg);
        }

        internal void SendRegularAndFailures(Message msg)
        {
           if (!Running) return;

           //When there was failure and it was not long enough
           if (m_LastErrorTimestamp.HasValue)
           if ((LocalizedTime - m_LastErrorTimestamp.Value).TotalMilliseconds < m_RestartProcessingAfterMs)
           {
             //this is after than throwing exception
             var error = new AzosException(string.Format(StringConsts.LOGSVC_DESTINATION_IS_OFFLINE_ERROR, Name));
             SetError(error, msg);
             return;
           }

          try
          {
            if (!Channel.IsZero && msg.Channel != Channel) return;

            if (!satisfyFilter(msg)) return;

            if (m_Levels.Count > 0)
            {
                bool found = false;
                foreach (var r in m_Levels)
                    if (r.Item1 <= msg.Type && msg.Type <= r.Item2)
                    {
                        found = true;
                        break;
                    }
                 if (!found)
                     return;
            }

            var msgLocalTime = UniversalTimeToLocalizedTime(msg.UTCTimeStamp);

            if (
                (!m_MinLevel.HasValue   || msg.Type >= m_MinLevel.Value) &&
                (!m_MaxLevel.HasValue   || msg.Type <= m_MaxLevel.Value) &&
                (!m_DaysOfWeek.HasValue || m_DaysOfWeek.Value.Contains(msgLocalTime.DayOfWeek)) &&
                (!m_StartDate.HasValue  || msgLocalTime >= m_StartDate.Value) &&
                (!m_EndDate.HasValue    || msgLocalTime <= m_EndDate.Value) &&
                (!m_StartTime.HasValue  || msgLocalTime.TimeOfDay >= m_StartTime.Value) &&
                (!m_EndTime.HasValue    || msgLocalTime.TimeOfDay <= m_EndTime.Value)
               )
            {
                if (!m_MaxProcessingTimeMs.HasValue)
                 DoSend(msg);
                else
                {
                   m_StopWatch.Restart();
                    DoSend(msg);
                   m_StopWatch.Stop();

                   if (m_LastError != null) m_AverageProcessingTimeMs = 0f;//reset average time to 0 after 1st successful execution after prior failure

                   //EMA filter
                   m_AverageProcessingTimeMs = ( PROCESSING_TIME_EMA_FILTER * m_StopWatch.ElapsedMilliseconds) +
                                               ( (1.0f - PROCESSING_TIME_EMA_FILTER) * m_AverageProcessingTimeMs );

                   if (m_AverageProcessingTimeMs > m_MaxProcessingTimeMs.Value)
                    throw new AzosException(string.Format(StringConsts.LOGSVC_DESTINATION_EXCEEDS_MAX_PROCESSING_TIME_ERROR,
                                                         Name,
                                                         m_MaxProcessingTimeMs,
                                                         m_StopWatch.ElapsedMilliseconds));
                }

                if (m_LastError != null) SetError(null, msg);//clear-out error
            }

          }
          catch (Exception error)
          {
            //WARNING!!!
            //under no condition MAY any exception escape from here
            SetError(error, msg);
          }
        }

        /// <summary>
        /// Provides periodic notification of destinations from central Log thread even if there are no messages to write.
        /// Override DoPulse to commit internal batching buffers provided by particular destinations
        /// </summary>
        public void Pulse()
        {
           if (!Running) return;
           try
           {
              DoPulse();
           }
           catch (Exception error)
           {
            //WARNING!!!
            //under no condition MAY any exception escape from here
            SetError(error, null);
           }
        }

        /// <summary>
        /// Parses levels into a tuple list of level ranges
        /// </summary>
        /// <param name="levels">String representation of levels using ',' or ';' or '|'
        /// as range group delimiters, and '-' as range indicators.  If first/second bound of the range
        /// is empty, the min/max value of that bound is assumed.
        /// Examples: "Debug-DebugZ | Error", "-DebugZ | Info | Warning", "Info-", "DebugB-DebugC, Error"</param>
        public static LevelsList ParseLevels(string levels)
        {
            var result = new LevelsList();

            if (!string.IsNullOrWhiteSpace(levels))
                foreach (var p in levels.Split(',', ';', '|'))
                {
                    var minmax = p.Split(new char[] { '-' }, 2).Select(s => s.Trim()).ToArray();

                    if (minmax.Length == 0)
                        throw new AzosException(StringConsts.ARGUMENT_ERROR + "levels: " + p);

                    MessageType min, max;

                    if (string.IsNullOrWhiteSpace(minmax[0]))
                        min = MessageType.Debug;
                    else if (!Enum.TryParse(minmax[0], true, out min))
                        throw new AzosException(StringConsts.ARGUMENT_ERROR +
                            "levels: {0} (error parsing: {1})".Args(p, minmax[0]));

                    if (minmax.Length < 2)
                        max = min;
                    else if (string.IsNullOrWhiteSpace(minmax[1]))
                        max = MessageType.CatastrophicError;
                    else if (!Enum.TryParse(minmax[1], true, out max))
                        throw new AzosException(StringConsts.ARGUMENT_ERROR +
                            "levels: {0} (error parsing: {1})".Args(p, minmax[1]));

                    result.Add(new Tuple<MessageType, MessageType>(min, max));
                }

            return result;
        }


        #endregion


        #region Protected

        /// <summary>
        /// Override to perform derivative-specific configuration
        /// </summary>
        protected override void DoConfigure(IConfigSectionNode node)
        {
          base.DoConfigure(node);

          var expr = node.AttrByName(CONFIG_FILTER_ATTR).Value;
          if (!string.IsNullOrWhiteSpace(expr))
           m_Filter = new MessageFilterExpression(expr);

          m_Levels = ParseLevels(node.AttrByName(CONFIG_LEVELS_ATTR).Value);
        }

        /// <summary>
        /// Notifies log service of exception that surfaced during processing of a particular message
        /// </summary>
        protected void SetError(Exception error, Message msg)
        {
          if (error != null)
          {
            ComponentDirector.LogDaemon.FailoverDestination(this, error, msg);
            m_LastError = error;
            m_LastErrorTimestamp = LocalizedTime;
          }
          else
          {
            m_LastError = null;
            ComponentDirector.LogDaemon.FailoverDestination(this, null, null);
            m_LastErrorTimestamp = null;
          }
        }

        /// <summary>
        /// Performs physical send, i.e. storage in file for FileDestinations
        /// </summary>
        protected internal abstract void DoSend(Message entry);

        /// <summary>
        /// Provides periodic notification of destinations from central Log thread even if there are no messages to write.
        /// Override to commit internal batching buffers provided by particular destinations
        /// </summary>
        protected internal virtual void DoPulse()
        {

        }

    #endregion


    #region .pvt

        private bool satisfyFilter(Message msg)
        {
          //to avoid possible thread collisions
          var mf = FilterMethod;
          var fe = m_Filter;

          if (mf != null)
             if (!mf(this, msg)) return false;

          if (fe!=null)
           return fe.Evaluate(this, msg);

          return true;
        }

    #endregion
  }

}
