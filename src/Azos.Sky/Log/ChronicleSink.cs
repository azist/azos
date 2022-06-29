/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using Azos.Conf;
using Azos.Log;
using Azos.Log.Sinks;

using Azos.Sky.Chronicle;

namespace Azos.Sky.Log
{
  /// <summary>
  /// Sends log messages to log chronicle service using ILogChronicle
  /// </summary>
  public sealed class ChronicleSink : Sink
  {
    public const int BUFFER_TRIM = 1024;
    public const int MAX_BUFFER_LENGTH = 200_000;
    public const int START_SEND_DELAY_SEC_DEFAULT = 30;
    public const int START_SEND_DELAY_SEC_MAX = 5 * 60;

    public ChronicleSink(ISinkOwner owner) : base(owner) { }
    public ChronicleSink(ISinkOwner owner, string name, int order) : base(owner, name, order) { }

    //The dependency may NOT be available at time of construction of this object
    //because this boots before other framework services, hence - service location
    ILogChronicleLogic m_Chronicle;
    ILogChronicleLogic Chronicle
    {
      get
      {
        if (m_Chronicle == null)
          m_Chronicle = App.ModuleRoot.Get<ILogChronicleLogic>();

        return m_Chronicle;
      }
    }


    private bool m_SendDelayed;
    private DateTime m_DoStartUtc;
    private int m_StartSendDelaySec = START_SEND_DELAY_SEC_DEFAULT;
    private List<Message> m_ToSend = new List<Message>();

    /// <summary>
    /// Imposes send delay on start. This prevents errors originating from log services on cluster restart
    /// </summary>
    [Config(Default = START_SEND_DELAY_SEC_DEFAULT)]
    public int StartSendDelaySec
    {
      get => m_StartSendDelaySec;
      set => m_StartSendDelaySec = SetOnInactiveDaemon(value.KeepBetween(0, START_SEND_DELAY_SEC_MAX));
    }

    protected override void DoStart()
    {
      base.DoStart();
      m_DoStartUtc = App.TimeSource.UTCNow;
      m_SendDelayed = true;
    }


    protected internal override void DoSend(Message entry)
    {
      if (entry==null) return;

      var count = m_ToSend.Count;

      if (count >= MAX_BUFFER_LENGTH) return;

      m_ToSend.Add(entry);

      if (count == MAX_BUFFER_LENGTH)
      {
        throw new LogException("Sink exceeded max internal buffer of {0} msgs".Args(MAX_BUFFER_LENGTH));
      }
    }

    protected internal override void DoPulse()
    {
      base.DoPulse();

      if (m_ToSend.Count==0) return;

      if (m_SendDelayed)
      {
        var utc = App.TimeSource.UTCNow;
        if ((utc - m_DoStartUtc).TotalSeconds < m_StartSendDelaySec) return;//keep buffering
        m_SendDelayed = false;
      }

      var toSend = m_ToSend.ToArray();

      if (m_ToSend.Count > BUFFER_TRIM)
        m_ToSend = new List<Message>();
      else
        m_ToSend.Clear();

      foreach(var slice in toSend.BatchBy(0xff))
      {
        var batch = new LogBatch
        {
          Data = slice.ToArray()
        };

        Chronicle.WriteAsync(batch)
                 .GetAwaiter()
                 .GetResult();
      }
    }

  }
}
