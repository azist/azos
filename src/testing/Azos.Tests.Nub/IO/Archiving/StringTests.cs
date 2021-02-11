/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Azos.Apps;
using Azos.Scripting;
using Azos.IO.Archiving;

namespace Azos.Tests.Nub.IO.Archiving
{
  [Runnable]
  public class StringTests
  {
    [Run("ascii=true pgsize=1024  compress=null  count=10   min=1000 max=1000")]
    [Run("ascii=true pgsize=9000  compress=null  count=10   min=1000 max=1000")]
    [Run("ascii=true pgsize=16000 compress=null  count=10   min=1000 max=1000")]
    [Run("ascii=true pgsize=1024  compress=null  count=1000 min=10   max=500")]
    [Run("ascii=true pgsize=9000  compress=null  count=1000 min=10   max=500")]
    [Run("ascii=true pgsize=16000 compress=null  count=1000 min=10   max=500")]
    [Run("ascii=true pgsize=1000000 compress=null  count=64000 min=10   max=345")]
    [Run("ascii=true pgsize=32000000 compress=null  count=64000 min=10   max=345")]

    [Run("ascii=false pgsize=1024  compress=null  count=10   min=1000 max=1000")]
    [Run("ascii=false pgsize=9000  compress=null  count=10   min=1000 max=1000")]
    [Run("ascii=false pgsize=16000 compress=null  count=10   min=1000 max=1000")]
    [Run("ascii=false pgsize=1024  compress=null  count=1000 min=10   max=500")]
    [Run("ascii=false pgsize=9000  compress=null  count=1000 min=10   max=500")]
    [Run("ascii=false pgsize=16000 compress=null  count=1000 min=10   max=500")]
    [Run("ascii=false pgsize=1000000 compress=null  count=64000 min=10   max=345")]
    [Run("ascii=false pgsize=32000000 compress=null  count=64000 min=10   max=345")]

    [Run("ascii=true pgsize=1024  compress=gzip  count=10   min=1000 max=1000")]
    [Run("ascii=true pgsize=9000  compress=gzip  count=10   min=1000 max=1000")]
    [Run("ascii=true pgsize=16000 compress=gzip  count=10   min=1000 max=1000")]
    [Run("ascii=true pgsize=1024  compress=gzip  count=1000 min=10   max=500")]
    [Run("ascii=true pgsize=9000  compress=gzip  count=1000 min=10   max=500")]
    [Run("ascii=true pgsize=16000 compress=gzip  count=1000 min=10   max=500")]
    [Run("ascii=true pgsize=1000000 compress=gzip  count=64000 min=10   max=345")]
    [Run("ascii=true pgsize=32000000 compress=gzip  count=64000 min=10   max=345")]

    [Run("ascii=false pgsize=1024  compress=gzip  count=10   min=1000 max=1000")]
    [Run("ascii=false pgsize=9000  compress=gzip  count=10   min=1000 max=1000")]
    [Run("ascii=false pgsize=16000 compress=gzip  count=10   min=1000 max=1000")]
    [Run("ascii=false pgsize=1024  compress=gzip  count=1000 min=10   max=500")]
    [Run("ascii=false pgsize=9000  compress=gzip  count=1000 min=10   max=500")]
    [Run("ascii=false pgsize=16000 compress=gzip  count=1000 min=10   max=500")]
    [Run("ascii=false pgsize=1000000 compress=gzip  count=64000 min=10   max=345")]
    [Run("ascii=false pgsize=32000000 compress=gzip  count=64000 min=10   max=345")]
    public void Write_Read_Compare_Strings(bool ascii, int pgsize, string compress, int count, int min, int max)
    {
      var expected = ascii ? FakeLogMessage.BuildRandomASCIIStringArr(count, min, max) : FakeLogMessage.BuildRandomUnicodeStringArr(count, min, max);
      var ms = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("String archive")
                                      .SetVersion(1, 0)
                                      .SetDescription("Testing string messages")
                                      .SetChannel(Atom.Encode("dvop"));

      if (compress.IsNotNullOrWhiteSpace())
      {
        meta.SetCompressionScheme(compress);
      }

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

      var reader = new StringArchiveReader(volume);

      var got = reader.GetEntriesStartingAt(new Bookmark()).ToArray();

      Aver.AreEqual(expected.Length, got.Length);
      for (int i = 0; i < count; i++)
      {
        Aver.AreEqual(expected[i], got[i]);
      }

      volume.Dispose();
    }
  }
}
