//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading;
//using System.Threading.Tasks;

//using Azos.Conf;

//namespace Azos.IO.Archiving
//{
//  /// <summary>
//  /// This class is not thread safe and can only be iterated once
//  /// </summary>
//  public abstract class ArchiveReader<TEntry> : DisposableObject, IEnumerable<TEntry>, IEnumerator<TEntry>
//  {
//    public ArchiveReader(Stream dataStream, ArchivePointer start)
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
//    private ArchivePointer m_Start;
//    private IConfigSectionNode m_Metadata;



//    public Stream Stream => m_Stream;

//    /// <summary>
//    /// Returns the archive pointer where reading started from
//    /// </summary>
//    public ArchivePointer Start => m_Start;

//    /// <summary>
//    /// Returns archive metadata extracted from the header page
//    /// </summary>
//    public IConfigSectionNode Metadata => m_Metadata;


//    /// <summary>
//    /// Restarts reading from the specified archive pointer position.
//    /// The enumeration is being affected
//    /// </summary>
//    public void RestartAt(ArchivePointer start)
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
//    public ArchivePointer Append(TEntry entry)
//    {
//      return default(ArchivePointer);
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
