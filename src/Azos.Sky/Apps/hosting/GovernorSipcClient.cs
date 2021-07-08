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
  /// <summary>
  /// Implements a simple IPC client for connection to host governor server
  /// </summary>
  public sealed class GovernorSipcClient : SipcClient
  {
    public GovernorSipcClient(int serverPort, string serverApplicationId, Func<IApplicationImplementation> appAccessor) : base(serverPort, serverApplicationId)
    {
      m_AppAccessor = appAccessor.NonNull(nameof(appAccessor));
    }

    private Func<IApplicationImplementation> m_AppAccessor;

    private IApplicationImplementation App => m_AppAccessor() ?? (IApplicationImplementation)NOPApplication.Instance;


    protected override void DoHandleCommand(Connection connection, string command)
    {
      if (command.EqualsOrdIgnoreCase(Protocol.CMD_PING))
      {
        log(MessageType.Trace, "Received server PING");
        return;
      }

      if (command.EqualsOrdIgnoreCase(Protocol.CMD_STOP))
      {
        log(MessageType.InfoD, "Received stop from gov");
        App.Stop();
        return;
      }

      try
      {
        var cmd = command.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);
        perform(connection, cmd);
      }
      catch(Exception error)
      {
        log(MessageType.Error, "Unparsable cmd: {0}".Args(command.TakeFirstChars(48), ".."), error);
      }
    }

    protected override void DoHandleError(Exception error, bool isCommunication)
    {
      log(MessageType.Critical, "{0} error: {1}".Args(isCommunication ? "Comm" : "Non-comm", error.ToMessageWithType()), error);
    }

    protected override void DoHandleFailure()
    {
      log(MessageType.CatastrophicError, "Gov uplink failure");
      App.Stop();
    }

    protected override Connection MakeNewConnection(string name, TcpClient client)
    {
      return new Connection(name, client);
    }

    private void log(MessageType type, string text, Exception error = null)
    {
      var msg = new Message
      {
        Type = type,
        Topic = Sky.SysConsts.LOG_TOPIC_HOST_GOV,
        From = nameof(GovernorSipcClient),
        Text = text,
        Exception = error
      };

      App.Log.Write(msg, urgent: true);
    }

    private void perform(Connection cnn, IConfigSectionNode cmd)
    {
      //perform commands as specified via CMD structured parameter
    }
  }
}
