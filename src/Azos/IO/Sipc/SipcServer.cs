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

    protected SipcServer(int startPort, int endPort)
    {
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

    private object m_Lock = new object();
    private int m_StartPort;
    private int m_EndPort;
    private int m_AssignedPort;
    private volatile TcpListener m_Listener;
    private Thread m_Thread;
    private AutoResetEvent m_Signal;

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
      lock(m_Lock)
      {
        m_Listener = tryBind();

        if (m_Listener == null)
         throw new AzosIOException("Unable to start SIPC server in the port range {0}-{1}".Args(m_StartPort, m_EndPort));

        m_Signal = new AutoResetEvent(false);

        m_Thread = new Thread(threadBody);
        m_Thread.Name = GetType().Name;
        m_Thread.IsBackground = false;
        m_Thread.Start();
      }
    }

    public void Stop()
    {
      lock(m_Lock)
      {
        if (m_Listener != null)
        {
          m_Listener.Stop();
          m_Listener = null;
        }

        if (m_Signal != null)
        {
          m_Signal.Set();
        }

        if (m_Thread != null)
        {
          m_Thread.Join();
          m_Thread = null;
        }

        DisposeAndNull(ref m_Signal); //after thread Join
      }
    }

    /// <summary>
    /// Override to create a connection instance needed for your use case
    /// </summary>
    protected abstract Connection MakeNewConnection(string name, TcpClient client);

    /// <summary>
    /// Override to handle exceptions, e.g. log them
    /// </summary>
    /// <param name="error">Error to handle</param>
    /// <param name="isCommunication">True if error came from communication circuit (e.g. socket)</param>
    protected virtual void DoHandleError(Exception error, bool isCommunication)
    {
      //use Log here
      throw error;
    }

    /// <summary>
    /// Override to take action per received command.
    /// The handler is called on the main thread and should not leak errors
    /// </summary>
    /// <param name="connection">Connection which received the command</param>
    /// <param name="command">Command text</param>
    protected abstract void DoHandleCommand(Connection connection, string command);


    //returns null on bind inability
    private TcpListener tryBind()
    {
      for(var port = m_StartPort; port <= m_EndPort; port++)
      {
        try
        {
          var result = new TcpListener(IPAddress.Loopback, port);
          result.ExclusiveAddressUse = true;
          result.Start();
          m_AssignedPort = port;
          return result;
        }
        catch(Exception error)
        {
          DoHandleError(error, true);
        }
      }
      return null;
    }

    private void threadBody()
    {
      const int GRANULARITY_MS = 357;

      while(true)
      {
        var listener = m_Listener;
        if (listener == null) break;

        try
        {
          if (listener.Pending())
          {
            var client = listener.AcceptTcpClient();
            connect(client);
          }
        }
        catch(Exception error)
        {
          DoHandleError(error, true);
        }

        foreach(var one in m_Connections)
        {
          readAndHandleSafe(one);
        }

        m_Signal.WaitOne(GRANULARITY_MS);
      }//while
    }


    private Connection connect(TcpClient client)
    {
      client.SendBufferSize = 8 * 1024;
      client.SendTimeout = 8000;

      string name;
      try
      {
        name = Utils.ReceiveHandshake(client);
      }
      catch(Exception error)
      {
        DoHandleError(error, true);
        return null;
      }

      var connection = m_Connections.GetOrRegister(name, n => MakeNewConnection(n, client), name, out var wasAdded);
      if (!wasAdded) connection.Reconnect(client);

      return connection;
    }

    private void readAndHandleSafe(Connection connection)
    {
      //read socket if nothing then return;
      string command = connection.TryReceive();

      if (command.IsNullOrWhiteSpace()) return;

      try
      {
        DoHandleCommand(connection, command);
      }
      catch(Exception error)
      {
        DoHandleError(error, false);
      }
    }

  }
}
