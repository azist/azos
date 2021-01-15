/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Provides default implementation for IVolume, providing optional
  /// page compression and encryption
  /// </summary>
  public class DefaultVolume : DisposableObject, IVolume
  {

    public DefaultVolume(Stream stream, bool ownsStream = true)
    {
      m_Stream = stream.NonNull(nameof(stream));
    }

    protected override void Destructor()
    {
      m_Stream.Flush();
      if (m_OwnsStream) DisposeAndNull(ref m_Stream);
      base.Destructor();
    }

    private bool m_OwnsStream;
    private Stream m_Stream;
    private IPageCache m_Cache;


    public int PageSizeBytes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    /// <summary>
    /// Fills the page instance with archive data performing necessary decompression/decryption
    /// when needed. Returns a positive long value with the next adjacent `pageId` or a negative
    /// value to indicate the EOF condition. This method MAY be called by multiple threads at the same time
    /// (even over the same source stream which this class accesses). The implementor MAY perform internal
    /// cache/access coordination and tries to satisfy requests in lock-free manner
    /// </summary>
    public long ReadPage(long pageId, Page page)
    {
      pageId.IsTrue(v => v > 0, "pageId <= 0");
      page.NonNull(nameof(page));

      //align pageId by 16

      var pageData = page.BeginReading(pageId);

      if (!m_Cache.TryGet(pageId, pageData))
      {
        lock (m_Stream)
        {
          if (!m_Cache.TryGet(pageId, pageData))
            loadFromStream(pageData);
        }
      }

      page.EndReading(new DateTime(), Atom.ZERO, "");
      return 0;
    }

    /// <summary>
    /// Appends the page at the end of volume. Returns the pageId of the appended page.
    /// The implementor MAY perform internal cache/access coordination and tries to satisfy requests in lock-free manner
    /// </summary>
    public long AppendPage(Page page)
    {
      page.NonNull(nameof(page));
      return -1;
    }


    //write:  1. compress ~25%  2. encrypt
    //read:  1. decrypt  2. decompress
    private void loadFromStream(MemoryStream pageData) //called under lock
    {
      //reads from m_Stream -> pageDataMemorySTream(a thread-safe copy)

      //////var len = m_Stream.ReadBEInt32();

      //////if (nocompressionorencryption)
      //////{
      //////  pageData.SetLength(len);
      //////  for (var got = 0; got < len;)
      //////    got += m_Stream.Read(pageData.GetBuffer(), 0, len - got);
      //////}


      //////byte[] buf = null;//private memebr

      //////for(var got=0; got<len;)
      ////// got += m_Stream.Read(buf, 0, len-got);





      //////pageData.Write(buf, 0, len);
      ////////1 decrypt
      ////////2 decompress

    }

  }
}
