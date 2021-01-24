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
using Azos.Data;
using Azos.Log;

namespace Azos.Tests.Nub.IO.Archiving
{
  [Runnable]
  public class DefaultVolumeTests
  {
    [Run]
    public void Metadata_Create_Multiple_Sections_Mount()
    {
      //var ms = new FileStream("c:\\azos\\archive.lar", FileMode.Create);//  new MemoryStream();
      var ms = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("20210115-1745-doctor")
                                      .SetVersion(99, 21)
                                      .SetDescription("B vs D De-Terminator for doctor bubblegumization")
                                      .SetChannel(Atom.Encode("dvop"))
                                      .SetCompressionScheme(DefaultVolume.COMPRESSION_SCHEME_GZIP_MAX)
                                  //    .SetEncryptionScheme("aes1")
                                      .SetApplicationSection(app => {
                                        app.AddChildNode("user").AddAttributeNode("id", 111222);
                                        app.AddChildNode("user").AddAttributeNode("id", 783945);
                                        app.AddAttributeNode("is-good", false);
                                        app.AddAttributeNode("king-signature", Platform.RandomGenerator.Instance.NextRandomWebSafeString(150, 150));
                                      })
                                      .SetApplicationSection(app => { app.AddAttributeNode("a", false); })
                                      .SetApplicationSection(app => { app.AddAttributeNode("b", true); })
                                      .SetCompressionSection(cmp => { cmp.AddAttributeNode("z", 41); });
                                   //   .SetEncryptionSection(enc => { enc.AddAttributeNode("z", 99); });

      var volume = new DefaultVolume(NOPApplication.Instance.SecurityManager.Cryptography, meta, ms);

      volume.Dispose();
     // ms.GetBuffer().ToDumpString(DumpFormat.Hex).See();

      volume = new DefaultVolume(NOPApplication.Instance.SecurityManager.Cryptography, ms);
      volume.Metadata.See();
    }


    [Run("!arch-log", "scheme=null          cnt=1000000 para=16")]
    [Run("!arch-log", "scheme=gzip          cnt=1000000 para=16")]
    [Run("!arch-log", "scheme=gzip-max      cnt=1000000 para=16")]
    public void Write_LogMessages(string scheme, int CNT, int PARA)
    {
      var msData = new FileStream("c:\\azos\\logging-{0}.lar".Args(scheme), FileMode.Create);
      var msIdxId = new FileStream("c:\\azos\\logging-{0}.guid.lix".Args(scheme), FileMode.Create);

      var meta = VolumeMetadataBuilder.Make("log messages")
                                      .SetVersion(1, 1)
                                      .SetDescription("Testing")
                                      .SetChannel(Atom.Encode("tezt"))
                                      .SetCompressionScheme(scheme);   // Add optional compression

      var volumeData = new DefaultVolume(NOPApplication.Instance.SecurityManager.Cryptography, meta, msData);
      var volumeIdxId = new DefaultVolume(NOPApplication.Instance.SecurityManager.Cryptography, meta, msIdxId);


      volumeData.PageSizeBytes = 1024 * 1024;
      volumeIdxId.PageSizeBytes = 128 * 1024;

      var time = Azos.Time.Timeter.StartNew();


      Parallel.For(0, PARA, _ => {

        using(var aIdxId = new GuidIdxAppender(volumeIdxId,
                                          NOPApplication.Instance.TimeSource,
                                          NOPApplication.Instance.AppId, "dima@zhaba"))
        {
          using(var appender = new LogMessageArchiveAppender(volumeData,
                                                 NOPApplication.Instance.TimeSource,
                                                 NOPApplication.Instance.AppId,
                                                 "dima@zhaba",
                                                 onPageCommit: (e, b) => aIdxId.Append(new GuidBookmark(e.Guid, b))))
          {

            for(var i=0; i<CNT / PARA; i++)
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


  }
}
