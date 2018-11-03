using System.Collections.Generic;

using Azos.Apps;
using Azos.Web.Messaging;
using Azos.Sky.Contracts;

namespace Azos.Sky.WebMessaging.Server
{
  /// <summary>
  /// Represents a messaging channel that hosts message boxes of the particular type
  /// </summary>
  public abstract class Channel : ServiceWithInstrumentationBase<WebMessageSystemService>
  {
    protected Channel(WebMessageSystemService director) : base(director)
    {

    }

    /// <summary>
    /// Writes msg into mailbox identified by the particular address.
    /// Reliability: this method must not leak errors, but should handle them depending on particular
    /// channel implementation and message importance (i.e. we may create ToDo that will try to redeliver)
    /// </summary>
    public abstract MsgChannelWriteResult Write(List<MsgSendInfo> deliveryList, int idx, string address, SkyWebMessage msg);


    /// <summary>
    /// Returns information about a particular mailbox on this channel or null if not found
    /// </summary>
    public abstract MailboxInfo GetMailboxInfo(MailboxID xid);

    /// <summary>
    /// Returns message headers for the specified mailbox and query or null
    /// </summary>
    public abstract MessageHeaders GetMessageHeaders(MailboxID xid, string query);

    /// <summary>
    /// Returns message count for the specified mailbox and query
    /// </summary>
    public abstract int GetMessageCount(MailboxID xid, string query);

    /// <summary>
    /// Fetches mailbox message by id or null if not found
    /// </summary>
    public abstract SkyWebMessage FetchMailboxMessage(MailboxMsgID mid);

    /// <summary>
    /// Fetches an attachment for the specified message by id and attachment index or null if not found
    /// </summary>
    public abstract Message.Attachment FetchMailboxMessageAttachment(MailboxMsgID mid, int attachmentIndex);

    /// <summary>
    /// Updates the particular mailbox message publication status along with operator and description
    /// </summary>
    public abstract void UpdateMailboxMessagePublication(MailboxMsgID mid, MsgPubStatus status, string oper, string description);

    /// <summary>
    /// Updates the particular mailbox message status
    /// </summary>
    public abstract void UpdateMailboxMessageStatus(MailboxMsgID mid, MsgStatus status, string folders, string adornments);

    /// <summary>
    /// Updates mailbox messages status
    /// </summary>
    public abstract void UpdateMailboxMessagesStatus(IEnumerable<MailboxMsgID> mids, MsgStatus status, string folders, string adornments);

  }
}
