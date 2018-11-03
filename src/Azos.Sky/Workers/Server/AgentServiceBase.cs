using System;
using System.Threading;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Log;

namespace Azos.Sky.Workers.Server
{
  public abstract class AgentServiceBase : ServiceWithInstrumentationBase<object>
  {
    public const int THREAD_GRANULARITY_MS = 1750;
    public const int DEFAULT_INSTRUMENTATION_GRANULARITY_MS = 5000;
    public const int DEFAULT_STARTUP_DELAY_SEC = 60;


    protected AgentServiceBase(object director) : base(director)
    {
      LogLevel = MessageType.Error;
    }

    private Thread m_Thread;
    private int m_StartupDelaySec = DEFAULT_STARTUP_DELAY_SEC;

    [Config]
    [ExternalParameter(SysConsts.EXT_PARAM_GROUP_WORKER, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled { get; set; }

    [Config(Default = MessageType.Error)]
    [ExternalParameter(SysConsts.EXT_PARAM_GROUP_WORKER, CoreConsts.EXT_PARAM_GROUP_LOG)]
    public MessageType LogLevel { get; set; }


    [Config(Default = DEFAULT_STARTUP_DELAY_SEC)]
    public int StartupDelaySec
    {
      get { return m_StartupDelaySec; }
      set { m_StartupDelaySec = value<0 ? 0 : value; }
    }

    /// <summary>
    /// Overrides main thread granularity, return 0 for auto
    /// </summary>
    public virtual int ThreadGranularityMs { get { return 0;} }

    /// <summary>
    /// Specifies how often isntrumentation gets dumped, return 0 for default
    /// </summary>
    [Config]
    public virtual int InstrumentationGranularityMs { get; set; }

    protected override void DoStart()
    {
      base.DoStart();
      m_Thread = new Thread(threadSpin);
      m_Thread.Name = GetType().Name;
      m_Thread.Start();
    }

    protected override void DoWaitForCompleteStop()
    {
      m_Thread.Join();
      m_Thread = null;

      base.DoWaitForCompleteStop();
    }

    /// <summary>
    /// Writes to log on behalf of worker service
    /// </summary>
    public Guid Log(MessageType type,
                    string from,
                    string message,
                    Exception error = null,
                    Guid? relatedMessageID = null,
                    string parameters = null)
    {
      if (type < LogLevel) return Guid.Empty;

      var logMessage = new Message
      {
        Topic = SysConsts.LOG_TOPIC_WORKER,
        Text = message ?? string.Empty,
        Type = type,
        From = "{0}.{1}".Args(this.GetType().Name, from),
        Exception = error,
        Parameters = parameters
      };
      if (relatedMessageID.HasValue) logMessage.RelatedTo = relatedMessageID.Value;

      App.Log.Write(logMessage);

      return logMessage.Guid;
    }

    protected abstract void DoThreadSpin(DateTime utcNow);
    protected virtual void DoDumpStats(IInstrumentation instr, DateTime utcNow) { }
    protected virtual void DoResetStats(DateTime utcNow) { }

    #region Private
    private void threadSpin()
    {
      try
      {
        var ie = false;
        var startTime = App.TimeSource.UTCNow;
        var prevInstr = ComponentStartTime;
        while (Running)
        {
          var granMs = ThreadGranularityMs;
          if (granMs>0)
            Thread.Sleep(granMs);
          else
            Thread.Sleep(THREAD_GRANULARITY_MS + App.Random.NextScaledRandomInteger(0, THREAD_GRANULARITY_MS / 5));

          var utcNow = App.TimeSource.UTCNow;
          if ((utcNow - startTime).TotalSeconds < m_StartupDelaySec) continue;
          try
          {
            DoThreadSpin(utcNow);
          }
          catch (Exception error)
          {
            Log(MessageType.CatastrophicError, "threadSpin().DoThreadSpin", error.ToMessageWithType(), error);
          }

          try
          {
            utcNow = App.TimeSource.UTCNow;

            var igms = InstrumentationGranularityMs;
            if (igms<=0) igms = DEFAULT_INSTRUMENTATION_GRANULARITY_MS;

            if (ie != InstrumentationEnabled)
            {
              ie = InstrumentationEnabled;
              if (ie) resetStats(utcNow);
            }

            if (ie && (utcNow - prevInstr).TotalMilliseconds >= igms)
            {
              dumpStats(utcNow);
              prevInstr = utcNow;
            }
          }
          catch (Exception error)
          {
            Log(MessageType.CatastrophicError, "threadSpin().dumpStats", error.ToMessageWithType(), error);
          }
        }
      }
      catch (Exception error)
      {
        Log(MessageType.CatastrophicError, "threadSpin", error.ToMessageWithType(), error);
      }
    }

    private void dumpStats(DateTime utcNow)
    {
      var i = App.Instrumentation;
      if (!i.Enabled || this.InstrumentationEnabled) return;
      DoDumpStats(i, utcNow);
    }

    private void resetStats(DateTime utcNow)
    {
      DoResetStats(utcNow);
    }
    #endregion
  }
}
