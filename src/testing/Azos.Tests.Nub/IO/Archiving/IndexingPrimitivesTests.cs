/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.IO;
using System.Linq;

using Azos.Apps;
using Azos.Scripting;
using Azos.IO.Archiving;

namespace Azos.Tests.Nub.IO.Archiving
{
  [Runnable]
  public class IndexingPrimitivesTests : CryptoTestBase
  {
    [Run("!arch-mumbo-write", "compress=gzip   encrypt=aes1   cnt=1000")]// trun Azos.Tests.Nub.dll -r namespaces=*IO.Archiving* names=arch-mumbo-write
    public void Write_To_File(string compress, string encrypt, int CNT)
    {
      var msData = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.lar".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Create);
      var msIdxId = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.id.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Create);
      var msIdxCid = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.cid.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Create);
      var msIdxDid = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.did.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Create);
      var msIdxPn = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.pn.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Create);
      var msIdxLt = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.lt.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Create);
      var msIdxLn = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.ln.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Create);
      var msIdxAl = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.al.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Create);
      var msIdxCd = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.cd.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Create);
      var msIdxNt = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.nt.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Create);
      var msIdxAmt = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.amt.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Create);

      var meta = VolumeMetadataBuilder.Make("Perf")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);
      var volumeIdxId = new DefaultVolume(CryptoMan, meta, msIdxId);
      var volumeIdxCid = new DefaultVolume(CryptoMan, meta, msIdxCid);
      var volumeIdxDid = new DefaultVolume(CryptoMan, meta, msIdxDid);
      var volumeIdxPn = new DefaultVolume(CryptoMan, meta, msIdxPn);
      var volumeIdxLt = new DefaultVolume(CryptoMan, meta, msIdxLt);
      var volumeIdxLn = new DefaultVolume(CryptoMan, meta, msIdxLn);
      var volumeIdxAl = new DefaultVolume(CryptoMan, meta, msIdxAl);
      var volumeIdxCd = new DefaultVolume(CryptoMan, meta, msIdxCd);
      var volumeIdxNt = new DefaultVolume(CryptoMan, meta, msIdxNt);
      var volumeIdxAmt = new DefaultVolume(CryptoMan, meta, msIdxAmt);

      volumeData.PageSizeBytes = 1024 * 1024;
      volumeIdxId.PageSizeBytes = 128 * 1024;
      volumeIdxCid.PageSizeBytes = 128 * 1024;
      volumeIdxDid.PageSizeBytes = 128 * 1024;
      volumeIdxPn.PageSizeBytes = 128 * 1024;
      volumeIdxLt.PageSizeBytes = 128 * 1024;
      volumeIdxLn.PageSizeBytes = 128 * 1024;
      volumeIdxAl.PageSizeBytes = 128 * 1024;
      volumeIdxCd.PageSizeBytes = 128 * 1024;
      volumeIdxNt.PageSizeBytes = 128 * 1024;
      volumeIdxAmt.PageSizeBytes = 128 * 1024;

      var time = Azos.Time.Timeter.StartNew();

      var app = Azos.Apps.ExecutionContext.Application;

      using (var aIdxId = new GdidIdxAppender(volumeIdxId, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxCid = new GuidIdxAppender(volumeIdxCid, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxDid = new LongIdxAppender(volumeIdxDid, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxPn = new LongIdxAppender(volumeIdxPn, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxLt = new DoubleIdxAppender(volumeIdxLt, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxLn = new DoubleIdxAppender(volumeIdxLn, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxAl = new DecimalIdxAppender(volumeIdxAl, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxCd = new DateTimeIdxAppender(volumeIdxCd, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxNt = new StringIdxAppender(volumeIdxNt, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxAmt = new AmountIdxAppender(volumeIdxAmt, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      {
        using(var appender = new MumboJumboArchiveAppender(volumeData,
                                                NOPApplication.Instance.TimeSource,
                                                NOPApplication.Instance.AppId,
                                                "dima@zhaba",
                                                onPageCommit: (e, b) =>
                                                {
                                                  aIdxId.Append(new GdidBookmark(e.ID, b));
                                                  aIdxCid.Append(new GuidBookmark(e.CorrelationId, b));
                                                  aIdxDid.Append(new LongBookmark(e.DeviceId, b));
                                                  aIdxPn.Append(new LongBookmark(e.PartNumber, b));
                                                  aIdxLt.Append(new DoubleBookmark(e.Latitude, b));
                                                  aIdxLn.Append(new DoubleBookmark(e.Longitude, b));
                                                  aIdxAl.Append(new DecimalBookmark(e.Altitude, b));
                                                  aIdxCd.Append(new DateTimeBookmark(e.CreateDate, b));
                                                  aIdxNt.Append(new StringBookmark(e.Note, b));
                                                  aIdxAmt.Append(new AmountBookmark(e.Amt, b));
                                                } 
                                                ))
        {
          var messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5)-1);
          foreach (var m in messages)
          {
            appender.Append(m);
          }
          appender.Append(MumboJumbo.GetControl());
          messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5));
          foreach (var m in messages)
          {
            appender.Append(m);
          }
        }
      }

      time.Stop();
      "Did {0:n0} in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, time.ElapsedSec, CNT / time.ElapsedSec);

      volumeIdxId.Dispose();
      volumeIdxCid.Dispose();
      volumeIdxDid.Dispose();
      volumeIdxPn.Dispose();
      volumeIdxLt.Dispose();
      volumeIdxLn.Dispose();
      volumeIdxAl.Dispose();
      volumeIdxCd.Dispose();
      volumeIdxNt.Dispose();
      volumeIdxAmt.Dispose();
      volumeData.Dispose();

      "CLOSED all volumes\n".See();
    }

    [Run("!arch-mumbo-read", "compress=gzip   encrypt=aes1")]// trun Azos.Tests.Nub.dll -r namespaces=*IO.Archiving* names=arch-mumbo-read
    public void Read_From_File(string compress, string encrypt)
    {
      var ctlMumbo = MumboJumbo.GetControl();

      var msData = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.lar".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Open);
      var msIdxId = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.id.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Open);
      var msIdxCid = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.cid.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Open);
      var msIdxDid = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.did.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Open);
      var msIdxPn = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.pn.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Open);
      var msIdxLt = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.lt.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Open);
      var msIdxLn = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.ln.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Open);
      var msIdxAl = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.al.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Open);
      var msIdxCd = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.cd.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Open);
      var msIdxNt = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.nt.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Open);
      var msIdxAmt = new FileStream("c:\\azos\\mumbojumbo-{0}-{1}.amt.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Open);


      var volumeData = new DefaultVolume(CryptoMan, msData);
      var volumeIdxId = new DefaultVolume(CryptoMan, msIdxId);
      var volumeIdxCid = new DefaultVolume(CryptoMan, msIdxCid);
      var volumeIdxDid = new DefaultVolume(CryptoMan, msIdxDid);
      var volumeIdxPn = new DefaultVolume(CryptoMan, msIdxPn);
      var volumeIdxLt = new DefaultVolume(CryptoMan, msIdxLt);
      var volumeIdxLn = new DefaultVolume(CryptoMan, msIdxLn);
      var volumeIdxAl = new DefaultVolume(CryptoMan, msIdxAl);
      var volumeIdxCd = new DefaultVolume(CryptoMan, msIdxCd);
      var volumeIdxNt = new DefaultVolume(CryptoMan, msIdxNt);
      var volumeIdxAmt = new DefaultVolume(CryptoMan, msIdxAmt);

      var reader = new MumboJumboArchiveReader(volumeData);
      var idxIdReader = new GdidIdxReader(volumeIdxId);
      var idxCidReader = new GuidIdxReader(volumeIdxCid);
      var idxDidReader = new LongIdxReader(volumeIdxDid);
      var idxPnReader = new LongIdxReader(volumeIdxPn);
      var idxLtReader = new DoubleIdxReader(volumeIdxLt);
      var idxLnReader = new DoubleIdxReader(volumeIdxLn);
      var idxAlReader = new DecimalIdxReader(volumeIdxAl);
      var idxCdReader = new DateTimeIdxReader(volumeIdxCd);
      var idxNtReader = new StringIdxReader(volumeIdxNt);
      var idxAmtReader = new AmountIdxReader(volumeIdxAmt);

      var gotOne = false;

      // Find by GDID (ID)
      foreach (var idx in idxIdReader.All)
      {
        if (idx.Value == ctlMumbo.ID)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.ID, data.ID);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find ID by {nameof(GdidIdxReader)}");
      gotOne = false;

      // Find by Guid (CorrelationId)
      foreach (var idx in idxCidReader.All)
      {
        if (idx.Value == ctlMumbo.CorrelationId)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.CorrelationId, data.CorrelationId);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find CorrelationId by {nameof(GuidIdxReader)}");
      gotOne = false;

      // Find by Long (DeviceId)
      foreach (var idx in idxDidReader.All)
      {
        if (idx.Value == ctlMumbo.DeviceId)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.DeviceId, data.DeviceId);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find DeviceId by {nameof(LongIdxReader)}");
      gotOne = false;

      // Find by Long (PartNumber)
      foreach (var idx in idxPnReader.All)
      {
        if (idx.Value == ctlMumbo.PartNumber)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.PartNumber, data.PartNumber);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find PartNumber by {nameof(LongIdxReader)}");
      gotOne = false;

      // Find by Double (Latitude)
      foreach (var idx in idxLtReader.All)
      {
        if (idx.Value == ctlMumbo.Latitude)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.Latitude, data.Latitude);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find Latitude by {nameof(DoubleIdxReader)}");
      gotOne = false;

      // Find by Double (Longitude)
      foreach (var idx in idxLnReader.All)
      {
        if (idx.Value == ctlMumbo.Longitude)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.Longitude, data.Longitude);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find Longitude by {nameof(DoubleIdxReader)}");
      gotOne = false;

      // Find by Decimal (Altitude)
      foreach (var idx in idxAlReader.All)
      {
        if (idx.Value == ctlMumbo.Altitude)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.Altitude, data.Altitude);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find Altitude by {nameof(DecimalIdxReader)}");
      gotOne = false;

      // Find by DateTime (CreateDate)
      foreach (var idx in idxCdReader.All)
      {
        if (idx.Value == ctlMumbo.CreateDate)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.CreateDate, data.CreateDate);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find CreateDate by {nameof(DateTimeIdxReader)}");
      gotOne = false;

      // Find by String (Note)
      foreach (var idx in idxNtReader.All)
      {
        if (idx.Value == ctlMumbo.Note)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.Note, data.Note);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find Note by {nameof(StringIdxReader)}");
      gotOne = false;

      // Find by Amount (Amt)
      foreach (var idx in idxAmtReader.All)
      {
        if (idx.Value == ctlMumbo.Amt)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.Amt, data.Amt);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find Amt by {nameof(AmountIdxReader)}");
      gotOne = false;

      volumeIdxId.Dispose();
      volumeIdxCid.Dispose();
      volumeIdxDid.Dispose();
      volumeIdxPn.Dispose();
      volumeIdxLt.Dispose();
      volumeIdxLn.Dispose();
      volumeIdxAl.Dispose();
      volumeIdxCd.Dispose();
      volumeIdxNt.Dispose();
      volumeIdxAmt.Dispose();
      volumeData.Dispose();
      "CLOSED all volumes\n".See();
    }

    [Run("compress=null   encrypt=null   cnt=50000")]
    [Run("compress=gzip   encrypt=aes1   cnt=50000")]// trun Azos.Tests.Nub.dll -r namespaces=*IO.Archiving* methods=IndexingPrimitivesTests*
    public void Write_Read_Gdid_Indexing_MemoryStream(string compress, string encrypt, int CNT)
    {
      var msData = new MemoryStream();
      var msIdxId = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Perf")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);
      var volumeIdxId = new DefaultVolume(CryptoMan, meta, msIdxId);

      volumeData.PageSizeBytes = 1024 * 1024;
      volumeIdxId.PageSizeBytes = 128 * 1024;

      var timeWrite = Azos.Time.Timeter.StartNew();

      var app = Azos.Apps.ExecutionContext.Application;

      using (var aIdxId = new GdidIdxAppender(volumeIdxId, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      {
        using (var appender = new MumboJumboArchiveAppender(volumeData,
                                                NOPApplication.Instance.TimeSource,
                                                NOPApplication.Instance.AppId,
                                                "dima@zhaba",
                                                onPageCommit: (e, b) =>
                                                {
                                                  aIdxId.Append(new GdidBookmark(e.ID, b));
                                                }
                                                ))
        {
          var messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5) - 1);
          foreach (var m in messages)
          {
            appender.Append(m);
          }
          appender.Append(MumboJumbo.GetControl());
          messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5));
          foreach (var m in messages)
          {
            appender.Append(m);
          }
        }
      }

      timeWrite.Stop();
      "Did {0:n0} writes in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, timeWrite.ElapsedSec, CNT / timeWrite.ElapsedSec);

      var ctlMumbo = MumboJumbo.GetControl();

      var reader = new MumboJumboArchiveReader(volumeData);
      var idxIdReader = new GdidIdxReader(volumeIdxId);

      var timeRead = Azos.Time.Timeter.StartNew();

      var gotOne = false;

      // Find by GDID (ID)
      foreach (var idx in idxIdReader.All)
      {
        if (idx.Value == ctlMumbo.ID)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.ID, data.ID);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find ID by {nameof(GdidIdxReader)}");      

      timeRead.Stop();
      "Did {0:n0} reads in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, timeRead.ElapsedSec, CNT / timeRead.ElapsedSec);

      volumeIdxId.Dispose();
      volumeData.Dispose();

      "CLOSED all volumes\n".See();
    }

    [Run("compress=null   encrypt=null   cnt=50000")]
    [Run("compress=gzip   encrypt=aes1   cnt=50000")]// trun Azos.Tests.Nub.dll -r namespaces=*IO.Archiving* methods=IndexingPrimitivesTests*
    public void Write_Read_Guid_Indexing_MemoryStream(string compress, string encrypt, int CNT)
    {
      var msData = new MemoryStream();
      var msIdxCid = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Perf")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);
      var volumeIdxCid = new DefaultVolume(CryptoMan, meta, msIdxCid);

      volumeData.PageSizeBytes = 1024 * 1024;
      volumeIdxCid.PageSizeBytes = 128 * 1024;

      var timeWrite = Azos.Time.Timeter.StartNew();

      var app = Azos.Apps.ExecutionContext.Application;

      using (var aIdxCid = new GuidIdxAppender(volumeIdxCid, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      {
        using (var appender = new MumboJumboArchiveAppender(volumeData,
                                                NOPApplication.Instance.TimeSource,
                                                NOPApplication.Instance.AppId,
                                                "dima@zhaba",
                                                onPageCommit: (e, b) =>
                                                {
                                                  aIdxCid.Append(new GuidBookmark(e.CorrelationId, b));
                                                }
                                                ))
        {
          var messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5) - 1);
          foreach (var m in messages)
          {
            appender.Append(m);
          }
          appender.Append(MumboJumbo.GetControl());
          messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5));
          foreach (var m in messages)
          {
            appender.Append(m);
          }
        }
      }

      timeWrite.Stop();
      "Did {0:n0} writes in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, timeWrite.ElapsedSec, CNT / timeWrite.ElapsedSec);

      var ctlMumbo = MumboJumbo.GetControl();

      var reader = new MumboJumboArchiveReader(volumeData);
      var idxCidReader = new GuidIdxReader(volumeIdxCid);

      var timeRead = Azos.Time.Timeter.StartNew();

      var gotOne = false;

      // Find by Guid (CorrelationId)
      foreach (var idx in idxCidReader.All)
      {
        if (idx.Value == ctlMumbo.CorrelationId)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.CorrelationId, data.CorrelationId);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find CorrelationId by {nameof(GuidIdxReader)}");
      

      timeRead.Stop();
      "Did {0:n0} reads in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, timeRead.ElapsedSec, CNT / timeRead.ElapsedSec);

      volumeIdxCid.Dispose();
      volumeData.Dispose();

      "CLOSED all volumes\n".See();
    }

    [Run("compress=null   encrypt=null   cnt=50000")]
    [Run("compress=gzip   encrypt=aes1   cnt=50000")]// trun Azos.Tests.Nub.dll -r namespaces=*IO.Archiving* methods=IndexingPrimitivesTests*
    public void Write_Read_Long_Indexing_MemoryStream(string compress, string encrypt, int CNT)
    {
      var msData = new MemoryStream();
      var msIdxDid = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Perf")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);
      var volumeIdxDid = new DefaultVolume(CryptoMan, meta, msIdxDid);

      volumeData.PageSizeBytes = 1024 * 1024;
      volumeIdxDid.PageSizeBytes = 128 * 1024;

      var timeWrite = Azos.Time.Timeter.StartNew();

      var app = Azos.Apps.ExecutionContext.Application;

      using (var aIdxDid = new LongIdxAppender(volumeIdxDid, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      {
        using (var appender = new MumboJumboArchiveAppender(volumeData,
                                                NOPApplication.Instance.TimeSource,
                                                NOPApplication.Instance.AppId,
                                                "dima@zhaba",
                                                onPageCommit: (e, b) =>
                                                {
                                                  aIdxDid.Append(new LongBookmark(e.DeviceId, b));
                                                }
                                                ))
        {
          var messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5) - 1);
          foreach (var m in messages)
          {
            appender.Append(m);
          }
          appender.Append(MumboJumbo.GetControl());
          messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5));
          foreach (var m in messages)
          {
            appender.Append(m);
          }
        }
      }

      timeWrite.Stop();
      "Did {0:n0} writes in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, timeWrite.ElapsedSec, CNT / timeWrite.ElapsedSec);


      var ctlMumbo = MumboJumbo.GetControl();

      var reader = new MumboJumboArchiveReader(volumeData);
      var idxDidReader = new LongIdxReader(volumeIdxDid);

      var timeRead = Azos.Time.Timeter.StartNew();

      var gotOne = false;

      // Find by Long (DeviceId)
      foreach (var idx in idxDidReader.All)
      {
        if (idx.Value == ctlMumbo.DeviceId)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.DeviceId, data.DeviceId);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find DeviceId by {nameof(LongIdxReader)}");
      

      timeRead.Stop();
      "Did {0:n0} reads in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, timeRead.ElapsedSec, CNT / timeRead.ElapsedSec);

      volumeIdxDid.Dispose();
      volumeData.Dispose();

      "CLOSED all volumes\n".See();
    }

    [Run("compress=null   encrypt=null   cnt=50000")]
    [Run("compress=gzip   encrypt=aes1   cnt=50000")]// trun Azos.Tests.Nub.dll -r namespaces=*IO.Archiving* methods=IndexingPrimitivesTests*
    public void Write_Read_Double_Indexing_MemoryStream(string compress, string encrypt, int CNT)
    {
      var msData = new MemoryStream();
      var msIdxLt = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Perf")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);
      var volumeIdxLt = new DefaultVolume(CryptoMan, meta, msIdxLt);

      volumeData.PageSizeBytes = 1024 * 1024;
      volumeIdxLt.PageSizeBytes = 128 * 1024;

      var timeWrite = Azos.Time.Timeter.StartNew();

      var app = Azos.Apps.ExecutionContext.Application;

      using (var aIdxLt = new DoubleIdxAppender(volumeIdxLt, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      {
        using (var appender = new MumboJumboArchiveAppender(volumeData,
                                                NOPApplication.Instance.TimeSource,
                                                NOPApplication.Instance.AppId,
                                                "dima@zhaba",
                                                onPageCommit: (e, b) =>
                                                {
                                                  aIdxLt.Append(new DoubleBookmark(e.Latitude, b));
                                                }
                                                ))
        {
          var messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5) - 1);
          foreach (var m in messages)
          {
            appender.Append(m);
          }
          appender.Append(MumboJumbo.GetControl());
          messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5));
          foreach (var m in messages)
          {
            appender.Append(m);
          }
        }
      }

      timeWrite.Stop();
      "Did {0:n0} writes in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, timeWrite.ElapsedSec, CNT / timeWrite.ElapsedSec);


      var ctlMumbo = MumboJumbo.GetControl();

      var reader = new MumboJumboArchiveReader(volumeData);
      var idxLtReader = new DoubleIdxReader(volumeIdxLt);

      var timeRead = Azos.Time.Timeter.StartNew();

      var gotOne = false;

      // Find by Double (Latitude)
      foreach (var idx in idxLtReader.All)
      {
        if (idx.Value == ctlMumbo.Latitude)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.Latitude, data.Latitude);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find Latitude by {nameof(DoubleIdxReader)}");
      

      timeRead.Stop();
      "Did {0:n0} reads in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, timeRead.ElapsedSec, CNT / timeRead.ElapsedSec);

      volumeIdxLt.Dispose();
      volumeData.Dispose();

      "CLOSED all volumes\n".See();
    }

    [Run("compress=null   encrypt=null   cnt=50000")]
    [Run("compress=gzip   encrypt=aes1   cnt=50000")]// trun Azos.Tests.Nub.dll -r namespaces=*IO.Archiving* methods=IndexingPrimitivesTests*
    public void Write_Read_Decimal_Indexing_MemoryStream(string compress, string encrypt, int CNT)
    {
      var msData = new MemoryStream();
      var msIdxAl = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Perf")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);
      var volumeIdxAl = new DefaultVolume(CryptoMan, meta, msIdxAl);

      volumeData.PageSizeBytes = 1024 * 1024;
      volumeIdxAl.PageSizeBytes = 128 * 1024;

      var timeWrite = Azos.Time.Timeter.StartNew();

      var app = Azos.Apps.ExecutionContext.Application;

      using (var aIdxAl = new DecimalIdxAppender(volumeIdxAl, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      {
        using (var appender = new MumboJumboArchiveAppender(volumeData,
                                                NOPApplication.Instance.TimeSource,
                                                NOPApplication.Instance.AppId,
                                                "dima@zhaba",
                                                onPageCommit: (e, b) =>
                                                {
                                                  aIdxAl.Append(new DecimalBookmark(e.Altitude, b));
                                                }
                                                ))
        {
          var messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5) - 1);
          foreach (var m in messages)
          {
            appender.Append(m);
          }
          appender.Append(MumboJumbo.GetControl());
          messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5));
          foreach (var m in messages)
          {
            appender.Append(m);
          }
        }
      }

      timeWrite.Stop();
      "Did {0:n0} writes in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, timeWrite.ElapsedSec, CNT / timeWrite.ElapsedSec);

      var ctlMumbo = MumboJumbo.GetControl();

      var reader = new MumboJumboArchiveReader(volumeData);
      var idxAlReader = new DecimalIdxReader(volumeIdxAl);

      var timeRead = Azos.Time.Timeter.StartNew();

      var gotOne = false;       

      // Find by Decimal (Altitude)
      foreach (var idx in idxAlReader.All)
      {
        if (idx.Value == ctlMumbo.Altitude)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.Altitude, data.Altitude);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find Altitude by {nameof(DecimalIdxReader)}");
      

      timeRead.Stop();
      "Did {0:n0} reads in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, timeRead.ElapsedSec, CNT / timeRead.ElapsedSec);

      volumeIdxAl.Dispose();
      volumeData.Dispose();

      "CLOSED all volumes\n".See();
    }

    [Run("compress=null   encrypt=null   cnt=50000")]
    [Run("compress=gzip   encrypt=aes1   cnt=50000")]// trun Azos.Tests.Nub.dll -r namespaces=*IO.Archiving* methods=IndexingPrimitivesTests*
    public void Write_Read_DateTime_Indexing_MemoryStream(string compress, string encrypt, int CNT)
    {
      var msData = new MemoryStream();
      var msIdxCd = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Perf")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);
      var volumeIdxCd = new DefaultVolume(CryptoMan, meta, msIdxCd);

      volumeData.PageSizeBytes = 1024 * 1024;
      volumeIdxCd.PageSizeBytes = 128 * 1024;

      var timeWrite = Azos.Time.Timeter.StartNew();

      var app = Azos.Apps.ExecutionContext.Application;

      using (var aIdxCd = new DateTimeIdxAppender(volumeIdxCd, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      {
        using (var appender = new MumboJumboArchiveAppender(volumeData,
                                                NOPApplication.Instance.TimeSource,
                                                NOPApplication.Instance.AppId,
                                                "dima@zhaba",
                                                onPageCommit: (e, b) =>
                                                {
                                                  aIdxCd.Append(new DateTimeBookmark(e.CreateDate, b));
                                                }
                                                ))
        {
          var messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5) - 1);
          foreach (var m in messages)
          {
            appender.Append(m);
          }
          appender.Append(MumboJumbo.GetControl());
          messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5));
          foreach (var m in messages)
          {
            appender.Append(m);
          }
        }
      }

      timeWrite.Stop();
      "Did {0:n0} writes in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, timeWrite.ElapsedSec, CNT / timeWrite.ElapsedSec);


      var ctlMumbo = MumboJumbo.GetControl();

      var reader = new MumboJumboArchiveReader(volumeData);
      var idxCdReader = new DateTimeIdxReader(volumeIdxCd);

      var timeRead = Azos.Time.Timeter.StartNew();

      var gotOne = false;      

      // Find by DateTime (CreateDate)
      foreach (var idx in idxCdReader.All)
      {
        if (idx.Value == ctlMumbo.CreateDate)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.CreateDate, data.CreateDate);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find CreateDate by {nameof(DateTimeIdxReader)}");      

      timeRead.Stop();
      "Did {0:n0} reads in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, timeRead.ElapsedSec, CNT / timeRead.ElapsedSec);

      volumeIdxCd.Dispose();
      volumeData.Dispose();

      "CLOSED all volumes\n".See();
    }

    [Run("compress=null   encrypt=null   cnt=50000")]
    [Run("compress=gzip   encrypt=aes1   cnt=50000")]// trun Azos.Tests.Nub.dll -r namespaces=*IO.Archiving* methods=IndexingPrimitivesTests*
    public void Write_Read_String_Indexing_MemoryStream(string compress, string encrypt, int CNT)
    {
      var msData = new MemoryStream();
      var msIdxNt = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Perf")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);
      var volumeIdxNt = new DefaultVolume(CryptoMan, meta, msIdxNt);

      volumeData.PageSizeBytes = 1024 * 1024;
      volumeIdxNt.PageSizeBytes = 128 * 1024;

      var timeWrite = Azos.Time.Timeter.StartNew();

      var app = Azos.Apps.ExecutionContext.Application;

      using (var aIdxNt = new StringIdxAppender(volumeIdxNt, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      {
        using (var appender = new MumboJumboArchiveAppender(volumeData,
                                                NOPApplication.Instance.TimeSource,
                                                NOPApplication.Instance.AppId,
                                                "dima@zhaba",
                                                onPageCommit: (e, b) =>
                                                {
                                                  aIdxNt.Append(new StringBookmark(e.Note, b));
                                                }
                                                ))
        {
          var messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5) - 1);
          foreach (var m in messages)
          {
            appender.Append(m);
          }
          appender.Append(MumboJumbo.GetControl());
          messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5));
          foreach (var m in messages)
          {
            appender.Append(m);
          }
        }
      }

      timeWrite.Stop();
      "Did {0:n0} writes in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, timeWrite.ElapsedSec, CNT / timeWrite.ElapsedSec);

      var ctlMumbo = MumboJumbo.GetControl();

      var reader = new MumboJumboArchiveReader(volumeData);
      var idxNtReader = new StringIdxReader(volumeIdxNt);

      var timeRead = Azos.Time.Timeter.StartNew();

      var gotOne = false;
      
      // Find by String (Note)
      foreach (var idx in idxNtReader.All)
      {
        if (idx.Value == ctlMumbo.Note)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.Note, data.Note);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find Note by {nameof(StringIdxReader)}");
      gotOne = false;

      timeRead.Stop();
      "Did {0:n0} reads in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, timeRead.ElapsedSec, CNT / timeRead.ElapsedSec);

      volumeIdxNt.Dispose();
      volumeData.Dispose();

      "CLOSED all volumes\n".See();
    }

    [Run("compress=null   encrypt=null   cnt=50000")]
    [Run("compress=gzip   encrypt=aes1   cnt=50000")]// trun Azos.Tests.Nub.dll -r namespaces=*IO.Archiving* methods=IndexingPrimitivesTests*
    public void Write_Read_Amount_Indexing_MemoryStream(string compress, string encrypt, int CNT)
    {
      var msData = new MemoryStream();
      var msIdxAmt = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Perf")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);
      var volumeIdxAmt = new DefaultVolume(CryptoMan, meta, msIdxAmt);

      volumeData.PageSizeBytes = 1024 * 1024;
      volumeIdxAmt.PageSizeBytes = 128 * 1024;

      var timeWrite = Azos.Time.Timeter.StartNew();

      var app = Azos.Apps.ExecutionContext.Application;

      using (var aIdxAmt = new AmountIdxAppender(volumeIdxAmt, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      {
        using (var appender = new MumboJumboArchiveAppender(volumeData,
                                                NOPApplication.Instance.TimeSource,
                                                NOPApplication.Instance.AppId,
                                                "dima@zhaba",
                                                onPageCommit: (e, b) =>
                                                {
                                                  aIdxAmt.Append(new AmountBookmark(e.Amt, b));
                                                }
                                                ))
        {
          var messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5) - 1);
          foreach (var m in messages)
          {
            appender.Append(m);
          }
          appender.Append(MumboJumbo.GetControl());
          messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5));
          foreach (var m in messages)
          {
            appender.Append(m);
          }
        }
      }

      timeWrite.Stop();
      "Did {0:n0} writes in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, timeWrite.ElapsedSec, CNT / timeWrite.ElapsedSec);


      var ctlMumbo = MumboJumbo.GetControl();

      var reader = new MumboJumboArchiveReader(volumeData);
      var idxAmtReader = new AmountIdxReader(volumeIdxAmt);

      var timeRead = Azos.Time.Timeter.StartNew();

      var gotOne = false;

      // Find by Amount (Amt)
      foreach (var idx in idxAmtReader.All)
      {
        if (idx.Value == ctlMumbo.Amt)
        {
          var data = reader.Entries(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.Amt, data.Amt);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find Amt by {nameof(AmountIdxReader)}");

      timeRead.Stop();
      "Did {0:n0} reads in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, timeRead.ElapsedSec, CNT / timeRead.ElapsedSec);

      volumeIdxAmt.Dispose();
      volumeData.Dispose();

      "CLOSED all volumes\n".See();
    }
  }
}
