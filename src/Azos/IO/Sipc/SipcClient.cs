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
  /// Client which connects to SIPC server
  /// </summary>
  public abstract class SipcClient : DisposableObject
  {
    protected SipcClient(int serverPort, string name)
    {
      if (serverPort <= 0 ) serverPort = Protocol.LISTENER_PORT_DEFAULT;

      m_ServerPort = Protocol.GuardPort(serverPort, nameof(serverPort));
      m_Name = name.Default(Guid.NewGuid().ToString());
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
    private bool m_HasFailed;

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
    /// Connection to send data into. Null if not active
    /// </summary>
    public Connection Connection => m_Connection;

    /// <summary>
    /// True if connection has been established but failed and needs to be re-started if possible.
    /// The system calls <see cref="DoHandleUplinkFailure"/> which typically tears the process, so nothing can be done in such a case
    /// </summary>
    public bool HasFailed => m_HasFailed;

    /// <summary>
    /// Starts the client - you MUST have server running at that time, otherwise start fails
    /// and DoUplinkError() is NOT called
    /// </summary>
    public void Start()
    {
      lock(m_Lock)
      {
        m_HasFailed = false;
        m_Client = tryConnect();
        if (m_Client == null)
        {
          throw new SipcException("Unable to establish an initial connection to SIPC server on port {0}".Args(m_ServerPort));
        }

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
        if (m_Connection != null)
        {
          m_Connection.Send(Protocol.CMD_DISCONNECT);
        }

        try
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
        }
        finally
        {
          m_Connection = null;
          DisposeAndNull(ref m_Signal); //after thread Join
        }
      }//lock
    }

    /// <summary>
    /// Override to create a connection instance needed for your use case
    /// </summary>
    protected abstract Connection MakeNewConnection(string name, TcpClient client);

    /// <summary>
    /// Override to handle uplink communication exceptions, e.g. log them. Default implementation does nothing.
    /// !!!WARNING: Implementation may NOT LEAK.
    /// </summary>
    /// <param name="error">Error to handle</param>
    protected virtual void DoHandleUplinkError(Exception error)
    {
    }

    /// <summary>
    /// Override to handle operation failure - when the client can not communicate with
    /// server. Override will typically abort the hosting process.
    /// !!!WARNING: This is called only once by main thread - do NOT BLOCK and do NOT LEAK exceptions
    /// </summary>
    protected virtual void DoHandleUplinkFailure()
    {
    }

    /// <summary>
    /// Override to take action on the client per received command from the server.
    /// !!!WARNING: The handler is called on the main thread and should NOT LEAK errors
    /// </summary>
    /// <param name="connection">Connection which received the command</param>
    /// <param name="command">Command text</param>
    protected abstract void DoHandleCommand(Connection connection, string command);


    //returns null on connect inability
    private TcpClient tryConnect()
    {
      try
      {
        var client = new TcpClient();
        client.Connect(new IPEndPoint(IPAddress.Loopback, m_ServerPort));
        client.ReceiveBufferSize = Protocol.RECEIVE_BUFFER_SIZE;
        client.SendBufferSize = Protocol.SEND_BUFFER_SIZE;
        client.ReceiveTimeout = Protocol.RECEIVE_TIMEOUT_MS;
        client.SendTimeout = Protocol.SEND_TIMEOUT_MS;

        Protocol.Send(client, m_Name);//handshake
        var ackn = Protocol.Receive(client);

        if (ackn != "ok:{0}".Args(m_Name))
        {
          throw new SipcException("Connect attempt was unacknowledged by server");
        }

        return client;
      }
      catch(Exception error)
      {
        DoHandleUplinkError(error);
      }
      return null;
    }

    private void threadBody()
    {
      const int GRANULARITY_MS = 200;
      const int PROGRESSIVE_FAILURE_DELAY_MS = 800;//times 10 iterations is about 66 seconds with random margin 50%
      const float PROGRESSIVE_FAILURE_DELAY_VARIATION = 0.5f;
      const int MAX_CONSEQUITIVE_FAILURES = 10;

      var totalFailures = 0;
      var nextReconnectAttempt = DateTime.UtcNow;

      while(m_Client != null)
      {
        var now = DateTime.UtcNow;

        if (m_Connection.State != ConnectionState.OK)
        {
          if (now < nextReconnectAttempt)
          {
            m_Signal.WaitOne(GRANULARITY_MS);
            continue;
          }

          var newClient = tryConnect();
          if (newClient != null)
          {
            m_Client.Dispose();
            m_Client = newClient;
            m_Connection.Reconnect(newClient);
            totalFailures = 0;
          }
          else
          {
            //uplink lost
            totalFailures++;
            if (totalFailures > MAX_CONSEQUITIVE_FAILURES)
            {
              m_HasFailed = true;
              DoHandleUplinkFailure();
              break;//permanently tear the connection
            }
            else
            {
              DoHandleUplinkError(new SipcException($"Reconnection failure {totalFailures} times: {m_Connection}"));
            }
            nextReconnectAttempt = DateTime.UtcNow.AddMilliseconds((PROGRESSIVE_FAILURE_DELAY_MS * totalFailures).ChangeByRndPct(PROGRESSIVE_FAILURE_DELAY_VARIATION));//progressive delay
            m_Signal.WaitOne(GRANULARITY_MS);
          }
          continue;
        }

        if (m_Connection.State == ConnectionState.OK && ((now - m_Connection.LastReceiveUtc).TotalMilliseconds > Protocol.LIMBO_TIMEOUT_MS))
        {
          m_Connection.PutInLimbo();
          nextReconnectAttempt = DateTime.UtcNow.AddMilliseconds(Ambient.Random.NextScaledRandomInteger(GRANULARITY_MS));
          DoHandleUplinkError(new SipcException("Uplink connection put in limbo: " + m_Connection.ToString()));
          continue;
        }

        //Connection is OK

        //if there is anything to read,
        var command = m_Connection.TryReceive();
        if (command.IsNotNullOrWhiteSpace())
        {
          DoHandleCommand(m_Connection, command);
        }

        //ping if it is due
        if (m_Connection.State == ConnectionState.OK && (now - m_Connection.LastSendUtc).TotalMilliseconds > Protocol.PING_INTERVAL_MS)
        {
          m_Connection.Send(Protocol.CMD_PING);
        }

        if (m_Connection.State != ConnectionState.OK)
        {
          DoHandleUplinkError(new SipcException("Connection is !OK: " + m_Connection.ToString()));
        }

        m_Signal.WaitOne(GRANULARITY_MS.ChangeByRndPct(0.25f));
      }//while
    }
  }
}
