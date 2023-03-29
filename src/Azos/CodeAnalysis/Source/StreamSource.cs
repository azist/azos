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
  /// which are then synchronously processed for performance.
  /// <para>
  /// This is a hybrid processing model which brings benefits of sync and async processing.
  /// This class is built for convenience and performance, as it is used by some deserializers (e.g. JSON)
  /// it must be able to efficiently process large bodies of source text, for example supplied via a network stream
  /// asynchronously. A naive async implementation with async `Read/PeekChar` would have been very inefficient, therefore
  /// this class provides a synchronous character-by-character read interface which is fed from internal memory segments
  /// which are pre-fetched asynchronously, therefore a large source input is still processed asynchronously in segments, each
  /// processed synchronously one-after another.
  /// </para>
  /// </summary>
  /// <remarks>
  /// <para>
  /// The caller inspects boolean <see cref="NearEndOfSegment"/> to trigger async call to <see cref="FetchSegmentAsync"/>
  /// while the sync caller may not have ended reading a current segment which was previously fetched,
  /// this way it is possible to consume source char-by-char synchronously without any extra allocations
  /// and overhead associated with async processing, while synchronously (efficiently) looking at <see cref="NearEndOfSegment"/>
  /// property and triggering an asynchronous prefetch of the next segment which does not happen for every character.
  /// </para>
  /// <para>
  /// The <see cref="NearEndOfSegment"/> is basically a speculative property which returns true as soon as segment read index
  /// approaches the end of the segment as dictated by % margin near the segment end.
  /// It is possible that a sync operation may need to read more than what was fetched in which case it will trigger
  /// a blocking sync call on the async <see cref="FetchSegmentAsync"/>, however statistically this is a rare case.
  /// </para>
  /// </remarks>
  public class StreamSource : DisposableObject, ISourceText
  {
    public const int MIN_SEG_TAIL_THRESHOLD = 64;
    public const float MAX_SEG_TAIL_THRESHOLD_PCT = 0.532f;
    public const int MIN_BUFFER_SIZE =   1 * 1024;
    public const int MAX_BUFFER_SIZE = 512 * 1024;

    public const int DEFAULT_BUFFER_SIZE = 32 * 1024;
    public const int DEFAULT_SEG_TAIL_THRESHOLD = 735;

    private static readonly ArrayPool<byte> s_BytePool;
    private static readonly ArrayPool<char> s_CharPool;

    static StreamSource()
    {
      s_BytePool = ArrayPool<byte>.Create(MAX_BUFFER_SIZE, 16);
      s_CharPool = ArrayPool<char>.Create(32/*reserved*/ + (2 * MAX_BUFFER_SIZE), 16);
    }

    protected StreamSource(){ }

    /// <summary>
    /// Constructs stream source with specified language and encoding
    /// </summary>
    public StreamSource(Stream stream, Encoding encoding, bool useBom, Language language, string name = null, int bufferSize = 0, int segmentTailThreshold = 0, bool sensitiveData = false)
     => ctor(stream, encoding, useBom, language, name, bufferSize, segmentTailThreshold, sensitiveData);

    protected void ctor(Stream stream, Encoding encoding, bool useBom, Language language, string name, int bufferSize, int segmentTailThreshold, bool sensitiveData)
    {
      m_Stream = stream.NonDisposed(nameof(stream));
      m_UseBom = useBom;
      m_Encoding = encoding;//no default here
      m_Decoder  = null;

      m_Language = language ?? UnspecifiedLanguage.Instance;
      m_Name = name ?? "<noname>";

      if (bufferSize <= 0) bufferSize = DEFAULT_BUFFER_SIZE;
      if (segmentTailThreshold <= 0) segmentTailThreshold = DEFAULT_SEG_TAIL_THRESHOLD;

      m_BufferSize = bufferSize.KeepBetween(MIN_BUFFER_SIZE, MAX_BUFFER_SIZE);
      m_SegmentTailThreshold = segmentTailThreshold.KeepBetween(MIN_SEG_TAIL_THRESHOLD, (int)(m_BufferSize * MAX_SEG_TAIL_THRESHOLD_PCT));

      m_Buffer = s_BytePool.Rent(m_BufferSize);
      m_Arena = s_CharPool.Rent(32/*reserved*/ + (2 * m_BufferSize));
      m_Segment1 = new ArraySegment<char>(m_Arena, 0, 0);
      m_Segment2 = new ArraySegment<char>(m_Arena, m_BufferSize, 0);
      m_SensitiveData = sensitiveData;
    }

    protected override void Destructor()
    {
      s_BytePool.Return(m_Buffer, m_SensitiveData);
      s_CharPool.Return(m_Arena, m_SensitiveData);
      base.Destructor();
    }

    private Stream m_Stream;
    private bool m_UseBom;
    private Encoding m_Encoding;
    private Decoder m_Decoder;
    private Language m_Language;
    private string m_Name;

    private int m_SegmentCount;
    private int m_SegmentPosition;
    private int m_BufferSize;
    private int m_SegmentTailThreshold;
    private bool m_StreamEof;

    private bool m_SensitiveData;
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
    public Encoding Encoding   => m_Encoding;
    public bool     EOF        => m_StreamEof && m_SegmentPosition >= m_Segment1.Count + m_Segment2.Count;
    public int      BufferSize => m_BufferSize;
    public int      SegmentTailThreshold => m_SegmentTailThreshold;
    public int      SegmentLength    => currentSegment.Count;
    public int      SegmentPosition  => m_SegmentPosition;
    public int      SegmentCount     => m_SegmentCount;
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
      EnsureObjectNotDisposed();
      if (m_StreamEof) return;
      var segment = standbySegment;
      if (segment.Count > 0) return;//already pre-fetched, nothing to do now

      var idxBuffer = 0;
      var totalBuffered = 0;
      while(totalBuffered < m_BufferSize && !ctk.IsCancellationRequested)
      {
        var got = await m_Stream.ReadAsync(m_Buffer, totalBuffered, m_BufferSize - totalBuffered, ctk).ConfigureAwait(false);

        if (got==0) //eof
        {
          m_StreamEof = true;
          break;
        }
        totalBuffered += got;
      }
      if (totalBuffered==0) return;//EOF (and end of stream)

      ensureDecoder(ref idxBuffer);

      //https://learn.microsoft.com/en-us/dotnet/api/system.text.decoder.getchars?view=net-7.0#system-text-decoder-getchars(system-byte()-system-int32-system-int32-system-char()-system-int32-system-boolean)
      //var gotc = m_Encoding.GetChars(m_Buffer, 0, total, segment.Array, segment.Offset);
      // MUST Use decoder, not encoding.GetChars() because decoder is stateful between calls
      var gotc = m_Decoder.GetChars(m_Buffer, idxBuffer, totalBuffered - idxBuffer, segment.Array, segment.Offset, flush: m_StreamEof);

      if (m_SegIdx)//populate standby(the opposite of m_SegIdx)
      {
        m_Segment1 = new ArraySegment<char>(m_Arena, 0, gotc);
      }
      else
      {
        m_Segment2 = new ArraySegment<char>(m_Arena, m_BufferSize, gotc);
      }

      m_SegmentCount++;
    }
    #endregion

    #region .pvt

    internal static readonly Encoding ENC_UTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true);
    internal static readonly Encoding ENC_UTF32_LE = new UTF32Encoding(bigEndian: false,   byteOrderMark: true, throwOnInvalidCharacters: true);
    internal static readonly Encoding ENC_UTF32_BE = new UTF32Encoding(bigEndian: true,    byteOrderMark: true, throwOnInvalidCharacters: true);
    internal static readonly Encoding ENC_UTF16_LE = new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true);
    internal static readonly Encoding ENC_UTF16_BE = new UnicodeEncoding(bigEndian: true,  byteOrderMark: true, throwOnInvalidBytes: true);

    private void ensureDecoder(ref int idx)
    {
      if (m_Decoder != null) return;

      //Detect BOM here
      if (m_UseBom)//spell out BOM cases
      {
        Encoding bomEncoding = null;

        if (m_Buffer[idx] == 0xEF && m_Buffer[idx + 1] == 0xBB && m_Buffer[idx + 2] == 0xBF)//UTF8
        {
          bomEncoding = ENC_UTF8;
          idx += 3;
        }
        else if (m_Buffer[idx] == 0xFF && m_Buffer[idx + 1] == 0xFE && m_Buffer[idx + 2] == 0x00 && m_Buffer[idx + 3] == 0x00)//UTF-32 little endian byte order: FF FE 00 00
        {
          bomEncoding = ENC_UTF32_LE;
          idx += 4;
        }
        else if (m_Buffer[idx] == 0x00 && m_Buffer[idx + 1] == 0x00 && m_Buffer[idx + 2] == 0xFE && m_Buffer[idx + 3] == 0xFF)//UTF-32 big endian byte order: 00 00 FE FF
        {
          bomEncoding = ENC_UTF32_BE;
          idx += 4;
        }
        else if (m_Buffer[idx] == 0xFE && m_Buffer[idx + 1] == 0xFF)//UTF-16 big endian byte order: FE FF
        {
          bomEncoding = ENC_UTF16_BE;
          idx += 2;
        }
        else if (m_Buffer[idx] == 0xFF && m_Buffer[idx + 1] == 0xFE)//UTF-16 little endian byte order: FF FE
        {
          bomEncoding = ENC_UTF16_LE;
          idx += 2;
        }

        //===============================
        if (bomEncoding != null)
        {
          if (m_Encoding == null)
          {
            m_Encoding = bomEncoding;
          }
          else//was specified
          {
            if (m_Encoding.GetType() != bomEncoding.GetType() ||
                !m_Encoding.Preamble.SequenceEqual(bomEncoding.Preamble)) throw new SourceTextException("BOM/preamble mismatch: content is `{0}` but `{1}` is expected".Args(bomEncoding.GetType().Name, m_Encoding.GetType().Name));
          }
        }
      }

      if (m_Encoding == null) m_Encoding = ENC_UTF8;//by default

      m_Decoder = m_Encoding.GetDecoder();
    }


    private bool prepData()
    {
      if (this._____getDisposeState() != STATE_ALIVE) return false;

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
