/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

using Azos.Apps;
using Azos.Conf;
using Azos.Log;

namespace Azos.Instrumentation
{
    /// <summary>
    /// Implements IInstrumentation. This service aggregates data by type,source and sends result into provider
    /// </summary>
    [ConfigMacroContext]
    public sealed class InstrumentationDaemon : DaemonWithInstrumentation<IApplicationComponent>, IInstrumentationImplementation
    {
        #region CONSTS
            private const int MIN_INTERVAL_MSEC = 500;
            private const int DEFAULT_INTERVAL_MSEC = 7397;

            private const string THREAD_NAME = "Instrumentation Daemon Thread";


            public const string CONFIG_PROVIDER_SECTION = "provider";


            public const int DEFAULT_MAX_REC_COUNT = 1 * 1024 * 1024;
            public const int MINIMUM_MAX_REC_COUNT = 1024;
            public const int MAXIMUM_MAX_REC_COUNT = 256 * 1024 * 1024;

            public const int DEFAULT_RESULT_BUFFER_SIZE = 128 * 1024;
            public const int MAX_RESULT_BUFFER_SIZE = 2 * 1024 * 1024;// 250 msg * 12/min = 3,000/min * 60 min = 180,000/hr * 12 hrs = 2,160,000

        #endregion

        #region .ctor

          /// <summary>
          /// Creates a instrumentation service instance
          /// </summary>
          public InstrumentationDaemon(IApplication app) : base(app) {}

          /// <summary>
          /// Creates a instrumentation service instance
          /// </summary>
          public InstrumentationDaemon(IApplicationComponent director) : base(director) {}


        #endregion


        #region Private Fields

            private int m_ProcessingIntervalMS = DEFAULT_INTERVAL_MSEC;

            private int m_OSInstrumentationIntervalMS;

            private bool m_SelfInstrumented;

            private Thread m_Thread;

            private InstrumentationProvider m_Provider;

            private AutoResetEvent m_Trigger = new AutoResetEvent(false);

            private TypeBucketedData m_TypeBucketed;

            private int m_RecordCount;

            private int m_MaxRecordCount = DEFAULT_MAX_REC_COUNT;

            private Datum[] m_ResultBuffer;
            private int m_ResultBufferIndex = 0;
            private int m_ResultBufferSize = DEFAULT_RESULT_BUFFER_SIZE;

        #endregion

        #region Properties

            public override string ComponentCommonName { get { return "instr"; }}

            public override string ComponentLogTopic => CoreConsts.INSTRUMENTATION_TOPIC;

            public bool Enabled => true;

            /// <summary>
            /// Returns true to indicate that instrumentation does not have any space left to record more data
            /// </summary>
            public bool Overflown => m_RecordCount>m_MaxRecordCount;


            /// <summary>
            /// References provider that persists instrumentation data
            /// </summary>
            public InstrumentationProvider Provider
            {
              get { return m_Provider; }
              set
              {
                CheckDaemonInactive();
                m_Provider = value;
              }
            }

            /// <summary>
            /// Specifies how often aggregation is performed
            /// </summary>
            [Config("$processing-interval-ms|$interval-ms", DEFAULT_INTERVAL_MSEC)]
            [ExternalParameter]
            public int ProcessingIntervalMS
            {
              get { return m_ProcessingIntervalMS; }
              set
              {
                if (value < MIN_INTERVAL_MSEC) value = MIN_INTERVAL_MSEC;
                m_ProcessingIntervalMS = value;
              }
            }

            /// <summary>
            /// Specifies how often OS instrumentation such as CPU and RAM is sampled.
            /// Value of zero disables OS sampling
            /// </summary>
            [Config("$os-interval-ms|$os-interval|$os-instrumentation-interval-ms")]
            [ExternalParameter]
            public int OSInstrumentationIntervalMS
            {
              get { return m_OSInstrumentationIntervalMS; }
              set
              {
                if (value < 0) value = 0;
                m_OSInstrumentationIntervalMS = value;
              }
            }

            /// <summary>
            /// When true, outputs instrumentation data about the self (how many datum buffers, etc.)
            /// </summary>
            [Config("$self-instrumented|$instrument-self|$instrumented", false)]
            public bool SelfInstrumented
            {
              get { return m_SelfInstrumented; }
              set { m_SelfInstrumented = value;}
            }

