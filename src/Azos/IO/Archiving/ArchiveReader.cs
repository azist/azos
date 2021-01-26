/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// This class is not thread safe and can only be iterated only once
  /// </summary>
  public abstract class ArchiveReader<TEntry> : DisposableObject, IEnumerable<TEntry>
  {
    public ArchiveReader(IVolume volume, Bookmark start)
    {
      m_Volume = volume.NonNull(nameof(volume));
      StartAt(start);
    }

    //enumerator destructor
    protected override void Destructor()
    {
      base.Destructor();
    }

    private readonly IVolume m_Volume;
    private Bookmark m_Start;
    private Bookmark m_Current;
    private Page m_Page;


    public IVolume Volume => m_Volume;

    /// <summary>
    /// Returns the archive pointer where reading started from
    /// </summary>
    public Bookmark Start => m_Start;

    /// <summary>
    /// Restarts reading from the specified archive pointer position as if the instance was just re-allocated.
    /// The enumeration is being affected
    /// </summary>
    public void StartAt(Bookmark start)
    {
      m_Start = start;
      m_Current = start;
    }

    /// <summary>
    /// Walks all pages
    /// </summary>
    public IEnumerable<Page> Pages
    {
      get
      {
        Page page = null;
        yield return page;
      }
    }

    /// <summary>
    /// Walks all raw page entries on all pages
    /// </summary>
    public IEnumerable<Entry> RawEntries
    {
      get
      {
        foreach(var page in Pages)
          foreach(var entry in page.Entries)
              yield return entry;
      }
    }

    /// <summary>
    /// Walks all materialized `TEntry` entries on all pages
    /// </summary>
    public IEnumerable<TEntry> Entries => RawEntries.Select(re => Materialize(re));


    public IEnumerator<TEntry> GetEnumerator() => Entries.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Entries.GetEnumerator();

    /// <summary>
    /// Override to perform physical deserialization of entries
    /// </summary>
    public abstract TEntry Materialize(Entry entry);
  }

}
