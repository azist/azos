/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

using Azos.Apps;
using Azos.Scripting;
using Azos.IO.Archiving;
using Azos.Security;

namespace Azos.Tests.Nub.IO.Archiving
{
  [Runnable]
  public class DefaultVolumeTests
  {
    public static ICryptoManager NopCrypto => NOPApplication.Instance.SecurityManager.Cryptography;


    [Run]
    public void Metadata_Basic()
    {
      var ms = new MemoryStream();
      var meta = VolumeMetadataBuilder.Make("Volume-1")
                                      .SetVersion(123,456)
                                      .SetDescription("My volume");
      var v1 = new DefaultVolume(NopCrypto, meta, ms);
      var id = v1.Metadata.Id;
      v1.Dispose();//it closes the stream

      Aver.IsTrue(ms.Length>0);

      var v2 = new DefaultVolume(NopCrypto, ms);
      Aver.AreEqual(id, v2.Metadata.Id);
      Aver.AreEqual("Volume-1", v2.Metadata.Label);
      Aver.AreEqual("My volume", v2.Metadata.Description);
      Aver.AreEqual(123, v2.Metadata.VersionMajor);
      Aver.AreEqual(456, v2.Metadata.VersionMinor);
      Aver.IsTrue(v2.Metadata.Channel.IsZero);
      Aver.IsFalse(v2.Metadata.IsCompressed);
      Aver.IsFalse(v2.Metadata.IsEncrypted);
    }

    [Run]
    public void Metadata_AppSection()
    {
      var ms = new MemoryStream();
      var meta = VolumeMetadataBuilder.Make("V1")
                                      .SetApplicationSection(app => { app.AddAttributeNode("a", 1); })
                                      .SetApplicationSection(app => { app.AddAttributeNode("b", -7); })
                                      .SetApplicationSection(app => { app.AddChildNode("sub{ q=true b=-9 }".AsLaconicConfig()); });

      var v1 = new DefaultVolume(NopCrypto, meta, ms);
      var id = v1.Metadata.Id;
      v1.Dispose();//it closes the stream

      Aver.IsTrue(ms.Length > 0);

      var v2 = new DefaultVolume(NopCrypto, ms);

      v2.Metadata.See();

      Aver.AreEqual(id, v2.Metadata.Id);
      Aver.AreEqual("V1", v2.Metadata.Label);

      Aver.IsTrue(v2.Metadata.SectionApplication.Exists);
      Aver.AreEqual(1, v2.Metadata.SectionApplication.Of("a").ValueAsInt());
      Aver.AreEqual(-7, v2.Metadata.SectionApplication.Of("b").ValueAsInt());
      Aver.AreEqual(true, v2.Metadata.SectionApplication["sub"].Of("q").ValueAsBool());
      Aver.AreEqual(-9, v2.Metadata.SectionApplication["sub"].Of("b").ValueAsInt());
    }

    [Run("compress=null     pad=1000 remount=false")]
    [Run("compress=gzip     pad=1000 remount=false")]
    [Run("compress=gzip-max pad=1000 remount=false")]

