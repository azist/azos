/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Text;
using System.IO;

namespace Azos.CodeAnalysis.Source
{
#warning AZ #731 Needs `ChunkEof` for async processing so we can `await streamSource.GetNextChunkAsync(1024)`
  /// <summary>
  /// Represents source code stored in a stream
  /// </summary>
  public class XStreamSource : StreamReader, ISourceText
  {
    /// <summary>
    /// Constructs stream source with specified language and default encoding
    /// </summary>
    public StreamSource(Stream stream, Language language, string name = null)
      : base(stream, Encoding.UTF8, true, 1024, leaveOpen: true)//#837
    {
      m_Language = language;
      m_Name = name;
    }

    /// <summary>
    /// Constructs stream source with specified language and encoding
    /// </summary>
    public StreamSource(Stream stream, Encoding encoding, Language language, string name = null)
      : base(stream, encoding, true, 1024, leaveOpen: true)//#837
    {
      m_Language = language;
      m_Name = name;
    }

    private Language m_Language;
    private string m_Name;

    public void Reset()
    {
      BaseStream.Position = 0;
      DiscardBufferedData();
    }

    /// <summary>
    /// Returns source's name
    /// </summary>
    public string Name => m_Name ?? string.Empty;

    public bool EOF => EndOfStream;

    public char ReadChar() => (char)Read();

    public char PeekChar() => (char)Peek();

    public Language Language => m_Language ?? UnspecifiedLanguage.Instance;

  }
}
