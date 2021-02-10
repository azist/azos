/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Serialization.Bix;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Facilitates reading archive data as IEnumerable(TEntry).
  /// The instance methods are thread-safe: multiple threads may enumerate the instance in-parallel, however
  /// each IEnumerable is not thread safe (by design)
  /// </summary>
  public abstract class ArchiveReader<TEntry>
  {
    //EMA filter: the lower the number the more smoothing is done (less sensitive to changes)
    private const float EMA_PAGE_SIZE_K = 0.257f;
    private const float EMA_PAGE_SIZE_J = 1f - EMA_PAGE_SIZE_K;

    public ArchiveReader(IVolume volume)
    {
      Volume = volume.NonNull(nameof(volume));
      m_AveragePageSizeBytes = volume.PageSizeBytes;
    }

    private int m_AveragePageSizeBytes;

    /// <summary>
    /// Volume which stores data
    /// </summary>
    public readonly IVolume Volume;

    /// <summary>
    /// Returns an average page size as detected while reading past few pages.
    /// The system tries to detect the real page sizes and use that to pre-allocate page buffers
    /// to reduce re-allocation
    /// </summary>
    public int AveragePageSizeBytes => m_AveragePageSizeBytes;


    /// <summary>
    /// Enumerates all pages starting at the specified `pageId`.
    /// This method yields back an enumeration of different page instances which could be used
    /// for parallel processing unless `preallocatedPage` is supplied in which case that given page instance is re-used
    /// for enumeration of the archive, consequently making it NOT a thread-safe operation.
    /// Existing page instances are used as optimization technique for tight read loops and should only be used in special cases
    /// to prevent extra page instance allocation (thus making it NOT thread-safe). The thread-safety concern only pertains to
    /// the instances returned by the enumerator. The IEnumerator itself is NOT thread-safe
    /// </summary>
    public IEnumerable<Page> Pages(long startPageId, bool skipCorruptPages = false, Page preallocatedPage = null)
    {
      //local flow average is needed because not all of the archive pages are necessarily the same size:
      //they are the similar size around the same read area, consequently the local EMA provide more accurate estimate
      float emaSize = AveragePageSizeBytes;

      var i = 0;
      var current = startPageId;
      while(true)
      {
        var page = preallocatedPage;

        if (page == null)
          page = new Page((int)emaSize);
        else
          page.AdjustDefaultCapacity((int)emaSize);

        try
        {
          current = Volume.ReadPage(current, page);
        }
        catch(ArchivingException)
        {
          if (skipCorruptPages)
          {
            current++; //re-align to next page header
            continue;
          }
          throw;
        }

        if (page.State == Page.Status.Reading)
        {
          var sz = (float)page.Data.Count;
          emaSize = (sz * EMA_PAGE_SIZE_K) + (emaSize * EMA_PAGE_SIZE_J);

          if ((i++ & 0b11) == 0)//every 4 iterations, calculate the average for the whole class based on an already averaged local
          {
            m_AveragePageSizeBytes = (int)((emaSize * EMA_PAGE_SIZE_K) + (m_AveragePageSizeBytes * EMA_PAGE_SIZE_J));
          }

          yield return page;
        }

        if (current <= 0) break;
      }
    }

    /// <summary>
    /// Walks all raw page entries on all pages. You can materialize entries into TEntry by calling `TEntry Materialize(Entry)`.
    /// Multiple threads can call this method at the same time
    /// </summary>
    public IEnumerable<Entry> RawEntries(Bookmark start, bool skipCorruptPages = false)
    {
      foreach(var page in Pages(start.PageId, skipCorruptPages))
        foreach(var entry in page.Entries.Where(e => e.Address >= start.Address))
            yield return entry;
    }

    /// <summary>
    /// Walks all materialized `TEntry` entries on all pages starting at the supplied bookmark.
    /// Multiple threads can call this method at the same time
    /// </summary>
    public IEnumerable<TEntry> Entries(Bookmark start, bool skipCorruptPages = false)
      => RawEntries(start, skipCorruptPages)
                            .Where(item => item.State == Entry.Status.Valid)
                            .Select(item => Materialize(item));

    /// <summary>
    /// Walks all materialized `TEntry` entries on all pages.
    /// Multiple threads can call this method at the same time
    /// </summary>
    public IEnumerable<TEntry> All => Entries(new Bookmark());


    /// <summary>
    /// Performs physical deserialization of entries. Override to materialize a concrete instance of TEntry.
    /// This method implementation is thread-safe
    /// </summary>
    public abstract TEntry Materialize(Entry entry);
  }


  /// <summary>
  /// Facilitates creating readers which use BixReader. The implementation is thread-safe
  /// </summary>
  public abstract class ArchiveBixReader<TEntry> : ArchiveReader<TEntry>
  {
    public ArchiveBixReader(IVolume volume) : base(volume) { }

    [ThreadStatic] private static BufferSegmentReadingStream ts_Stream;

    public sealed override TEntry Materialize(Entry entry)
    {
      if (entry.State != Entry.Status.Valid) return default(TEntry);

      var stream = ts_Stream;
      if (stream == null)
      {
        stream = new BufferSegmentReadingStream();
        ts_Stream = stream;
      }

      stream.UnsafeBindBuffer(entry.Raw);
      var reader = new BixReader(stream);

      TEntry result = MaterializeBix(reader);

      stream.UnbindBuffer();

      return result;
    }

    public abstract TEntry MaterializeBix(BixReader reader);
  }

}
