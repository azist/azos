/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Implements a destination that caches up to specified number of latest log messages in memory
  /// </summary>
  public sealed class MemoryBufferSink : Sink
  {
    public const int BUFFER_SIZE_DEFAULT = 1024;
    public const int BUFFER_SIZE_MAX = 250 * 1024;

    public MemoryBufferSink(ISinkOwner owner) : base(owner)
    {
    }

    public MemoryBufferSink(ISinkOwner owner, string name, int order) : base(owner, name, order)
    {
    }

    internal MemoryBufferSink(LogDaemonBase owner, bool  _) : base(owner, false)
    {
    }

    private Message[] m_Buffer;
    private int m_Index;
    private int m_BufferSize = BUFFER_SIZE_DEFAULT;

    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public int BufferSize
    {
      get { return m_BufferSize;}
      set
      {
        if (value==m_BufferSize) return;
        if (value<1) value = 1;
        if (value>BUFFER_SIZE_MAX) value = BUFFER_SIZE_MAX;
        m_BufferSize = value;
        m_Buffer = null; //atomic
      }
    }


    /// <summary>
    /// Returns all buffered log messages, where X = BufferSize property
    /// </summary>
    public IEnumerable<Message> Buffered {get{ return buffered(true);}}

    /// <summary>
    /// Returns all buffered log messages ordered by timestamp ascending
    /// </summary>
    public IEnumerable<Message> BufferedTimeAscending { get { return buffered(true).OrderBy( msg => msg.UTCTimeStamp.Ticks ); } }

    /// <summary>
    /// Returns all buffered log messages ordered by timestamp descending
    /// </summary>
    public IEnumerable<Message> BufferedTimeDescending  { get { return buffered(false).OrderBy( msg => -msg.UTCTimeStamp.Ticks ); } }

    private IEnumerable<Message> buffered(bool asc)
    {
        var buffer = m_Buffer;  //atomic
        if (buffer==null) yield break;

        if (asc)
          for(var i=0; i<buffer.Length; i++)
          {
            var elm = buffer[i];
            if (elm!=null) yield return elm;
          }
        else
          for(var i=buffer.Length-1; i>=0; i--)
          {
            var elm = buffer[i];
            if (elm!=null) yield return elm;
          }
    }

    /// <summary>
    /// Deletes all buffered messages
    /// </summary>
    public void ClearBuffer()
    {
        m_Buffer = null;//atomic
    }

    protected internal override void DoSend(Message entry)
    {
      if (m_Buffer==null)
      {
        m_Buffer = new Message[m_BufferSize]; //atomic
        m_Index = 0;
      }

      m_Buffer[m_Index] = entry;//atomic
      m_Index++;
      if (m_Index>=m_Buffer.Length)
        m_Index = 0;
    }
  }
}