            /// <summary>
            /// Shortcut to SelfInstrumented, implements IInstrumentable
            /// </summary>
            [Config(Default=false)]
            [ExternalParameter]
            public override bool InstrumentationEnabled
            {
              get { return this.SelfInstrumented;}
              set { SelfInstrumented = value;}
            }

            /// <summary>
            /// Returns current record count in the instance
            /// </summary>
            public int RecordCount { get{ return m_RecordCount;}}

            /// <summary>
            /// Gets/Sets the maximum record count that this instance can store
            /// </summary>
            [Config(null, DEFAULT_MAX_REC_COUNT)]
            [ExternalParameter]
            public int MaxRecordCount
            {
              get{ return m_MaxRecordCount;}
              set
              {
                if (value<MINIMUM_MAX_REC_COUNT) value = MINIMUM_MAX_REC_COUNT;
                else
                if (value>MAXIMUM_MAX_REC_COUNT) value = MAXIMUM_MAX_REC_COUNT;

                m_MaxRecordCount = value;
              }
            }

            /// <summary>
            /// Returns the size of the ring buffer where result (aggregated) instrumentation records are kept in memory.
            /// The maximum buffer capacity is returned, not how many results have been buffered so far.
            ///  If this property is less than or equal to zero then result buffering in memory is disabled.
            ///  This property can be set only on a stopped service
            /// </summary>
            [Config(null, DEFAULT_RESULT_BUFFER_SIZE)]
            public int ResultBufferSize
            {
              get { return m_ResultBufferSize;}
              set
              {
                CheckDaemonInactive();
                if (value>MAX_RESULT_BUFFER_SIZE) value = MAX_RESULT_BUFFER_SIZE;
                m_ResultBufferSize = value;
              }
            }

            /// <summary>
            /// Enumerates distinct types of Datum ever recorded in the instance. This property may be used to build
            ///  UIs for instrumentation, i.e. datum type tree. Returned data is NOT ORDERED
            /// </summary>
            public IEnumerable<Type> DataTypes
            {
              get
              {
                var bucketed = m_TypeBucketed;
                if (bucketed == null) return Enumerable.Empty<Type>();
                return bucketed.Keys.ToArray();
              }
            }

        #endregion


        #region Public

            /// <summary>
            /// Records instrumentation datum
            /// </summary>
            public void Record(Datum datum)
            {
              if (Status != DaemonStatus.Active) return;
              if (datum==null) return;
              if (Overflown) return;

              var t = datum.GetType();

              var srcBucketed = m_TypeBucketed.GetOrAdd(t, (tp) => new SrcBucketedData());

              if (srcBucketed.DefaultDatum==null)
               srcBucketed.DefaultDatum = datum;

              var bag = srcBucketed.GetOrAdd(datum.Source, (src) => new DatumBag());

              bag.Add(datum);
              Interlocked.Increment(ref m_RecordCount);
            }

            /// <summary>
            /// Returns the specified number of samples from the ring result buffer in the near-chronological order,
            /// meaning that data is already sorted by time MOST of the TIME, however sorting is NOT GUARANTEED for all
            ///  result records returned as enumeration is a lazy procedure that does not make copies/take locks.
            /// The enumeration is empty if ResultBufferSize is less or equal to zero entries.
            /// If count is less or equal to zero then the system returns all results available.
            /// </summary>
            public IEnumerable<Datum> GetBufferedResults(int count=0)
            {
              var data = m_ResultBuffer;//thread-safe copy
              if (data==null) yield break;

              var curIdx = m_ResultBufferIndex;
              if (curIdx>=data.Length) curIdx = 0;

              var idx = curIdx + 1;
              if (count>0)
              {
                if (count>m_ResultBufferSize) count = m_ResultBufferSize;

                if (curIdx>=count)
                  idx = curIdx - count;
                else
                  idx = (data.Length-1) - ((count-1) - curIdx);
              }
              //else dump all


              if (idx>=curIdx)//capture the tail first
              {
                for(;idx<data.Length;idx++)
                {
                   var datum = data[idx];
                   if (datum!=null) yield return datum;
                }
                idx = 0;
              }

              for(;idx<curIdx;idx++)
              {
                 var datum = data[idx];
                 if (datum!=null) yield return datum;
              }
            }

