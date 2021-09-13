/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Text;

using Azos.Conf;

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Implements a destination group that stops message flood
  /// </summary>
  public class FloodSink : CompositeSink
  {
    public const int DEFAULT_MAX_TEXT_LENGTH = 0;

    public const int DEFAULT_MAX_COUNT = 25;
    public const int MAX_MAX_COUNT = 1000;
    public const int DEFAULT_INTERVAL_SEC = 10;

    /// <summary>
    /// Creates a filter that prevents message flood
    /// </summary>
    public FloodSink(ISinkOwner owner) : base(owner)
    {
    }

    public FloodSink(ISinkOwner owner, string name, int order) : base(owner, name, order)
    {
    }

    protected override void Destructor()
    {
      base.Destructor();
    }


    private int m_IntervalSec = DEFAULT_INTERVAL_SEC;

    private int m_MaxCount = DEFAULT_MAX_COUNT;
    private int m_MaxTextLength = DEFAULT_MAX_TEXT_LENGTH;
    private MessageList m_List = new MessageList();
    private DateTime m_LastFlush;

    private int m_Count;

    private MessageType m_MessageType;
    private string m_MessageTopic;
    private string m_MessageFrom;
    private int m_MessageSource;


    [Config]
    public int IntervalSec
    {
      get { return m_IntervalSec; }
      set
      {
        if (value<1) value = 1;
        m_IntervalSec = value;
      }
    }

    /// <summary>
    /// Sets how many messages may be batched per interval. If more messages arrive then their data is not going to be logged
    /// </summary>
    [Config]
    public int MaxCount
    {
      get { return m_MaxCount; }
      set
      {
        if (value<0) value = 0;
        if (value>MAX_MAX_COUNT) value = MAX_MAX_COUNT;
        m_MaxCount = value;
      }
    }

    /// <summary>
    /// Imposes a limit in character length of combined message test
    /// </summary>
    [Config]
    public int MaxTextLength
    {
      get { return m_MaxTextLength; }
      set
      {
        if (value<0) value = 0;
        m_MaxTextLength = value;
      }
    }

    /// <summary>
    /// Determines the message type for message emitted when flood is detected
    /// </summary>
    [Config]
    public MessageType MessageType
    {
      get { return m_MessageType;}
      set { m_MessageType = value; }
    }

    /// <summary>
    /// Determines the message topic for message emitted when flood is detected
    /// </summary>
    [Config]
    public string MessageTopic
    {
      get { return m_MessageTopic;}
      set { m_MessageTopic = value; }
    }


    /// <summary>
    /// Determines the message from for message emitted when flood is detected
    /// </summary>
    [Config]
    public string MessageFrom
    {
      get { return m_MessageFrom;}
      set { m_MessageFrom = value; }
    }

    /// <summary>
    /// Determines the message topic for message emitted when flood is detected
    /// </summary>
    [Config]
    public int MessageSource
    {
      get { return m_MessageSource;}
      set { m_MessageSource = value; }
    }


    protected override void DoStart()
    {
        m_LastFlush = DateTime.UtcNow;
        m_Count = 0;
        base.DoStart();
    }

    protected override void DoWaitForCompleteStop()
    {
        flush();
        base.DoWaitForCompleteStop();
    }


    protected internal override void DoSend(Message entry)
    {
        if (m_List.Count<m_MaxCount)
          m_List.Add(entry);

        m_Count++;
    }


    protected internal override void DoPulse()
    {
      if ((DateTime.UtcNow - m_LastFlush).TotalSeconds > m_IntervalSec)
      {
        flush();
      }
    }



    private void flush()
    {
      if (!Running && !ComponentDirector.LogDaemon.Reliable) return;

      try
      {
        if (m_Count==0) return;

        Message msg = null;

        if (m_List.Count==1)
          msg = m_List[0];
        else
        {
          msg = new Message();

          msg.InitDefaultFields(App);

          msg.Type = m_MessageType;
          msg.Topic = m_MessageTopic;
          msg.From = m_MessageFrom;
          msg.Source = m_MessageSource;

          if (msg.Topic.IsNullOrWhiteSpace()) msg.Topic = CoreConsts.LOG_TOPIC;
          if (msg.From.IsNullOrWhiteSpace()) msg.From = "FloodFilter";

          var txt = new StringBuilder();
          foreach(var m in m_List)
          {
            if (!Running && !ComponentDirector.LogDaemon.Reliable) return;
            txt.Append("@");
            txt.AppendLine( m.ToString() );
            txt.AppendLine( m.Parameters.TakeFirstChars(128, "..") );
            txt.AppendLine();

            if (m_MaxTextLength>0 && txt.Length>m_MaxTextLength) break;
          }

          var txtl = txt.ToString();

          if (m_MaxTextLength>0)
            if (txtl.Length > m_MaxTextLength)
            {
              txtl = txtl.Substring(0, m_MaxTextLength) + " ...... " + "truncated at {0} chars".Args(m_MaxTextLength);
            }

          msg.Text = "{0} log msgs, {1} combined:\r\n\n{2}".Args(m_Count, m_List.Count, txtl);
        }

        m_List.Clear();
        m_Count = 0;

        base.DoSend(msg);
      }
      finally
      {
        m_LastFlush = DateTime.UtcNow;
      }
    }

  }
}
