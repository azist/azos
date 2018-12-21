using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Azos.Log;
using Azos.Instrumentation;

namespace Azos.Sky.Instrumentation
{
  /// <summary>
  /// Reduces instrumentation data stream and uploads it to the higher-standing zone governor
  /// </summary>
  public class SkyZoneInstrumentationProvider : InstrumentationProvider
  {
    #region CONSTS
       public const string THREAD_NAME = "SkyZoneInstrumentationProvider";
       public const int THREAD_GRANULARITY_MS = 3750;
       public const int GLUE_CALL_SLA_MS = 5137;
    #endregion


          private class Chunk : Queue<Datum>
          {
              const int MAX_SIZE = 7;

              public void Push(Datum datum)
              {
                this.Enqueue(datum);
                if (this.Count>MAX_SIZE) this.Dequeue();
              }

              public Datum Reaggregate()
              {
                if (this.Count==0) return null;
                var first = this.Peek();
                var aggregated = first.Aggregate(this);
                this.Clear();
                return aggregated;
              }
          }

          private class BySrc : Dictionary<string, Chunk>{}
          private class ByType : Dictionary<Type, BySrc>{}


    #region .ctor
    public SkyZoneInstrumentationProvider(InstrumentationDaemon director) : base(director)
    {
    }
    #endregion

    #region Fields

      private ByType m_ByType = new ByType();

      private List<Datum> m_Uploading;
      private bool m_IAmRootHost;

      private Thread m_Thread;
      private AutoResetEvent m_WaitEvent;

      private int m_ZGovCallTimeoutMs;
    #endregion


    #region Properties

      /// <summary>
      /// Overrides default service timeout when set to value greater than 0
      /// </summary>
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE)]
      public int ZGovCallTimeoutMs
      {
        get { return m_ZGovCallTimeoutMs;}
        set { m_ZGovCallTimeoutMs = value >0 ? value : 0;}
      }

    #endregion

    #region Protected
      protected internal override void Write(Datum aggregatedDatum, object batchContext, object typeContext)
      {
        if (!Running) return;
        if (aggregatedDatum==null) return;
        if (m_IAmRootHost) return;

        var t = aggregatedDatum.GetType();

        BySrc bySrc;
        if (!m_ByType.TryGetValue(t, out bySrc))
        {
          bySrc = new BySrc();
          m_ByType[t] = bySrc;
        }

        Chunk chunk;
        if (!bySrc.TryGetValue(aggregatedDatum.Source, out chunk))
        {
          chunk = new Chunk();
          bySrc[aggregatedDatum.Source] = chunk;
        }

        chunk.Push(aggregatedDatum);
        if (m_Uploading!=null) return;


        var toUpload = new List<Datum>();
        foreach(var kvpT in m_ByType)
          foreach(var kvpS in kvpT.Value)
          {
            var reaggr = kvpS.Value.Reaggregate();
            if (reaggr==null) continue;
            toUpload.Add(reaggr);
          }

        m_Uploading = toUpload;
      }

      protected override void DoStart()
      {
         m_WaitEvent = new AutoResetEvent(false);

         m_Thread = new Thread(threadSpin);
         m_Thread.Name = THREAD_NAME;
         m_Thread.Start();

         m_IAmRootHost = App.AsSky().ParentZoneGovernorPrimaryHostName==null;
      }

      protected override void DoSignalStop()
      {
      }

      protected override void DoWaitForCompleteStop()
      {
        m_WaitEvent.Set();

        m_Thread.Join();
        m_Thread = null;

        m_WaitEvent.Close();
        m_WaitEvent = null;

        m_ByType = new ByType();
      }
    #endregion


    #region .pvt
          private void threadSpin()
          {
             const string FROM = "threadSpin()";
             const int MAX_FAILURES_PER_RANK = 3;

             int MAX_SEND_BATCH = 1024;

             while(Running)
             {
                if (m_Uploading==null)
                {
                  m_WaitEvent.WaitOne(THREAD_GRANULARITY_MS);
                  continue;
                }

                var sendNextTime = MAX_SEND_BATCH;
                var zgHost = App.AsSky().ParentZoneGovernorPrimaryHostName;
                var client = App.GetServiceClientHub().MakeNew<Contracts.IZoneTelemetryReceiverClient>( zgHost );
                if (m_ZGovCallTimeoutMs>0) client.TimeoutMs = m_ZGovCallTimeoutMs;

                try
                {
                  var alreadySent = 0;
                  var failures = 0;
                  while(Running && alreadySent<m_Uploading.Count)
                  {
                    var toSend = m_Uploading.Skip(alreadySent).Take(sendNextTime).ToArray();

                    try
                    {
                      sendNextTime = client.SendTelemetry(App.GetThisHostName(), toSend);
                      if (sendNextTime>MAX_SEND_BATCH) sendNextTime = MAX_SEND_BATCH;
                    }
                    catch(Exception error)
                    {
                      //todo INSTRUMENT errors

                      if (error is Azos.Glue.MessageSizeException)
                      {
                        if (MAX_SEND_BATCH>128)
                        {
                           MAX_SEND_BATCH = (int)(MAX_SEND_BATCH * 0.75d);
                           if (sendNextTime>MAX_SEND_BATCH) sendNextTime = MAX_SEND_BATCH;
                           continue;
                        }
                      }

                      failures++;
                      if (failures>MAX_FAILURES_PER_RANK)
                      {
                        failures = 0;
                        var phost = App.GetMetabase().CatalogReg.NavigateHost(zgHost).ParentZoneGovernorPrimaryHost(transcendNOC: true);// if not found here, go to the NOC higher than this one
                        if (phost==null)
                        {//we came to very root - data lost
                          log(MessageType.Error, FROM+".retryTop", StringConsts.INSTR_SEND_TELEMETRY_TOP_LOST_ERROR.Args(error.ToMessageWithType()));
                          break;
                        }
                        zgHost = phost.RegionPath;
                        client.Dispose();
                        client = App.GetServiceClientHub().MakeNew<Contracts.IZoneTelemetryReceiverClient>( zgHost );
                        client.TimeoutMs = GLUE_CALL_SLA_MS;
                      }
                      continue;
                    }

                    alreadySent += toSend.Length;
                  }

                }
                catch(Exception error)
                {
                  log(MessageType.Error, FROM, error.ToMessageWithType(), error);
                }
                finally
                {
                  m_Uploading = null;
                  client.Dispose();
                }
             }//while Running
          }

          internal void log(MessageType type, string from, string text, Exception error = null, Guid? related = null)
          {
             var msg = new Message
                {
                   Type = type,
                   Topic = SysConsts.LOG_TOPIC_INSTRUMENTATION,
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