    [Run("compress=null     pad=1000 remount=true")]
    [Run("compress=gzip     pad=1000 remount=true")]
    [Run("compress=gzip-max pad=1000 remount=true")]
    public void Page_Write_Read(string compress, int pad, bool remount)
    {
      var ms = new MemoryStream();
      var meta = VolumeMetadataBuilder.Make("Volume-1")
                                      .SetCompressionScheme(compress);

      var v1 = new DefaultVolume(NopCrypto, meta, ms);

      var page = new Page(0);
      Aver.IsTrue(page.State == Page.Status.Unset);
      page.BeginWriting(new DateTime(1980, 7, 1, 15, 0, 0, DateTimeKind.Utc), Atom.Encode("app"), "dima@zhaba.com");
      Aver.IsTrue(page.State == Page.Status.Writing);
      var adr1 = page.Append(new ArraySegment<byte>(new byte[] { 1, 2, 3 }, 0, 3));
      Aver.AreEqual(0, adr1);
      var adr2 = page.Append(new ArraySegment<byte>(new byte[] { 4, 5 }, 0, 2));
      Aver.IsTrue(adr2 > 0);
      var adr3 = page.Append(new ArraySegment<byte>(new byte[pad]));
      Aver.IsTrue(adr3 > adr2);
      page.EndWriting();

      Aver.IsTrue(page.State == Page.Status.Written);
      var pid = v1.AppendPage(page);  //append to volume
      Aver.IsTrue(page.State == Page.Status.Written);


      "Written volume {0} Stream size is {1} bytes".SeeArgs(v1.Metadata.Id, ms.Length);

      if (remount)
      {
        v1.Dispose();
        v1 = new DefaultVolume(NopCrypto, ms);//re-mount existing data from stream
        "Re-mounted volume {0}".SeeArgs(v1.Metadata.Id);
      }

      page = new Page(0);//we could have reused the existing page but we re-allocate for experiment cleanness
      Aver.IsTrue(page.State == Page.Status.Unset);
      v1.ReadPage(pid, page);
      Aver.IsTrue(page.State == Page.Status.Reading);

      //page header read correctly
      Aver.AreEqual(new DateTime(1980, 7, 1, 15, 0, 0, DateTimeKind.Utc), page.CreateUtc);
      Aver.AreEqual(Atom.Encode("app"), page.CreateApp);
      Aver.AreEqual("dima@zhaba.com", page.CreateHost);


      var raw = page.Entries.ToArray();//all entry enumeration test
      Aver.AreEqual(4, raw.Length);

      Aver.IsTrue(raw[0].State == Entry.Status.Valid);
      Aver.IsTrue(raw[1].State == Entry.Status.Valid);
      Aver.IsTrue(raw[2].State == Entry.Status.Valid);
      Aver.IsTrue(raw[3].State == Entry.Status.EOF);
      Aver.AreEqual(0, raw[0].Address);
      Aver.IsTrue(raw[1].Address > 0);

      Aver.AreEqual(3, raw[0].Raw.Count);
      Aver.AreEqual(1, raw[0].Raw.Array[raw[0].Raw.Offset + 0]);
      Aver.AreEqual(2, raw[0].Raw.Array[raw[0].Raw.Offset + 1]);
      Aver.AreEqual(3, raw[0].Raw.Array[raw[0].Raw.Offset + 2]);

      Aver.AreEqual(2, raw[1].Raw.Count);
      Aver.AreEqual(4, raw[1].Raw.Array[raw[1].Raw.Offset + 0]);
      Aver.AreEqual(5, raw[1].Raw.Array[raw[1].Raw.Offset + 1]);

      Aver.AreEqual(pad, raw[2].Raw.Count);

      var one = page[adr1]; //indexer test
      Aver.IsTrue(one.State == Entry.Status.Valid);
      Aver.AreEqual(3, one.Raw.Count);
      Aver.AreEqual(1, one.Raw.Array[one.Raw.Offset + 0]);
      Aver.AreEqual(2, one.Raw.Array[one.Raw.Offset + 1]);
      Aver.AreEqual(3, one.Raw.Array[one.Raw.Offset + 2]);

      one = page[adr2];
      Aver.IsTrue(one.State == Entry.Status.Valid);
      Aver.AreEqual(2, one.Raw.Count);
      Aver.AreEqual(4, one.Raw.Array[one.Raw.Offset + 0]);
      Aver.AreEqual(5, one.Raw.Array[one.Raw.Offset + 1]);

      one = page[adr3];
      Aver.IsTrue(one.State == Entry.Status.Valid);
      Aver.AreEqual(pad, one.Raw.Count);
    }


