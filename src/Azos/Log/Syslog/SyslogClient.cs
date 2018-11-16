/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Text;
using System.Net.Sockets;

using Azos.Conf;

namespace Azos.Log.Syslog
{

  /// <summary>
  /// Implements SYSLOG UDP client
  /// </summary>
  public sealed class SyslogClient : DisposableObject, IConfigurable
  {
    public const int SYSLOG_PORT = 514;

    public SyslogClient()
    {
    }

    public SyslogClient(string host): this(host, SYSLOG_PORT)
    {
    }

    public SyslogClient(string host, int port)
    {
      m_Host = host;
      m_Port = port;
    }

    protected override void Destructor()
    {
      close();
      base.Destructor();
    }



    private string m_Host;
    private int m_Port = SYSLOG_PORT;

    private object m_SocketSync = new Object();
    private UdpClient m_UdpClient;


    [Config]
    public string Host
    {
      get { return m_Host;}
      set
      {
        if (m_Host != value)
        {
          m_Host = value;
          close();
        }
      }
    }

    [Config]
    public int Port
    {
      get { return m_Port;}
      set
      {
        if (m_Port != value)
        {
          m_Port = value;
          close();
        }
      }
    }

    public void Configure(IConfigSectionNode node)
    {
        ConfigAttribute.Apply(this, node);
    }


    public void Send(SyslogMessage message)
    {
      var dgram = string.Format(
                              "<{0}>{1}: {2} {3}",
                              message.Priority,
                              message.LocalTimeStamp.ToString("MMM dd HH:mm:ss"),
                              System.Environment.MachineName,
                              message.Text);


      var buf = Encoding.ASCII.GetBytes(dgram);

      lock(m_SocketSync)
      {
        open();
        m_UdpClient.Send(buf, buf.Length);
      }
    }


    public void Close()
    {
      close();
    }


    private void close()
    {
      lock (m_SocketSync)
      {
        if (m_UdpClient!=null)
        {
          m_UdpClient.Close();
          m_UdpClient = null;
        }
      }
    }

    private void open()
    {
      lock (m_SocketSync)
      {
        if (m_UdpClient==null)
        {
          var cl  = new UdpClient();
          cl.Connect(m_Host, m_Port);
          m_UdpClient = cl;
        }
      }
    }
  }
}
