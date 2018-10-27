/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

using Azos.Apps;
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
    public abstract class LogServiceBase : ServiceWithInstrumentationBase<object>, ILogImplementation
    {
        public class SinkList : List<Sink> {}

        #region CONSTS
            internal const string CONFIG_SINK_SECTION   = "sink";
            internal const string CONFIG_DEFAULT_FAILOVER_ATTR = "default-failover";
        #endregion


        #region .ctor

            /// <summary>
            /// Creates a new logging service instance
            /// </summary>
            protected LogServiceBase() : base(null) { ctor();  }


            /// <summary>
            /// Creates a new logging service instance
            /// </summary>
            protected LogServiceBase(Service director = null) : base(director) { ctor(); }

            private void ctor()
            {
              m_InstrBuffer = new MemoryBufferSink();
              m_InstrBuffer.__setLogSvc(this);
            }

            protected override void Destructor()
            {
                base.Destructor();

                foreach (var sink in m_Sinks)
                    sink.Dispose();

                m_InstrBuffer.Dispose();
            }

        #endregion


        #region Private Fields

            protected SinkList m_Sinks = new SinkList();

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

        #endregion


        #region Properties

            public override string ComponentCommonName { get { return "log"; }}

            /// <summary>
            /// Latches last problematic msg
            /// </summary>
            public Message LastWarning     { get {return m_LastWarning;}}

            /// <summary>
            /// Latches last problematic msg
            /// </summary>
            public Message LastError       { get {return m_LastError;}}

            /// <summary>
            /// Latches last problematic msg
            /// </summary>
            public Message LastCatastrophe { get {return m_LastCatastrophy;}}

            /// <summary>
            /// Returns registered destinations. This call is thread safe
            /// </summary>
            public IEnumerable<Sink> Sinks
            {
                get { lock (m_Sinks) return m_Sinks.ToList(); }
            }


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
            /// Sets destination name used for failover on the service-level
            /// if particular failing destination did not specify its specific failover
            /// </summary>
            [Config("$" + CONFIG_DEFAULT_FAILOVER_ATTR)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
            public string DefaultFailover
            {
                get { return m_DefaultFailover ?? string.Empty; }
                set { m_DefaultFailover = value; }
            }

            /// <summary>
            /// Returns a destination that threw last exception that happened durng failover. This kind of exceptions is never propagated and always handled
            /// </summary>
            public Sink FailoverErrorSink { get { return m_FailoverErrorSink; } }

            /// <summary>
            /// Returns last exception that happened during failover. This kind of exceptions is never propagated and always handled
            /// </summary>
            public Exception FailoverError { get { return m_FailoverError; } }

            /// <summary>
            /// Returns localized log time
            /// </summary>
            public DateTime Now { get { return this.LocalizedTime; } }


            /// <summary>
            /// Indicates whether the service can operate without any destinations registered, i.e. some test loggers may not need
            ///  any destinations to operate as they synchronously write to some buffer without any extra destinations
            /// </summary>
            public virtual bool DestinationsAreOptional
            {
              get{ return false;}
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
                if (Status != ServiceStatus.Active) return;


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
            /// Adds a destination to this service active destinations. Negative index to append
            /// </summary>
            public void RegisterSink(Sink sink, int atIdx = -1)
            {
                if (sink == null) return;

                lock (m_Sinks)
                {
                    if (m_Sinks.Count==0 || atIdx<0 || atIdx > m_Sinks.Count)
                      m_Sinks.Add(sink);
                    else
                      m_Sinks.Insert(atIdx, sink);
                    sink.__setLogSvc(this);
                }
            }

            /// <summary>
            /// Removes a destiantion from this service active destinations, returns true if destination was found and removed
            /// </summary>
            public bool UnRegisterSink(Sink sink)
            {
                if (sink == null) return false;

                lock (m_Sinks)
                {
                    bool r = m_Sinks.Remove(sink);
                    if (r) sink.__setLogSvc(null);
                    return r;
                }
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

                foreach (ConfigSectionNode dnode in
                    node.Children.Where(n => n.IsSameName(CONFIG_SINK_SECTION)))
                {
                    Sink sink = FactoryUtils.MakeAndConfigure<Sink>(dnode);
                    this.RegisterSink(sink);
                }
            }

            protected override void DoStart()
            {
                base.DoStart();

                lock (m_Sinks)
                {
                    if (!DestinationsAreOptional && m_Sinks.Count == 0)
                        throw new AzosException(StringConsts.LOGSVC_NODESTINATIONS_ERROR);

                    foreach (var sink in m_Sinks)
                        try
                        {
                            sink.Open();
                        }
                        catch (Exception error)
                        {
                            throw new AzosException(
                                StringConsts.LOGSVC_DESTINATION_OPEN_ERROR.Args(Name, sink.Name, sink.TestOnStart, error.Message),
                                error);
                        }
                }
            }

            protected override void DoWaitForCompleteStop()
            {
                base.DoWaitForCompleteStop();

                foreach (var sink in m_Sinks)
                    try { sink.Close(); } catch {}  // Can't do much here in case of an error
            }

            protected void Pulse()
            {
                lock (m_Sinks)
                    foreach (var sink in m_Sinks)
                        sink.Pulse();
            }

            /// <summary>
            /// When error=null => error cleared. When msg==null => exceptions surfaced from DoPulse()
            /// </summary>
            internal void FailoverDestination(Sink sink, Exception error, Message msg)
            {
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

                      Sink failover = null;
                      lock(m_Sinks)
                         failover = m_Sinks.FirstOrDefault(d => string.Equals(d.Name , failoverName, StringComparison.InvariantCultureIgnoreCase));


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

        #endregion
    }
}
