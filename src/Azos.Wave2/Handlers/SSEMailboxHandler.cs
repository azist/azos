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
    private const int MANAGER_INTERVAL_MS = 3_179;

    public const int MIN_IDLE_TIMEOUT_SEC = 5;
    public const int MAX_IDLE_TIMEOUT_SEC = 3 * 60;
    public const int DEFAULLT_IDLE_TIMEOUT_SEC = 25;


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
      if (sb == null)
      {
        sb = ts_Builder = new StringBuilder(512);//typical event size < 512 bytes
      }
      else
      {
        sb.Clear();
      }


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

    private void ctor() => scheduleNextManage();


    protected override void Destructor()
    {
      try { m_CancelSource.Cancel(true); } catch{ /* canceling task.Delay() - nothing to log */ }
      m_Mailboxes.ForEach(kvp => this.DontLeak(() => CloseMailbox(kvp.Value), errorFrom: "dctor.CloseMailbox()"));
      m_CancelSource.Dispose();
      base.Destructor();
    }

    private CancellationTokenSource m_CancelSource = new CancellationTokenSource();
    private Task m_ManagerTask;
    protected readonly ConcurrentDictionary<string, Mailbox> m_Mailboxes = new ConcurrentDictionary<string, Mailbox>(StringComparer.OrdinalIgnoreCase);
    private int m_IdleTimeoutSec = DEFAULLT_IDLE_TIMEOUT_SEC;


    /// <summary>
    /// Idle mailbox timeout after which the mailbox gets disposed.
    /// A mailbox is idle for a period of time when no events are being delivered to that mailbox
    /// </summary>
    [Config(Default = DEFAULLT_IDLE_TIMEOUT_SEC)]
    public int IdleTimeoutSec
    {
      get => m_IdleTimeoutSec;
      set => value.KeepBetween(MIN_IDLE_TIMEOUT_SEC, MAX_IDLE_TIMEOUT_SEC);
    }


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
    /// Return null to deny this connection request. The default implementation creates a general-purpose mailbox
    /// keyed on CurrentCallUser.Name context
    /// </summary>
    /// <param name="work">Work context under which the allocation of Mailbox takes place</param>
    /// <returns>
    ///   New or existing Mailbox to handle the connection; an existing instance may be returned for clients that re-connect.
    ///   Return null to close the connection with HTTP status code
    /// </returns>
    protected virtual (bool isNew, Mailbox mbox) ConnectMailbox(WorkContext work)
    {
      work.NeedsSession(onlyExisting: true);
      var user = work.Session?.User;

      if (user == null || !user.IsAuthenticated)
        throw HTTPStatusException.Unauthorized_401();

      if (user.Name.IsNullOrWhiteSpace())
        return (false, null);

      var result  = new Mailbox(App.TimeSource.UTCNow, user.Name, work);
      return (true, result);
    }

    /// <summary>
    /// Override to do custom implementation, such as send an unsubscribe command to external event source.
    /// Default implementation disposes the mailbox
    /// </summary>
    /// <param name="mailbox">An instance of mailbox being closed</param>
    protected virtual void CloseMailbox(Mailbox mailbox)
    {
      if (mailbox==null) return;
      mailbox.Dispose();
    }

    private void flushFirstChunk(WorkContext work)
    {
      work.Response.Write("".PadLeft(1000, ' '));//Microsoft bug. need 1000 chars at first to start buffer flushing
      work.Response.Write("event: connect\ndata: {0}\n\n".Args(new {startDate = App.TimeSource.UTCNow}.ToJson(JsonWritingOptions.CompactASCII)));
      work.Response.Flush();
    }

    private void scheduleNextManage()
    {
      if (Disposed) return;
      m_ManagerTask = Task.Delay(MANAGER_INTERVAL_MS, m_CancelSource.Token).ContinueWith(_ => manage());
    }

    private void manage()
    {
      if (Disposed) return;
      this.DontLeak(()=> manageUnsafe());
      scheduleNextManage();
    }

    private void manageUnsafe()
    {
      if (Disposed) return;

      var allMailboxes = m_Mailboxes.Values.ToArray();//snapshot
      List<Mailbox> tokill = null;
      var now = App.TimeSource.UTCNow;

      foreach(var mbox in allMailboxes)
      {
        if (Disposed) break;
        var isAlive = mbox.CheckAlive(now, m_IdleTimeoutSec);
        if (isAlive) continue;

        if (tokill==null) tokill = new List<Mailbox>();
        tokill.Add(mbox);
      }

      if (tokill != null)
      {
        foreach(var dead in tokill)
        {
          if (Disposed) break;
          m_Mailboxes.TryRemove(dead.Id, out var _);
          this.CloseMailbox(dead);
          this.DontLeak(() => dead.Dispose(), errorFrom: "dead.Dispose()");
        }
      }
    }

  }

}
