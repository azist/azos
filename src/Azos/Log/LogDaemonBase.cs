/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Log.Sinks;
using Azos.Instrumentation;

namespace Azos.Log
{
  /// <summary>
  /// Based class for implementing test and non-test logging services.
  /// Destinations may fail and the message will be failed-over into another destination in the same logger
  ///  as specified by 'failover' attribute of destination. This attribute is also present on service level.
  /// Cascading failover is not supported (failover of failovers). Another consideration is that messages
  ///  get sent into destinations synchronously by internal thread so specifying too many destinations may
  ///  limit overall LogService throughput. In complex scenarios consider using LogServiceDestination instead.
  /// </summary>
  public abstract class LogDaemonBase : DaemonWithInstrumentation<IApplicationComponent>, ILogImplementation, ISinkOwnerRegistration
  {
    #region CONSTS
    public const string CONFIG_SINK_SECTION   = "sink";
    public const string CONFIG_DEFAULT_FAILOVER_ATTR = "default-failover";
    public const string CONFIG_RELIABLE_ATTR = "reliable";
    #endregion


    #region .ctor

    /// <summary>
    /// Creates a new logging daemon instance
    /// </summary>
    protected LogDaemonBase(IApplication app) : base(app)
    {
      ctor();
    }

    /// <summary>
    /// Creates a new logging daemon instance
    /// </summary>
    protected LogDaemonBase(IApplicationComponent director) : base(director)
    {
      ctor();
    }

    private void ctor() => m_InstrBuffer = new MemoryBufferSink(this, false);//does not get registered in sinks

    private void cleanupSinks()
    {
      foreach (var sink in m_Sinks.OrderedValues.Reverse())
        sink.Dispose();

      m_Sinks.Clear();
    }

    protected override void Destructor()
    {
      base.Destructor();
      cleanupSinks();
      DisposeAndNull(ref m_InstrBuffer);
    }

    #endregion


    #region Private Fields

    protected OrderedRegistry<Sink> m_Sinks = new OrderedRegistry<Sink>();

    private int             MAX_NESTED_FAILURES = 8;
    private int             m_NestedFailureCount;
    private string          m_DefaultFailover;

    private Sink            m_FailoverErrorSink;
    private Exception       m_FailoverError;

    private Message m_LastWarning;
    private Message m_LastError;
    private Message m_LastCatastrophy;

    protected bool m_InstrumentationEnabled;

    private MemoryBufferSink m_InstrBuffer;

    private bool m_Reliable = true;
    #endregion


    #region Properties

    public override string ComponentCommonName => "log";

    public override string ComponentLogTopic => CoreConsts.LOG_TOPIC;

    /// <summary>
    /// Latches last problematic msg
    /// </summary>
    public Message LastWarning     => m_LastWarning;

    /// <summary>
    /// Latches last problematic msg
    /// </summary>
    public Message LastError       =>  m_LastError;

    /// <summary>
    /// Latches last problematic msg
    /// </summary>
    public Message LastCatastrophe => m_LastCatastrophy;

    /// <summary>
    /// Returns sinks. This call is thread safe
    /// </summary>
    public IOrderedRegistry<Sink> Sinks => m_Sinks;


