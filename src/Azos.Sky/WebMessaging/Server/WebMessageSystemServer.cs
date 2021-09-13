/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Collections;
using Azos.Web.Messaging;

using Azos.Sky.Contracts;

namespace Azos.Sky.WebMessaging.Server
{
  /// <summary>
  /// Glue adapter for Contracts.IWebMessageSystem
  /// </summary>
  public sealed class WebMessageSystemServer : Contracts.IWebMessageSystem
  {
    [Inject] IApplication m_App;

    public WebMessageSystemService Service => m_App.NonNull(nameof(m_App))
                                              .Singletons
                                              .Get<WebMessageSystemService>()
                                              .NonNull(nameof(WebMessageSystemService));

    public SkyWebMessage FetchMailboxMessage(MailboxMsgID mid)
      => Service.FetchMailboxMessage(mid);

    public Message.Attachment FetchMailboxMessageAttachment(MailboxMsgID mid, int attachmentIndex)
      => Service.FetchMailboxMessageAttachment(mid, attachmentIndex);

    public MailboxInfo GetMailboxInfo(MailboxID xid)
      => Service .GetMailboxInfo(xid);

    public int GetMailboxMessageCount(MailboxID xid, string query)
      => Service.GetMailboxMessageCount(xid, query);

    public MessageHeaders GetMailboxMessageHeaders(MailboxID xid, string query)
      => Service.GetMailboxMessageHeaders(xid, query);

    public MsgSendInfo[] SendMessage(SkyWebMessage msg)
      => Service.SendMessage(msg);

    public void UpdateMailboxMessagePublication(MailboxMsgID mid, MsgPubStatus status, string oper, string description)
      => Service.UpdateMailboxMessagePublication(mid, status, oper, description);

    public void UpdateMailboxMessageStatus(MailboxMsgID mid, MsgStatus status, string folders, string adornments)
      => Service.UpdateMailboxMessageStatus(mid, status, folders, adornments);

    public void UpdateMailboxMessagesStatus(IEnumerable<MailboxMsgID> mids, MsgStatus status, string folders, string adornments)
      => Service.UpdateMailboxMessagesStatus(mids, status, folders, adornments);
  }

}
