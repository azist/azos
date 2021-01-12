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
    private Page()
    {
      m_Raw = new MemoryStream();
    }

    internal void CreateNew(IApplication app)
    {
      m_Raw.Position = 0;
      m_State = Status.Writing;
      m_CreateUtc = app.TimeSource.UTCNow;
      m_CreateApp = app.AppId;
      m_CreateHost = Platform.Computer.HostName;
    }


    public enum Status { Invalid, Read, Writing }

    private Status m_State;
    private DateTime m_CreateUtc;
    private string m_CreateHost;
    private Atom m_CreateApp;

    private MemoryStream m_Raw; //we can use byte[] directly not to use MemoryStream instance
    private byte[] m_Buffer;


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
        for (var i = 0; i < m_Buffer.Length;)
          yield return get(ref i);
      }
    }

    /// <summary>
    /// Returns an entry by its address, if address is invalid then invalid entry is returned
    /// </summary>
    public Entry this[int address]
    {
      get
      {
        if (address < 0 || address >= m_Buffer.Length) return new Entry();
        return get(ref address);
      }
    }

    private Entry get(ref int address)
    {
      var ptr = address;
      if (address < 0 || address + Format.ENTRY_MIN_LEN >= m_Buffer.Length) return new Entry(Entry.Status.BadAddress);

      var h1 = m_Buffer[address];
      if (h1 == Format.ENTRY_HEADER_1 && m_Buffer[++address] == Format.ENTRY_HEADER_2)
      {
        var len = m_Buffer.ReadBEInt32(ref address);//todo use varbit encoding 1-5 bytes
        //check max length

        if (len <= 0 || len > Format.ENTRY_MAX_LEN) return new Entry(Entry.Status.InvalidLength);//max length exceeded

        var start = address;
        var end = address + len;
        if (end >= m_Buffer.Length) return new Entry(Entry.Status.BadAddress);//beyond the block

        return new Entry(Entry.Status.Valid, ptr, new ArraySegment<byte>(m_Buffer, start, len));
      }
      else if (h1 == Format.ENTRY_HEADER_EOF_1 && m_Buffer[++address] == Format.ENTRY_HEADER_EOF_2)
      {
        return new Entry(Entry.Status.EOF);//EOF
      }
      else return new Entry(Entry.Status.BadHeader);//corruption
    }

  }
}
