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
using Azos.Pile;

namespace Azos.Tests.Nub.IO.Archiving
{
  [Runnable]
  public class IndexingPrimitivesTests : CryptoTestBase
  {
    // trun Azos.Tests.Nub.dll -r namespaces=*IO.Archiving* methods=IndexingPrimitivesTests*
    [Run("usecache=false compress=gzip   encrypt=aes1   cnt=10    idxCompress=null   idxEncrypt=null   pageLboundKb=8    pageUboundKb=16     idxLboundKb=4     idxUboundKb=8")]
    [Run("usecache=false compress=gzip   encrypt=aes1   cnt=100   idxCompress=null   idxEncrypt=null   pageLboundKb=64   pageUboundKb=512    idxLboundKb=8     idxUboundKb=64")]
    [Run("usecache=false compress=gzip   encrypt=aes1   cnt=250   idxCompress=null   idxEncrypt=null   pageLboundKb=512  pageUboundKb=2048   idxLboundKb=128   idxUboundKb=512")]

    [Run("usecache=false compress=null   encrypt=null   cnt=10    idxCompress=gzip   idxEncrypt=aes1   pageLboundKb=8    pageUboundKb=16     idxLboundKb=4     idxUboundKb=8")]
    [Run("usecache=false compress=null   encrypt=null   cnt=100   idxCompress=gzip   idxEncrypt=aes1   pageLboundKb=64   pageUboundKb=512    idxLboundKb=8     idxUboundKb=64")]
    [Run("usecache=false compress=null   encrypt=null   cnt=250   idxCompress=gzip   idxEncrypt=aes1   pageLboundKb=512  pageUboundKb=2048   idxLboundKb=128   idxUboundKb=512")]

    [Run("usecache=false compress=gzip   encrypt=null   cnt=10    idxCompress=gzip   idxEncrypt=null   pageLboundKb=8    pageUboundKb=16     idxLboundKb=4     idxUboundKb=8")]
    [Run("usecache=false compress=gzip   encrypt=null   cnt=100   idxCompress=gzip   idxEncrypt=null   pageLboundKb=64   pageUboundKb=512    idxLboundKb=8     idxUboundKb=64")]
    [Run("usecache=false compress=gzip   encrypt=null   cnt=250   idxCompress=gzip   idxEncrypt=null   pageLboundKb=512  pageUboundKb=2048   idxLboundKb=128   idxUboundKb=512")]

    [Run("usecache=false compress=null   encrypt=aes1   cnt=10    idxCompress=null   idxEncrypt=aes1   pageLboundKb=8    pageUboundKb=16     idxLboundKb=4     idxUboundKb=8")]
    [Run("usecache=false compress=null   encrypt=aes1   cnt=100   idxCompress=null   idxEncrypt=aes1   pageLboundKb=64   pageUboundKb=512    idxLboundKb=8     idxUboundKb=64")]
    [Run("usecache=false compress=null   encrypt=aes1   cnt=250   idxCompress=null   idxEncrypt=aes1   pageLboundKb=512  pageUboundKb=2048   idxLboundKb=128   idxUboundKb=512")]


