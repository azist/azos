/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

using Azos.IO.Sipc;

namespace Azos.Tests.Nub.IO.Ipc
{
  public class MockSipcServer : SipcServer
  {
    public MockSipcServer(int startPort, int endPort) : base(startPort, endPort)
    {
    }

    public List<(Connection con, string smd)> Received = new List<(Connection con, string smd)>();
    public List<(Exception err, bool iscom)> Errors = new List<(Exception err, bool iscom)>();


    protected override void DoHandleError(Exception error, bool isCommunication)
    {
      lock (Errors)
      {
        Errors.Add((error, isCommunication));
      }
    }

    protected override void DoHandleCommand(Connection connection, string command)
    {
      lock(Received)
      {
        Received.Add((connection, command));
      }
    }

    protected override Connection MakeNewConnection(string name, TcpClient client)
    {
      return new Connection(name, client);
    }
  }

  public class MockSipcClient : SipcClient
  {
    public MockSipcClient(int serverPort) : base(serverPort)
    {
    }

    protected override void DoHandleFailure()
    {
      WasFailure = true;
    }

    public bool WasFailure;
    public List<(Connection con, string smd)> Received = new List<(Connection con, string smd)>();
    public List<(Exception err, bool iscom)> Errors = new List<(Exception err, bool iscom)>();


    protected override void DoHandleError(Exception error, bool isCommunication)
    {
      lock (Errors)
      {
        Errors.Add((error, isCommunication));
      }
    }

    protected override void DoHandleCommand(Connection connection, string command)
    {
      lock (Received)
      {
        Received.Add((connection, command));
      }
    }

    protected override Connection MakeNewConnection(string name, TcpClient client)
    {
      return new Connection(name, client);
    }
  }
}
