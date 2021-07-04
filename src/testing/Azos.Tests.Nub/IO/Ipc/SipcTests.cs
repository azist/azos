/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Azos.Scripting;

namespace Azos.Tests.Nub.IO.Ipc
{
  [Runnable]
  public class SipcTests
  {
    [Run]
    public void Test01()
    {
      using(var srv = new MockSipcServer(0,0))
      {
        Aver.IsFalse(srv.Active);
        srv.Start();
        Aver.IsTrue(srv.Active);
        Aver.IsTrue(srv.AssignedPort > 0);
        using(var cli = new MockSipcClient(srv.AssignedPort))
        {
          Aver.IsFalse(cli.Active);
          cli.Start();
          Aver.IsTrue(cli.Active);

          Thread.Sleep(2000);

          lock(srv.Errors)
            foreach(var e in srv.Errors)
             new WrappedExceptionData(e.err).See();

          Aver.IsTrue(srv.Connections.Count > 0);
          var srvConnection = srv.Connections.First();

          Aver.IsNotNull(cli.Connection);
          Aver.AreEqual(srvConnection.Name, cli.Connection.Name);//are the same

        }
      }
    }
  }
}