    [Run("usecache=false compress=gzip   encrypt=null   cnt=10000   idxCompress=gzip   idxEncrypt=aes1   pageLboundKb=4   pageUboundKb=4    idxLboundKb=8     idxUboundKb=8")]
    [Run("usecache=true compress=gzip   encrypt=null   cnt=10000   idxCompress=gzip   idxEncrypt=aes1   pageLboundKb=4   pageUboundKb=4    idxLboundKb=8     idxUboundKb=8")]
    public void Write_Read_Index_Primitives(bool usecache, string compress, string encrypt, int CNT, string idxCompress, string idxEncrypt, int pageLboundKb, int pageUboundKb, int idxLboundKb, int idxUboundKb)
    {
      var ctlMumbo = MumboJumbo.GetControl();

      IPileImplementation pile = null;
      ICacheImplementation cache = null;
      IPageCache pageCache = null;
      if (usecache)
      {
        pile = new DefaultPile(App);
        cache = new LocalCache(App)
        {
          Pile = pile,
          DefaultTableOptions = new TableOptions("*") { CollisionMode = CollisionMode.Durable },
          PileMaxMemoryLimit = 32L * 1024 * 1024 * 1024
        };
        pile.Start();
        cache.Start();
        pageCache = new PilePageCache(cache);
      }
try
{

      var msData = new MemoryStream();
      var msIdxId =  new MemoryStream();
      var msIdxCid =  new MemoryStream();
      var msIdxDid =  new MemoryStream();
      var msIdxPn =  new MemoryStream();
      var msIdxLt =  new MemoryStream();
      var msIdxLn =  new MemoryStream();
      var msIdxAl =  new MemoryStream();
      var msIdxCd =  new MemoryStream();
      var msIdxNt =  new MemoryStream();
      var msIdxAmt =  new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Primitive Idx", "mjumbo")
                                .SetVersion(1, 1)
                                .SetDescription("MumboJumbo testing")
                                .SetCompressionScheme(compress)
                                .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, pageCache, meta, msData);

      var metaIdx = VolumeMetadataBuilder.Make("Primitive Idx Meta", "index-this-needs to be done by type")
                                      .SetVersion(1, 1)
                                      .SetDescription("MumboJumbo testing")
                                      .SetCompressionScheme(idxCompress)
                                      .SetEncryptionScheme(idxEncrypt);

      var volumeIdxId  = new DefaultVolume(CryptoMan, metaIdx, msIdxId);
      var volumeIdxCid = new DefaultVolume(CryptoMan, metaIdx, msIdxCid);
      var volumeIdxDid = new DefaultVolume(CryptoMan, metaIdx, msIdxDid);
      var volumeIdxPn  = new DefaultVolume(CryptoMan, metaIdx, msIdxPn);
      var volumeIdxLt  = new DefaultVolume(CryptoMan, metaIdx, msIdxLt);
      var volumeIdxLn  = new DefaultVolume(CryptoMan, metaIdx, msIdxLn);
      var volumeIdxAl  = new DefaultVolume(CryptoMan, metaIdx, msIdxAl);
      var volumeIdxCd  = new DefaultVolume(CryptoMan, metaIdx, msIdxCd);
      var volumeIdxNt  = new DefaultVolume(CryptoMan, metaIdx, msIdxNt);
      var volumeIdxAmt = new DefaultVolume(CryptoMan, metaIdx, msIdxAmt);

      volumeData.PageSizeBytes = Ambient.Random.NextScaledRandomInteger(pageLboundKb, pageUboundKb) * 1024;

      volumeIdxId.PageSizeBytes  = Ambient.Random.NextScaledRandomInteger(idxLboundKb, idxUboundKb) * 1024;
      volumeIdxCid.PageSizeBytes = Ambient.Random.NextScaledRandomInteger(idxLboundKb, idxUboundKb) * 1024;
      volumeIdxDid.PageSizeBytes = Ambient.Random.NextScaledRandomInteger(idxLboundKb, idxUboundKb) * 1024;
      volumeIdxPn.PageSizeBytes  = Ambient.Random.NextScaledRandomInteger(idxLboundKb, idxUboundKb) * 1024;
      volumeIdxLt.PageSizeBytes  = Ambient.Random.NextScaledRandomInteger(idxLboundKb, idxUboundKb) * 1024;
      volumeIdxLn.PageSizeBytes  = Ambient.Random.NextScaledRandomInteger(idxLboundKb, idxUboundKb) * 1024;
      volumeIdxAl.PageSizeBytes  = Ambient.Random.NextScaledRandomInteger(idxLboundKb, idxUboundKb) * 1024;
      volumeIdxCd.PageSizeBytes  = Ambient.Random.NextScaledRandomInteger(idxLboundKb, idxUboundKb) * 1024;
      volumeIdxNt.PageSizeBytes  = Ambient.Random.NextScaledRandomInteger(idxLboundKb, idxUboundKb) * 1024;
      volumeIdxAmt.PageSizeBytes = Ambient.Random.NextScaledRandomInteger(idxLboundKb, idxUboundKb) * 1024;

      var reader = new MumboJumboArchiveReader(volumeData);
      var idxIdReader = new GdidIdxReader(volumeIdxId);
      var idxCidReader = new GuidIdxReader(volumeIdxCid);
      var idxDidReader = new LongIdxReader(volumeIdxDid);
      var idxPnReader = new IntIdxReader(volumeIdxPn);
      var idxLtReader = new DoubleIdxReader(volumeIdxLt);
      var idxLnReader = new DoubleIdxReader(volumeIdxLn);
      var idxAlReader = new DecimalIdxReader(volumeIdxAl);
      var idxCdReader = new DateTimeIdxReader(volumeIdxCd);
      var idxNtReader = new StringIdxReader(volumeIdxNt);
      var idxAmtReader = new AmountIdxReader(volumeIdxAmt);


      var time = Azos.Time.Timeter.StartNew();

      var app = Azos.Apps.ExecutionContext.Application;

      using (var aIdxId = new GdidIdxAppender(volumeIdxId, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxCid = new GuidIdxAppender(volumeIdxCid, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxDid = new LongIdxAppender(volumeIdxDid, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxPn = new IntIdxAppender(volumeIdxPn, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxLt = new DoubleIdxAppender(volumeIdxLt, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxLn = new DoubleIdxAppender(volumeIdxLn, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxAl = new DecimalIdxAppender(volumeIdxAl, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxCd = new DateTimeIdxAppender(volumeIdxCd, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxNt = new StringIdxAppender(volumeIdxNt, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      using (var aIdxAmt = new AmountIdxAppender(volumeIdxAmt, NOPApplication.Instance.TimeSource, NOPApplication.Instance.AppId, "dima@zhaba"))
      {
        using (var appender = new MumboJumboArchiveAppender(volumeData,
                                                NOPApplication.Instance.TimeSource,
                                                NOPApplication.Instance.AppId,
                                                "dima@zhaba",
                                                onPageCommit: (e, b) =>
                                                {
                                                  aIdxId.Append(new GdidBookmark(e.ID, b));
                                                  aIdxCid.Append(new GuidBookmark(e.CorrelationId, b));
                                                  aIdxDid.Append(new LongBookmark(e.DeviceId, b));
                                                  aIdxPn.Append(new IntBookmark(e.PartNumber, b));
                                                  aIdxLt.Append(new DoubleBookmark(e.Latitude, b));
                                                  aIdxLn.Append(new DoubleBookmark(e.Longitude, b));
                                                  aIdxAl.Append(new DecimalBookmark(e.Altitude, b));
                                                  aIdxCd.Append(new DateTimeBookmark(e.CreateDate, b));
                                                  aIdxNt.Append(new StringBookmark(e.Note, b));
                                                  aIdxAmt.Append(new AmountBookmark(e.Amt, b));
                                                }))
        {
          var messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5) - 1);
          foreach (var m in messages)
          {
            if (!App.Active) break;
            appender.Append(m);
          }
          ///////////////////////////////////////////////
          appender.Append(MumboJumbo.GetControl()); //we add 1 control message in the middle
          ///////////////////////////////////////////////
          messages = FakeRow.GenerateMany<MumboJumbo>(1, 1, (ulong)(CNT * .5));
          foreach (var m in messages)
          {
            if (!App.Active) break;
            appender.Append(m);
          }
        }
      }

      "Wrote {0} items, now reading".SeeArgs(CNT);
      if (cache!=null)
       "Cache has {0} items".SeeArgs(cache.Count);

      var gotOne = false;

      // Find by GDID (ID)
      foreach (var idx in idxIdReader.All)
      {
        if (!App.Active) break;
        var data = reader.GetEntriesStartingAt(idx.Bookmark).First();
        Aver.AreEqual(data.ID, idx.Value);

        if (idx.Value == ctlMumbo.ID)
        {
          //data.See();
          Aver.AreEqual(ctlMumbo.ID, data.ID);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find ID by {nameof(GdidIdxReader)}");
      "Finished reading {0}".SeeArgs(nameof(GdidIdxReader));

      gotOne = false;



      // Find by Guid (CorrelationId)
      foreach (var idx in idxCidReader.All)
      {
        if (!App.Active) break;
        var data = reader.GetEntriesStartingAt(idx.Bookmark).First();
        Aver.AreEqual(data.CorrelationId, idx.Value);

        if (idx.Value == ctlMumbo.CorrelationId)
        {
          //data.See();
          Aver.AreEqual(ctlMumbo.CorrelationId, data.CorrelationId);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find CorrelationId by {nameof(GuidIdxReader)}");
      "Finished reading {0}".SeeArgs(nameof(GuidIdxReader));
      gotOne = false;

      // Find by Long (DeviceId)
      foreach (var idx in idxDidReader.All)
      {
        if (!App.Active) break;
        var data = reader.GetEntriesStartingAt(idx.Bookmark).First();
        Aver.AreEqual(data.DeviceId, idx.Value);

        if (idx.Value == ctlMumbo.DeviceId)
        {
          //data.See();
          Aver.AreEqual(ctlMumbo.DeviceId, data.DeviceId);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find DeviceId by {nameof(LongIdxReader)}");
      "Finished reading {0}".SeeArgs(nameof(LongIdxReader));
      gotOne = false;

      // Find by Int (PartNumber)
      foreach (var idx in idxPnReader.All)
      {
        if (!App.Active) break;
        var data = reader.GetEntriesStartingAt(idx.Bookmark).First();
        Aver.AreEqual(data.PartNumber, idx.Value);

        if (idx.Value == ctlMumbo.PartNumber)
        {
          //data.See();
          Aver.AreEqual(ctlMumbo.PartNumber, data.PartNumber);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find PartNumber by {nameof(IntIdxReader)}");
      "Finished reading {0}".SeeArgs(nameof(IntIdxReader));
      gotOne = false;

      // Find by Double (Latitude)
      foreach (var idx in idxLtReader.All)
      {
        if (!App.Active) break;
        var data = reader.GetEntriesStartingAt(idx.Bookmark).First();
        Aver.AreEqual(data.Latitude, idx.Value);

        if (idx.Value == ctlMumbo.Latitude)
        {
          //data.See();
          Aver.AreEqual(ctlMumbo.Latitude, data.Latitude);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find Latitude by {nameof(DoubleIdxReader)}");
      "Finished reading {0}".SeeArgs(nameof(DoubleIdxReader));
      gotOne = false;

      // Find by Double (Longitude)
      foreach (var idx in idxLnReader.All)
      {
        if (!App.Active) break;
        var data = reader.GetEntriesStartingAt(idx.Bookmark).First();
        Aver.AreEqual(data.Longitude, idx.Value);

        if (idx.Value == ctlMumbo.Longitude)
        {
          //data.See();
          Aver.AreEqual(ctlMumbo.Longitude, data.Longitude);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find Longitude by {nameof(DoubleIdxReader)}");
      "Finished reading {0}".SeeArgs(nameof(DoubleIdxReader));
      gotOne = false;

      // Find by Decimal (Altitude)
      foreach (var idx in idxAlReader.All)
      {
        if (!App.Active) break;
        var data = reader.GetEntriesStartingAt(idx.Bookmark).First();
        Aver.AreEqual(data.Altitude, idx.Value);

        if (idx.Value == ctlMumbo.Altitude)
        {
          //data.See();
          Aver.AreEqual(ctlMumbo.Altitude, data.Altitude);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find Altitude by {nameof(DecimalIdxReader)}");
      "Finished reading {0}".SeeArgs(nameof(DecimalIdxReader));
      gotOne = false;

      // Find by DateTime (CreateDate)
      foreach (var idx in idxCdReader.All)
      {
        if (!App.Active) break;
        var data = reader.GetEntriesStartingAt(idx.Bookmark).First();
        Aver.AreEqual(data.CreateDate, idx.Value);

        if (idx.Value == ctlMumbo.CreateDate)
        {
          //data.See();
          Aver.AreEqual(ctlMumbo.CreateDate, data.CreateDate);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find CreateDate by {nameof(DateTimeIdxReader)}");
      "Finished reading {0}".SeeArgs(nameof(DateTimeIdxReader));
      gotOne = false;

      // Find by String (Note)
      foreach (var idx in idxNtReader.All)
      {
        if (!App.Active) break;
        var data = reader.GetEntriesStartingAt(idx.Bookmark).First();
        Aver.AreEqual(data.Note, idx.Value);

        if (idx.Value == ctlMumbo.Note)
        {
          //data.See();
          Aver.AreEqual(ctlMumbo.Note, data.Note);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find Note by {nameof(StringIdxReader)}");
      "Finished reading {0}".SeeArgs(nameof(StringIdxReader));
      gotOne = false;

      // Find by Amount (Amt)
      foreach (var idx in idxAmtReader.All)
      {
        if (!App.Active) break;
        var data = reader.GetEntriesStartingAt(idx.Bookmark).First();
        Aver.AreEqual(data.Amt, idx.Value);

        if (idx.Value == ctlMumbo.Amt)
        {
          //data.See();
          Aver.AreEqual(ctlMumbo.Amt, data.Amt);
          gotOne = true;
          break;
        }
      }
      if (!gotOne) Aver.Fail($"Failed to find Amt by {nameof(AmountIdxReader)}");
      "Finished reading {0}".SeeArgs(nameof(AmountIdxReader));
      //gotOne = false;


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
}
finally
{
      DisposableObject.DisposeIfDisposableAndNull(ref pageCache);
      DisposableObject.DisposeIfDisposableAndNull(ref cache);
      DisposableObject.DisposeIfDisposableAndNull(ref pile);
      "CLOSED all volumes\n".See();
}
    }
  }
}
