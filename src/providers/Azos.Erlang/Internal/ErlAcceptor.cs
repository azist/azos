/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Azos.Erlang.Internal
{
  /// <summary>
  /// This thread simply listens for incoming connections
  /// </summary>
  internal class ErlAcceptor : DisposableObject
  {
  #region .ctor

    public ErlAcceptor(ErlLocalNode node, int port, IPAddress address = null)
    {
      m_Node = node;

      m_Sock = new TcpListener(address ?? Dns.GetHostEntry("localhost").AddressList[0], port);
      m_Sock.Start();
      m_Port = ((System.Net.IPEndPoint)m_Sock.LocalEndpoint).Port;
      node.Port = this.m_Port;

      publishPort();
      m_Thread = new Thread(threadSpin);
      m_Thread.IsBackground = true;
      m_Thread.Name = "acceptor " + node.NodeName;
      m_Thread.Start();
    }

    protected override void Destructor()
    {
      base.Destructor();
      Stop();
    }

  #endregion

  #region Fields

    private ErlLocalNode    m_Node;
    private TcpListener     m_Sock;
    private int             m_Port;
    private bool            m_Active = true;
    private Thread          m_Thread;

  #endregion

  #region Props

    public int Port { get { return m_Port; } }

  #endregion

  #region Public



    public void Stop()
    {
      m_Active = false;
      if (m_Thread != null)
      {
        closeSock(m_Sock);
        m_Thread.Join();
        m_Thread = null;
      }
    }

  #endregion

  #region .pvt
    private void threadSpin()
    {
      try
      {
        threadSpinCore();
      }
      catch(Exception error)
      {
        var em = new Azos.Log.Message
        {
          Type = Log.MessageType.CatastrophicError,
          Topic = CoreConsts.ERLANG_TOPIC,
          From = GetType().Name + "threadSpin()",
          Text = "threadSpinCore leaked: " + error.ToMessageWithType(),
          Exception = error
        };
        App.Log.Write(em);
      }
    }

    private void threadSpinCore()
    {
      TcpClient newsock = null;
      ErlConnection conn;

      m_Node.OnNodeStatusChange(m_Node.NodeName, true, null);

      while (m_Active)
      {
        conn = null;

        try
        {
          newsock = m_Sock.AcceptTcpClient();
        }
        catch (System.Exception e)
        {
          m_Node.OnNodeStatusChange(m_Node.NodeName, false, e);
          break;
        }

        try
        {
          conn = new ErlConnection(m_Node, newsock);
          m_Node.Add(conn);
        }
        catch (ErlAuthException e)
        {
          m_Node.OnConnectAttempt(conn != null ? conn.Name : ErlAtom.Null, Direction.Inbound, e);
          closeSock(newsock);
        }
        catch (ErlException e)
        {
          m_Node.OnConnectAttempt(conn != null ? conn.Name : ErlAtom.Null, Direction.Inbound, e);
          closeSock(newsock);
        }
        catch (Exception e)
        {
          closeSock(newsock);
          closeSock(m_Sock);
          m_Node.OnNodeStatusChange(m_Node.NodeName, false, e);
          break;
        }
      }

      App.Log.Write(Log.MessageType.Info,
          StringConsts.ERL_STOPPING_SERVER.Args(m_Node.NodeLongName, "Listener"));

      stop();
      m_Thread = null;
    }


    private void stop()
    {
      unPublishPort();
      m_Active = true;
      closeSock(m_Sock);
    }

    private bool publishPort()
    {
      if (m_Node.Epmd != null)
        return false;
      // already published
      ErlEpmd.PublishPort(m_Node);
      return true;
    }

    private void unPublishPort()
    {
      // unregister with epmd
      ErlEpmd.UnPublishPort(m_Node);

      // close the local descriptor (if we have one)
      closeSock(m_Node.Epmd);
      m_Node.Epmd = null;
    }

    private void closeSock(TcpListener s)
    {
      try { if (s != null) s.Stop(); }
      catch { }
    }

    private void closeSock(TcpClient s)
    {
        try { if (s != null) s.Close(); }
        catch { }
    }

    private void closeSock(IErlTransport s)
    {
      try { if (s != null) s.Close(); }
      catch { }
    }

  #endregion
  }
}
