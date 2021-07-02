/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Azos.Apps;
using Azos.Collections;
using Azos.Log;

namespace Azos.IO.Sipc
{
  /// <summary>
  /// Server which client (subordinate process) connects to
  /// </summary>
  public abstract class SipcServer : DisposableObject
  {
    public const int DEFAULT_PORT = 49123;


    public sealed class Connection : INamed
    {
      public readonly string ID;
      internal TcpClient Client;

      public string Name => ID;

      public string TryReceive()
      {
        if (Client.Available<=0) return null;

        return Utils.Receive(Client);
      }

      public void Send(string command)
      {
        if (command.IsNullOrWhiteSpace()) return;
        Utils.Send(Client, command);
      }
    }


    protected SipcServer(ILog log, int startPort, int endPort)
    {
      m_Log = log.NonNull(nameof(log));

      if (startPort <=0 ) startPort = DEFAULT_PORT;
      if (endPort <= 0) endPort = DEFAULT_PORT;

      (startPort <= endPort).IsTrue("{0} <= {1}".Args(nameof(startPort), nameof(endPort)));

      m_StartPort = startPort;
      m_EndPort = endPort;
    }

    protected override void Destructor()
    {
      Stop();
      base.Destructor();
    }

    private ILog m_Log;
    private int m_StartPort;
    private int m_EndPort;
    private int m_AssignedPort;
    private TcpListener m_Listener;
    private Thread m_Thread;

    private Registry<Connection> m_Connections = new Registry<Connection>();


    /// <summary>
    /// Inclusive port range start
    /// </summary>
    private int StartPort => m_StartPort;

    /// <summary>
    /// Inclusive port range end
    /// </summary>
    private int EndPort => m_EndPort;

    /// <summary>
    /// True if the server is activated
    /// </summary>
    public bool Active => m_Listener != null;

    /// <summary>
    /// Specifies the port which the server listens on
    /// </summary>
    public int AssignedPort => m_AssignedPort;


    public void Start()
    {
      m_Listener = tryBind();

      m_Thread = new Thread(threadBody);
      m_Thread.Name = GetType().Name;
      m_Thread.IsBackground = false;
      m_Thread.Start();

      m_Listener.Pending();
    }

    public void Stop()
    {
      if (m_Listener==null) return;
      m_Listener.Stop();
      m_Listener = null;
      if (m_Thread!=null)
      {
        m_Thread.Join();
        m_Thread = null;
      }
    }

    private TcpListener tryBind()
    {
      var port = AssignedPort;
      try
      {
        var result = new TcpListener(IPAddress.Loopback, port);
        result.ExclusiveAddressUse = true;
        result.Start();
        return result;
      }
      catch
      {

      }
      return null;
    }

    private void threadBody()
    {
      while(true)
      {
        var listener = m_Listener;
        if (listener == null) break;

        if (listener.Pending())
        {
          var client = listener.AcceptTcpClient();
          //read ID header
          var connection = connect(client);
        }

        foreach(var one in m_Connections)
        {
          read(one);
        }

      }
    }


    private Connection connect(TcpClient client)
    {
      client.SendBufferSize = 8 * 1024;
      client.SendTimeout = 8000;
      return null;
    }

    private void read(Connection connection)
    {
      //read socket if nothing the return;
      string command = connection.TryReceive();

      if (command.IsNullOrWhiteSpace()) return;

      try
      {
        DoHandleCommand(connection, command);
      }
      catch(Exception error)
      {
        DoHandleError(error);
      }
    }

    protected virtual void DoHandleError(Exception error)
    {
      //use Log here
      throw error;
    }

    protected abstract void DoHandleCommand(Connection connection, string command);

  }
}
