using System;
using System.Collections.Generic;
using System.Linq;

using Azos.IO.FileSystem.Local;
using Azos.Scripting;

using Azos.Sky.Apps;
using Azos.Sky.Metabase;

namespace Azos.Tests.Unit.Sky.Metabase
{
  [Runnable]
  public class ValidationTests
  {
      [Run]
      public void ValidateMetabank()
      {
        using(var fs = new LocalFileSystem(null))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        using(BootConfLoader.LoadForTest(SystemApplicationType.TestRig, mb, "US/East/CLE/A/I/wmed0001"))
        {
          var output = new List<MetabaseValidationMsg>();

          mb.Validate(output);

          foreach(var msg in output)
            Console.WriteLine(msg);

          Aver.AreEqual(5, output.Count( m => m.Type== MetabaseValidationMessageType.Warning ));
          Aver.AreEqual(6, output.Count( m => m.Type== MetabaseValidationMessageType.Info    ));
        }
      }
  }
}
