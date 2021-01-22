/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Text;

using Azos.Serialization.JSON;
using Azos.Time;

namespace Azos.IO.Archiving
{

  /// <summary>
  /// Appends objects encoded in JSON format
  /// </summary>
  public sealed class JsonArchiveAppender : ArchiveAppender<object>
  {
    public static readonly Encoding ENCODING = new UTF8Encoding(false, false);

    public JsonArchiveAppender(IVolume volume,
                               ITimeSource time,
                               Atom app,
                               string host,
                               JsonWritingOptions options = null,
                               Action<object, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit)
    {
      m_Stream = new MemoryStream();
      m_Writer = new StreamWriter(m_Stream, ENCODING, 4096, true);
      m_Options = options ?? JsonWritingOptions.CompactRowsAsMap;
    }

    private MemoryStream m_Stream;
    private TextWriter m_Writer;
    private JsonWritingOptions m_Options;

    protected override ArraySegment<byte> DoSerialize(object entry)
    {
      m_Stream.SetLength(0);
      JsonWriter.Write(entry, m_Writer, m_Options);  //todo Rewrite without copies
      m_Writer.Flush();
      return new ArraySegment<byte>(m_Stream.GetBuffer(), 0, (int)m_Stream.Length);
    }
  }
}
