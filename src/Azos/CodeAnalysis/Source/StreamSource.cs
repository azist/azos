/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Text;
using System.IO;
using System.Buffers;
using System.Threading.Tasks;

namespace Azos.CodeAnalysis.Source
{
  /// <summary>
  /// Represents a textual source stored in a stream which can be asynchronously read in segments
  /// which are then synchronously processed for performance. <br /><br/>
  /// This is a hybrid processing model which brings benefits of sync and async processing.
  /// This class is built for convenience and performance, as it is used by some deserializers (e.g. JSON)
  /// it must be able to efficiently process large bodies of source text, for example supplied via a network stream
  /// asynchronously. A naive async implementation with async `Read/PeekChar` would have been very inefficient, therefore
  /// this class provides a synchronous character-by-character read interface which is fed from internal memory segments
  /// which are pre-fetched asynchronously, therefore a large source input is still processed asynchronously in segments, each
  /// processed synchronously one-after another.
  /// </summary>
  /// <remarks>
  /// The caller inspects boolean <see cref="NearEndOfSegment"/> to trigger async call to <see cref="FetchSegmentAsync"/>
  /// while the sync caller may not have ended reading a current segment which was previously fetched,
  /// this way it is possible to consume source char-by-char synchronously without any extra allocations
  /// and overhead associated with async processing, while synchronously (efficiently) looking at <see cref="NearEndOfSegment"/>
  /// property and triggering an asynchronous prefetch of the next segment which does not happen for every character.
  /// <br/><br/>
  /// The <see cref="NearEndOfSegment"/> is basically a speculative property which returns true as soon as segment read index
  /// approaches the end of the segment as dictated by % margin near the segment end.
  /// It is possible that a sync operation may need to read more than what was fetched in which case it will trigger
  /// a blocking sync call on the async <see cref="FetchSegmentAsync"/>, however statistically this is a rare case.
  /// </remarks>
  public class StreamSource : DisposableObject, ISourceText
  {
    public const int MIN_SEG_TAIL_THRESHOLD = 64;
    public const float MAX_SEG_TAIL_THRESHOLD_PCT = 0.532f;
    public const int MIN_BUFFER_SIZE =   1 * 1024;
    public const int MAX_BUFFER_SIZE = 512 * 1024;

    private static readonly ArrayPool<byte> s_BytePool;
    private static readonly ArrayPool<char> s_CharPool;

    static StreamSource()
    {
      s_BytePool = ArrayPool<byte>.Create(MAX_BUFFER_SIZE, 16);
      s_CharPool = ArrayPool<char>.Create(2 * MAX_BUFFER_SIZE, 16);
    }

    /// <summary>
    /// Constructs stream source with specified language and encoding
    /// </summary>
    public StreamSource(Stream stream, Encoding encoding, Language language, string name = null, int bufferSize = 0, int segmentTailThreshold = 0)
    {
      m_Stream = stream.NonDisposed(nameof(stream));
      m_Language = language ?? UnspecifiedLanguage.Instance;
      m_Encoding = encoding ?? Encoding.UTF8;
      m_Name = name ?? "<noname>";

      m_BufferSize = bufferSize.KeepBetween(MIN_BUFFER_SIZE, MAX_BUFFER_SIZE);
      m_SegmentTailThreshold = segmentTailThreshold.KeepBetween(MIN_SEG_TAIL_THRESHOLD, (int)(m_BufferSize * MAX_SEG_TAIL_THRESHOLD_PCT));
      m_Buffer = s_BytePool.Rent(m_BufferSize);
      m_Arena = s_CharPool.Rent(2 * m_BufferSize);
      m_Segment1 = new ArraySegment<char>(m_Arena, 0, 0);
      m_Segment2 = new ArraySegment<char>(m_Arena, m_BufferSize, 0);
    }

    protected override void Destructor()
    {
      s_BytePool.Return(m_Buffer);
      s_CharPool.Return(m_Arena);
      base.Destructor();
    }

    private Stream m_Stream;
    private Encoding m_Encoding;
    private Language m_Language;
    private string m_Name;

    private int m_SegmentPosition;
    private int m_BufferSize;
    private int m_SegmentTailThreshold;
    private bool m_StreamEof;

    private byte[] m_Buffer;
    private char[] m_Arena;
    private bool m_SegIdx;
    private ArraySegment<char> m_Segment1;
    private ArraySegment<char> m_Segment2;

    private ArraySegment<char> currentSegment => m_SegIdx ? m_Segment2 : m_Segment1;
    private ArraySegment<char> standbySegment => m_SegIdx ? m_Segment1 : m_Segment2;


    #region ISourceText Members

    public string   Name       => m_Name;
    public Language Language   => m_Language;
    public bool     EOF        => m_StreamEof && m_SegmentPosition >= m_Segment1.Count + m_Segment2.Count;
    public int      BufferSize => m_BufferSize;
    public int      SegmentLength    => currentSegment.Count;
    public int      SegmentPosition  => m_SegmentPosition;
    public bool     NearEndOfSegment => ((m_Segment1.Count + m_Segment2.Count) - m_SegmentPosition) < m_SegmentTailThreshold;
    public bool     IsLastSegment    => m_StreamEof && standbySegment.Count == 0;

    public char ReadChar()
    {
      if (!prepData()) return (char)0;
      return currentSegment[m_SegmentPosition++];
    }

    public char PeekChar()
    {
      if (!prepData()) return (char)0;
      return currentSegment[m_SegmentPosition];
    }

    public async Task FetchSegmentAsync(System.Threading.CancellationToken ctk = default)
    {
      if (m_StreamEof) return;
      var segment = standbySegment;
      if (segment.Count > 0) return;//already pre-fetched, nothing to do now

      var total = 0;
      while(total < m_BufferSize && !ctk.IsCancellationRequested)
      {
        var got = await m_Stream.ReadAsync(m_Buffer, total, m_BufferSize - total, ctk).ConfigureAwait(false);

        if (got==0) //eof
        {
          m_StreamEof = true;
          break;
        }
        total += got;
      }
      if (total==0) return;//EOF (and end of stream)

      var gotc = m_Encoding.GetChars(m_Buffer, 0, total, segment.Array, segment.Offset);

      if (m_SegIdx)//populate standby(the opposite of m_SegIdx)
      {
        m_Segment1 = new ArraySegment<char>(m_Arena, 0, gotc);
      }
      else
      {
        m_Segment2 = new ArraySegment<char>(m_Arena, m_BufferSize, gotc);
      }
    }
    #endregion

    #region .pvt
    private bool prepData()
    {
      var segmentEof = m_SegmentPosition >= currentSegment.Count;
      if (!segmentEof) return true;

      //try to switch to standby segment
      if (switchSegments()) return true;

      if (m_StreamEof) return false;//final EOF

      FetchSegmentAsync().Await();//try to fetch more from the underlying stream into standby segment

      //try to switch to standby segment after fetch
      return switchSegments();
    }

    private bool switchSegments()
    {
      if (m_SegIdx)
      {
        m_Segment2 = new ArraySegment<char>(m_Segment2.Array, m_BufferSize, 0);
        m_SegIdx = false;
        m_SegmentPosition = 0;
        return m_Segment1.Count > 0;
      }
      else
      {
        m_Segment1 = new ArraySegment<char>(m_Segment1.Array, 0, 0);
        m_SegIdx = true;
        m_SegmentPosition = 0;
        return m_Segment2.Count > 0;
      }
    }

    #endregion
  }
}
