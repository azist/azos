using System;
using System.Collections.Generic;

using Azos.Log;
using Azos.Log.Sinks;
using Azos.Instrumentation;

namespace Azos.Sky.Log
{
  /// <summary>
  /// Sends log messages to parent zone governor
  /// </summary>
  public sealed class SkyZoneSink : Sink
  {
    public const int DEFAULT_BUF_SIZE = 15;
    public const int MAX_BUF_SIZE = 0xff;
    public const int FLUSH_INTERVAL_MS = 7341;


    public SkyZoneSink() : base() { }
    public SkyZoneSink(string name) : base(name) { }


    private List<Message> m_Buf = new List<Message>(DEFAULT_BUF_SIZE);
    private int m_BufSize = DEFAULT_BUF_SIZE;
    private DateTime m_LastFlush;
    private int m_ZGovCallTimeoutMs;


    /// <summary>
    /// Overrides default service timeout when set to value greater than 0
    /// </summary>
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE)]
    public int ZGovCallTimeoutMs
    {
      get { return m_ZGovCallTimeoutMs;}
      set { m_ZGovCallTimeoutMs = value>0 ? value : 0;}
    }

    public override void Close()
    {
      flush();
      base.Close();
    }

    protected override void DoSend(Message entry)
    {
      var message = entry.Clone();
      if (message.Exception != null)
        message.Exception = new WrappedException(new WrappedExceptionData(message.Exception, captureStack: false));
      m_Buf.Add(message);
      if (m_Buf.Count > m_BufSize) flush();
    }

    protected override void DoPulse()
    {
      base.DoPulse();

      var flushEvery = FLUSH_INTERVAL_MS + App.Random.NextScaledRandomInteger(0, 5000);
      if ((Service.Now - m_LastFlush).TotalMilliseconds > flushEvery)
        flush();
    }

    private void flush()
    {
      m_LastFlush = Service.Now;

      if (m_Buf.Count == 0) return;
      if (!SkySystem.IsMetabase) return;

      try
      {
        var myHost = SkySystem.HostName;
        var zgHost = SkySystem.ParentZoneGovernorPrimaryHostName;
        if (zgHost.IsNullOrWhiteSpace()) return;

        //TODO  Cache the client instance, do not create client on every call
        using (var client = Contracts.ServiceClientHub.New<Contracts.IZoneLogReceiverClient>( zgHost ))
        {
          if (m_ZGovCallTimeoutMs>0) client.TimeoutMs = m_ZGovCallTimeoutMs;
          var expect = client.SendLog(myHost, SkySystem.MetabaseApplicationName, m_Buf.ToArray());
          if (expect > MAX_BUF_SIZE) expect = MAX_BUF_SIZE;
          if (expect < 1) expect = 1;
          m_BufSize = expect;
        }
      }
      catch (Exception error)
      {
        log(MessageType.Error, ".flush()", StringConsts.LOG_SEND_TOP_LOST_ERROR.Args(error.ToMessageWithType()));
      }
      finally
      {
        m_Buf.Clear();
      }
    }

    private void log(MessageType type, string from, string text, Exception error = null, Guid? related = null)
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

      App.Log.Write(msg);
    }

  }
}
