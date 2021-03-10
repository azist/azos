/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Web.Messaging.Services.Server
{
  /// <summary>
  /// Implements IMessagingLogic  and IMessageArchiveLogic
  /// </summary>
  public sealed class MessagingLogic : ModuleBase, IMessagingLogic, IMessageArchiveLogic
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

    //todo Implement future document storage
    //    [Inject] Data.IDocStorage m_DocStorage;

    private MessageDaemon m_Router;

    public override bool IsHardcodedModule => true;

    public override string ComponentLogTopic => CoreConsts.WEBMSG_TOPIC;

    public Task<string> SendAsync(Message message, MessageProps props)
    {
      //1.  Validate
      var ve = message.NonNull(nameof(message)).Validate();
      if (ve != null) throw ve;

      //2. Save message into document storage, getting back a unique Id
      //which can be later used for query/retrieval
      message.ArchiveId = Guid.NewGuid().ToString();  //await m_DocStorage.PutMessageAsync(message.NonNull(nameof(message)), props);

      //3. Route message for delivery
      //the router implementation is 100% asynchronous by design
      m_Router.SendMsg(message);

      return Task.FromResult<string>(null); //message.ArchiveId;
    }

    public Task<IEnumerable<MessageInfo>> GetMessageListAsync(MessageListFilter filter)
      => Task.FromResult(Enumerable.Empty<MessageInfo>());

    public Task<(Message msg, MessageProps props)> GetMessageAsync(string msgId, bool fetchProps = false)
      => Task.FromResult<(Message, MessageProps)>((null, null));

    public Task<Message.Attachment> GetMessageAttachmentAsync(string msgId, int attId)
      => Task.FromResult<Message.Attachment>(null);

    public Task<MessageStatusLog> GetMessageStatusLogAsync(string msgId)
      => Task.FromResult<MessageStatusLog>(null);


    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      if (node == null) return;

      m_Router.Configure(node[CONFIG_MESSAGE_ROUTER_SECTION]);
    }

    protected override bool DoApplicationAfterInit()
    {
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
