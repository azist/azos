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
using Azos.Log;
using System;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.IO.Archiving
{
  [Runnable]
  public class FactTests : CryptoTestBase
  {
    [Run("compress=null encrypt=null pgsize=1000 count=2")]
    [Run("compress=null encrypt=null pgsize=1000 count=100")]
    [Run("compress=null encrypt=null pgsize=1000 count=1000")]
    [Run("compress=null encrypt=null pgsize=1000 count=16000")]
    [Run("compress=null encrypt=null pgsize=1000 count=32000")]
    [Run("compress=null encrypt=null pgsize=1000000 count=32000")]

    [Run("compress=gzip encrypt=null pgsize=1000 count=2")]
    [Run("compress=gzip encrypt=null pgsize=1000 count=100")]
    [Run("compress=gzip encrypt=null pgsize=1000 count=1000")]
    [Run("compress=gzip encrypt=null pgsize=1000 count=16000")]
    [Run("compress=gzip encrypt=null pgsize=1000 count=32000")]
    [Run("compress=gzip encrypt=null pgsize=1000000 count=32000")]

    [Run("compress=null encrypt=aes1 pgsize=1000 count=2")]
    [Run("compress=null encrypt=aes1 pgsize=1000 count=100")]
    [Run("compress=null encrypt=aes1 pgsize=1000 count=1000")]
    [Run("compress=null encrypt=aes1 pgsize=1000 count=16000")]
    [Run("compress=null encrypt=aes1 pgsize=1000 count=32000")]
    [Run("compress=null encrypt=aes1 pgsize=1000000 count=32000")]

    [Run("compress=gzip encrypt=aes1 pgsize=1000 count=2")]
    [Run("compress=gzip encrypt=aes1 pgsize=1000 count=100")]
    [Run("compress=gzip encrypt=aes1 pgsize=1000 count=1000")]
    [Run("compress=gzip encrypt=aes1 pgsize=1000 count=16000")]
    [Run("compress=gzip encrypt=aes1 pgsize=1000 count=32000")]
    [Run("compress=gzip encrypt=aes1 pgsize=1000000 count=32000")]
    public void WriteRead(string compress, string encrypt, int count, int pgsize)
    {
      ////var ms = new FileStream("c:\\azos\\archive.far", FileMode.Create);//  new MemoryStream();
      var ms = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Fact archive", FactArchiveAppender.CONTENT_TYPE_FACTS)
                                            .SetVersion(1, 0)
                                            .SetDescription("Testing facts")
                                            .SetChannel(Atom.Encode("dvop"))
                                            .SetCompressionScheme(compress)
                                            .SetEncryptionScheme(encrypt);

      var volume = new DefaultVolume(CryptoMan, meta, ms)
      {
        PageSizeBytes = pgsize
      };

      var tp = Atom.Encode("fact1");
      var top = Atom.Encode("topic1");
      var chn = Atom.Encode("chn1");
      var app = Atom.Encode("app123");

      using(var appender = new FactArchiveAppender(volume,
                                          NOPApplication.Instance.TimeSource,
                                          NOPApplication.Instance.AppId, "dima@zhaba"))
      {

        for (var i=0; i<count; i++)
        {
          var fact = new Fact()
          {
            Id = Guid.NewGuid(),
            FactType = tp,
            Topic = top,
            Channel = chn,
            App = app,
            Gdid = new Data.GDID(7, 123456),
            Host = "my host",
            RecordType = MessageType.Info,
            RelatedId = i%5==0 ? Guid.Empty : Guid.NewGuid(),
            Source = 7890,
            UtcTimestamp = DateTime.UtcNow,
            Dimensions = new JsonDataMap{ {"id", i}},
            Metrics = new JsonDataMap { { "t2", i*2 }, { "t3", i * 3 }, {"bin", Ambient.Random.NextRandom16Bytes} }
          };

          if (i%3 == 0)
          {
            fact.SetAmorphousData(new JsonDataMap{ { "name", "Molly"+i }, {"i", i} });
          }

          appender.Append(fact);
        }
      }


      var reader = new FactArchiveReader(volume);

      Aver.AreEqual(count, reader.All.Count());
      Aver.AreEqual(count, reader.GetEntriesStartingAt(new Bookmark()).Count());
      Aver.AreEqual(count, reader.GetBookmarkedEntriesStartingAt(new Bookmark()).Count());
      Aver.AreEqual(count, reader.GetEntriesAsObjectsStartingAt(new Bookmark()).Count());
      Aver.AreEqual(count, reader.GetBookmarkedEntriesAsObjectsStartingAt(new Bookmark()).Count());

      reader.All.ForEach((f, i) =>
      {
        Aver.AreEqual(i, (int)f.Dimensions["id"]);
        Aver.AreEqual(i*2, (int)f.Metrics["t2"]);
        Aver.AreEqual(i*3, (int)f.Metrics["t3"]);
        Aver.AreEqual(top, f.Topic);
        Aver.AreEqual(tp, f.FactType);
        Aver.AreEqual(chn, f.Channel);
        Aver.AreEqual(app, f.App);

        Aver.AreEqual(new Data.GDID(7, 123456), f.Gdid);

        if (i % 5 == 0)
        {
          Aver.AreEqual(Guid.Empty, f.RelatedId);
        }
        else
        {
          Aver.AreNotEqual(Guid.Empty, f.RelatedId);
        }


        if (i%3==0)
        {
          Aver.IsTrue(f.HasAmorphousData);
          Aver.AreEqual(2, f.AmorphousData.Count);
          Aver.AreEqual("Molly" + i, (string)f.AmorphousData["name"]);
        }
        else
        {
          Aver.IsFalse(f.HasAmorphousData);
        }
      });

      reader.GetBookmarkedEntriesStartingAt(new Bookmark()).ForEach( (t, i) =>
      {
        var pointed = i < 500 ? reader.GetEntriesStartingAt(t.bm).First() : t.entry;
        Aver.AreEqual(i, (int)pointed.Dimensions["id"]);
        Aver.AreEqual(i * 2, (int)pointed.Metrics["t2"]);
        Aver.AreEqual(i * 3, (int)pointed.Metrics["t3"]);
      });

      volume.Dispose();
    }
  }
}
