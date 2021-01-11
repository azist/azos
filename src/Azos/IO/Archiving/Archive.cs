//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading;
//using System.Threading.Tasks;

//using Azos.Conf;

//namespace Azos.IO.Archiving
//{
//  public struct ArchiveMeta
//  {
//    public static ArchiveMeta Make(ConfigSectionNode data)
//    {

//    }
//    public IConfigSectionNode Data;

//    public bool IsCompressed => Data.Of("COMPRESS").ValueAsBool();
//    public bool IsEncrypted => false;

//    //public AccessorCompressor Compressor =>
//  }


//  /// <summary>
//  /// Points to a data
//  /// </summary>
//  public struct ArchivePtr
//  {
//    public long PageOffset;
//    public int Address;
//  }

//  public struct ArchiveEntry
//  {
//    public enum Status { Corrupt = 0, OK }
//    public bool OK;
//    public ArraySegment<byte> Entry;
//  }

//  /// <summary>
//  /// Must be thread-safe.
//  /// This method may be called by multiple threads at the same time (even over the same source stream).
//  /// The class performs internal cache/access coordination and tries to satisfy requests in lock-free manner
//  /// </summary>
//  public abstract class ArchiveDataAccessor : DisposableObject
//  {
//    public const int MIN_PAGE_SIZE = 0xff;
//    public const int MAX_PAGE_SIZE = 128 * 1024;//128k
//    public const int DFLT_PAGE_SIZE = 1024;

//    /// <summary>
//    /// Controls page split on writing. Does not affect reading
//    /// </summary>
//    public int PageSizeBytes { get; set; }


//    /// <summary>
//    /// Return true if page was read into existing memory stream, false if it is torn in the underlying source.
//    /// This method may be called by multiple threads at the same time (even over the same source stream).
//    /// The class performs internal cache/access coordination and tries to satisfy requests in lock-free manner
//    /// </summary>
//    public bool ReadPage(long pageOffset, Page page)//so it can grow as needed
//    {
//      return false;//torn
//    }

//    //appends at the end of file
//    public void AppendPage(Page page)//page is formed for writing in some existing buffer
//    {
//      //where is logical address?
//    }
//  }

//  /// <summary>
//  /// Represents a "page" in an archive data stream. Archives can only be appended to.
//  /// A page is an atomic unit of read and append to archives.
//  /// Pages represent binary raw content present in RAM.
//  /// Pages are obtained from ArchiveDataAccessor. Pages are appended to archives
//  /// using ArchiveDataAccessor.AppendPage(page);
//  /// </summary>
//  public sealed class Page
//  {
//    private DateTime m_CreateUtc;
//    private string m_CreateHost;
//    private Atom m_CreateApp;

//    private MemoryStream m_Raw; //we can use byte[] directly not to use MemoryStream instance

//    /// <summary>
//    /// Walks all raw entries. This is purely in-memory operation
//    /// </summary>
//    public IEnumerable<ArchiveEntry> All => null;
//  }



//  /// <summary>
//  /// This class is not thread safe and can only be iterated once
//  /// </summary>
//  public abstract class ArchiveReader<TEntry> : DisposableObject, IEnumerable<TEntry>, IEnumerator<TEntry>
//  {
//    public ArchiveReader(Stream dataStream, ArchivePtr start)
//    {
//      m_Stream = dataStream.NonNull(nameof(Stream));
//      m_Start = start;
//    }

//    //enumerator destructor
//    protected override void Destructor()
//    {
//      base.Destructor();
//    }

//    private Stream m_Stream;
//    private ArchivePtr m_Start;
//    private IConfigSectionNode m_Metadata;



//    public Stream Stream => m_Stream;

//    /// <summary>
//    /// Returns the archive pointer where reading started from
//    /// </summary>
//    public ArchivePtr Start => m_Start;

//    /// <summary>
//    /// Returns archive metadata extracted from the header page
//    /// </summary>
//    public IConfigSectionNode Metadata => m_Metadata;


//    /// <summary>
//    /// Restarts reading from the specified archive pointer position.
//    /// The enumeration is being affected
//    /// </summary>
//    public void RestartAt(ArchivePtr start)
//    {

//    }
//  }

//  public abstract class ArchiveAppender<TEntry> : DisposableObject
//  {
//    /// <summary>
//    /// Creates appender for adding data at the end of the existing reader
//    /// </summary>
//    public ArchiveAppender(ArchiveReader<TEntry> existing)
//    {
//      m_Stream = existing.NonNull(nameof(existing)).Stream;
//      m_Metadata = existing.Metadata;
//      m_Stream.Seek(0, SeekOrigin.End);
//    }


//    /// <summary>
//    /// Creates a new archive. The stream must be at position 0
//    /// </summary>
//    public ArchiveAppender(Stream dataStream, IConfigSectionNode metadata)
//    {
//      m_Stream = dataStream.NonNull(nameof(Stream));
//      m_Metadata = metadata.NonEmpty(nameof(metadata));
//      Aver.IsTrue(m_Stream.Position == 0, "stream.pos!=0");
//      //write header
//      //write metadata
//      //...
//    }

//    private Stream m_Stream;
//    private IConfigSectionNode m_Metadata;

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
