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
  /// Represents source code stored in a stream which can be asynchronously read in segments
  /// </summary>
  public class StreamSource : DisposableObject, ISourceText
  {
    /// <summary>
    /// Constructs stream source with specified language and encoding
    /// </summary>
    public StreamSource(Stream stream, Encoding encoding, Language language, string name = null)
    {
      m_Stream = stream.NonDisposed(nameof(stream));
      m_Language = language ?? UnspecifiedLanguage.Instance;
      m_Encoding = encoding ?? Encoding.UTF8;
      m_Name = name ?? Guid.NewGuid().ToString();

      m_Buffer = System.Buffers.MemoryPool<char>.Shared.Rent(1024).Memory;
    }

    private Stream m_Stream;
    private Encoding m_Encoding;
    private Language m_Language;
    private string m_Name;

    private int m_SegmentLength;
    private int m_SegmentPosition;
    private int m_SegmentTailThreshold;
    private Memory<char> m_Buffer;
    private bool m_StreamEof;

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
      return m_Buffer.Span[m_SegmentPosition++];
    }

    public char PeekChar()
    {
      if (!prepData()) return (char)0;
      return m_Buffer[m_SegmentPosition];
    }

    public async Task FetchSegmentAsync()
    {
      using var bin = MemoryPool<byte>.Shared.Rent(1024);
      var got = await m_Stream.ReadAsync(bin.Memory).ConfigureAwait(false);

      if (got==0)//eof
      {
        m_StreamEof = true;
        return;
      }

      var gotc = m_Encoding.GetChars(bin.Memory.Slice(0, got).Span, new Span<char>());

     // m_Encoding.GetMaxByteCount(charCount);

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
