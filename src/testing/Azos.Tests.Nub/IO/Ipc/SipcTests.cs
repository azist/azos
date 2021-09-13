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

using Azos.IO.Sipc;
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
        Connection srvConnection = null;
        using(var cli = new MockSipcClient(srv.AssignedPort, "myid-123"))
        {
          Aver.IsFalse(cli.Active);
          cli.Start();
          Aver.IsTrue(cli.Active);

          cli.Connection.Send("Hello from client!");

          Thread.Sleep(1200);//allow client to connect

          Aver.IsTrue(srv.Connections.Count == 1);
          srvConnection = srv.Connections.First();

          srvConnection.Send("Hi!");

          Aver.IsNotNull(cli.Connection);
          Aver.AreEqual(srvConnection.Name, cli.Connection.Name);//are the same
          Aver.AreEqual("myid-123", srvConnection.Name);

          Aver.IsTrue(cli.Connection.State == ConnectionState.OK);
          Aver.IsTrue(srvConnection.State == ConnectionState.OK);

          Thread.Sleep(1200);//allow client to receive from server

          lock (cli.Received)
          {
            cli.Received.ForEach(item => "Client received: {0} => {1}".SeeArgs(item.con.Name, item.cmd));
            Aver.IsTrue(cli.Received.Any(itm => itm.cmd == "Hi!"));//server-sent command found in client
          }
        } //client closed

        for(var i=0; i < 8; i++)
        {
          "State: {0} Last r: {1} Last s: {2}".SeeArgs(srvConnection.State, srvConnection.LastReceiveUtc, srvConnection.LastSendUtc);
          Thread.Sleep(1000);//allow thread processing delay
        }

        Aver.IsTrue(srvConnection.State ==  ConnectionState.Torn);

        lock(srv.Errors)
        {
          srv.Errors.ForEach(err => "Error: {0}".SeeArgs(err));
        }

        lock(srv.Received)
        {
          srv.Received.ForEach(item => "Server received: {0} => {1}".SeeArgs(item.con.Name, item.cmd));
          Aver.IsTrue(srv.Received.Any(itm => itm.cmd == "Hello from client!"));//client-sent command found on server
        }

      }
    }
  }
}
