/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using Azos.Apps;
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
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        using(new SkyApplication(NOPApplication.Instance, SystemApplicationType.TestRig, mb, "US/East/CLE/A/I/wmed0001", true, null, null))
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
