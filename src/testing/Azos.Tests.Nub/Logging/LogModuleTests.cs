/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Apps;
using Azos.Conf;
using Azos.Log;
using Azos.Scripting;

namespace Azos.Tests.Nub.Logging
{
  [Runnable]
  public class LogModuleTests
  {
    static readonly ConfigSectionNode CONF = @"
       app{
         modules
         {
           module
           {
             name='logSec'
             type='Azos.Log.LogModule, Azos'
             default-channel='sec'
             log
             {
               sink{type='Azos.Log.Sinks.NullSink'}
             }
           }

           module
           {
             name='logOp'
             type='Azos.Log.LogModule, Azos'
             default-channel='ops'
             log
             {
               sink{type='Azos.Log.Sinks.NullSink'}
             }
           }
         }
       }
       ".AsLaconicConfig();

    [Run]
    public void MountModule()
    {
      using(var app = new AzosApplication(null, CONF))
      {
        var logSec = app.ModuleRoot.Get<ILogModule>("logSec");
        var logOp = app.ModuleRoot.Get<ILogModule>("logOp");

        Aver.IsNotNull(logSec);
        Aver.IsNotNull(logOp);

        Aver.AreEqual(Atom.Encode("sec"), logSec.DefaultChannel);
        Aver.AreEqual(Atom.Encode("ops"), logOp.DefaultChannel);

        Aver.IsTrue(logSec.Log is LogDaemon d1 && d1.Status == DaemonStatus.Active);
        Aver.IsTrue(logOp.Log is LogDaemon d2 && d2.Status == DaemonStatus.Active);
      }
    }
  }
}
