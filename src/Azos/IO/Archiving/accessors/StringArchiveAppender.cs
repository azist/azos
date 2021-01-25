/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Azos.Serialization.Bix;
using Azos.Time;

namespace Azos.IO.Archiving
{
  public sealed class StringArchiveAppender : ArchiveAppender<string>
  {
    public StringArchiveAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<string, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit)
    {
      m_Stream = new MemoryStream();
      m_Writer = new BixWriter(m_Stream);
    }

    private MemoryStream m_Stream;
    private BixWriter m_Writer;

    protected override ArraySegment<byte> DoSerialize(string entry)
    {
       m_Stream.SetLength(0);
       m_Writer.Write(entry);
       return new ArraySegment<byte>(m_Stream.GetBuffer(), 0, (int)m_Stream.Length);
    }
  }
}
