using System;
using System.Reflection;
using System.Collections.Generic;

using Azos.Glue;
using Azos.Glue.Protocol;
using Azos.Data;
using Azos.Web.Messaging;

using Azos.Sky.WebMessaging;

namespace Azos.Sky.Contracts
{
  /// <summary>
  /// Represents a distributed messaging system akin to GMail/Live etc.
  /// Consumers should use SkyWebMessageSink via MessageService instance to route messages
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface IWebMessageSystem : ISkyService
  {
    /// <summary>
    /// Sends message into the system routing it to appropriate mailbox/es.
    /// The function does not guarantee to return all of the mailboxes (as there may be too many of them).
    /// The return is used to quickly pick who the message was routed to.
    /// Null is returned if the message could not be routed to any recipient
    /// </summary>
    MsgSendInfo[] SendMessage(SkyWebMessage msg);

    /// <summary>
    /// Returns information about the specified mailbox or null if not found
    /// </summary>
    MailboxInfo GetMailboxInfo(MailboxID xid);

    /// <summary>
    /// Updates the particular mailbox message status
    /// </summary>
    [OneWay]
    [ArgsMarshalling(typeof(RequestMsg_IWebMessageSystem_UpdateMailboxMessageStatus))]
    void UpdateMailboxMessageStatus(MailboxMsgID mid, MsgStatus status, string folders, string adornments);

    /// <summary>
    /// Updates mailbox messages status
    /// </summary>
    [OneWay]
    [ArgsMarshalling(typeof(RequestMsg_IWebMessageSystem_UpdateMailboxMessagesStatus))]
    void UpdateMailboxMessagesStatus(IEnumerable<MailboxMsgID> mids, MsgStatus status, string folders, string adornments);

    /// <summary>
    /// Updates the particular mailbox message publication status along with operator and description
    /// </summary>
    [OneWay]
    [ArgsMarshalling(typeof(RequestMsg_IWebMessageSystem_UpdateMailboxMessagePublication))]
    void UpdateMailboxMessagePublication(MailboxMsgID mid, MsgPubStatus status, string oper, string description);

    /// <summary>
    /// Gets the mailbox message headers without body or attachments.
    /// The query format is implementation-specific, by default the system should fetch up to 32 latest unread messages
    /// </summary>
    MessageHeaders GetMailboxMessageHeaders(MailboxID xid, string query);

    /// <summary>
    /// Gets the mailbox messages count
    /// </summary>
    int GetMailboxMessageCount(MailboxID xid, string query);

    /// <summary>
    /// Fetches mailbox message by id or null if not found
    /// </summary>
    SkyWebMessage FetchMailboxMessage(MailboxMsgID mid);

    /// <summary>
    /// Fetches an attachment for the specified message by id and attachment index or null if not found
    /// </summary>
    SkyWebMessage.Attachment FetchMailboxMessageAttachment(MailboxMsgID mid, int attachmentIndex);
  }

  /// <summary>
  /// Contract for client of IWebMessageStore svc
  /// </summary>
  public interface IWebMessageSystemClient : ISkyServiceClient, IWebMessageSystem
  {
    CallSlot Async_SendMessage(SkyWebMessage data);
    CallSlot Async_GetMailboxMessageHeaders(MailboxID xid, string query);
    CallSlot Async_FetchMailboxMessage(MailboxMsgID mid);
    CallSlot Async_FetchMailboxMessageAttachment(MailboxMsgID mid, int attachmentIndex);
  }

  /// <summary>
  /// Returns messages without body/attachments aka "headers" with additional metadata
  /// </summary>
  public class MessageHeaders
  {
    public MessageHeaders(MailboxID xid, SkyWebMessage[] headers) { Mailbox = xid; Headers = headers;}

    public readonly MailboxID Mailbox;
    public readonly SkyWebMessage[] Headers;

    //future: additional inromation returned for paging etc..
  }

  /// <summary>
  /// Result of the message write operation
  /// </summary>
  public struct MsgSendInfo
  {
    public MsgSendInfo(MsgChannelWriteResult writeResult, MailboxMsgID? delivered, int addresseeIdx)
    {
      WriteResult = writeResult;
      Delivered = delivered;
      AddresseeIdx =addresseeIdx;
    }

    public readonly MsgChannelWriteResult WriteResult;
    public readonly MailboxMsgID? Delivered;
    public readonly int AddresseeIdx;
  }

  public sealed class RequestMsg_IWebMessageSystem_UpdateMailboxMessageStatus : RequestMsg
  {
    public RequestMsg_IWebMessageSystem_UpdateMailboxMessageStatus(MethodInfo method, Guid? instance) : base(method, instance) { }
    public RequestMsg_IWebMessageSystem_UpdateMailboxMessageStatus(TypeSpec contract, MethodSpec method, bool oneWay, Guid? instance) : base(contract, method, oneWay, instance) { }

    public MailboxMsgID MethodArg_0_mid;
    public MsgStatus    MethodArg_1_status;
    public string       MethodArg_2_folders;
    public string       MethodArg_3_adornments;
  }

  public sealed class RequestMsg_IWebMessageSystem_UpdateMailboxMessagesStatus : RequestMsg
  {
    public RequestMsg_IWebMessageSystem_UpdateMailboxMessagesStatus(MethodInfo method, Guid? instance) : base(method, instance) { }
    public RequestMsg_IWebMessageSystem_UpdateMailboxMessagesStatus(TypeSpec contract, MethodSpec method, bool oneWay, Guid? instance) : base(contract, method, oneWay, instance) { }

    public IEnumerable<MailboxMsgID> MethodArg_0_mids;
    public MsgStatus                 MethodArg_1_status;
    public string                    MethodArg_2_folders;
    public string                    MethodArg_3_adornments;
  }

  public sealed class RequestMsg_IWebMessageSystem_UpdateMailboxMessagePublication : RequestMsg
  {
    public RequestMsg_IWebMessageSystem_UpdateMailboxMessagePublication(MethodInfo method, Guid? instance) : base(method, instance) { }
    public RequestMsg_IWebMessageSystem_UpdateMailboxMessagePublication(TypeSpec contract, MethodSpec method, bool oneWay, Guid? instance) : base(contract, method, oneWay, instance) { }

    public MailboxMsgID MethodArg_0_mid;
    public MsgPubStatus MethodArg_1_status;
    public string       MethodArg_2_oper;
    public string       MethodArg_3_description;
  }
}