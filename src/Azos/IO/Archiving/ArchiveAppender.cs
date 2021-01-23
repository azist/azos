/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using Azos.Conf;
using Azos.Time;

namespace Azos.IO.Archiving
{
#pragma warning disable CA1063
  public abstract class ArchiveAppender<TEntry> : IDisposable
  {
    /// <summary>
    /// Creates appender for adding data at the end of the archive volume
    /// </summary>
    public ArchiveAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<TEntry, Bookmark> onPageCommit = null)
    {
      m_Volume = volume.NonNull(nameof(volume));
      m_Time = time.NonNull(nameof(time));
      m_Page = new Page(0);//?
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

    /// <summary>
    /// Appends the entry and immediately returns. This method is asynchronous by design as
    /// it never blocks on internal write. Call Flush() to ensure the proper archive closure or dispose this instance
    /// </summary>
    /// <param name="entry"></param>
    /// <returns>Entry address on a page</returns>
    public int Append(TEntry entry)
    {
      if (m_Page.Data.Count > m_Volume.PageSizeBytes) Flush();

      if (m_Page.State != Page.Status.Writing)
        m_Page.BeginWriting(-1, m_Time.UTCNow, m_App, m_Host);


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


    public bool Flush()
    {
      if (m_Page.State != Page.Status.Writing || m_Page.Data.Count == 0) return false;

      m_Page.EndWriting();
      var pageId = m_Volume.AppendPage(m_Page);

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

#pragma warning restore CA1063
}
