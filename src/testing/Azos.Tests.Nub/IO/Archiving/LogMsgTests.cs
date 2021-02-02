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
using Azos.Log;

namespace Azos.Tests.Nub.IO.Archiving
{
  [Runnable]
  public class LogMsgTests
  {
    [Run]
    public void WriteRead_1()
    {
      //var ms = new FileStream("c:\\azos\\archive.lar", FileMode.Create);//  new MemoryStream();
      var ms = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Log archive")
                                      .SetVersion(1, 0)
                                      .SetDescription("Testing log messages")
                                      .SetChannel(Atom.Encode("dvop"));

      var volume = new DefaultVolume(NOPApplication.Instance.SecurityManager.Cryptography, meta, ms);

      using(var appender = new LogMessageArchiveAppender(volume,
                                          NOPApplication.Instance.TimeSource,
                                          NOPApplication.Instance.AppId, "dima@zhaba"))
      {
        for(var i=0; i<10; i++)
        {
          var msg = new Message()
          {
            Text = "Message#" + i.ToString()
          }.InitDefaultFields();

          appender.Append(msg);
        }
      }


      var reader = new LogMessageArchiveReader(volume);

      foreach(var msg in reader.Entries(new Bookmark()))
      {
       // msg.State.See();
        msg.See();
      }

      volume.Dispose();
    }


    [Run("compress=null pgsize=1000 count=2")]
    [Run("compress=null pgsize=1000 count=100")]
    [Run("compress=null pgsize=1000 count=1000")]
    [Run("compress=null pgsize=1000 count=16000")]
    [Run("compress=null pgsize=1000 count=32000")]
    [Run("compress=null pgsize=1000000 count=32000")]

    [Run("compress=gzip pgsize=1000 count=2")]
    [Run("compress=gzip pgsize=1000 count=100")]
    [Run("compress=gzip pgsize=1000 count=1000")]
    [Run("compress=gzip pgsize=1000 count=16000")]
    [Run("compress=gzip pgsize=1000 count=32000")]
    [Run("compress=gzip pgsize=1000000 count=32000")]
    public void Write_Read_Compare(string compress, int count, int pgsize)
    {
      var expected = FakeLogMessage.BuildRandomArr(count);
      var ms = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Log archive")
                                      .SetVersion(1, 0)
                                      .SetDescription("Testing log messages")
                                      .SetChannel(Atom.Encode("dvop"));

      if (compress.IsNotNullOrWhiteSpace())
      {
        meta.SetCompressionScheme(compress);
      }

      var volume = new DefaultVolume(NOPApplication.Instance.SecurityManager.Cryptography, meta, ms)
      {
        PageSizeBytes = pgsize
      };

      using (var appender = new LogMessageArchiveAppender(volume,
                                          NOPApplication.Instance.TimeSource,
                                          NOPApplication.Instance.AppId, "dima@zhaba"))
      {
        for (var i = 0; i < count; i++)
        {
          appender.Append(expected[i]);
        }
      }

      "The volume is {0:n0} bytes".SeeArgs(ms.Length);

      var reader = new LogMessageArchiveReader(volume);

      var got = reader.Entries(new Bookmark()).ToArray();

      Aver.AreEqual(expected.Length, got.Length);
      for (int i = 0; i < count; i++)
      {
        Aver.AreEqual(expected[i].App, got[i].App);
        Aver.AreEqual(expected[i].ArchiveDimensions, got[i].ArchiveDimensions);
        Aver.AreEqual(expected[i].Channel, got[i].Channel);
        Aver.AreEqual(expected[i].Exception?.Message, got[i].Exception?.Message);
        Aver.AreEqual(expected[i].From, got[i].From);
        Aver.AreEqual(expected[i].Gdid, got[i].Gdid);
        Aver.AreEqual(expected[i].Guid.ToString(), got[i].Guid.ToString());
        Aver.AreEqual(expected[i].Host, got[i].Host);
        Aver.AreEqual(expected[i].Parameters, got[i].Parameters);
        Aver.AreEqual(expected[i].RelatedTo.ToString(), got[i].RelatedTo.ToString());
        Aver.AreEqual(expected[i].Text, got[i].Text);
        Aver.AreEqual(expected[i].Topic, got[i].Topic);
        Aver.AreEqual((int)expected[i].Type, (int)got[i].Type);
        Aver.AreEqual(expected[i].UTCTimeStamp, got[i].UTCTimeStamp);
      }

      volume.Dispose();
    }
  }
}
