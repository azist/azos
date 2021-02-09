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


    [Run("!arch-perf-read", "compress=null   encrypt=null   search=$(~@term)")]// -r args='term=abcd'
    //[Run("!arch-perf-read", "compress=gzip   encrypt=aes1   search=$(~@term)")]// -r args='term=abcd'
    public void Read_LogMessages(string compress, string encrypt, string search)
    {
      search = search.Default("ABBA");

      var msData = new FileStream("c:\\azos\\logging-{0}-{1}.lar".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Open);
      var msIdxId = new FileStream("c:\\azos\\logging-{0}-{1}.guid.lix".Args(compress.Default("none"), encrypt.Default("none")), FileMode.Open);

      var volumeData = new DefaultVolume(CryptoMan, msData);
      var volumeIdxId = new DefaultVolume(CryptoMan, msIdxId);

      var reader = new LogMessageArchiveReader(volumeData);
      var idxReader = new StringIdxReader(volumeIdxId);

      var time = Azos.Time.Timeter.StartNew();

      var total = 0;
      var found = 0;
      foreach (var idx in idxReader.All)
      {
         total++;
         if (idx.Value.EqualsOrdIgnoreCase(search))
         {
           found++;
           var data = reader.Entries(idx.Bookmark).FirstOrDefault();
           data.See();
         }
      }

      time.Stop();
      "Did {0:n0} found {1:n0}({2:n5}%) in {3:n1} sec at {4:n2} ops/sec\n".SeeArgs(total, found, (double)found / total, time.ElapsedSec, total / time.ElapsedSec);

      volumeIdxId.Dispose();
      volumeData.Dispose();
      "CLOSED all volumes\n".See();
    }


  }
}
