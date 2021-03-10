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
  public class IndexingPrimitivesPerfTests : CryptoTestBase
  {
    // trun Azos.Tests.Nub.dll -r namespaces=*IO.Archiving* names=mumbo-idx-perf

    [Run("!mumbo-idx-perf", "compress=null   encrypt=null   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=gzip   idxEncrypt=aes1")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    public void Write_Read_Gdid_Indexing_MemoryStream(string compress, string encrypt, int CNT, string idxCompress, string idxEncrypt)
    {
      var msData = new MemoryStream();
      var msIdxId = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Primitive Idx", "mjumbo")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);

      var metaIdx = VolumeMetadataBuilder.Make("Primitive Idx Meta", GdidIdxAppender.CONTENT_TYPE_IDX_GDID)
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(idxCompress)
                                      .SetEncryptionScheme(idxEncrypt);

      var volumeIdxId = new DefaultVolume(CryptoMan, metaIdx, msIdxId);

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
          var data = reader.GetEntriesStartingAt(idx.Bookmark).FirstOrDefault();
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

    [Run("!mumbo-idx-perf", "compress=null   encrypt=null   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=gzip   idxEncrypt=aes1")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    public void Write_Read_Guid_Indexing_MemoryStream(string compress, string encrypt, int CNT, string idxCompress, string idxEncrypt)
    {
      var msData = new MemoryStream();
      var msIdxCid = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Primitive Idx", "mjumbo")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);

      var metaIdx = VolumeMetadataBuilder.Make("Primitive Idx Meta", GuidIdxAppender.CONTENT_TYPE_IDX_GUID)
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(idxCompress)
                                      .SetEncryptionScheme(idxEncrypt);

      var volumeIdxCid = new DefaultVolume(CryptoMan, metaIdx, msIdxCid);

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
          var data = reader.GetEntriesStartingAt(idx.Bookmark).FirstOrDefault();
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

    [Run("!mumbo-idx-perf", "compress=null   encrypt=null   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=gzip   idxEncrypt=aes1")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    public void Write_Read_Long_Indexing_MemoryStream(string compress, string encrypt, int CNT, string idxCompress, string idxEncrypt)
    {
      var msData = new MemoryStream();
      var msIdxDid = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Primitive Idx", "mjumbo")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);

      var metaIdx = VolumeMetadataBuilder.Make("Primitive Idx Meta", LongIdxAppender.CONTENT_TYPE_IDX_LONG)
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(idxCompress)
                                      .SetEncryptionScheme(idxEncrypt);

      var volumeIdxDid = new DefaultVolume(CryptoMan, metaIdx, msIdxDid);

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
          var data = reader.GetEntriesStartingAt(idx.Bookmark).FirstOrDefault();
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

    [Run("!mumbo-idx-perf", "compress=null   encrypt=null   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=gzip   idxEncrypt=aes1")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    public void Write_Read_Int_Indexing_MemoryStream(string compress, string encrypt, int CNT, string idxCompress, string idxEncrypt)
    {
      var msData = new MemoryStream();
      var msIdxPn = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Primitive Idx", "mjumbo")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);

      var metaIdx = VolumeMetadataBuilder.Make("Primitive Idx Meta", IntIdxAppender.CONTENT_TYPE_IDX_INT)
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(idxCompress)
                                      .SetEncryptionScheme(idxEncrypt);

      var volumeIdxPn = new DefaultVolume(CryptoMan, metaIdx, msIdxPn);

      volumeData.PageSizeBytes = 1024 * 1024;
      volumeIdxPn.PageSizeBytes = 128 * 1024;

      var timeWrite = Azos.Time.Timeter.StartNew();

      var app = Azos.Apps.ExecutionContext.Application;

      using (var aIdxPn = new IntIdxAppender(volumeIdxPn, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      {
        using (var appender = new MumboJumboArchiveAppender(volumeData,
                                                NOPApplication.Instance.TimeSource,
                                                NOPApplication.Instance.AppId,
                                                "dima@zhaba",
                                                onPageCommit: (e, b) =>
                                                {
                                                  aIdxPn.Append(new IntBookmark(e.PartNumber, b));
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
      var idxPnReader = new IntIdxReader(volumeIdxPn);

      var timeRead = Azos.Time.Timeter.StartNew();

      var gotOne = false;

      // Find by Int (PartNumber)
      foreach (var idx in idxPnReader.All)
      {
        if (idx.Value == ctlMumbo.PartNumber)
        {
          var data = reader.GetEntriesStartingAt(idx.Bookmark).FirstOrDefault();
          data.See();
          Aver.AreEqual(ctlMumbo.PartNumber, data.PartNumber);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find DeviceId by {nameof(IntIdxReader)}");


      timeRead.Stop();
      "Did {0:n0} reads in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, timeRead.ElapsedSec, CNT / timeRead.ElapsedSec);

      volumeIdxPn.Dispose();
      volumeData.Dispose();

      "CLOSED all volumes\n".See();
    }

    [Run("!mumbo-idx-perf", "compress=null   encrypt=null   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=gzip   idxEncrypt=aes1")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    public void Write_Read_Double_Indexing_MemoryStream(string compress, string encrypt, int CNT, string idxCompress, string idxEncrypt)
    {
      var msData = new MemoryStream();
      var msIdxLt = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Primitive Idx", "mjumbo")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);

      var metaIdx = VolumeMetadataBuilder.Make("Primitive Idx Meta", DoubleIdxAppender.CONTENT_TYPE_IDX_DOUBLE)
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(idxCompress)
                                      .SetEncryptionScheme(idxEncrypt);

      var volumeIdxLt = new DefaultVolume(CryptoMan, metaIdx, msIdxLt);

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
          var data = reader.GetEntriesStartingAt(idx.Bookmark).FirstOrDefault();
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

    [Run("!mumbo-idx-perf", "compress=null   encrypt=null   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=gzip   idxEncrypt=aes1")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    public void Write_Read_Decimal_Indexing_MemoryStream(string compress, string encrypt, int CNT, string idxCompress, string idxEncrypt)
    {
      var msData = new MemoryStream();
      var msIdxAl = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Primitive Idx", "mjumbo")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);

      var metaIdx = VolumeMetadataBuilder.Make("Primitive Idx Meta", DecimalIdxAppender.CONTENT_TYPE_IDX_DECIMAL)
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(idxCompress)
                                      .SetEncryptionScheme(idxEncrypt);

      var volumeIdxAl = new DefaultVolume(CryptoMan, metaIdx, msIdxAl);

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
          var data = reader.GetEntriesStartingAt(idx.Bookmark).FirstOrDefault();
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

    [Run("!mumbo-idx-perf", "compress=null   encrypt=null   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=gzip   idxEncrypt=aes1")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    public void Write_Read_DateTime_Indexing_MemoryStream(string compress, string encrypt, int CNT, string idxCompress, string idxEncrypt)
    {
      var msData = new MemoryStream();
      var msIdxCd = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Primitive Idx", "mjumbo")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);

      var metaIdx = VolumeMetadataBuilder.Make("Primitive Idx Meta", DateTimeIdxAppender.CONTENT_TYPE_IDX_DATETIME)
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(idxCompress)
                                      .SetEncryptionScheme(idxEncrypt);

      var volumeIdxCd = new DefaultVolume(CryptoMan, metaIdx, msIdxCd);

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
          var data = reader.GetEntriesStartingAt(idx.Bookmark).FirstOrDefault();
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

    [Run("!mumbo-idx-perf", "compress=null   encrypt=null   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=gzip   idxEncrypt=aes1")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    public void Write_Read_String_Indexing_MemoryStream(string compress, string encrypt, int CNT, string idxCompress, string idxEncrypt)
    {
      var msData = new MemoryStream();
      var msIdxNt = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Primitive Idx", "mjumbo")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);

      var metaIdx = VolumeMetadataBuilder.Make("Primitive Idx Meta", StringIdxAppender.CONTENT_TYPE_IDX_STRING)
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(idxCompress)
                                      .SetEncryptionScheme(idxEncrypt);

      var volumeIdxNt = new DefaultVolume(CryptoMan, metaIdx, msIdxNt);

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
          var data = reader.GetEntriesStartingAt(idx.Bookmark).FirstOrDefault();
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

    [Run("!mumbo-idx-perf", "compress=null   encrypt=null   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=gzip   idxEncrypt=aes1")]
    [Run("!mumbo-idx-perf", "compress=gzip   encrypt=aes1   cnt=1000000   idxCompress=null   idxEncrypt=null")]
    public void Write_Read_Amount_Indexing_MemoryStream(string compress, string encrypt, int CNT, string idxCompress, string idxEncrypt)
    {
      var msData = new MemoryStream();
      var msIdxAmt = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Primitive Idx", "mjumbo")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);

      var metaIdx = VolumeMetadataBuilder.Make("Primitive Idx Meta", AmountIdxAppender.CONTENT_TYPE_IDX_AMOUNT)
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(idxCompress)
                                      .SetEncryptionScheme(idxEncrypt);

      var volumeIdxAmt = new DefaultVolume(CryptoMan, metaIdx, msIdxAmt);

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
          var data = reader.GetEntriesStartingAt(idx.Bookmark).FirstOrDefault();
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
