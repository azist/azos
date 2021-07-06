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
  /// IPC connection for a subordinate process connecting into governor server
  /// </summary>
  public sealed class ServerAppConnection : Connection
  {
    public ServerAppConnection(App app, TcpClient client) : base(app.NonNull(nameof(app)).Name, client)
    {
      m_App = app;
    }

    private readonly App m_App;


  }
}
