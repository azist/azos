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
using Azos.Collections;
using Azos.Conf;
using Azos.Data;
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
      m_Handlers = new Registry<CommandHandler>(caseSensitive: false);
      m_Router = new MessageDaemon(this);
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Router);
      cleanup();
      base.Destructor();
    }

    private void cleanup()
    {
      m_Handlers.ForEach(one => this.DontLeak(() =>  one.Dispose()));
      m_Handlers.Clear();
    }

    protected MessageDaemon m_Router;
    protected ILog m_OpLog;
    protected Registry<CommandHandler> m_Handlers;


    public override bool IsHardcodedModule => true;
    public override string ComponentLogTopic => CoreConsts.WEBMSG_TOPIC;


    /// <summary>
    /// When set, turns on op logging
    /// </summary>
    [Config]
    public string OplogModuleName{ get; set; }


    /// <summary>
    /// Registr of configured command handlers
    /// </summary>
    public IRegistry<CommandHandler> Handlers => m_Handlers;


    /// <summary>
    /// Returns a string which should be matched by requested content type
    /// to trigger command execution.
    /// If null is returned, then command execution is bypassed altogether
    /// </summary>
    /// <remarks>
    /// When this service gets a message with RichBodyContentType set to this command content type value,
    /// it initializes the <see cref="MessageCommand"/> instance from <see cref="Message.RichBody"/> property
    /// and passes it to handler for execution. A handler then turns a command into some real message, like
    /// a content template populated from CMS etc..
    /// </remarks>
    public virtual string CommandContentType => null;


    /// <inheritdoc/>
    public virtual ValidState CheckPreconditions(MessageEnvelope envelope, ValidState state)
    {
      return state;
    }

    /// <inheritdoc/>
    public virtual async Task<string> SendAsync(MessageEnvelope envelope)
    {
      var original = envelope.NonNull(nameof(envelope)); ;

      try
      {
        //1. Process message, e.g. expand templates. The new message is returned
        envelope = await DoProcessMessageAsync(original).ConfigureAwait(false);

        //2. Save message into document storage, getting back a unique Id
        //which can be later used for query/retrieval
        envelope.Content.ArchiveId = await DoStoreMessageOnSendAsync(original, envelope).ConfigureAwait(false);

        //3. Route message for delivery
        //the router implementation is 100% asynchronous by design
        m_Router.SendMsg(envelope.Content);

        DoWriteOplog(original, envelope, null);

        return envelope.Content.ArchiveId;
      }
      catch(Exception error)
      {
        DoWriteOplog(original, envelope, error);
        throw;
      }
    }

    /// <summary>
    /// Returns a new message which represents a message envelope which needs to be sent, such as
    /// the one after expansion of tenmplates.
    /// You can return the original envelop as-is if there is no pre-processing needed
    /// </summary>
    protected virtual async Task<MessageEnvelope> DoProcessMessageAsync(MessageEnvelope envelope)
    {
      if (CommandContentType.IsNotNullOrWhiteSpace() && (envelope.Content?.RichBodyContentType).EqualsIgnoreCase(CommandContentType))
      {
        var result = await DoHandleCommandAsync(envelope).ConfigureAwait(false);
        return result;
      }

      return envelope;
    }

    /// <summary>
    /// By default this gets called by <see cref="DoProcessMessageAsync(MessageEnvelope)"/> when
    /// the rich body content type matches the <see cref="CommandContentType"/> which triggers the command handler
    /// flow.
    /// </summary>
    protected virtual async Task<MessageEnvelope> DoHandleCommandAsync(MessageEnvelope envelope)
    {
      var cmdText = envelope.Content.RichBody.NonBlank("Command text in RichBody", putHttpDetails: true, putExternalDetails: true);

      //make instance of MessagingCommandClass an execute
      var command = JsonReader.ToDoc<MessageCommand>(envelope.Content.RichBody, false);

      //Handler, handle the command and generate the envelope
      //==============================================================================================
      //==============================================================================================

      var handler = m_Handlers[command.Name];
      if (handler == null)
        throw $"No handler for command `{command.Name.TakeLastChars(32, "...")}`".IsNotFound(putHttpDetails: true, putExternalDetails: true);

      //==============================================================================================

      try
      {
        var result = await handler.HandleCommandAsync(envelope, command).ConfigureAwait(false);
        return result;
      }
      catch(Exception error)
      {
        var txt = "Error executing message command: {0}".Args(error.ToMessageWithType());
        WriteLogFromHere(MessageType.Error, txt, error);
        throw new SkyException(txt, error);
      }
      //==============================================================================================
      //==============================================================================================
      //==============================================================================================
    }


    /// <summary>
    /// Override to store message envelopes - and original and the real one obtained from command execution by handler.
    /// Keep in moind that the envelope and original may be the same instance
    /// </summary>
    /// <returns>Id assigned by storage, or NULL if the storage/archive is not supported</returns>
    protected virtual Task<string> DoStoreMessageOnSendAsync(MessageEnvelope original, MessageEnvelope envelope)
    {
      return Task.FromResult<string>(null);
    }

    /// <summary>
    /// Override to write communication message into an oplog
    /// </summary>
    protected virtual void DoWriteOplog(MessageEnvelope original, MessageEnvelope envelope, Exception error)
    {
      if (m_OpLog == null) return;

      var msgLog = CreateOplogMessage(original, error);
      if (msgLog != null) m_OpLog.Write(msgLog);

      if (object.ReferenceEquals(envelope, original)) return;

      msgLog = CreateOplogMessage(envelope, error);
      if (msgLog != null) m_OpLog.Write(msgLog);
    }

    /// <summary>
    /// Override to create an oplog message representation of communication message
    /// </summary>
    protected virtual Azos.Log.Message CreateOplogMessage(MessageEnvelope envelope, Exception error)
    {
      if (envelope ==null) return null;

      var msg = envelope.Content;
      if (msg == null) return null;

      var result = new Azos.Log.Message();

      result.Guid = msg.Id;
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
        cdt = msg.CreateDateUTC,
        pri = msg.Priority,
        imp = msg.Importance,
        from = msg.AddressFrom,
        to = msg.AddressTo,
        repl = msg.AddressReplyTo,
        cc = msg.AddressCC,
        bcc = msg.AddressBCC,
        props = envelope.Props,

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

    public virtual Task<MessageEnvelope> GetMessageAsync(string msgId, bool fetchProps = false)
      => Task.FromResult<MessageEnvelope>(null);

    public virtual Task<Message.Attachment> GetMessageAttachmentAsync(string msgId, int attId)
      => Task.FromResult<Message.Attachment>(null);

    public virtual Task<MessageStatusLog> GetMessageStatusLogAsync(string msgId)
      => Task.FromResult<MessageStatusLog>(null);


    protected override void DoConfigure(IConfigSectionNode node)
    {
      cleanup();

      base.DoConfigure(node);
      if (node == null) return;

      m_Router.Configure(node[CONFIG_MESSAGE_ROUTER_SECTION]);


      foreach(var nh in node.ChildrenNamed(CommandHandler.CONFIG_HANDLER_SECTION))
      {
        var handler = FactoryUtils.MakeDirectedComponent<CommandHandler>(this, nh, null, new object[]{ nh });
        m_Handlers.Register(handler).IsTrue($"Uniquely named handler `{handler.Name}`");
      }
    }

    protected override bool DoApplicationAfterInit()
    {
      if (OplogModuleName.IsNotNullOrWhiteSpace())  m_OpLog = App.ModuleRoot.Get<ILogModule>(OplogModuleName).Log;
      m_Handlers.ForEach(one => App.InjectInto(one));
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
