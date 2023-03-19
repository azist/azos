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
    public const int MAX_BUFFER_SIZE = 256 * 1024;

    private static readonly ArrayPool<byte> s_BytePool;
    private static readonly ArrayPool<char> s_CharPool;

    static StreamSource()
    {
      s_BytePool = ArrayPool<byte>.Create(MAX_BUFFER_SIZE, 16);
      s_CharPool = ArrayPool<char>.Create(MAX_BUFFER_SIZE, 16);
    }

    /// <summary>
    /// Constructs stream source with specified language and encoding
    /// </summary>
    public StreamSource(Stream stream, Encoding encoding, Language language, string name = null)
    {
      m_Stream = stream.NonDisposed(nameof(stream));
      m_Language = language ?? UnspecifiedLanguage.Instance;
      m_Encoding = encoding ?? Encoding.UTF8;
      m_Name = name ?? Guid.NewGuid().ToString();

      m_Buffer = s_BytePool.Rent(4 * 1024);
      m_Segment1 = s_CharPool.Rent(m_Buffer.Length);
      m_Segment2 = s_CharPool.Rent(m_Buffer.Length);
      m_Segment = m_Segment1;
    }

    protected override void Destructor()
    {
      s_BytePool.Return(m_Buffer);
      s_CharPool.Return(m_Segment1.Array);
      s_CharPool.Return(m_Segment2.Array);
      base.Destructor();
    }

    private Stream m_Stream;
    private Encoding m_Encoding;
    private Language m_Language;
    private string m_Name;

    private int m_SegmentLength;
    private int m_SegmentPosition;
    private int m_SegmentTailThreshold;
    private bool m_StreamEof;


    private byte[] m_Buffer;
    private ArraySegment<char> m_Segment;
    private ArraySegment<char> m_Segment1;
    private ArraySegment<char> m_Segment2;

    #region ISourceText Members
    public string Name => m_Name;
    public Language Language => m_Language;
    public bool EOF => m_StreamEof && m_SegmentPosition >= m_SegmentLength;
    public int BufferSize => m_Buffer.Length;
    public int SegmentLength => m_SegmentLength;
    public int SegmentPosition => m_SegmentPosition;
    public bool NearEndOfSegment => (m_SegmentLength - m_SegmentPosition) < m_SegmentTailThreshold;
    public bool IsLastSegment => m_StreamEof;


    public char ReadChar()
    {
      if (!prepData()) return (char)0;
      return m_Segment[m_SegmentPosition++];
    }

    public char PeekChar()
    {
      if (!prepData()) return (char)0;
      return m_Segment[m_SegmentPosition];
    }

    public async Task FetchSegmentAsync()
    {
      ArraySegment<char> segment;
      if (m_Segment1.Count == 0) segment = m_Segment1;
      else if(m_Segment2.Count == 0) segment = m_Segment2;
      else return;//all segments are pre-fetched

      var got = await m_Stream.ReadAsync(m_Buffer, 0, m_Buffer.Length).ConfigureAwait(false);

      if (got==0)//eof
      {
        m_StreamEof = true;
        return;
      }

      var gotc = m_Encoding.GetChars(m_Buffer, 0, got, segment.Array, 0);
      m_Segment = new ArraySegment<char>(segment.Array, 0, gotc);
      m_SegmentPosition = 0;
    }
    #endregion

    #region .pvt
    private bool prepData()
    {
      var segmentEof = m_SegmentPosition >= m_SegmentLength;
      if (segmentEof)
      {
        if (m_StreamEof) return false;//real EOF
        FetchSegmentAsync().Await();//try to fetch more from the underlying stream
        segmentEof = m_SegmentPosition >= m_SegmentLength;
        if (segmentEof) return false;//nothing extra was fetched
      }
      return true;
    }
    #endregion
  }
}
