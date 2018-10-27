using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Azos.Erlang
{
  /// <summary>
  /// General interface of TCP transports (i.e. usual TCP channel or SSH tunneled TCP channel)
  /// </summary>
  public interface IErlTransport : IDisposable
  {
    /// <summary>
    /// Set receive buffer size
    /// </summary>
    int ReceiveBufferSize { get; set; }

    /// <summary>
    /// Set send buffer size
    /// </summary>
    int SendBufferSize { get; set; }

    /// <summary>
    /// Network stream
    /// </summary>
    Stream GetStream();

    /// <summary>
    /// Connects to remote host:port
    /// </summary>
    void Connect(string host, int port);

    /// <summary>
    /// Connects to remote host:port with a timeout in milliseconds
    /// </summary>
    void Connect(string host, int port, int timeoutMsec);

    /// <summary>
    /// Close connection
    /// </summary>
    void Close();

    /// <summary>
    /// Remote endpoint
    /// </summary>
    EndPoint RemoteEndPoint { get; }

    /// <summary>
    /// Sets cocket options
    /// </summary>
    void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName,
                         bool optionValue);

    /// <summary>
    /// NoDelay socket
    /// </summary>
    bool NoDelay { get; set; }

    /// <summary>
    /// Trace event
    /// </summary>
    event TraceEventHandler Trace;

    /// <summary>
    /// Erlang node name
    /// </summary>
    string NodeName { get; set; }

    /// <summary>
    /// ConnectTimeout in milliseconds
    /// </summary>
    int ConnectTimeout { get; set; }

    /// <summary>
    /// Port of SSH server
    /// </summary>
    int SSHServerPort { get; set; }

    /// <summary>
    /// SSH user name
    /// </summary>
    string SSHUserName { get; set; }

    /// <summary>
    /// Private key file path (only for AuthenticationType = PublicKey)
    /// Required SSH2 ENCRYPTED PRIVATE KEY format.
    /// </summary>
    string SSHPrivateKeyFilePath { get; set; }

    /// <summary>
    /// Type of auth on SSH server.
    /// Valid values: "PublicKey", "Password".
    /// </summary>
    string SSHAuthenticationType { get; set; }
  }
}