    [Run("compress=null  count=100")]
    [Run("compress=null  count=1000")]
    [Run("compress=null  count=16000")]
    [Run("compress=null  count=128000")]
    public void Page_Write_CorruptPage_Read(int count)
    {
      var ms = new MemoryStream();
      var meta = VolumeMetadataBuilder.Make("Volume-1");

      var v1 = new DefaultVolume(NopCrypto, meta, ms);

      var page = new Page(0);
      Aver.IsTrue(page.State == Page.Status.Unset);

      page.BeginWriting(new DateTime(1980, 7, 1, 15, 0, 0, DateTimeKind.Utc), Atom.Encode("app"), "dima@zhaba.com");

      var data = new Dictionary<int, byte[]>();

      for(var i=0; i < count; i++)
      {
        //generate 1/2 empty arrays for best compression, another 1/2/ filled with random data
        var buf = ((i & 1) == 0) ? new byte[1 + (i & 0x7f)] : Platform.RandomGenerator.Instance.NextRandomBytes(1 + (i & 0x7f));
        var adr = page.Append(new ArraySegment<byte>(buf));
        data[adr] = buf;
      }

      Aver.AreEqual(data.Count, data.Keys.Distinct().Count());//all addresses are unique

      page.EndWriting();

      var pid = v1.AppendPage(page);  //append to volume

      page = new Page(0);//we could have reused the existing page but we re-allocate for experiment cleanness
      v1.ReadPage(pid, page);

      //Aver that all are readable
      foreach(var kvp in data)
      {
        var got = page[kvp.Key];
        Aver.IsTrue(got.State == Entry.Status.Valid);
        Aver.IsTrue(IOUtils.MemBufferEquals(kvp.Value, got.Raw.ToArray()));
      }

      //now corrupt First
      var cadr = data.First().Key;
      page.Data.Array[cadr] = 0xff;//corrupt underlying page memory
      data[cadr] = null;//corrupted

      //corrupt last
      cadr = data.Last().Key;
      page.Data.Array[cadr] = 0x00;//corrupt underlying page memory
      data[cadr] = null;//corrupted


      var keys = data.Keys.ToArray();
      //corrupt a half of written
      for(var i=0; i < data.Count / 2; i++)
      {
        cadr = keys[Platform.RandomGenerator.Instance.NextScaledRandomInteger(2, data.Count - 2)];
        page.Data.Array[cadr] = 0xff;//corrupt underlying page memory
        data[cadr] = null;//corrupted
      }

      "\nStream size is: {0:n0} bytes".SeeArgs(ms.Length);
      "{0:n0} total entries, {1:n0} are corrupt \n".SeeArgs(data.Count, data.Where(kvp => kvp.Value == null).Count());

      //Aver that all which are SET are still readable, others are corrupt
      foreach (var kvp in data)
      {
        var got = page[kvp.Key];
        if (kvp.Value != null)//was not corrupted
        {
          Aver.IsTrue(got.State == Entry.Status.Valid);
          Aver.IsTrue(IOUtils.MemBufferEquals(kvp.Value, got.Raw.ToArray()));
        }
        else
        {
          Aver.IsTrue(got.State == Entry.Status.BadHeader);
        }
      }

    }


