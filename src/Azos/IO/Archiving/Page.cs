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
    public enum Status { Unset, Loading, Reading, EOF, Writing, Written }

    internal Page(int defaultCapacity)
    {
      m_DefaultCapacity = defaultCapacity.KeepBetween(1024, Format.PAGE_MAX_LEN);
      m_Raw = new MemoryStream(m_DefaultCapacity);
    }

    /*
      NOTE Re: design choice for reuse of Page instances

      Keep in mind, that archive reading/scrolling is not necessarily always going to materialize Entry objects.
      You might just get the binary page data and analyze it in place without a higher-level read accessor, consequently
      scrolling thorough a page may be a completely allocation-free procedure, therefore
      it is important to NOT make any extra allocations, more so in a small LOH range which will load GC.

      By avoiding Page instance allocation we can reuse the underlying memory chunk which otherwise would have been trashed
      with every page fetch.
    */


    /// <summary>
    /// Initializes the page instance for writing.
    /// After this call you can call `Append()` and finalize the append with
    /// the call to `EndWriting()`
    /// </summary>
    internal void BeginWriting(long pageId, DateTime utcCreate, Atom app, string host)
    {
      m_PageId = pageId;
      m_Raw.SetLength(0);
      m_State = Status.Writing;
      m_CreateUtc = utcCreate;
      m_CreateApp = app;
      m_CreateHost = host;
    }

    public void EndWriting()
    {
      Ensure(Status.Writing);
      m_Raw.WriteByte(Format.ENTRY_HEADER_EOF_1);
      m_Raw.WriteByte(Format.ENTRY_HEADER_EOF_2);

      //varbit entry header padding
      m_Raw.WriteByte(0xAA);
      m_Raw.WriteByte(0xAA);
      m_Raw.WriteByte(0xAA);

      m_State = Status.Written;
    }

    /// <summary>
    /// Returns the stream which the content should be loaded into.
    /// The stream is reset to pos/len zero.
    /// The accessor first calls `BeginRead(): MemoryStream`, the returned stream is filled with data
    /// (e.g. from a compressed/encrypted source, then the accessor calls `EndReading()` to indicate
    /// that the Page is ready to be enumerated
    /// </summary>
    /// <param name="pageId">The requested pageId</param>
    internal MemoryStream BeginReading(long pageId)
    {
      m_PageId = pageId;
      if (m_Raw.Capacity > m_DefaultCapacity)
      {
        m_Raw = new MemoryStream(m_DefaultCapacity);
      }
      else
      {
        m_Raw.SetLength(0);
      }
      m_State = Status.Loading;
      return m_Raw;
    }

    /// <summary>
    /// Sets the page into Reading status. The page should be in `Loading` status before this call.
    /// The accessor first calls `BeginRead(): MemoryStream`, the returned stream is filled with data
    /// (e.g. from a compressed/encrypted source, then the accessor calls `EndReading()` to indicate
    /// that the Page is ready to be enumerated
    /// </summary>
    internal void EndReading(long actualPageId, DateTime utcCreate, Atom app, string host)
    {
      Ensure(Status.Loading);

      if (actualPageId > 0)
        m_PageId = actualPageId;

      m_Raw.Position = 0;
      m_State = m_Raw.Length > 0 ? Status.Reading : Status.EOF;
      m_CreateUtc = utcCreate;
      m_CreateApp = app;
      m_CreateHost = host;
      m_State = Status.Reading;
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
    /// The UTC of page creation
    /// </summary>
    public DateTime CreateUtc => m_CreateUtc;

    /// <summary>
    /// Information about the user/host which created the page.
    /// By Convention a user name is prepended before host name like so: `user@host` e.g. `dave@linmed001`
    /// </summary>
    public string CreateHost => m_CreateHost;

    /// <summary>
    /// Id of application which created this page
    /// </summary>
    public Atom CreateApp => m_CreateApp;

    /// <summary>
    /// Current page size
    /// </summary>
    public ArraySegment<byte> Data => new ArraySegment<byte>(m_Raw.GetBuffer(), 0, (int)m_Raw.Length);

    /// <summary>
    /// Walks all raw entries. This is purely in-memory operation
    /// </summary>
    public IEnumerable<Entry> Entries
    {
      get
      {
        Ensure(Status.Reading);
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
        Ensure(Status.Reading);
        var buffer = m_Raw.GetBuffer();
        return get(buffer, ref address);
      }
    }

    /// <summary>
    /// Writes a raw representation of an entry, returning its address on a page
    /// </summary>
    public int Append(ArraySegment<byte> entry)
    {
      Ensure(Status.Writing);
      Aver.IsTrue(entry.Array != null && entry.Count > 0);

      var addr = (int)m_Raw.Position;

      m_Raw.WriteByte(Format.ENTRY_HEADER_1);
      m_Raw.WriteByte(Format.ENTRY_HEADER_2);
      m_Raw.WriteBEInt32(entry.Count); //todo change to VarLen32

      m_Raw.Write(entry.Array, entry.Offset, entry.Count);

      return addr;
    }


    /// <summary>
    /// Asserts page to be in specific state
    /// </summary>
    /// <param name="need"></param>
    public void Ensure(Status need)
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

