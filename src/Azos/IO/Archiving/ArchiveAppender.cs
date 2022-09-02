/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Azos.Serialization.Bix;
using Azos.Text;
using Azos.Time;

namespace Azos.IO.Archiving
{
#pragma warning disable CA1063
  /// <summary>
  /// Provides base functionality for appending entries to archives.
  /// Concrete implementations handle serialization of specific `TEntry` entry types.
  /// This class instance is NOT thread-safe
  /// </summary>
  public abstract class ArchiveAppender<TEntry> : IDisposable
  {
    /// <summary>
    /// Creates appender for adding data at the end of the archive volume
    /// </summary>
    public ArchiveAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<TEntry, Bookmark> onPageCommit = null)
    {
      var vctp = volume.NonNull(nameof(volume)).Metadata.ContentType;
      if (vctp.IsNotNullOrWhiteSpace())//legacy support: older archives do not have content type and will generate error on read if mismatch happens
        if (!IsContentTypeCompatible(vctp))
          throw new ArchivingException(StringConsts.ARCHIVE_APPENDER_CONTENT_TYPE_ERROR.Args(volume.Metadata.Id, vctp, GetType().DisplayNameWithExpandedGenericArgs()));


      m_Volume = volume;
      m_Time = time.NonNull(nameof(time));
      m_Page = new Page(volume.PageSizeBytes);
      m_OnPageCommit = onPageCommit;//optional

      if (onPageCommit != null)
        m_Buffer = new List<(TEntry, int addr)>(0xff);
    }

    public void Dispose() => Flush();  //this is a "fake" dispose used just to be able to use `using` to cause flush


    private readonly IVolume m_Volume;
    private readonly ITimeSource m_Time;
    private readonly Page m_Page;
    private readonly Atom m_App;
    private readonly string m_Host;
    private readonly Action<TEntry, Bookmark> m_OnPageCommit;

    private readonly List<(TEntry entry, int addr)> m_Buffer;


    public IVolume Volume         =>  m_Volume;
    public ITimeSource TimeSource =>  m_Time;
    public Page    Page           =>  m_Page;
    public Atom    App            =>  m_App;
    public string  Host           =>  m_Host;


    /// <summary>
    /// Returns true if a specified contentType is supported by any of content types by this appender, including
    /// its logical inheritance chain. The support testing is performed using a match against patterns specified by [ContentTypeSupport].
    /// For example, a volume may be set to 'bix/string' content type - an archive of strings.
    /// One could use `JsonArchiveAppender` to add data to this archive because it adds JSON data as strings and is
    /// compatible with 'bix/string' more general format
    /// </summary>
    public bool IsContentTypeCompatible(string contentType)
      => ContentTypeSupportAttribute.GetSupportedContentTypePatternsFor(GetType())
                                    .Any( ctp => contentType.MatchPattern(ctp, senseCase: false));

    /// <summary>
    /// Appends the entry and immediately returns. This method is asynchronous by design as
    /// it never blocks on internal write. The pages are split automatically depending on Volume.PageSizeBytes setting.
    /// Call Flush() to ensure the proper archive closure or dispose this instance
    /// </summary>
    /// <returns>Entry address on a page</returns>
    public int Append(TEntry entry)
    {
      var splitAt = m_Volume.PageSizeBytes;
      if (splitAt > Format.PAGE_MAX_LEN) splitAt = Format.PAGE_MAX_LEN;

      if (m_Page.Data.Count > splitAt) Flush();

      if (m_Page.State != Page.Status.Writing)
        m_Page.BeginWriting(m_Time.UTCNow, m_App, m_Host);


      var binary = DoSerialize(entry);

      var addr = m_Page.Append(binary);

      if (m_Buffer != null)
        m_Buffer.Add((entry, addr));

      return addr;
    }

    /// <summary>
    /// Override to perform physical serialization of entries
    /// </summary>
    protected abstract ArraySegment<byte> DoSerialize(TEntry entry);


    /// <summary>
    /// Appends accumulated page data to Volume. The `Append()` method
    /// calls this automatically, so does `.Dispose()`
    /// </summary>
    public bool Flush()
    {
      if (m_Page.State != Page.Status.Writing || m_Page.Data.Count == 0) return false;

      m_Page.EndWriting();
      var pageId = m_Volume.AppendPage(m_Page);
      m_Page.__SetPageId(pageId);

      if (m_Buffer != null)
      {
        foreach(var item in m_Buffer)
        {
          m_OnPageCommit(item.entry, new Bookmark(pageId, item.addr));
        }
        m_Buffer.Clear();
      }

      return true;
    }
  }

  /// <summary>
  /// Facilitates creating appenders which use BixWriter.
  /// Warning: the instance is NOT thread-safe
  /// </summary>
  public abstract class ArchiveBixAppender<TEntry> : ArchiveAppender<TEntry>
  {
    public ArchiveBixAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<TEntry, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit)
    {
      m_Stream = new MemoryStream();
      m_Writer = new BixWriter(m_Stream);
    }

    private MemoryStream m_Stream;
    private BixWriter m_Writer;

    private long m_TotalBytesAppended;
    private long m_TotalEntriesAppended;

    public long TotalBytesAppended => m_TotalBytesAppended;
    public long TotalEntriesAppended => m_TotalEntriesAppended;

    /// <summary>
    /// Resets Totals back to zero
    /// </summary>
    public virtual void ResetStats()
    {
      m_TotalBytesAppended = 0;
      m_TotalEntriesAppended = 0;
    }

    protected sealed override ArraySegment<byte> DoSerialize(TEntry entry)
    {
      m_Stream.SetLength(0);
      DoSerializeBix(m_Writer, entry);
      unchecked
      {
        m_TotalEntriesAppended++;
        m_TotalBytesAppended += m_Stream.Length;
      }
      return new ArraySegment<byte>(m_Stream.GetBuffer(), 0, (int)m_Stream.Length);
    }

    /// <summary>
    /// Override to perform actual TEntry serialization using BixWriter
    /// </summary>
    protected abstract void DoSerializeBix(BixWriter wri, TEntry entry);
  }

#pragma warning restore CA1063
}
