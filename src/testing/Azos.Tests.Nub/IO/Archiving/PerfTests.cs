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
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.IO.Archiving
{
  [Runnable]
  public class PerfTests : CryptoTestBase
  {

    [Run("!arch-perf-write", "compress=null   encrypt=null   cnt=16000000 para=16")]
    //[Run("!arch-perf-write", "compress=gzip   encrypt=aes1   cnt=16000000 para=16")]
    public void Write_LogMessages(string compress, string encrypt, int CNT, int PARA)
    {
      var msData = new FileStream("c:\\azos\\logging-{0}-{1}.lar".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Create);
      var msIdxId = new FileStream("c:\\azos\\logging-{0}-{1}.guid.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Create);

      var meta = VolumeMetadataBuilder.Make("Perf")
                                      .SetVersion(1, 1)
                                      .SetDescription("Perf testing")
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volumeData = new DefaultVolume(CryptoMan, meta, msData);
      var volumeIdxId = new DefaultVolume(CryptoMan, meta, msIdxId);


      volumeData.PageSizeBytes = 1024 * 1024;
      volumeIdxId.PageSizeBytes = 128 * 1024;

      var time = Azos.Time.Timeter.StartNew();


      Parallel.For(0, PARA, _ => {

        var app = Azos.Apps.ExecutionContext.Application;

        using(var aIdxId = new StringIdxAppender(volumeIdxId,
                                          NOPApplication.Instance.TimeSource,
                                          NOPApplication.Instance.AppId, "dima@zhaba"))
        {
          using(var appender = new LogMessageArchiveAppender(volumeData,
                                                 NOPApplication.Instance.TimeSource,
                                                 NOPApplication.Instance.AppId,
                                                 "dima@zhaba",
                                                 onPageCommit: (e, b) => aIdxId.Append(new StringBookmark(e.Guid.ToString().TakeLastChars(4), b))))
          {

            for(var i=0; app.Active && i<CNT / PARA; i++)
            {
              var msg = FakeLogMessage.BuildRandom();
              appender.Append(msg);
            }

          }
        }
      });

      time.Stop();
      "Did {0:n0} in {1:n1} sec at {2:n2} ops/sec\n".SeeArgs(CNT, time.ElapsedSec, CNT / time.ElapsedSec);

      volumeIdxId.Dispose();
      volumeData.Dispose();

      "CLOSED all volumes\n".See();
    }

    private static readonly char[] SEPARATORS = new char[] { ',', '<', '>', ',', '"', '{', '}' };


    [Run("!arch-perf-read", "compress=null   encrypt=null   search=$(~@term)")]// -r args='term=abcd'
    //[Run("!arch-perf-read", "compress=gzip   encrypt=aes1   search=$(~@term)")]// -r args='term=abcd'
    public void Read_LogMessages(string compress, string encrypt, string search)
    {
      search = search.Default("ABBA");

      var msData = new FileStream("c:\\azos\\logging-{0}-{1}.lar".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Open);
      var msIdxId = new FileStream("c:\\azos\\logging-{0}-{1}.guid.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Open);

      var volumeData = new DefaultVolume(CryptoMan, msData);
      var volumeIdxId = new DefaultVolume(CryptoMan, msIdxId);

      //volumeData.PageSizeBytes = 4 * 1024 * 1024;//<-------------------!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
      //volumeIdxId.PageSizeBytes = 4 * 128 * 1024;

      var reader = new LogMessageArchiveReader(volumeData);
      var idxReader = new StringIdxReader(volumeIdxId);

      var time = Azos.Time.Timeter.StartNew();

      var total = 0;
      var wordCount = 0;
      var found = 0;
      //foreach (var idx in idxReader.All)
      //{
      //  total++;
      //  if (idx.Value.EqualsOrdIgnoreCase(search))
      //  {
      //    found++;
      //    var data = reader.Entries(idx.Bookmark).FirstOrDefault();
      //    data.See();
      //  }
      //}

      ////////Rent buffers from arena instead? who is going to release pages? page.Recycle() to release the buffer to the pool? What if they forget to call it?
      ////////var prealloc = new Page(0); //SIGNIFICANT SLOW-DOWN WHILE ALLOCATING PAGES.   WHY MEMORY STREAM DOES ARRAY.CLEAR?
      //////foreach (var page in reader.GetPagesStartingAt(0/*, preallocatedPage: prealloc*/).Take(200))
      //////{
      //////  total++;
      //////  //if (total % 100 ==0)
      //////  "Page {0:X8} is {1:n0}  --- Average size: {2:n0}  Entries per page: {3:n0}".SeeArgs(page.PageId, page.Data.Array.Length, reader.AveragePageSizeBytes, page.Entries.Count());
      //////  reader.Recycle(page);
      //////}


      ////////read all entries
      //////foreach (var entry in reader.GetEntriesStartingAt(new Bookmark()/*, preallocatedPage: prealloc*/))//.Take(200));
      //////{
      //////  if (!App.Active) break;
      //////  total++;
      //////  if (entry.ExceptionData!=null) wordCount++;
      //////  if ((total & 0xffff)==0) "Did {0:n0}".SeeArgs(total);
      //////}



      //reader.ParallelProcessRawEntryBatchesStartingAt(new Bookmark(), 50, (entries, loop, _) =>
      //{
      //  var ec = 0;
      //  var wc = 0;
      //  foreach (var entry in entries)
      //  {
      //    if (!App.Active)
      //    {
      //      loop.Break();
      //      return;
      //    }

      //    if (entry.State == Entry.Status.Valid)
      //    {
      //      ec++;

      //      var msg = reader.Materialize(entry);

      //      try
      //      {
      //      //  System.Threading.Thread.SpinWait(8_000);
      //      if (msg.Parameters.Length>0 && msg.Parameters.FirstOrDefault(c => !char.IsWhiteSpace(c))=='{')
            ////{
            ////  var map = msg.Parameters.JsonToDataObject() as JsonDataMap;
            ////  wc++;
            ////}
      //      }
      //      catch
      //      { }

      //      // if (msg.Guid.ToString().StartsWith("faca")) msg.See();
      //    }
      //  }
      //  if ((total & 0x0fff) == 0) "{0}".SeeArgs(total);
      //  System.Threading.Interlocked.Add(ref total, ec);//1);
      //  System.Threading.Interlocked.Add(ref wordCount, wc);//1);
      //}, new ParallelOptions { MaxDegreeOfParallelism = 10 });

      reader.ParallelProcessPageBatchesStartingAt(0, 5, (page, loop, _) =>
      {
        var ec = 0;
        var wc = 0;
        foreach (var entry in page.Entries)
        {
          if (!App.Active)
          {
            loop.Break();
            return;
          }

          if (entry.State == Entry.Status.Valid)
          {
            ec++;

            var msg = reader.Materialize(entry);

            try
            {
              //  System.Threading.Thread.SpinWait(8_000);
              if (msg.Parameters.Length > 0 && msg.Parameters.FirstOrDefault(c => !char.IsWhiteSpace(c)) == '{')
              {
                var map = msg.Parameters.JsonToDataObject() as JsonDataMap;
                wc++;
              }
            }
            catch
            { }

            // if (msg.Guid.ToString().StartsWith("faca")) msg.See();
          }
        }
        if ((total & 0x0fff) == 0) "{0}".SeeArgs(total);
        System.Threading.Interlocked.Add(ref total, ec);//1);
        System.Threading.Interlocked.Add(ref wordCount, wc);//1);
      }, new ParallelOptions { MaxDegreeOfParallelism = 10 });

      time.Stop();
      "Did {0:n0} found {1:n0}({2:n5}%) in {3:n1} sec at {4:n2} ops/sec  WC = {5:n0}\n".SeeArgs(total, found, (double)found / total, time.ElapsedSec, total / time.ElapsedSec, wordCount);

      volumeIdxId.Dispose();
      volumeData.Dispose();
      "CLOSED all volumes\n".See();
    }


    [Run("!arch-perf-read-2", "compress=null   encrypt=null   search=$(~@term) scount=16")]// -r args='term=abcd'
    //[Run("!arch-perf-read", "compress=gzip   encrypt=aes1   search=$(~@term) scount=4")]// -r args='term=abcd'
    public void Read_LogMessages_ByVolume(string compress, string encrypt, string search, int scount)
    {
      search = search.Default("ABBA");

      var files = new List<FileStream>();
      for(var i=0; i<scount;i++)
      {
        var data = new FileStream("c:\\azos\\logging-{0}-{1}.lar".Args(compress.Default("none"), encrypt.Default("none")),
                                  FileMode.Open,
                                  FileAccess.Read,
                                  FileShare.Read,
                                  4096,
                                  FileOptions.RandomAccess);
        files.Add(data);
      }

      var time = Azos.Time.Timeter.StartNew();

      var total = 0;
      var wordCount = 0;
      var found = 0;

      var psecond = 0;
      files.ParallelProcessVolumeBatchesStartingAt(CryptoMan, 0, volume => new LogMessageArchiveReader(volume),
        (page, reader, cancel) =>{
          var ec = 0;
          var wc = 0;
          foreach (var entry in page.Entries)
          {
            if (cancel != null && cancel()) break;

            if (entry.State == Entry.Status.Valid)
            {
              ec++;

              var msg = reader.Materialize(entry);

              try
              {
                if (msg.Parameters.Length>0 && msg.Parameters.FirstOrDefault(c => !char.IsWhiteSpace(c))=='{')
                {
                  var map = msg.Parameters.JsonToDataObject() as JsonDataMap;
                  wc++;
                }
              }
              catch
              { }

            }
          }
          var now = DateTime.UtcNow.Second;
          if (now!=psecond)
          {
            psecond = now;
            "{0}".SeeArgs(total);
          }
          System.Threading.Interlocked.Add(ref total, ec);//1);
          System.Threading.Interlocked.Add(ref wordCount, wc);//1);

        },
        () => !App.Active//cancellation source
      );

      time.Stop();
      "Did {0:n0} found {1:n0}({2:n5}%) in {3:n1} sec at {4:n2} ops/sec  WC = {5:n0}\n".SeeArgs(total, found, (double)found / total, time.ElapsedSec, total / time.ElapsedSec, wordCount);

      var fLen = 0L;
      files.ForEach(f => {
        fLen = f.Length;
        "Closing file `{0}` of {1}".SeeArgs(f.Name, IOUtils.FormatByteSizeWithPrefix(fLen));
        f.Close();
       });

      "Processed {0} at {1}/sec".SeeArgs(IOUtils.FormatByteSizeWithPrefix(fLen), IOUtils.FormatByteSizeWithPrefix((long)(fLen / time.ElapsedSec)));

      "CLOSED all volumes\n".See();
    }


  }
}