            /// <summary>
            /// Returns samples starting around the the specified UTCdate in the near-chronological order,
            /// meaning that data is already sorted by time MOST of the TIME, however sorting is NOT GUARANTEED for all
            ///  result records returned as enumeration is a lazy procedure that does not make copies/take locks.
            /// The enumeration is empty if ResultBufferSize is less or equal to zero entries
            /// </summary>
            public IEnumerable<Datum> GetBufferedResultsSince(DateTime utcStart)
            {
              const int COARSE_SEC_SAMPLES = 50;
              const int FINE_SEC_SAMPLES = 10;

              var data = m_ResultBuffer;//thread-safe copy
              if (data==null) yield break;

              var curIdx = m_ResultBufferIndex;
              if (curIdx>=data.Length) curIdx = 0;

              var taking = false;
              var idx = curIdx;
              //capture the tail first
              for(int pass=0;pass<2;pass++)//0=tail, 1=head, 2...exit
              {
                while(idx< (pass==0 ? data.Length : curIdx))
                {
                  var datum = data[idx];
                  if (datum==null)
                  {
                    idx += taking ? 1 : COARSE_SEC_SAMPLES;
                    continue;
                  }

                  if (taking)
                  {
                    if (datum.StartUtc >= utcStart)
                     yield return datum;
                    idx++;
                    continue;
                  }

                  if (datum.StartUtc>=utcStart)
                  {
                    taking = true;
                    continue;
                  }

                  var span = (int)(utcStart - datum.StartUtc).TotalSeconds;//can not be negative because of prior if
                  var offset = span<5 ? 1 : span<10 ? FINE_SEC_SAMPLES : span * COARSE_SEC_SAMPLES;
                  idx += offset;
                }//while
                idx=0;
              }//for

            }


            /// <summary>
            /// Enumerates sources per Datum type ever recorded by the instance. This property may be used to build
            ///  UIs for instrumentation, i.e. datum type tree. Returned data is NOT ORDERED
            /// </summary>
            public IEnumerable<string> GetDatumTypeSources(Type datumType, out Datum defaultInstance)
            {
               var tBucketed = m_TypeBucketed;
               if (datumType!=null && tBucketed!=null)
               {
                 SrcBucketedData srcBucketed = null;
                 if (tBucketed.TryGetValue(datumType, out srcBucketed))
                 {
                   defaultInstance = srcBucketed.DefaultDatum;
                   return srcBucketed.Keys.ToArray();
                 }
               }
               defaultInstance = null;
               return Enumerable.Empty<string>();
            }

        #endregion


        #region Protected
            protected override void DoConfigure(Azos.Conf.IConfigSectionNode node)
            {
              try
              {
                base.DoConfigure(node);

                m_Provider = FactoryUtils.MakeAndConfigure(node[CONFIG_PROVIDER_SECTION], typeof(NOPInstrumentationProvider), new[] { this }) as InstrumentationProvider;
              }
              catch (Exception error)
              {
                throw new AzosException(StringConsts.INSTRUMENTATIONSVC_PROVIDER_CONFIG_ERROR + error.Message, error);
              }
            }

            protected override void DoStart()
            {
              WriteLog(MessageType.Info, nameof(DoStart), "Entering");

              try
              {

                //pre-flight checks
                if (m_Provider == null)
                  throw new AzosException(StringConsts.DAEMON_INVALID_STATE + "InstrumentationService.DoStart(Provider=null)");


                m_BufferOldestDatumUTC = null;

                m_TypeBucketed = new TypeBucketedData();

                m_Provider.Start();

                m_ResultBufferIndex = 0;
                if (m_ResultBufferSize>0)
                  m_ResultBuffer = new Datum[m_ResultBufferSize];


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
                  try {m_Thread.Join();} catch{}
                  m_Thread = null;
                }

                m_ResultBuffer = null;

                WriteLog(MessageType.CatastrophicError, nameof(DoStart), "Leaked exception: " + error.Message);
                throw error;
              }

             WriteLog(MessageType.Info, nameof(DoStart), "Exiting");
          }

