/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azos.Conf;
using Azos.Serialization.JSON;

namespace Azos.Wave.Handlers
{
  /// <summary>
  /// Designates delivery statuses
  /// </summary>
  public enum MailboxDeliverStatus
  {
    Unavailable = -1,
    Success = 0,
    UnknownMailbox
  }


  /// <summary>
  /// Provides Server-Sent Events push support
  /// </summary>
  public class SSEMailboxHandler : WorkHandler
  {

     [ThreadStatic] private static StringBuilder ts_Builder;

    /// <summary>
    /// Formats SSE payload
    /// </summary>
    /// <param name="id">Optional ID</param>
    /// <param name="evt">Optional event</param>
    /// <param name="data">Optional data</param>
    /// <returns>A string encoded per SSE protocol spec</returns>
    /// <remarks>
    /// See:
    ///   https://hpbn.co/server-sent-events-sse/
    ///   https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events
    /// </remarks>
    public static string EncodeSSEContent(string id, string evt, string data)
    {
      var sb = ts_Builder;
      if (sb==null) sb = ts_Builder = new StringBuilder(512);//typical event size < 512 bytes

      if (id.IsNotNullOrWhiteSpace())
      {
        sb.Append("id: ");
        sb.Append(id);
        sb.Append('\n');
      }

      if (evt.IsNotNullOrWhiteSpace())
      {
        sb.Append("event: ");
        sb.Append(evt);
        sb.Append('\n');
      }

      if (data.IsNotNullOrWhiteSpace())
      {
        var i = data.IndexOf('\n');
        if (i<0)
        {
          sb.Append("data: ");
          sb.Append(data);
          sb.Append('\n');
        }
        else
        {
          var segs = data.Split('\n');
          foreach(var seg in segs)
          {
            sb.Append("data: ");
            sb.Append(seg);
            sb.Append('\n');
          }
        }
      }

      sb.Append('\n');

      var result = sb.ToString();

      if (sb.Length > 2048)
        ts_Builder = null;
      else
        sb.Clear();

      return result;
    }



    public SSEMailboxHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match)
                     : base(dispatcher, name, order, match)
    {
      ctor();
    }

    public SSEMailboxHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode)
    {
      ctor();
    }

    private void ctor()
    {
    }


    protected override void Destructor()
    {
      m_Mailboxes.ForEach(kvp => this.DontLeak(() => kvp.Value.Dispose(), errorFrom: "mbox.dctor()"));
      base.Destructor();
    }

    protected readonly ConcurrentDictionary<string, Mailbox> m_Mailboxes = new ConcurrentDictionary<string, Mailbox>(StringComparer.OrdinalIgnoreCase);


    /// <summary>
    /// Tries to deliver a named event into a mailbox with the specified ID returning the
    /// status of delivery. The actual delivery is asynchronous  by design, so this method returns instantly
    /// </summary>
    /// <param name="mailboxId">Id of the mailbox to deliver into</param>
    /// <param name="sseContent">Event to deliver. Encode the SSE with static EncodeSSEContent() function </param>
    /// <returns>Status, such as: Success | UnknownMailbox | MailboxNoSpace</returns>
    public MailboxDeliverStatus DeliverEvent(string mailboxId, string sseContent)
    {
      if (Disposed) return MailboxDeliverStatus.Unavailable;

      if (!m_Mailboxes.TryGetValue(mailboxId.NonBlank(nameof(mailboxId)), out var mbox)) return MailboxDeliverStatus.UnknownMailbox;

      return mbox.Deliver(sseContent.NonBlank(nameof(sseContent))) ? MailboxDeliverStatus.Success : MailboxDeliverStatus.Unavailable;
    }


    protected override void DoHandleWork(WorkContext work)
    {
      if (Disposed) throw HTTPStatusException.InternalError_500("Unavailable");

      work.Response.ContentType = Web.ContentType.SSE;
      work.Response.Buffered = false;

      var (isNew, mbox) = ConnectMailbox(work);

      if (mbox==null)
        throw HTTPStatusException.BadRequest_400("No mailbox could be connected");

      if (isNew)
        m_Mailboxes[mbox.Id] = mbox;

      try
      {
        flushFirstChunk(work);//Microsoft Http bug. need 1000 chars at first to start buffer flushing
        work.NoDefaultAutoClose = true;
      }
      catch
      {
        mbox.DisconnectClient(work);
      }
    }

    /// <summary>
    /// Override to connect a Mailbox descendant instance and subscribe to external event source if needed.
    /// This method may either create new or reuse existing instance, depending on logical mailbox identity which
    /// can be based on headers, session, cookies and other attributes of the incoming request.
    /// Return null to deny this connection request
    /// </summary>
    /// <param name="work">Work context under which the allocation of Mailbox takes place</param>
    /// <returns>
    ///   New or existing Mailbox to handle the connection; an existing instance may be returned for clients that re-connect.
    ///   Return null to close the connection with HTTP status code
    /// </returns>
    protected virtual (bool isNew, Mailbox mbox) ConnectMailbox(WorkContext work)
    {
      return (true, null);
    }

    /// <summary>
    /// Override to do custom implementation, such as send an unsubscribe command to external event source
    /// </summary>
    /// <param name="mailbox">An instance of mailbox being closed</param>
    protected virtual void CloseMailbox(Mailbox mailbox)
    {

    }

    private void flushFirstChunk(WorkContext work)
    {
      work.Response.Write("".PadLeft(1000, ' '));//Microsoft bug. need 1000 chars at first to start buffer flushing
      work.Response.Write("event:connect\ndata: {0}\n\n".Args(new {startDate = App.TimeSource.UTCNow}.ToJson(JsonWritingOptions.CompactASCII)));
      work.Response.Flush();
    }
  }


  public class Mailbox : DisposableObject
  {
    internal Mailbox(DateTime utcNow, string id, WorkContext work)
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

    private object m_TaskLock = new object();
    private volatile Task m_PendingTask;

    /// <summary>
    /// Mailbox unique immutable identifier/address
    /// </summary>
    public string Id => m_Id;

    /// <summary>
    /// Connects an additional web client returning true if it was added, false on duplicate or disposed
    /// </summary>
    public bool ConnectAnotherClient(WorkContext work)
    {
      if (work==null || work.Disposed || Disposed) return false;
      lock(m_Clients)
      {
        if (m_Clients.Contains(work)) return false;
        m_Clients.Add(work);
        return true;
      }
    }

    /// <summary>
    /// Disconnects a client removing it form list. Does not dispose it.
    /// Returns true if found and removed
    /// </summary>
    protected internal  bool DisconnectClient(WorkContext work)
    {
      if (work==null || work.Disposed || Disposed) return false;

      var result = false;
      lock (m_Clients)
        result = m_Clients.Remove(work);

      return result;
    }


    /// <summary>
    /// Enqueue event for delivery. Returns true if event was enqueued
    /// </summary>
    protected internal bool Deliver(string sseContent)
    {
      if (Disposed) return false;

      lock(m_TaskLock)
        m_PendingTask = m_PendingTask.ContinueWith((_, sse) =>{ try{ deliverCore((string)sse); }catch{ }}, sseContent);

      return true;
    }

    private void deliverCore(string sseContent)
    {
      WorkContext[] clients;
      List<WorkContext> tokill = null;

      lock(m_Clients) clients = m_Clients.ToArray();

      //this may take time, t4 outside lock
      foreach(var client in clients)
      {
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

  }


}
