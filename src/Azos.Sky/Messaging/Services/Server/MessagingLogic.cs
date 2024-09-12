/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Log;
using Azos.Serialization.JSON;

namespace Azos.Sky.Messaging.Services.Server
{
  /// <summary>
  /// Implements IMessagingLogic  and IMessageArchiveLogic
  /// </summary>
  public class MessagingLogic : ModuleBase, IMessagingLogic, IMessageArchiveLogic
  {
    public const string CONFIG_MESSAGE_ROUTER_SECTION = "message-router";

    public MessagingLogic(IApplication app) : base(app) => ctor();
    public MessagingLogic(IModule parent) : base(parent) => ctor();

    private void ctor()
    {
      m_Router = new MessageDaemon(this);
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Router);
      base.Destructor();
    }

    protected MessageDaemon m_Router;
    protected ILog m_OpLog;

    public override bool IsHardcodedModule => true;
    public override string ComponentLogTopic => CoreConsts.WEBMSG_TOPIC;

    [Config]
    public string OplogModuleName{ get; set; }


    /// <inheritdoc/>
    public virtual async Task<string> SendAsync(MessageEnvelope envelope)
    {
      try
      {
        //1.  Validate
        var ve = envelope.NonNull(nameof(envelope)).Validate();
        if (ve != null) throw ve;

        //2. Save message into document storage, getting back a unique Id
        //which can be later used for query/retrieval
        envelope.Content.ArchiveId = await DoStoreMessageOnSendAsync(envelope).ConfigureAwait(false);

        //3. Route message for delivery
        //the router implementation is 100% asynchronous by design
        m_Router.SendMsg(envelope.Content);

        DoWriteOplog(envelope, null);

        return envelope.Content.ArchiveId;
      }
      catch(Exception error)
      {
        DoWriteOplog(envelope, error);
        throw;
      }
    }

    protected Task<string> DoStoreMessageOnSendAsync(MessageEnvelope envelope)
    {
      return Task.FromResult(Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Override to write communication message into an oplog
    /// </summary>
    protected virtual void DoWriteOplog(MessageEnvelope envelope, Exception error)
    {
      if (m_OpLog == null) return;
      var msgLog = CreateOplogMessage(envelope, error);
      if (msgLog == null) return;
      m_OpLog.Write(msgLog);
    }

    /// <summary>
    /// Override to create an oplog message representation of communication message
    /// </summary>
    protected virtual Azos.Log.Message CreateOplogMessage(MessageEnvelope envelope, Exception error)
    {
      Message msg = null;
      MessageProps props = null;

      try
      {
        msg = envelope.Content;
        props = envelope.GetMessageProps();
      }
      catch
      {
        return null;//data structure is not valid
      }

      var result = new Azos.Log.Message();

      result.App = App.AppId;
      result.Host = Platform.Computer.HostName;
      result.Topic = CoreConsts.WEBMSG_TOPIC;
      result.From = msg.AddressFrom;
      result.Type = error == null ? MessageType.Info : MessageType.Error;
      result.RelatedTo = msg.RelatedId ?? Guid.Empty;
      result.Exception = error;
      result.Text = msg.Subject;

      var attachmentSummary = new StringBuilder();
      msg.Attachments?
         .ForEach(one => attachmentSummary.Append($"'{one.Name}' ({IOUtils.FormatByteSizeWithPrefix(one.Content?.Length ?? 0)} / {one.UnitWeight}) \n"));

      result.Parameters = new
      {
        archiveId = msg.ArchiveId,
        id = msg.Id,
        cdt = msg.CreateDateUTC,
        pri = msg.Priority,
        imp = msg.Importance,
        from = msg.AddressFrom,
        to = msg.AddressTo,
        repl = msg.AddressReplyTo,
        cc = msg.AddressCC,
        bcc = msg.AddressBCC,
        props = props,

        bodyShort = msg.ShortBody.TakeFirstChars(100, "..."),
        body = msg.Body.TakeFirstChars(100, "..."),
        bodyRich = msg.RichBody.TakeFirstChars(100, "..."),
        bodyRichCtp = msg.RichBodyContentType,
        att = attachmentSummary.ToString()
      }.ToJson();

      return result;
    }




    public virtual Task<IEnumerable<MessageInfo>> GetMessageListAsync(MessageListFilter filter)
      => Task.FromResult(Enumerable.Empty<MessageInfo>());

    public virtual Task<(Message msg, MessageProps props)> GetMessageAsync(string msgId, bool fetchProps = false)
      => Task.FromResult<(Message, MessageProps)>((null, null));

    public virtual Task<Message.Attachment> GetMessageAttachmentAsync(string msgId, int attId)
      => Task.FromResult<Message.Attachment>(null);

    public virtual Task<MessageStatusLog> GetMessageStatusLogAsync(string msgId)
      => Task.FromResult<MessageStatusLog>(null);


    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      if (node == null) return;

      m_Router.Configure(node[CONFIG_MESSAGE_ROUTER_SECTION]);
    }

    protected override bool DoApplicationAfterInit()
    {
      if (OplogModuleName.IsNotNullOrWhiteSpace())  m_OpLog = App.ModuleRoot.Get<ILogModule>(OplogModuleName).Log;
      m_Router.Start();
      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      m_Router.WaitForCompleteStop();
      return base.DoApplicationBeforeCleanup();
    }
  }
}