            protected override void DoSignalStop()
            {
              WriteLog(MessageType.Info, nameof(DoSignalStop), "Entering");

              try
              {
                base.DoSignalStop();

                m_Trigger.Set();

                //m_Provider should not be touched here
              }
              catch (Exception error)
              {
                WriteLog(MessageType.CatastrophicError, nameof(DoSignalStop), "Leaked exception: " + error.Message);
                throw error;
              }

              WriteLog(MessageType.Info, nameof(DoSignalStop), "Exiting ");
            }

            protected override void DoWaitForCompleteStop()
            {
              WriteLog(MessageType.Info, nameof(DoWaitForCompleteStop), "Entering");

              try
              {
                base.DoWaitForCompleteStop();

                m_Thread.Join();
                m_Thread = null;

                m_Provider.WaitForCompleteStop();

                m_TypeBucketed = null;
                m_ResultBuffer = null;
              }
              catch (Exception error)
              {
                WriteLog(MessageType.CatastrophicError, nameof(DoWaitForCompleteStop), "Leaked exception: " + error.Message);
                throw error;
              }

              WriteLog(MessageType.Info, nameof(DoWaitForCompleteStop), "Exiting");
            }



        #endregion



        #region .pvt. impl.



                private void threadSpin()
                {
                  var rel = Guid.NewGuid();
                  var errCount = 0;
                  try
                  {

                    while (Running)
                    {

                      try
                      {
                        if (m_SelfInstrumented)
                          instrumentSelf();

                        write();

                        if (m_OSInstrumentationIntervalMS <= 0)
                          m_Trigger.WaitOne(m_ProcessingIntervalMS);
                        else
                          instrumentOS();

                        errCount = 0;//success
                      }
                      catch(Exception bodyError)
                      {
                        errCount++;
                        WriteLog(errCount > 5 ? MessageType.Emergency : errCount > 1 ? MessageType.CriticalAlert : MessageType.Critical,
                                 "threadSpin() loop",
                                 "Leaked {0} consecutive times Exception: {1}".Args(errCount, bodyError.ToMessageWithType()),
                                 error: bodyError,
                                 related: rel);

                        //progressive throttle
                        m_Trigger.WaitOne(2500 + (1000 * errCount.KeepBetween(1, 15)));
                      }

                    }//while

                    write();
                  }
                  catch(Exception e)
                  {
                    WriteLog(MessageType.Emergency, " threadSpin()", "Leaked: " + e.ToMessageWithType(), e, related: rel);
                  }

                  WriteLog(MessageType.Trace, "Exiting threadSpin()", null, related: rel);
                }

                //adds data that described this very instance
                private void instrumentSelf()
                {
                   var rc = this.RecordCount;
                   Self.RecordCount.Record( this, rc );
                   Self.RecordLoad.Record( this, (int)Math.Round( 100d *  (rc / (double)m_MaxRecordCount)) );//cant be 0
                   Self.ProcessingInterval.Record( this, m_ProcessingIntervalMS );


                   var buf = m_ResultBuffer;
                   if (buf!=null)
                   {
                     if (m_BufferOldestDatumUTC.HasValue)
                     {
                       var ageSec = (int)(App.TimeSource.UTCNow - m_BufferOldestDatumUTC.Value).TotalSeconds;
                       Self.BufferMaxAge.Record( this, ageSec );
                     }
                   }
                }


                // Pauses up to m_ProcessingIntervalMS
                private void instrumentOS()
                {
                  const int MIN_SAMPLING_RATE_MS = 500;

                  var samplingRate = m_OSInstrumentationIntervalMS;
                  if (samplingRate < MIN_SAMPLING_RATE_MS) samplingRate = MIN_SAMPLING_RATE_MS;
                  if (samplingRate > m_ProcessingIntervalMS) samplingRate = m_ProcessingIntervalMS;
                  var remainder = m_ProcessingIntervalMS % samplingRate;

                  var count = m_ProcessingIntervalMS / samplingRate;
                  for (int i = 0; i < count && Running; i++)
                  {
                    Platform.Instrumentation.CPUUsage.Record     (this, Platform.Computer.CurrentProcessorUsagePct);
                    Platform.Instrumentation.RAMUsage.Record     (this, Platform.Computer.GetMemoryStatus().LoadPct);
                    Platform.Instrumentation.AvailableRAM.Record (this, Platform.Computer.CurrentAvailableMemoryMb);
                    m_Trigger.WaitOne(samplingRate);
                  }

                  if (Running && remainder > 20) m_Trigger.WaitOne(remainder);
                }


