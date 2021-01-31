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
        //msg.State.See();
        msg.See();
      }

      volume.Dispose();
    }
  }
}
