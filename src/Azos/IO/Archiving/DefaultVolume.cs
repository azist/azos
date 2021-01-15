/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;

using Azos.Serialization.Bix;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Provides default implementation for IVolume, providing optional
  /// page compression and encryption
  /// </summary>
  public class DefaultVolume : DisposableObject, IVolume
  {
    /// <summary>
    /// Create a new volume
    /// </summary>
    public DefaultVolume(VolumeMetadataBuilder metadataBuilder, Stream stream, bool ownsStream = true)
    {
      m_Stream = stream.NonNull(nameof(stream));
      (m_Stream.Length == 0).IsTrue("stream.!Empty");
      metadataBuilder.Assigned.IsTrue("meta.!Assigned");

      m_Reader = new BixReader(m_Stream);
      m_Writer = new BixWriter(m_Stream);

      m_Metadata = metadataBuilder.Built;
      writeVolumeHeader();
    }

    /// <summary>
    /// Mounts an existing volume
    /// </summary>
    public DefaultVolume(Stream stream, bool ownsStream = true)
    {
      m_Stream = stream.NonNull(nameof(stream));
      (m_Stream.Length > 0).IsTrue("stream.!Empty");

      m_Reader = new BixReader(m_Stream);
      m_Writer = new BixWriter(m_Stream);

      m_Metadata = readVolumeHeader();
    }

    protected override void Destructor()
    {
      m_Stream.Flush();
      if (m_OwnsStream) DisposeAndNull(ref m_Stream);
      base.Destructor();
    }

    private bool m_OwnsStream;
    private Stream m_Stream;
    private BixReader m_Reader;
    private BixWriter m_Writer;
    private IPageCache m_Cache;
    private VolumeMetadata m_Metadata;



    /// <summary>
    /// Returns archive volume metadata. Metadata gets set only at the time of new archive creation and
    /// it can not be mutated after creation
    /// </summary>
    public VolumeMetadata Metadata => m_Metadata;

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

      //align pageId by 16 in a loop...

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
      page.NonNull(nameof(page))
          .Ensure(Page.Status.Written);

      lock(m_Stream)
      {
        var pageId = seekToNewPageLocation();
        writeHeader(pageId, page);
        writeData(page);
       // m_Cache.Put(pageId, page.)
        return pageId;
      }
    }


    private VolumeMetadata readVolumeHeader()
    {
      m_Stream.Position = 0;

      try
      {
        //Volume file header
        Format.VOLUME_HEADER.ForEach(c => Aver.IsTrue((byte)c == m_Reader.ReadByte(), "sig mismatch"));
        Aver.AreEqual(0, m_Reader.ReadByte(), "no \0x00");//null terminator
        Aver.AreEqual(0, m_Reader.ReadByte(), "no \0x00");

        //Info
        var info = m_Reader.ReadString();
        Aver.IsFalse(info.IsNullOrWhiteSpace(), "no info");

        //Json metadata
        var json = m_Reader.ReadString();
        Aver.IsFalse(info.IsNullOrWhiteSpace(), "no json");
        var meta = Data.ObjectValueConversion.AsJSONConfig(json, handling: Data.ConvertErrorHandling.Throw);

        //Pad
        for (var i = 0; i < Format.VOLUME_PAD_LEN; i++)
          Aver.AreEqual(Format.VOLUME_PAD_ASCII, m_Reader.ReadByte(), "bad pad");

        return new VolumeMetadata(meta);
      }
      catch(Exception cause)
      {
        throw new ArchivingException(StringConsts.ARCHIVE_VOLUME_HEADER_READ_ERROR.Args(GetType().Name, cause.ToMessageWithType()), cause);
      }
    }


    //the stream is guaranteed to be at 0
    private void writeVolumeHeader()
    {
      //Volume file header
      Format.VOLUME_HEADER.ForEach(c => m_Writer.Write((byte)c));
      m_Writer.Write((byte)0x00);//null terminator
      m_Writer.Write((byte)0x00);

      //Info
      m_Writer.Write($"Platform=Azos\nUri=https://github.com/azist/azos\nVolume={this.GetType().Name}\n");

      //Json Metadata
      var json = m_Metadata.Data.ToJSONString(Serialization.JSON.JsonWritingOptions.Compact);
      m_Writer.Write(json);

      //Pad
      for(var i=0; i<Format.VOLUME_PAD_LEN; i++)
        m_Writer.Write(Format.VOLUME_PAD_ASCII);
    }

    private long seekToNewPageLocation()
    {
      var result = IntUtils.Align16(m_Stream.Length);
      m_Stream.Position = result;
      return result;
    }

    private void writeHeader(long pageId, Page page)
    {
      //PAGE-HDR
      m_Writer.Write(Format.PAGE_HEADER_1);
      m_Writer.Write(Format.PAGE_HEADER_2);

      //position: 2 bytes of pageId (3rd and 2nd)
      m_Writer.Write((byte)(pageId >> 16));
      m_Writer.Write((byte)(pageId >> 8));

      //utcCreateDate
      m_Writer.Write((ulong)page.CreateUtc.ToSecondsSinceUnixEpochStart());

      //host
      m_Writer.Write(page.CreateHost);

      //app
      m_Writer.Write(page.CreateApp);
    }

    private void writeData(Page page)
    {
      var data = page.Data;

      //len
      m_Writer.Write((uint)data.Count);

      //data
      m_Stream.Write(data.Array, 0, data.Count);
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
      //////    got += m_Stream.Read(pageData.GetBuffer(), got, len - got);
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
