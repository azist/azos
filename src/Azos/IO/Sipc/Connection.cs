/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Collections;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Azos.IO.Sipc
{
  /// <summary>
  /// Defines states for simple IPC connection
  /// </summary>
  public enum ConnectionState
  {
    /// <summary>
    /// The communication channel was not torn (yet) but the ping messages are not coming from the other side
    /// </summary>
    Limbo = -2,

    /// <summary>
    /// The circuit is deterministically broken,
    /// e.g. as a result of socket communication error
    /// </summary>
    Torn = -1,

    /// <summary>
    /// The status is unset yet
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// The communication is established and ping signals are coming through
    /// </summary>
    OK = 1
  }

  /// <summary>
  /// Represents a connection between server/client. Derived class may represent something more
  /// concrete such as a connected process with `AppProcessConnection`
  /// </summary>
  public class Connection : INamed
  {

    public Connection(string name, TcpClient client)
    {
      m_Client = client.NonNull(nameof(client));
      m_State = ConnectionState.OK;
      m_Name = name.NonBlank(nameof(name));
      m_StartUtc = m_LastReceiveUtc = m_LastSendUtc = DateTime.UtcNow;
      System.Threading.Thread.MemoryBarrier();
    }

    /// <summary>
    /// Called to re-link the connection with a new socket
    /// </summary>
    internal void Reconnect(TcpClient client)
    {
      lock(m_Lock)
      {
        m_Client = client.NonNull(nameof(client));
        m_State = ConnectionState.OK;
        m_LastReceiveUtc = m_LastSendUtc = DateTime.UtcNow;
      }
    }

    private object m_Lock = new object();
    private ConnectionState m_State;
    private string m_Name;
    private TcpClient m_Client;
    private DateTime m_StartUtc;
    private DateTime m_LastReceiveUtc;
    private DateTime m_LastSendUtc;

    internal volatile System.Threading.Tasks.Task m_ServerPendingWork;

    public string Name           => m_Name;
    public TcpClient Client      => m_Client;
    public ConnectionState State => m_State;
    public DateTime StartUtc     => m_StartUtc;
    public DateTime LastReceiveUtc  => m_LastReceiveUtc;
    public DateTime LastSendUtc     => m_LastSendUtc;

    public DateTime LastReceiveOrSendUtc => m_LastReceiveUtc > m_LastSendUtc ? m_LastReceiveUtc : m_LastSendUtc;
    public DateTime LastActivityUtc => LastReceiveOrSendUtc > m_StartUtc ? LastReceiveOrSendUtc : m_StartUtc;


    public override string ToString()
     => $"{GetType().DisplayNameWithExpandedGenericArgs()}(`{Name}`) => {State} :: {LastActivityUtc}";

    internal void PutInLimbo()
    {
      lock(m_Lock)
      {
        m_State = ConnectionState.Limbo;
      }
    }

    //called by a single thread at a time, does not leak
    internal string TryReceive()
    {
      lock (m_Lock)
      {
        try
        {
          if (Client.Available <= 0) return null;
        }
        catch
        {
          m_State = ConnectionState.Torn;
          return null;
        }

        if (m_State != ConnectionState.OK && m_State != ConnectionState.Limbo) return null;//failed to receive

        try
        {
          var result = Protocol.Receive(Client);
          m_State = ConnectionState.OK;//A successful receipt (of anything) gets rid of LIMBO
          m_LastReceiveUtc = DateTime.UtcNow;
          return result;
        }
        catch
        {
          m_State = ConnectionState.Torn;
          return null;
        }
      }
    }

    /// <summary>
    /// Call to send command to the other side.
    /// The connection has to be in a Ok or Limbo state
    /// </summary>
    /// <param name="command">Command test to send</param>
    /// <returns>True if sent, false on inability to send</returns>
    public bool Send(string command)
    {
      command.NonBlank(nameof(command));
      lock(m_Lock)
      {
        if (m_State != ConnectionState.OK && m_State != ConnectionState.Limbo) return false;//failed to send

        try
        {
          Protocol.Send(Client, command);
          ////20221017 DKh #786 - Send works in OK and Limbo, but it DOES NOT put Limbo -> OK. Only Receive does that
          ////m_State = ConnectionState.OK;
          m_LastSendUtc = DateTime.UtcNow;
          return true;
        }
        catch
        {
          m_State = ConnectionState.Torn;
          return false;
        }
      }
    }
  }
}
