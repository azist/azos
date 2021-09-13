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

namespace Azos.Tests.Nub.IO.Archiving
{
  [Runnable]
  public class LogMsgTests : CryptoTestBase
  {
    [Run("cnt=1")]
    [Run("cnt=100")]
    [Run("cnt=250")]
    [Run("cnt=1000")]
    [Run("cnt=3700")]
    public void WriteRead(int cnt)
    {
      ////var ms = new FileStream("c:\\azos\\archive.lar", FileMode.Create);//  new MemoryStream();
      var ms = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Log archive", LogMessageArchiveAppender.CONTENT_TYPE_LOG)
                                      .SetVersion(1, 0)
                                      .SetDescription("Testing log messages")
                                      .SetChannel(Atom.Encode("dvop"));

      var volume = new DefaultVolume(CryptoMan, meta, ms);

      using(var appender = new LogMessageArchiveAppender(volume,
                                          NOPApplication.Instance.TimeSource,
                                          NOPApplication.Instance.AppId, "dima@zhaba"))
      {
        for(var i=0; i<cnt; i++)
        {
          var msg = new Message()
          {
            Text = "Message#" + i.ToString()
          }.InitDefaultFields();

          appender.Append(msg);
        }
      }


      var reader = new LogMessageArchiveReader(volume);

      Aver.AreEqual(cnt, reader.All.Count());
      Aver.AreEqual(cnt, reader.GetEntriesStartingAt(new Bookmark()).Count());
      Aver.AreEqual(cnt, reader.GetBookmarkedEntriesStartingAt(new Bookmark()).Count());
      Aver.AreEqual(cnt, reader.GetEntriesAsObjectsStartingAt(new Bookmark()).Count());
      Aver.AreEqual(cnt, reader.GetBookmarkedEntriesAsObjectsStartingAt(new Bookmark()).Count());

      reader.All.ForEach((m, i) => Aver.AreEqual("Message#" + i.ToString(), m.Text));

      reader.GetBookmarkedEntriesStartingAt(new Bookmark()).ForEach( (t, i) =>
      {
        var pointed = reader.GetEntriesStartingAt(t.bm).First();
        Aver.AreEqual("Message#" + i.ToString(), pointed.Text);
        Aver.AreEqual(t.entry.Text, pointed.Text);
      });

      volume.Dispose();
    }


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
    public void Write_Read_Compare(string compress, string encrypt, int count, int pgsize)
    {
      var expected = FakeLogMessage.BuildRandomArr(count);
      var ms = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("Log archive", LogMessageArchiveAppender.CONTENT_TYPE_LOG)
                                      .SetVersion(1, 0)
                                      .SetDescription("Testing log messages")
                                      .SetChannel(Atom.Encode("dvop"))
                                      .SetCompressionScheme(compress)
                                      .SetEncryptionScheme(encrypt);

      var volume = new DefaultVolume(CryptoMan, meta, ms)
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

      var got = reader.GetEntriesStartingAt(new Bookmark()).ToArray();

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
