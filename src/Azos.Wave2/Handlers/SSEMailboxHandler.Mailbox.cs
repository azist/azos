/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azos.Wave.Handlers
{
  public class Mailbox : DisposableObject
  {
    public Mailbox(DateTime utcNow, string id, WorkContext work)
    {
      m_Id = id.NonBlank(nameof(id));
      m_ConnectTimestampUtc = utcNow;
      m_Clients = new List<WorkContext>();
      m_Clients.Add(work.NonNull(nameof(work)));
      m_PendingTask = Task.CompletedTask;
    }

    protected override void Destructor()
    {
      m_Clients.ForEach( c => {try{ c.Dispose(); }catch{ }});
      base.Destructor();
    }

    private string m_Id;
    private DateTime m_ConnectTimestampUtc;
    private volatile List<WorkContext> m_Clients;
    private DateTime? m_LastIdleCheckUtc;
    private volatile bool m_IsDead;

    private object m_TaskLock = new object();
    private volatile Task m_PendingTask;

    /// <summary>
    /// Mailbox unique immutable identifier/address
    /// </summary>
    public string Id => m_Id;

    /// <summary>
    /// This mailbox has been marked for deletion (e.g. because it has been idle) and
    /// will not be able to process more requests
    /// </summary>
    public bool IsDead => m_IsDead;

    /// <summary>
    /// Connects an additional web client returning true if it was added, false on duplicate or disposed
    /// </summary>
    public bool ConnectAnotherClient(WorkContext work)
    {
      if (work==null || work.Disposed || m_IsDead || Disposed) return false;

      lock(m_Clients)
      {
        if (m_IsDead || m_Clients.Contains(work)) return false;
        m_Clients.Add(work);
        return true;
      }
    }

    /// <summary>
    /// Disconnects a client removing it form list. Does not dispose it.
    /// Returns true if found and removed
    /// </summary>
    protected internal bool DisconnectClient(WorkContext work)
    {
      if (work==null || work.Disposed || Disposed) return false;

      var result = false;
      lock (m_Clients)
        result = m_Clients.Remove(work);

      return result;
    }

    /// <summary>
    /// Checks if the mailbox has had some delivery attempts made within the idle time
    /// and has at least one client connected
    /// </summary>
    protected internal bool CheckAlive(DateTime utcNow, int maxIdleSec)
    {
      if (m_IsDead || Disposed) return false;

      lock(m_Clients)
      {
        if (m_IsDead || Disposed) return false;

        var idleSec = m_LastIdleCheckUtc.HasValue ? (int)(utcNow - m_LastIdleCheckUtc.Value).TotalSeconds : 0;
        m_LastIdleCheckUtc = utcNow;
        if (idleSec > maxIdleSec)
        {
          m_IsDead = true; //timed out for inactivity
          return false;
        }

        var hasClients = m_Clients.Count > 0;
        if (!hasClients)
        {
          m_IsDead = true;//noone is connected
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Enqueue event for delivery. Returns true if event was enqueued
    /// </summary>
    protected internal bool Deliver(string sseContent)
    {
      if (m_IsDead || Disposed || sseContent.IsNullOrWhiteSpace()) return false;

      m_LastIdleCheckUtc = null;//something gets delivered, the mailbox is NOT idle

      lock(m_TaskLock)
        m_PendingTask = m_PendingTask.ContinueWith((_, sse) =>{ try{ deliverCore((string)sse); }catch{ }}, sseContent);

      return true;
    }

    private void deliverCore(string sseContent)
    {
      if (m_IsDead || Disposed) return;

      WorkContext[] clients;
      List<WorkContext> tokill = null;

      lock(m_Clients) clients = m_Clients.ToArray();

      //this may take time, t4 outside lock
      foreach(var client in clients)
      {
        if (m_IsDead || Disposed) return;
        try
        {
          client.Response.Write(sseContent);
        }
        catch
        {
          try { client.Dispose(); } catch { }//this may rethrow on various partially torn connections
          if (tokill==null) tokill = new List<WorkContext>();
          tokill.Add(client);
        }
      }

      //remove dead connections - quick
      if (tokill!=null)
        lock(m_Clients)
          foreach(var dead in tokill)
           m_Clients.Remove(dead);
    }//deliverCore

  }//Mailbox
}
