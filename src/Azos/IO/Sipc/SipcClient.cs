/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Azos.IO.Sipc
{
  /// <summary>
  /// Client which connects to sipc server
  /// </summary>
  public abstract class SipcClient : DisposableObject
  {
    protected SipcClient(int serverPort)
    {
      if (serverPort <= 0 ) serverPort = Protocol.LISTENER_PORT_DEFAULT;

      m_ServerPort = Protocol.GuardPort(serverPort, nameof(serverPort));
      m_Name = Guid.NewGuid().ToString();
    }

    protected override void Destructor()
    {
      Stop();
      base.Destructor();
    }

    private string m_Name;

    private object m_Lock = new object();
    private int m_ServerPort;
    private volatile TcpClient m_Client;
    private Thread m_Thread;
    private AutoResetEvent m_Signal;

    private Connection m_Connection;

    /// <summary>
    /// Unique name assigned to this instance
    /// </summary>
    public string Name => m_Name;


    /// <summary>
    /// Server port
    /// </summary>
    private int ServerPort => m_ServerPort;

    /// <summary>
    /// True if the client is active
    /// </summary>
    public bool Active => m_Client != null;

    /// <summary>
    /// Connection to send data into
    /// </summary>
    public Connection Connection => m_Connection;

    public void Start()
    {
      lock(m_Lock)
      {
        m_Client = tryConnect();
        if (m_Client == null)
         throw new AzosIOException("Unable to connect to SIPC server on port {0}".Args(m_ServerPort));

        m_Connection = MakeNewConnection(m_Name, m_Client);

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
#pragma warning disable CS0420 // A reference to a volatile field will not be treated as volatile
        DisposeAndNull(ref m_Client);
#pragma warning restore CS0420 // A reference to a volatile field will not be treated as volatile

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
    /// Override to handle exceptions, e.g. log them. Default implementation does nothing
    /// </summary>
    /// <param name="error">Error to handle</param>
    /// <param name="isCommunication">True if error came from communication circuit (e.g. socket)</param>
    protected virtual void DoHandleError(Exception error, bool isCommunication)
    {
    }

    /// <summary>
    /// Override to take action on the client per received command from the server.
    /// The handler is called on the main thread and should not leak errors
    /// </summary>
    /// <param name="connection">Connection which received the command</param>
    /// <param name="command">Command text</param>
    protected abstract void DoHandleCommand(Connection connection, string command);


    //returns null on connect inability
    private TcpClient tryConnect()
    {
      try
      {
        var client = new TcpClient(new IPEndPoint(IPAddress.Loopback, m_ServerPort));
        client.ReceiveBufferSize = Protocol.RECEIVE_BUFFER_SIZE;
        client.SendBufferSize = Protocol.SEND_BUFFER_SIZE;
        client.ReceiveTimeout = Protocol.RECEIVE_TIMEOUT_MS;
        client.SendTimeout = Protocol.SEND_TIMEOUT_MS;

        Protocol.Send(client, m_Name);//handshake

        return client;
      }
      catch(Exception error)
      {
        DoHandleError(error, true);
      }
      return null;
    }

    private void threadBody()
    {
      const int GRANULARITY_MS = 357;

      while(true)
      {
        var client = m_Client;
        if (client == null) break;

        var now = DateTime.UtcNow;
        var state = m_Connection.State;

        if (state != ConnectionState.OK && state != ConnectionState.Limbo)
        {
          var newClient = tryConnect();
          if (newClient != null)
          {
            m_Client.Dispose();
            m_Client = newClient;
            m_Connection.Reconnect(newClient);
          }
          else
          {
            m_Signal.WaitOne(GRANULARITY_MS);
          }
          continue;
        }

        if (state == ConnectionState.OK && ((now - m_Connection.LastReceive).TotalMilliseconds > Protocol.LIMBO_TIMEOUT_MS))
        {
          m_Connection.PutInLimbo();
          continue;
        }


        try
        {
          tryReadAndHandleSafe(m_Connection);
        }
        catch(Exception error)
        {
          DoHandleError(error, true);
        }

        //ping
        if ((now - m_Connection.LastSend).TotalMilliseconds > Protocol.PING_INTERVAL_MS)
        {
          m_Connection.Send(Protocol.CMD_PING);
        }


        m_Signal.WaitOne(GRANULARITY_MS);
      }//while
    }


    private void tryReadAndHandleSafe(Connection connection)
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
