/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Azos.Data;
using Azos.Serialization.Bix;
using Azos.Time;

namespace Azos.IO.Archiving
{

  public struct StringBookmark
  {
    public StringBookmark(string value, Bookmark bm)
    {
      Value = value;
      Bookmark = bm;
    }

    public readonly string Value;
    public readonly Bookmark Bookmark;
  }


  public sealed class StringIdxAppender : ArchiveAppender<StringBookmark>
  {
    public StringIdxAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<StringBookmark, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit)
    {
      m_Stream = new MemoryStream();
      m_Writer = new BixWriter(m_Stream);
    }

    private MemoryStream m_Stream;
    private BixWriter m_Writer;

    protected override ArraySegment<byte> DoSerialize(StringBookmark entry)
    {
      m_Stream.SetLength(0);
      m_Writer.Write(entry.Value);
      m_Writer.Write(entry.Bookmark.PageId);
      m_Writer.Write(entry.Bookmark.Address);
      return new ArraySegment<byte>(m_Stream.GetBuffer(), 0, (int)m_Stream.Length);
    }
  }

  public sealed class StringIdxReader : ArchiveBixReader<StringBookmark>
  {
    public StringIdxReader(IVolume volume) : base(volume){ }

    public override StringBookmark MaterializeBix(BixReader reader)
    {
      var v = reader.ReadString();
      var pid = reader.ReadLong();
      var adr = reader.ReadInt();
      return new StringBookmark(v, new Bookmark(pid, adr));
    }
  }




  public struct GuidBookmark
  {
    public GuidBookmark(Guid value, Bookmark bm)
    {
      Value = value;
      Bookmark = bm;
    }

    public readonly Guid Value;
    public readonly Bookmark Bookmark;
  }


  public sealed class GuidIdxAppender : ArchiveAppender<GuidBookmark>
  {
    public GuidIdxAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<GuidBookmark, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit)
    {
      m_Stream = new MemoryStream();
      m_Writer = new BixWriter(m_Stream);
    }

    private MemoryStream m_Stream;
    private BixWriter m_Writer;

    protected override ArraySegment<byte> DoSerialize(GuidBookmark entry)
    {
      m_Stream.SetLength(0);
      m_Writer.Write(entry.Value);
      m_Writer.Write(entry.Bookmark.PageId);
      m_Writer.Write(entry.Bookmark.Address);
      return new ArraySegment<byte>(m_Stream.GetBuffer(), 0, (int)m_Stream.Length);
    }
  }


  public struct GdidBookmark
  {
    public GdidBookmark(GDID value, Bookmark bm)
    {
      Value = value;
      Bookmark = bm;
    }

    public readonly GDID Value;
    public readonly Bookmark Bookmark;
  }


  public sealed class GdidIdxAppender : ArchiveAppender<GdidBookmark>
  {
    public GdidIdxAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<GdidBookmark, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit)
    {
      m_Stream = new MemoryStream();
      m_Writer = new BixWriter(m_Stream);
    }

    private MemoryStream m_Stream;
    private BixWriter m_Writer;

    protected override ArraySegment<byte> DoSerialize(GdidBookmark entry)
    {
      m_Stream.SetLength(0);
      m_Writer.Write(entry.Value);
      m_Writer.Write(entry.Bookmark.PageId);
      m_Writer.Write(entry.Bookmark.Address);
      return new ArraySegment<byte>(m_Stream.GetBuffer(), 0, (int)m_Stream.Length);
    }
  }





  public struct DateTimeBookmark
  {
    public DateTimeBookmark(DateTime value, Bookmark bm)
    {
      Value = value;
      Bookmark = bm;
    }

    public readonly DateTime Value;
    public readonly Bookmark Bookmark;
  }


  public sealed class DateTimeIdxAppender : ArchiveAppender<DateTimeBookmark>
  {
    public DateTimeIdxAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<DateTimeBookmark, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit)
    {
      m_Stream = new MemoryStream();
      m_Writer = new BixWriter(m_Stream);
    }

    private MemoryStream m_Stream;
    private BixWriter m_Writer;

    protected override ArraySegment<byte> DoSerialize(DateTimeBookmark entry)
    {
      m_Stream.SetLength(0);
      m_Writer.Write(entry.Value);
      m_Writer.Write(entry.Bookmark.PageId);
      m_Writer.Write(entry.Bookmark.Address);
      return new ArraySegment<byte>(m_Stream.GetBuffer(), 0, (int)m_Stream.Length);
    }
  }



  public struct LongBookmark
  {
    public LongBookmark(long value, Bookmark bm)
    {
      Value = value;
      Bookmark = bm;
    }

    public readonly long Value;
    public readonly Bookmark Bookmark;
  }


  public sealed class LongIdxAppender : ArchiveAppender<LongBookmark>
  {
    public LongIdxAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<LongBookmark, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit)
    {
      m_Stream = new MemoryStream();
      m_Writer = new BixWriter(m_Stream);
    }

    private MemoryStream m_Stream;
    private BixWriter m_Writer;

    protected override ArraySegment<byte> DoSerialize(LongBookmark entry)
    {
      m_Stream.SetLength(0);
      m_Writer.Write(entry.Value);
      m_Writer.Write(entry.Bookmark.PageId);
      m_Writer.Write(entry.Bookmark.Address);
      return new ArraySegment<byte>(m_Stream.GetBuffer(), 0, (int)m_Stream.Length);
    }
  }





}
