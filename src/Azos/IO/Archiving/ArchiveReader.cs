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
    public ArchiveReader(IVolume volume)
    {
      Volume = volume.NonNull(nameof(volume));
    }

    /// <summary>
    /// Volume which stores data
    /// </summary>
    public readonly IVolume Volume;

    /// <summary>
    /// Enumerates all pages starting at the specified pageId. Multiple threads can call this method at the same time
    /// each getting its own enumerator
    /// </summary>
    public IEnumerable<Page> Pages(long startPageId, bool skipCorruptPages = false)
    {
      var current = startPageId;
      var page = new Page(Volume.PageSizeBytes);
      while(true)
      {
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
          yield return page;
        }

        if (current <=0) break;
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