    /// <summary>
    /// Implements IInstrumentable
    /// </summary>
    [Config(Default=false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled
    {
      get { return m_InstrumentationEnabled;}
      set { m_InstrumentationEnabled = value;}
    }

    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public int InstrumentationBufferSize
    {
      get { return m_InstrBuffer.BufferSize; }
      set { m_InstrBuffer.BufferSize = value; }
    }

    /// <summary>
    /// Determines whether this service blocks on stop longer until all buffered messages have been tried to be dispatched into all sinks.
    /// This property is true by default.
    /// Certain sinks may take considerable time to fail per message (e.g. database connection timeout), consequently buffered messages
    ///  processing may delay service stop significantly if this property is true
    /// </summary>
    [Config("$" + CONFIG_RELIABLE_ATTR, true)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
    public bool Reliable
    {
      get { return m_Reliable; }
      set { m_Reliable = value; }
    }

    /// <summary>
    /// Sets sink name used for failover on the service-level
    /// if a particular failing sink did not specify its specific failover
    /// </summary>
    [Config("$" + CONFIG_DEFAULT_FAILOVER_ATTR)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
    public string DefaultFailover
    {
        get { return m_DefaultFailover ?? string.Empty; }
        set { m_DefaultFailover = value; }
    }

    /// <summary>
    /// Returns a sink that threw last exception that happened during failover. This kind of exceptions is never propagated and always handled
    /// </summary>
    public Sink FailoverErrorSink { get { return m_FailoverErrorSink; } }

    /// <summary>
    /// Returns last exception that happened during failover. This kind of exceptions is never propagated and always handled
    /// </summary>
    public Exception FailoverError { get { return m_FailoverError; } }

    /// <summary>
    /// Indicates whether the service can operate without any sinks registered, i.e. some test loggers may not need
    ///  any destinations to operate as they synchronously write to some buffer without any extra sinks
    /// </summary>
    public virtual bool SinksAreOptional
    {
      get{ return false; }
    }

    #endregion

    #region Public

    /// <summary>
    /// Writes log message into log
    /// </summary>
    public void Write(MessageType type, string text, string topic = null, string from = null)
    {
        Write(type, text, false, topic, from);
    }

    /// <summary>
    /// Writes log message into log
    /// </summary>
    public void Write(MessageType type, string text, bool urgent, string topic = null, string from = null)
    {
        Write(new Message
        {
            Type = type,
            Topic = topic,
            From = from,
            Text = text
        }, urgent);
    }

    /// <summary>
    /// Writes log message into log
    /// </summary>
    /// <param name="msg">Message to write</param>
    public void Write(Message msg)
    {
        Write(msg, false);
    }

    /// <summary>
    /// Writes log message into log
    /// </summary>
    /// <param name="msg">Message to write</param>
    /// <param name="urgent">Indicates that the logging service implementation must
    /// make an effort to write the message to its destinations urgently</param>
    public void Write(Message msg, bool urgent)
    {
        if (Status != DaemonStatus.Active) return;


        if (msg==null) return;

        if (msg.Type>=MessageType.Emergency) m_LastCatastrophy = msg;
        else
        if (msg.Type>=MessageType.Error) m_LastError = msg;
        else
        if (msg.Type>=MessageType.Warning) m_LastWarning = msg;

        if (m_InstrumentationEnabled) m_InstrBuffer.Send( msg);

        DoWrite(msg, urgent);
    }

    /// <summary>
    /// Returns instrumentation buffer if instrumentation enabled
    /// </summary>
    public IEnumerable<Message> GetInstrumentationBuffer(bool asc)
    {
      return asc ? m_InstrBuffer.BufferedTimeAscending : m_InstrBuffer.BufferedTimeDescending;
    }

    #endregion


    #region Protected

    /// <summary>
    /// Writes log message into log
    /// </summary>
    /// <param name="msg">Message to write</param>
    /// <param name="urgent">Indicates that the logging service implementation must
    /// make an effort to write the message to its destinations urgently</param>
    protected abstract void DoWrite(Message msg, bool urgent);

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      if (node==null || !node.Exists) return;

      cleanupSinks();

      foreach (var snode in node.Children.Where(n => n.IsSameName(CONFIG_SINK_SECTION)))
      {
        var sname = snode.AttrByName(CONFIG_NAME_ATTR).Value;
        var sorder = snode.AttrByName(Configuration.CONFIG_ORDER_ATTR).ValueAsInt();
        var sink = FactoryUtils.MakeAndConfigure<Sink>(snode, typeof(CSVFileSink), new object[]{ this, sname, sorder });
      }
    }

    protected override void DoStart()
    {
      base.DoStart();

      if (!SinksAreOptional && m_Sinks.Count == 0)
        throw new AzosException(StringConsts.LOGSVC_NODESTINATIONS_ERROR);

      foreach (var sink in m_Sinks.OrderedValues)
        try
        {
          sink.Start();
        }
        catch (Exception error)
        {
          throw new AzosException(
                StringConsts.LOGDAEMON_SINK_START_ERROR.Args(Name, sink.Name, sink.TestOnStart, error.Message),
                error);
        }
    }

    protected override void DoSignalStop()
    {
      base.DoSignalStop();
      //Attention!!! It is important here NOT TO NOTIFY sinks of pending shutdown,
      //so that LogDaemon may start terminating all by itself and it commits all messages to sinks
      //that should be still operational.
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();
      // at this point the thread has stopped and we can now stop the sinks

      foreach (var sink in m_Sinks.OrderedValues.Reverse())
      {
          try
          {
            sink.WaitForCompleteStop();
          } catch
          {
#warning REVISE - must not eat exceptions - use Conout?
          }  // Can't do much here in case of an error
      }
    }

    protected void Pulse()
    {
      foreach (var sink in m_Sinks.OrderedValues)
        sink.Pulse();
    }

    /// <summary>
    /// When error=null => error cleared. When msg==null => exceptions surfaced from DoPulse()
    /// </summary>
    internal void FailoverDestination(Sink sink, Exception error, Message msg)
    {
      if (!Running) return;
      if (m_NestedFailureCount>=MAX_NESTED_FAILURES) return;//stop cascade recursion

      m_NestedFailureCount++;
      try
      {
        if (error==null)//error lifted
        {
          if (sink==m_FailoverErrorSink)
          {
            m_FailoverErrorSink = null;
            m_FailoverError = null;
          }
          return;
        }

        if (msg==null) return; //i.e. OnPulse()

        var failoverName = sink.Failover;
        if (string.IsNullOrEmpty(failoverName))
            failoverName = this.DefaultFailover;
        if (string.IsNullOrEmpty(failoverName))  return;//nowhere to failover

        var failover = m_Sinks[failoverName];

        if (failover==null) return;

        if (failover==sink) return;//circular reference, cant fail into destination that failed

        try
        {
          failover.SendRegularAndFailures(msg);

            if (sink.GenerateFailoverMessages || failover.GenerateFailoverMessages)
            {
              var emsg = new Message();
              emsg.Type = MessageType.Error;
              emsg.From = sink.Name;
              emsg.Topic = CoreConsts.LOG_TOPIC;
              emsg.Text = string.Format(
                      StringConsts.LOGSVC_FAILOVER_MSG_TEXT,
                      msg.Guid,
                      sink.Name,
                      failover.Name,
                      sink.AverageProcessingTimeMs);
              emsg.RelatedTo = msg.Guid;
              emsg.Exception = error;


              failover.SendRegularAndFailures(emsg);
            }

          failover.DoPulse();
        }
        catch(Exception failoverError)
        {
          m_FailoverErrorSink = failover;
          m_FailoverError = failoverError;
        }
      }
      finally
      {
        m_NestedFailureCount--;
      }

    }

    LogDaemonBase ISinkOwner.LogDaemon => this;
    void ISinkOwnerRegistration.Register(Sink sink) => m_Sinks.Register(sink);
    void ISinkOwnerRegistration.Unregister(Sink sink) => m_Sinks.Unregister(sink);

    #endregion
  }
}
