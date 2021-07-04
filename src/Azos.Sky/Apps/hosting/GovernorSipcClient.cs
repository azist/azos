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
    public GovernorSipcClient(int serverPort, Func<IApplicationImplementation> appAccessor) : base(serverPort)
    {
      m_AppAccessor = appAccessor.NonNull(nameof(appAccessor));
    }

    private Func<IApplicationImplementation> m_AppAccessor;

    private IApplicationImplementation App => m_AppAccessor() ?? (IApplicationImplementation)NOPApplication.Instance;


    protected override void DoHandleCommand(Connection connection, string command)
    {
      if (command.EqualsOrdIgnoreCase(Protocol.CMD_STOP))
      {
        App.Stop();
      }
    }

    protected override void DoHandleError(Exception error, bool isCommunication)
    {
      var msg = new Log.Message
      {
        Type = Log.MessageType.CriticalAlert,
        Topic = "todo",
        From = nameof(GovernorSipcClient),
        Text = "{0} error: ".Args(error.ToMessageWithType()),
        Exception = error
      };
      App.Log.Write(msg);
    }

    protected override void DoHandleFailure()
    {
      App.Stop();
    }

    protected override Connection MakeNewConnection(string name, TcpClient client)
    {
      throw new NotImplementedException();
    }
  }
}
