//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading;
//using System.Threading.Tasks;

//using Azos.Conf;

//namespace Azos.IO.Archiving
//{

//  public struct Journal<TEntry> : IEnumerable<TEntry>
//  {

//    public readonly ArchivePtr Start;

//    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
//    public IEnumerator<TEntry> GetEnumerator()
//    {
//      throw new NotImplementedException();
//    }
//  }

//  public struct ArchivePtr
//  {
//    public long PageOffset;
//    public int Address;
//  }

//  public struct ArchiveEntry
//  {
//    public enum Status{ Corrupt =0, OK }
//    public bool OK;
//    public ArraySegment<byte> Entry;
//  }

//  /*
//   Compression and Encryption of data at rest are to be handled by the FileSystem/External implementor of Stream

//    Archive Container Format (Binary)
//    ---------------------------------
//    Header:
//      FILE-HEADER = "#!/usr/bin/env bix\n";
//      <file-hdeader> <metedata: json> <pages>
//        metadata = JSON string with system attributes at the root:
//        archive
//        {
//          cdt=createUTC
//          version=integer
//          entry-format
//          {
//            type=....
//          }
//        }

//   Pages:
//      page[]
//   Page:
//      ALIGNED BY 16
//      PAGE-HDR = ASCII(`PAGE`)
//      <PAGE-HDR><position: long><entry-stream>  <eof|page-hdr>

//   Entry-stream:
//      entries[]<eof | TERMINATOR-ENTRY-HDR>

//    Entry format:
//      ENTRY-HDR = `xABxBA` or  TERMINATOR-ENTRY-HDR = `\x00x00`
//      <ENTRY-HDR><entry-len: varulong> <entry-content: byte[len]>
//        2 bytes     1..9 bytes             1 byte...X        Shortest message:  4 bytes
//      entry-len: varlong(e.g. LEB128) < MAX-ITEM-SZ configurable (16 MByte by default)

//  */

//  // An Archive is a unidirectional journal of byte[] entries which can only be sequentially appended to the very end of the archive.
//  // Physically the entries are housed in frames
//  //Represents binary archive container
//  /// <summary>
//  /// This class is not thread safe for writing and reading
//  /// </summary>
//  /// <typeparam name="TEntry"></typeparam>
//  public abstract class ArchiveReader<TEntry> : DisposableObject
//  {
//    public ArchiveReader(Stream dataStream)
//    {
//      m_Stream = dataStream.NonNull(nameof(Stream));
//    }

//    protected override void Destructor()
//    {
//      base.Destructor();
//    }

//    private Stream m_Stream;
//    private IConfigSectionNode m_Metadata;


//    /// <summary>
//    /// Returns archive metadata extracted from the header page
//    /// </summary>
//    public IConfigSectionNode Metadata => m_Metadata;

//    /// <summary>
//    /// Starts a lazy enumeration of the archive data, the actual processing is done
//    /// as enumeration advances through the journal
//    /// </summary>
//    /// <param name="pointer">A pointer to start reading journal entries from</param>
//    /// <returns></returns>
//    public Journal<TEntry> ReadFrom(ArchivePtr pointer)
//    {
//      return null;
//    }

//  }

//  public abstract class ArchiveAppender<TEntry> : DisposableObject
//  {
//    public ArchiveAppender()
//    {

//    }

//    private Stream m_Stream;

//    protected override void Destructor()
//    {
//      Flush();
//      base.Destructor();
//    }

//    /// <summary>
//    /// Appends the entry and immediately returns. This method is asynchronous by design as
//    /// it never blocks on internal write. Call FlushAsync() to ensure the proper archive closure
//    /// </summary>
//    /// <param name="entry"></param>
//    /// <returns></returns>
//    public ArchivePtr Append(TEntry entry)
//    {
//      return default(ArchivePtr);
//    }

//    public async Task FlushAsync()
//    {
//      await m_Stream.FlushAsync().ConfigureAwait(false);
//    }

//    public async Task FlushAsync(CancellationToken ct)
//    {
//      await m_Stream.FlushAsync(ct).ConfigureAwait(false);
//    }

//    public void Flush()
//    {
//      m_Stream.Flush();
//    }

//  }
//}
