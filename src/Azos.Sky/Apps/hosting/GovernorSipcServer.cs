/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Net.Sockets;

using Azos.Conf;
using Azos.IO.Sipc;
using Azos.Log;

namespace Azos.Apps.Hosting
{
  public sealed class GovernorSipcServer : SipcServer
  {
    public GovernorSipcServer(GovernorDaemon governor, int startPort, int endPort) : base(startPort, endPort)
    {
      m_Governor = governor;
    }

    private readonly GovernorDaemon m_Governor;

    protected override void DoHandleError(Exception error, bool isCommunication)
    {
      log(MessageType.Critical, nameof(DoHandleError), "{0} error: {1}".Args(isCommunication ? "Comm" : "Non-comm", error.ToMessageWithType()), error);
    }

    protected override void DoHandleCommand(Connection connection, string command)
    {
      if (command.EqualsOrdIgnoreCase(Protocol.CMD_PING))
      {
        log(MessageType.Trace, nameof(DoHandleCommand), "Received client PING");
        return;
      }

      if (command.EqualsOrdIgnoreCase(Protocol.CMD_DISCONNECT))
      {
        log(MessageType.Trace, nameof(DoHandleCommand), "Received client Disconnect");
        return;
      }

      try
      {
        var cmd = command.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);
        perform(connection, cmd);
      }
      catch (Exception error)
      {
        log(MessageType.Error, nameof(DoHandleCommand), "Unparsable cmd: {0}".Args(command.TakeFirstChars(48), ".."), error);
      }
    }

    protected override Connection MakeNewConnection(string name, TcpClient client)
    {
      var app = m_Governor.Applications[name].NonNull("app `{0}` not found".Args(name));

      if (app.Connection != null)
      {
        return app.Connection;
      }
      else
      {
        var result = new ServerAppConnection(app, client);
        app.Connection = result;
        return result;
      }
    }

    private void log(MessageType type, string from, string text, Exception error = null)
      =>  m_Governor.WriteLog(type, "{0}.{1}".Args(nameof(GovernorSipcServer), from), text, error);


    private void perform(Connection cnn, IConfigSectionNode cmd)
    {
      //perform commands as specified via CMD structured parameter
    }
  }
}
