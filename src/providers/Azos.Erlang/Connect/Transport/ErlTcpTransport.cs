
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Azos.Erlang
{
  /// <summary>
  /// TCP transport
  /// </summary>
  public class ErlTcpTransport : IErlTransport
  {
    #region Fields

    protected TcpClient m_Client;

    #endregion

    #region Events

    /// <summary>
    /// Transmits trace messages
    /// </summary>
    public event TraceEventHandler Trace = delegate { };

    #endregion

    #region .ctor

    public ErlTcpTransport()
    {
      m_Client = new TcpClient();
    }

    public ErlTcpTransport(TcpClient client)
    {
      this.m_Client = client;
    }

    public ErlTcpTransport(string host, int port)
    {
      m_Client = new TcpClient(host, port);
    }

    #endregion

    #region Public

    public string NodeName { get; set; }

    public int ReceiveBufferSize
    {
      get { return m_Client.ReceiveBufferSize; }
      set { m_Client.ReceiveBufferSize = value; }
    }

    public int SendBufferSize
    {
      get { return m_Client.SendBufferSize; }
      set { m_Client.SendBufferSize = value; }
    }

    public bool NoDelay
    {
      get { return m_Client.NoDelay; }
      set { m_Client.NoDelay = value; }
    }

    public Stream GetStream()
    {
      return m_Client.GetStream();
    }

    public virtual void Connect(string host, int port)
    {
      m_Client.Connect(host, port);
    }

    public virtual void Connect(string host, int port, int timeoutMsec)
    {
      if (!m_Client.ConnectAsync(host, port).Wait(timeoutMsec))
        throw new ErlException(StringConsts.ERL_CONN_CANT_CONNECT_TO_HOST_ERROR.Args("TCP", host, port));
    }

    public void Close()   { m_Client.Close(); }
    public void Dispose() { m_Client.Client.Dispose(); }

    public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName,
                                bool optionValue)
    {
      m_Client.Client.SetSocketOption(optionLevel, optionName, optionValue);
    }

    public EndPoint RemoteEndPoint        { get { return m_Client.Client.RemoteEndPoint; } }

    public int      ConnectTimeout        { get; set; }
    public int      SSHServerPort         { get; set; }
    public string   SSHUserName           { get; set; }
    public string   SSHPrivateKeyFilePath { get; set; }
    public string   SSHAuthenticationType { get; set; }

    #endregion
  }
}