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

namespace Azos.Apps.Hosting
{
  public sealed class GovernorSipcClient : SipcClient
  {
    public GovernorSipcClient(int serverPort) : base(serverPort)
    {
    }

    //todo we need to use app thread instead of allocating one, so
    //console blocks until app becomes inactive
    //maybe have a delegate which takes App init under client ctor

    protected override void DoHandleCommand(Connection connection, string command)
    {
      if (command.EqualsOrdIgnoreCase(Protocol.CMD_STOP))
      {
        ((IApplicationImplementation)Azos.Apps.ExecutionContext.Application).Stop();
      }
    }

    protected override Connection MakeNewConnection(string name, TcpClient client)
    {
      throw new NotImplementedException();
    }
  }
}
