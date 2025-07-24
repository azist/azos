/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Concurrent;
using System.Threading;

using Azos.Log;
using Azos.Conf;
using Azos.Apps;
using Azos.Instrumentation;
using System.Threading.Tasks;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Sky.Messaging
{

  /// <summary>
  /// Provides implementation for IMessenger service
  /// </summary>
  public sealed class MessageDaemon : DaemonWithInstrumentation<IApplicationComponent>, IMessengerImplementation
  {
    #region CONSTS

    public const string CONFIG_MESSAGING_SECTION = "messaging";
    public const string CONFIG_SINK_SECTION = "sink";
    public const string CONFIG_FALLBACK_SINK_SECTION = "fallback-sink";

    private const string THREAD_NAME = "MailerService Thread";
    private const int INSTRUMENTATION_GRANULARITY_MS = 10000;
    private const MessageType DEFAULT_LOG_LEVEL = MessageType.Warning;
    #endregion

    public MessageDaemon(IApplication app) : base(app) { }
    public MessageDaemon(IApplicationComponent director) : base(director) { }

    #region Private Fields

    private Thread m_Thread;
    private ConcurrentQueue<Message>[] m_Queues;
    private MessageSink m_Sink;
    private MessageSink m_FallbackSink;
    private AutoResetEvent m_Trigger;
    private long m_stat_MessagesCount;
    private long m_stat_MessagesErrorCount;
    private long m_stat_FallbacksCount;
    private long m_stat_FallbackErrorCount;

    #endregion

    #region Properties

    public override string ComponentLogTopic => CoreConsts.WEBMSG_TOPIC;

    /// <summary>
    /// Turns instrumentation on/off
    /// </summary>
    [Config(Default = false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_MESSAGING)]
    public override bool InstrumentationEnabled { get; set; }

    /// <summary>
    /// Gets/sets sink that performs sending
    /// </summary>
    public IMessageSink Sink
    {
      get { return m_Sink; }
      set
      {
        CheckDaemonInactive();

        if (value != null && value.Messenger != this)
          throw new SkyException(StringConsts.MESSAGE_SINK_IS_NOT_OWNED_ERROR);
        m_Sink = value as MessageSink;
      }
    }

    public IMessageSink FallbackSink
    {
      get { return m_FallbackSink; }
      set
      {
        CheckDaemonInactive();

        if (value != null && value.Messenger != this)
          throw new SkyException(StringConsts.MESSAGE_SINK_IS_NOT_OWNED_ERROR);
        m_FallbackSink = value as MessageSink;
      }
    }

    #endregion

    #region Public

    public void SendMsg(Message msg)
    {
      if (!Running || msg == null) return;
      var queues = m_Queues;
      if (queues == null) return;

      var idx = (int)msg.Priority;
      if (idx > queues.Length) idx = queues.Length - 1;


      var queue = queues[idx];
      queue.Enqueue(msg);
      var trigger = m_Trigger;
      if (trigger != null) trigger.Set();
    }

    #endregion

    #region Protected

    protected override void DoConfigure(IConfigSectionNode node)
    {
      using(var scope = new Security.SecurityFlowScope(Security.TheSafe.SAFE_ACCESS_FLAG))
      {
        base.DoConfigure(node);
        m_Sink = FactoryUtils.MakeAndConfigure<MessageSink>(node[CONFIG_SINK_SECTION], typeof(Sinks.SMTPMessageSink), args: new object[] { this });
        m_FallbackSink = FactoryUtils.MakeAndConfigure<MessageSink>(node[CONFIG_FALLBACK_SINK_SECTION], typeof(Sinks.NOPMessageSink), args: new object[] { this });
      }
    }

    protected override void DoStart()
    {
      WriteLog(MessageType.Trace, nameof(DoStart), "Entering");

      try
      {
        if (m_Sink == null)
          throw new SkyException(StringConsts.MAILER_SINK_IS_NOT_SET_ERROR);

        m_Trigger = new AutoResetEvent(false);

        m_Queues = new ConcurrentQueue<Message>[(int)MsgPriority.Slowest + 1];
        for (var i = 0; i < m_Queues.Length; i++)
          m_Queues[i] = new ConcurrentQueue<Message>();

        m_Sink.Start();
        m_FallbackSink.Start();

        m_Thread = new Thread(threadSpin);
        m_Thread.Name = THREAD_NAME;
        m_Thread.IsBackground = false;

        m_Thread.Start();
      }
      catch (Exception error)
      {
        AbortStart();

        if (m_Thread != null)
        {
          m_Thread.Join();
          m_Thread = null;
        }

        WriteLog(MessageType.CatastrophicError, nameof(DoStart), "Leaked: " + error.ToMessageWithType(), error);
        throw error;
      }

      WriteLog(MessageType.Trace, nameof(DoStart), "Exiting");
    }

    protected override void DoSignalStop()
    {
      WriteLog(MessageType.Trace, nameof(DoSignalStop), "Entering DoSignalStop()");
      try
      {
        m_Sink.SignalStop();
        m_FallbackSink.SignalStop();
        m_Trigger.Set();
      }
      catch (Exception error)
      {
        WriteLog(MessageType.CatastrophicError, nameof(DoSignalStop), "Leaked: " + error.ToMessageWithType(), error);
        throw error;
      }

      WriteLog(MessageType.Trace, nameof(DoSignalStop), "Exiting");
    }

    protected override void DoWaitForCompleteStop()
    {
      WriteLog(MessageType.Trace, nameof(DoWaitForCompleteStop), "Entering");

      try
      {
        base.DoWaitForCompleteStop();

        m_Thread.Join();
        m_Thread = null;

        m_Sink.WaitForCompleteStop();
        m_FallbackSink.WaitForCompleteStop();
        m_Trigger.Dispose();
        m_Trigger = null;
      }
      catch (Exception error)
      {
        WriteLog(MessageType.CatastrophicError, nameof(DoWaitForCompleteStop), "Leaked: " + error.ToMessageWithType(), error);
        throw error;
      }

      WriteLog(MessageType.Trace, nameof(DoWaitForCompleteStop), "Exiting");
    }

    #endregion



    #region .pvt

    private void threadSpin()
    {
      try
      {
        var lastInstr = App.TimeSource.UTCNow;

        while (Running)
        {
          var count = 50;
          for (var i = 0; i < m_Queues.Length && Running; i++)
          {
            write(m_Queues[i], count < 1 ? 1 : count);
            count /= 2;
          }

          m_Trigger.WaitOne(1000.ChangeByRndPct(0.2f));

          var now = App.TimeSource.UTCNow;
          if (InstrumentationEnabled && (now - lastInstr).TotalMilliseconds > INSTRUMENTATION_GRANULARITY_MS)
          {
            dumpStats();
            lastInstr = now;
          }
        }//while

        for (var i = 0; i < m_Queues.Length; i++)
          write(m_Queues[i], -1);

        dumpStats();
      }
      catch (Exception e)
      {
        WriteLog(MessageType.Emergency, nameof(threadSpin), "Leaked exception: "+e.ToMessageWithType(), e);
      }

      WriteLog(MessageType.Trace, nameof(threadSpin), "Exiting");
    }

    private void write(ConcurrentQueue<Message> queue, int count)  //-1 ==all
    {
      const int ABORT_TIMEOUT_MS = 10000;

      var processed = 0;
      Message msg;
      var started = App.TimeSource.UTCNow;

      while ((count < 0 || processed < count) && queue.TryDequeue(out msg))
      {
        if (!Running && (App.TimeSource.UTCNow - started).TotalMilliseconds > ABORT_TIMEOUT_MS)
        {
          WriteLog(MessageType.Error, nameof(write), "{0}.Write(msg) aborted on svc shutdown: timed-out after {1} ms.".Args(m_Sink.GetType().FullName, ABORT_TIMEOUT_MS));
          break;
        }


        DistributedCallFlow dcf = null;
        if (msg.CallFlowHeader.IsNotNullOrWhiteSpace())
        {
          //Continue the call flow stored in a message
          dcf = DistributedCallFlow.Continue(App, msg.CallFlowHeader, msg.Id);
        }

        try
        {
          var sent = false;

          try
          {
            statSend();
            sent = m_Sink.SendMsg(msg);
          }
          catch (Exception error)
          {
            statSendError();
            var et = error.ToMessageWithType();
            WriteLog(MessageType.Error,
                     nameof(write),
                     "{0}.Write(msg) leaked {1}".Args(m_Sink.GetType().FullName, et),
                     error,
                     pars: new {
                       id = msg.Id,
                       sub = msg.Subject.TakeFirstChars(48, ".."),
                       dcf = dcf
                     }.ToJson(JsonWritingOptions.CompactRowsAsMap),
                     related: dcf?.ID
                    );
          }

          if (!sent) writeFallback(msg, dcf);
        }
        finally
        {
          Apps.ExecutionContext.__SetThreadLevelCallContext(null);
        }

        processed++;
      }
    }

    private void writeFallback(Message msg, DistributedCallFlow dcf)
    {
      try
      {
        statFallback();
        m_FallbackSink.SendMsg(msg);
      }
      catch (Exception error)
      {
        statFallbackError();
        var et = error.ToMessageWithType();
        WriteLog(MessageType.Error,
                 nameof(writeFallback),
                 "{0}.Write(msg) leaked {1}".Args(m_FallbackSink.GetType().FullName, et),
                  error,
                  pars: new {
                    id = msg.Id,
                    sub = msg.Subject.TakeFirstChars(48, ".."),
                    dcf = dcf
                  }.ToJson(JsonWritingOptions.CompactRowsAsMap),
                  related: dcf?.ID
                 );
      }
    }

    private void dumpStats()
    {
      var inst = App.Instrumentation;

      Instrumentation.MessagingSinkCount         .Record(inst, Name, Interlocked.Exchange(ref m_stat_MessagesCount, 0));
      Instrumentation.MessagingSinkErrorCount    .Record(inst, Name, Interlocked.Exchange(ref m_stat_MessagesErrorCount, 0));
      Instrumentation.MessagingFallbackCount     .Record(inst, Name, Interlocked.Exchange(ref m_stat_FallbacksCount, 0));
      Instrumentation.MessagingFallbackErrorCount.Record(inst, Name, Interlocked.Exchange(ref m_stat_FallbackErrorCount, 0));
    }

    private void statSendError()
    {
      Interlocked.Increment(ref m_stat_MessagesErrorCount);
    }

    private void statSend()
    {
      Interlocked.Increment(ref m_stat_MessagesCount);
    }

    private void statFallback()
    {
      Interlocked.Increment(ref m_stat_FallbacksCount);
    }

    private void statFallbackError()
    {
      Interlocked.Increment(ref m_stat_FallbackErrorCount);
    }
    #endregion

  }//service

}
