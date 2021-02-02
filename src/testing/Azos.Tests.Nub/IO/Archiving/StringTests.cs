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
  public class StringTests
  {
    
    [Run("compress=null  count=1")]
    [Run("compress=null  count=100")]
    [Run("compress=null  count=1000")]
    [Run("compress=null  count=16000")]
    [Run("compress=null  count=64000")]
    public void Write_Read_Compare_Strings(string compress, int count)
    {
      var expected = FakeLogMessage.BuildRandomStringArr(count);
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

      using (var appender = new StringArchiveAppender(volume,
                                          NOPApplication.Instance.TimeSource,
                                          NOPApplication.Instance.AppId, "dima@zhaba"))
      {
        for (var i = 0; i < count; i++)
        {
          appender.Append(expected[i]);
        }
      }

      var reader = new StringArchiveReader(volume);

      var got = reader.Entries(new Bookmark()).ToArray();

      Aver.AreEqual(expected.Length, got.Length);
      for (int i = 0; i < count; i++)
      {        
        Aver.AreEqual(expected[i], got[i]);
      }

      volume.Dispose();
    }
  }
}
