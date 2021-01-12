/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Represents a "page" in an archive data stream. Archives can only be appended to.
  /// A page is an atomic unit of reading from and appending to archives.
  /// Pages represent binary raw content present in RAM.
  /// Pages are created by Reader/Appender and populated by `ArchiveDataAccessor`. Pages are appended to archives
  /// using `ArchiveDataAccessor.AppendPage(page)`;
  /// Page instances are NOT thread-safe: any parallel/concurrent operation shall get
  /// a different instance via a corresponding call to Reader/Appender
  /// </summary>
  public sealed class Page
  {
    public enum Status { Invalid, Preloading, Reading, Writing }

    internal Page(int defaultCapacity)
    {
      m_DefaultCapacity = defaultCapacity.KeepBetween(1024, Format.PAGE_MAX_LEN);
      m_Raw = new MemoryStream(m_DefaultCapacity);
    }

    internal void CreateNew(IApplication app)
    {
      m_Raw.Position = 0;
      m_State = Status.Writing;
      m_CreateUtc = app.TimeSource.UTCNow;
      m_CreateApp = app.AppId;
      m_CreateHost = Platform.Computer.HostName;
    }


    internal MemoryStream BeginReading(long pageId)
    {
      m_PageId = pageId;
      if (m_Raw.Capacity > m_DefaultCapacity)
      {
        m_Raw = new MemoryStream(m_DefaultCapacity);
      }
      else
      {
        m_Raw.Position = 0;
        m_Raw.SetLength(0);
      }
      m_State = Status.Preloading;
      return m_Raw;
    }

    internal void EndReading(DateTime utcCreate, Atom app, string host)
    {
      ensure(Status.Preloading);
      m_Raw.Position = 0;
      m_State = Status.Reading;
      m_CreateUtc = utcCreate;
      m_CreateApp = app;
      m_CreateHost = host;
    }



    private int m_DefaultCapacity;
    private Status m_State;
    private long m_PageId;
    private DateTime m_CreateUtc;
    private string m_CreateHost;
    private Atom m_CreateApp;

    private MemoryStream m_Raw; //the stream contains raw  <entry-stream> without page headers etc...



    public int DefaultCapacity => m_DefaultCapacity;

    /// <summary>
    /// Returns the current state of the page instance
    /// </summary>
    public Status State => m_State;

    /// <summary>
    /// Walks all raw entries. This is purely in-memory operation
    /// </summary>
    public IEnumerable<Entry> Entries
    {
      get
      {
        ensure(Status.Reading);
        var buffer = m_Raw.GetBuffer();
        for (var adr = 0; adr < m_Raw.Length;)
        {
          var entry = get(buffer, ref adr);
          yield return entry;
          if (entry.State==Entry.Status.EOF) yield break;
          if (entry.State < 0) yield break;//any error
        }
      }
    }

    /// <summary>
    /// Returns an entry by its address, if address is invalid then invalid entry is returned
    /// </summary>
    public Entry this[int address]
    {
      get
      {
        ensure(Status.Reading);
        var buffer = m_Raw.GetBuffer();
        return get(buffer, ref address);
      }
    }

    private void ensure(Status need)
    {
      if (m_State != need) throw new ArchivingException(StringConsts.ARCHIVE_PAGE_STATE_ERROR.Args(m_PageId, need));
    }

    private Entry get(byte[] buffer, ref int address)
    {
      var ptr = address;
      if (address < 0 || address + Format.ENTRY_MIN_LEN >= m_Raw.Length) return new Entry(ptr, Entry.Status.BadAddress);

      var h1 = buffer[address];
      if (h1 == Format.ENTRY_HEADER_1 && buffer[++address] == Format.ENTRY_HEADER_2) // @>
      {
        var len = 0;//buffer.ReadVarLen32(ref address);//todo use varbit encoding 1-5 bytes
        //check max length

        if (len == 0 || len > Format.ENTRY_MAX_LEN) return new Entry(ptr, Entry.Status.InvalidLength);//max length exceeded

        var start = address;
        var end = address + len;
        if (end >= m_Raw.Length) return new Entry(ptr, Entry.Status.InvalidLength);//beyond the block

        return new Entry(ptr, new ArraySegment<byte>(buffer, start, len));//VALID!!!
      }
      else if (h1 == Format.ENTRY_HEADER_EOF_1 && buffer[++address] == Format.ENTRY_HEADER_EOF_2)
      {
        return new Entry(ptr, Entry.Status.EOF);//EOF
      }
      else return new Entry(ptr, Entry.Status.BadHeader);//corruption
    }

  }
}