    [Run("compress=null pgsz=1024  cnt=2000")]
    [Run("compress=null pgsz=16000 cnt=2000")]
    [Run("compress=gzip pgsz=1024  cnt=2000")]
    [Run("compress=gzip pgsz=16000 cnt=2000")]
    public void Corrupt_Volume_Read(string compress, int pgsz, int cnt)
    {
      var ms = new MemoryStream();
      var meta = VolumeMetadataBuilder.Make("String archive")
                                      .SetVersion(1, 0)
                                      .SetDescription("Testing string messages")
                                      .SetChannel(Atom.Encode("dvop"))
                                      .SetCompressionScheme(compress);

      var volume = new DefaultVolume(NOPApplication.Instance.SecurityManager.Cryptography, meta, ms)
      {
          PageSizeBytes = pgsz
      };

      using(var appender = new StringArchiveAppender(volume, NOPApplication.Instance.TimeSource, new Atom(65), "A@b.com"))
      {
        for(var i=0; i<cnt; i++)
        {
          appender.Append("string-data--------------------------------------------------------" + i.ToString());
        }
      }

      "Volume size is {0:n0} bytes".SeeArgs(ms.Length);

      var reader = new StringArchiveReader(volume);

      var allCount = reader.All.Count();
      Aver.AreEqual(cnt, allCount);

      reader.All.ForEach( (s, i) => Aver.IsTrue(s.EndsWith("---"+i.ToString())));

      var pageCount = reader.Pages(0).Count();
      Aver.IsTrue(pageCount > 0);

      "Before corruption: there are {0:n0} total records in {1:n0} pages".SeeArgs(allCount, pageCount);

      var midPoint = ms.Length / 2;
      ms.Position = midPoint;
      for(var j=0; j<pgsz*2; j++) ms.WriteByte(0x00);//corruption

      "-------------- corrupted {0:n0} bytes of {1:n0} total at {2:n0} position -----------  ".SeeArgs(pgsz*2, ms.Length, midPoint);

      var allCount2 = reader.Entries(new Bookmark(), true).Count();
      Aver.IsTrue( allCount > allCount2);

      var pageCount2 = reader.Pages(0, true).Count();
      Aver.IsTrue(pageCount > pageCount2);

      "After corruption: there are {0:n0} total records in {1:n0} pages".SeeArgs(allCount2, pageCount2);

      volume.Dispose();
    }



    [Run("pc=5 vsz=11000 pgsize=1024  compress=null  count=10   sz=998")]
    [Run("pc=2 vsz=11000 pgsize=9000  compress=null  count=10   sz=1000")]
    [Run("pc=1 vsz=11000 pgsize=16000 compress=null  count=10   sz=1000")]

    [Run("pc=5 vsz=1000 pgsize=1024  compress=gzip  count=10   sz=1000")]
    [Run("pc=2 vsz=1000 pgsize=9000  compress=gzip  count=10   sz=1000")]
    [Run("pc=1 vsz=1000 pgsize=16000 compress=gzip  count=10   sz=1000")]
    public void Write_Read_Compare_PageSplit(int pc, int vsz, int pgsize, string compress, int count, int sz)
    {
      var expected = Enumerable.Range(0, count).Select(_ => new string(' ', sz)).ToArray();
      var ms = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("String archive")
                                      .SetVersion(1, 0)
                                      .SetDescription("Testing string messages")
                                      .SetChannel(Atom.Encode("dvop"))
                                      .SetCompressionScheme(compress);

      var volume = new DefaultVolume(NOPApplication.Instance.SecurityManager.Cryptography, meta, ms);
      volume.PageSizeBytes = pgsize;

      using (var appender = new StringArchiveAppender(volume,
                                          NOPApplication.Instance.TimeSource,
                                          NOPApplication.Instance.AppId, "dima@zhaba"))
      {
        for (var i = 0; i < count; i++)
        {
          appender.Append(expected[i]);
        }
      }

      "Volume data stream is {0:n0} bytes".SeeArgs(ms.Length);
      Aver.IsTrue(ms.Length < vsz);

   //   ms.GetBuffer().ToHexDump().See();

      var reader = new StringArchiveReader(volume);

   //   reader.Pages(0).Select(p => (p.State, p.Entries.Count())).See("PAGES/Entries: ");


      var pageCount = reader.Pages(0).Count();
      "Volume page count is: {0}".SeeArgs(pageCount);
      Aver.AreEqual(pc, pageCount);



      var got = reader.Entries(new Bookmark()).ToArray();

      Aver.AreEqual(expected.Length, got.Length);
      for (int i = 0; i < count; i++)
      {
        Aver.AreEqual(expected[i], got[i]);
      }

      volume.Dispose();
    }

  }
}
