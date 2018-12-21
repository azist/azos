using System;
using System.Threading;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Log;

namespace Azos.Sky.Workers.Server
{
  /// <summary>
  /// Provides base abstraction for some continuously running daemon (an agent) having a dedicated thread
  /// </summary>
  public abstract class AgentServiceBase : DaemonWithInstrumentation<IApplicationComponent>
  {
    public const int THREAD_GRANULARITY_MS = 1750;
    public const int DEFAULT_INSTRUMENTATION_GRANULARITY_MS = 5000;
    public const int DEFAULT_STARTUP_DELAY_SEC = 60;


    protected AgentServiceBase(IApplication app) : base(app)
    {
    }

    protected AgentServiceBase(IApplicationComponent director) : base(director)
    {
    }

    private Thread m_Thread;
    private AutoResetEvent m_Waiter;
    private int m_StartupDelaySec = DEFAULT_STARTUP_DELAY_SEC;


    public new ISkyApplication App => base.App.AsSky();

    [Config]
    [ExternalParameter(SysConsts.EXT_PARAM_GROUP_WORKER, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled { get; set; }


    /// <summary>
    /// When set, imposes a delay between agent daemon start and actual processing start
    /// </summary>
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
    /// Specifies how often instrumentation gets dumped, return 0 for default
    /// </summary>
    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public virtual int InstrumentationGranularityMs { get; set; }


    protected override void DoStart()
    {
      base.DoStart();
      m_Waiter = new AutoResetEvent(false);
      m_Thread = new Thread(threadSpin);
      m_Thread.Name = GetType().Name;
      m_Thread.IsBackground = false;
      m_Thread.Start();
    }

    protected override void DoSignalStop()
    {
      base.DoSignalStop();
      m_Waiter.Set();
    }

    protected override void DoWaitForCompleteStop()
    {
      if (m_Thread!=null)
      {
        m_Thread.Join();
        m_Thread = null;
      }
      DisposeAndNull(ref m_Waiter);

      base.DoWaitForCompleteStop();
    }

    protected abstract void DoThreadSpin(DateTime utcNow);
    protected virtual void DoDumpStats(IInstrumentation instr, DateTime utcNow) { }
    protected virtual void DoResetStats(DateTime utcNow) { }

    #region .pvt
    private void threadSpin()
    {
      try
      {
        var ie = false;
        var startTime = App.TimeSource.UTCNow;
        var prevInstr = startTime;
        while(Running)
        {
          var granMs = ThreadGranularityMs;
          if (granMs < 1)
            granMs = THREAD_GRANULARITY_MS.ChangeByRndPct(0.213f);

          m_Waiter.WaitOne(granMs);
          if (!Running) break;

          var utcNow = App.TimeSource.UTCNow;
          if ((utcNow - startTime).TotalSeconds < m_StartupDelaySec) continue;
          try
          {
            DoThreadSpin(utcNow);
          }
          catch (Exception error)
          {
            WriteLog(MessageType.CatastrophicError, nameof(DoThreadSpin), error.ToMessageWithType(), error);
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
            WriteLog(MessageType.CatastrophicError, nameof(dumpStats), error.ToMessageWithType(), error);
          }
        }//while
      }
      catch (Exception error)
      {
        WriteLog(MessageType.CatastrophicError, nameof(threadSpin), error.ToMessageWithType(), error);
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
