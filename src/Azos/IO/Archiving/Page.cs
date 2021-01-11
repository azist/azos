///*<FILE_LICENSE>
// * Azos (A to Z Application Operating System) Framework
// * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
// * See the LICENSE file in the project root for more information.
//</FILE_LICENSE>*/

//using System;
//using System.Collections.Generic;
//using System.IO;

//namespace Azos.IO.Archiving
//{
//  /// <summary>
//  /// Represents a "page" in an archive data stream. Archives can only be appended to.
//  /// A page is an atomic unit of reading from and appending to archives.
//  /// Pages represent binary raw content present in RAM.
//  /// Pages are obtained from `ArchiveDataAccessor`. Pages are appended to archives
//  /// using `ArchiveDataAccessor.AppendPage(page)`;
//  /// Page is NOT a thread-safe instance.
//  /// </summary>
//  public sealed class Page
//  {
//    private Page()
//    {
//      m_Raw = new MemoryStream();
//    }

//    internal void CreateNew(IApplication app)
//    {
//      m_Raw.Position = 0;
//      m_State = Status.Writing;
//      m_CreateUtc = app.TimeSource.UTCNow;
//      m_CreateApp = app.AppId;
//      m_CreateHost = Platform.Computer.HostName;
//    }


//    public enum Status{ Invalid, Read, Writing }

//    private Status m_State;
//    private DateTime m_CreateUtc;
//    private string m_CreateHost;
//    private Atom m_CreateApp;

//    private MemoryStream m_Raw; //we can use byte[] directly not to use MemoryStream instance
//    private byte[] m_Buffer;


//    /// <summary>
//    /// Returns the current state of the page instance
//    /// </summary>
//    public Status State => m_State;

//    /// <summary>
//    /// Walks all raw entries. This is purely in-memory operation
//    /// </summary>
//    public IEnumerable<ArchiveEntry> Entries
//    {
//      get
//      {
//        for(var i=0; i<m_Buffer.Length; )
//          yield return get(ref i);
//      }
//    }

//    /// <summary>
//    /// Returns an entry by its address, if address is invalid then invalid entry is returned
//    /// </summary>
//    public ArchiveEntry this[int address]
//    {
//      get
//      {
//        if (address<0 || address>=m_Buffer.Length) return new ArchiveEntry(/*invalid ctor*/);
//        return get(ref address);
//      }
//    }

//    private ArchiveEntry get(ref int address)
//    {
//      return new ArchiveEntry();
//    }

//  }
//}
