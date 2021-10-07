/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Security.Web;
using Azos.Wave.Mvc;

namespace Azos.Web.Messaging.Services.Server
{
  /// <summary>
  /// Provides API controller service for Web Messaging functionality
  /// </summary>
  [NoCache]
  [ApiControllerDoc(
    BaseUri = "/messaging",
    Title = "Message management",
    Description = @"Sends messages (e.g. email, text, fax) to recipient/s via any of the configured messaging channels",
    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE }
  )]
  [MessagingPermission(MessagingAccessLevel.Minimum)]
  public class Message : ApiProtocolController
  {

    [Inject] IMessagingLogic m_MessagingLogic;
    [Inject] IMessageArchiveLogic m_ArchiveLogic;

    [ApiEndpointDoc(
        Uri = "list",
        Title = "Message List Filter",
        Description = "Returns an array of MessageInfo objects for the supplied filter",
        Methods = new[] { "POST: post JSON filter form, get json with array of MessageInfo objects per filter" },
        RequestHeaders = new[] { "Accept: application/json (required)" },
        RequestBody = "JSON representation of filter form",
        ResponseContent = "JSON [] of MemberInfo",
        TypeSchemas = new[] { typeof(MessageInfo) }
      )]
    [MessagingPermission(MessagingAccessLevel.QueryOwn)]
    [ActionOnPost(Name = "list"), AcceptsJson]
    public async Task<object> ListMessages(MessageListFilter filter) => await ApplyFilterAsync(filter).ConfigureAwait(false);


    [ApiEndpointDoc(
      Uri = "send",
      Title = "Sends a single message envelope",
      Description = "Sends a single message envelope returning unique ID for the sent message; the ID can be later used for querying",
      Methods = new[] { "POST: post Json message envelope, get Json with unique message id" },
      RequestHeaders = new[] { "Accept: application/json (required)" },
      RequestBody = "Json representation of message envelope",
      ResponseContent = "Json with message ID assigned by the server on send",
      TypeSchemas = new[] { typeof(MessageEnvelope) }
    )]
    [MessagingPermission(MessagingAccessLevel.Send)]
    [ActionOnPost(Name = "send"), AcceptsJson]
    public async Task<object> SendMessage(MessageEnvelope envelope) => await SaveNewAsync(envelope).ConfigureAwait(false);


    [ApiEndpointDoc(
      Uri = "message",
      Title = "Gets message by id",
      Description = "Gets a message by its ID",
      Methods = new[] { "GET: gets Json message representation" },
      RequestHeaders = new[] { "Accept: application/json (required)" },
      ResponseContent = "Json with message content without attachment bodies",
      RequestQueryParameters = new[] { "msgId: id of message that was assigned on message send" },
      TypeSchemas = new[] { typeof(Azos.Web.Messaging.Message) }
    )]
    [MessagingPermission(MessagingAccessLevel.QueryOwn)]
    [ActionOnGet(Name = "message"), AcceptsJson]
    public async Task<object> GetMessage(string msgId) => GetLogicResult(await m_ArchiveLogic.GetMessageAsync(msgId).ConfigureAwait(false));

    [ApiEndpointDoc(
      Uri = "attachment",
      Title = "Gets message attachment by id",
      Description = "Gets a message attachment by its ID",
      Methods = new[] { "GET: gets Json message attachment representation" },
      RequestHeaders = new[] { "Accept: application/json (required)" },
      ResponseContent = "Json with message attachment content body",
      RequestQueryParameters = new[] { "msgId: id of message that was assigned on message send",
                                       "attId: int sequence of attachment in the message" },
      TypeSchemas = new[] { typeof(Azos.Web.Messaging.Message), typeof(Azos.Web.Messaging.Message.Attachment) }
    )]
    [MessagingPermission(MessagingAccessLevel.QueryOwn)]
    [ActionOnGet(Name = "attachment"), AcceptsJson]
    public async Task<object> GetAttachment(string msgId, int attId) => GetLogicResult(await m_ArchiveLogic.GetMessageAttachmentAsync(msgId, attId).ConfigureAwait(false));

    [ApiEndpointDoc(
      Uri = "status",
      Title = "Gets status log for specified message id",
      Description = "Gets status log for specified message id",
      Methods = new[] { "GET: gets message status log" },
      RequestHeaders = new[] { "Accept: application/json (required)" },
      ResponseContent = "Json with message log",
      RequestQueryParameters = new[] { "msgId: id of message that was assigned on message send"},
      TypeSchemas = new[] { typeof(Azos.Web.Messaging.Message), typeof(MessageStatusLog) }
    )]
    [MessagingPermission(MessagingAccessLevel.QueryOwn)]
    [ActionOnGet(Name = "status"), AcceptsJson]
    public async Task<object> GetStatusLog(string msgId) => GetLogicResult(await m_ArchiveLogic.GetMessageStatusLogAsync(msgId).ConfigureAwait(false));
  }
}
