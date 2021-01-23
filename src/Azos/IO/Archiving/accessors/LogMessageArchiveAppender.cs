/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Azos.Log;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;
using Azos.Time;

namespace Azos.IO.Archiving
{
  public sealed class LogMessageArchiveAppender : ArchiveAppender<Message>
  {
    public LogMessageArchiveAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<Message, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit)
    {
      m_Stream = new MemoryStream();
      m_Writer = new BixWriter(m_Stream);
    }

    private MemoryStream m_Stream;
    private BixWriter m_Writer;

    protected override ArraySegment<byte> DoSerialize(Message entry)
    {
      m_Stream.SetLength(0);

      if (entry == null)
      {
        m_Writer.Write(false); //NULL
      }
      else
      {
        m_Writer.Write(true); // NON-NULL

        m_Writer.Write(entry.Gdid);
        m_Writer.Write(entry.Guid);
        m_Writer.Write(entry.RelatedTo);
        m_Writer.Write(entry.Channel);
        m_Writer.Write(entry.App);
        m_Writer.Write((int)entry.Type);
        m_Writer.Write(entry.Source);
        m_Writer.Write(entry.UTCTimeStamp);

        m_Writer.Write(entry.Host);
        m_Writer.Write(entry.From);
        m_Writer.Write(entry.Topic);
        m_Writer.Write(entry.Text);
        m_Writer.Write(entry.Parameters);
        m_Writer.Write(entry.ArchiveDimensions);

        string edata = null;
        if (entry.ExceptionData != null)
        {
          edata = JsonWriter.Write(entry.ExceptionData, JsonWritingOptions.CompactRowsAsMap);
        }
        m_Writer.Write(edata);
      }

      return new ArraySegment<byte>(m_Stream.GetBuffer(), 0, (int)m_Stream.Length);
    }
  }
}