                private void write()
                {
                  try
                  {
                    var ctxBatch = m_Provider.BeforeBatch();
                    foreach (var tvp in m_TypeBucketed)
                    {
                      if (!Running) break;
                      try
                      {
                        var ctxType = m_Provider.BeforeType(tvp.Key, ctxBatch);
                        foreach (var svp in tvp.Value)
                        {
                          if (!Running) break;

                          var bag = svp.Value;
                          Datum datum = null;
                          if (bag.TryPeek(out datum))
                          {
                            Datum aggregated = null;

                            try
                            {
                              var lst = new List<Datum>();
                              Datum elm;
                              while (bag.TryTake(out elm))
                              {
                                lst.Add(elm);
                                Interlocked.Decrement(ref m_RecordCount);
                              }

                              aggregated = datum.Aggregate(lst);
                            }
                            catch (Exception error)
                            {
                              var et = error.ToMessageWithType();
                              WriteLog(MessageType.Error, string.Format("{0}.Aggregate(IEnumerable<Datum>) leaked {1}",
                                                                    datum.GetType().FullName,
                                                                    et), et);
                            }

                            try
                            {
                              if (aggregated != null)
                                m_Provider.Write(aggregated, ctxBatch, ctxType);
                            }
                            catch (Exception error)
                            {
                              var et = error.ToMessageWithType();
                              WriteLog(MessageType.CatastrophicError, string.Format("{0}.Write(datum) leaked {1}",
                                                                    m_Provider.GetType().FullName,
                                                                    et), et);
                            }

                            if (aggregated != null) bufferResult(aggregated);

                          }//if
                        }
                        m_Provider.AfterType(tvp.Key, ctxBatch, ctxType);

                      }
                      catch (Exception error)
                      {
                        var et = error.ToMessageWithType();
                        WriteLog(MessageType.CatastrophicError, string.Format("{0}.BeforeType() leaked {1}",
                                                              m_Provider.GetType().FullName,
                                                              et), et);
                      }
                    }
                    m_Provider.AfterBatch(ctxBatch);
                  }
                  catch(Exception error)
                  {
                    var et = error.ToMessageWithType();
                    WriteLog(MessageType.CatastrophicError, string.Format("{0}.BeforeBatch() leaked {1}",
                                                          m_Provider.GetType().FullName,
                                                          et), et);
                  }
                }


                private DateTime? m_BufferOldestDatumUTC;

                //revise
                private void bufferResult(Datum result)
                {
                  var data = m_ResultBuffer;
                  if (data==null || result==null) return;

                  var idx = Interlocked.Increment(ref m_ResultBufferIndex);

                  while(idx>=data.Length)//while needed in case VolatileWrite is not properly implemented on platform and does not write-through cache right away
                  {
                    Thread.VolatileWrite(ref m_ResultBufferIndex, -1);
                    idx = Interlocked.Increment(ref m_ResultBufferIndex);
                  }

                  if (m_BufferOldestDatumUTC.HasValue)
                  {
                    var existing = data[idx];
                    if (existing!=null)
                      m_BufferOldestDatumUTC = existing.StartUtc;
                  }
                  else
                   m_BufferOldestDatumUTC = result.StartUtc;

                  data[idx] = result;
                }

        #endregion



    }



    /// <summary>
    /// Internal concurrent dictionary used for instrumentation data aggregation
    /// </summary>
    internal class TypeBucketedData : ConcurrentDictionary<Type, SrcBucketedData> {}


    /// <summary>
    /// Internal concurrent dictionary used for instrumentation data aggregation
    /// </summary>
    internal class SrcBucketedData : ConcurrentDictionary<string, DatumBag>
    {
      internal Datum DefaultDatum;
    }


    /// <summary>
    /// Internal concurrent bag used for instrumentation data aggregation
    /// </summary>
    internal class DatumBag : ConcurrentBag<Datum> {}
}
