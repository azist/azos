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
using Azos.Instrumentation;

namespace Azos.Sky.WebMessaging.Server
{
  /// <summary>
  /// Provides server implementation of Contracts.IWebMessageSystem
  /// </summary>
  public sealed class WebMessageSystemService : DaemonWithInstrumentation<IApplicationComponent>, Contracts.IWebMessageSystem
  {
    #region CONSTS
      public const string CONFIG_CHANNEL_SECTION = "channel";
      public const string CONFIG_GATEWAY_SECTION = "gateway";
    #endregion

    #region STATIC/.ctor
    public WebMessageSystemService(IApplicationComponent director) : base(director)
    {
      m_Channels = new Registry<Channel>();
      m_Gateway = new MessageDaemon(this);

      if (!App.Singletons.GetOrCreate(() => this).created)
        throw new WebMessagingException("{0} is already allocated".Args(typeof(WebMessageSystemService).FullName));
    }

    protected override void Destructor()
    {
      base.Destructor();
      deleteChannels();
      DisposeAndNull(ref m_Gateway);
      App.Singletons.Remove<WebMessageSystemService>();
    }

    #endregion

    #region Fields

       private Registry<Channel> m_Channels;
       private MessageDaemon m_Gateway;

    #endregion;

    public override string ComponentLogTopic => SysConsts.LOG_TOPIC_WMSG;

    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_MESSAGING, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled { get; set;}

    #region IWebMessageSystem
    public MsgSendInfo[] SendMessage(SkyWebMessage msg)
    {
      if (msg==null) throw new WebMessagingException(StringConsts.ARGUMENT_ERROR+"{0}.SendMessage(null)".Args(GetType().Name));

      var deliveryList = new List<MsgSendInfo>();

      var idx = -1;
      var matchedAll = true;
      foreach (var adr in msg.AddressToBuilder.All)
      {
        idx++;
        var channel = m_Channels[adr.Channel];
        if (channel==null)
        {
          matchedAll = false;
          deliveryList.Add( new MsgSendInfo(MsgChannelWriteResult.Gateway, null, idx) );
          continue;
        }

        try
        {
          var result = channel.Write(deliveryList, idx, adr.Address, msg);
          if (result<0)
            deliveryList.Add(new MsgSendInfo(result, null, idx));

          // TODO: instrumentation by result, etc.
        }
        catch (Exception error)
        {
          log(Azos.Log.MessageType.Critical, "sndmsg.chn.wrt", "Channel '{0}' leaked:".Args(channel.Name, error.ToMessageWithType()), error);
          deliveryList.Add(new MsgSendInfo(MsgChannelWriteResult.ChannelError, null, idx));
        }
      }

      //route to gateway
      if (!matchedAll)
        m_Gateway.SendMsg(msg);

      return deliveryList.ToArray();
    }

    public MailboxInfo GetMailboxInfo(MailboxID xid)
    {
      var channel = m_Channels[xid.Channel];
      if (channel==null) return null;

      return channel.GetMailboxInfo(xid);
    }

    public int GetMailboxMessageCount(MailboxID xid, string query)
    {
      var channel = m_Channels[xid.Channel];
      if (channel==null) return 0;

      return channel.GetMessageCount(xid, query);
    }

    public MessageHeaders GetMailboxMessageHeaders(MailboxID xid, string query)
    {
      var channel = m_Channels[xid.Channel];
      if (channel==null) return null;

      return channel.GetMessageHeaders(xid, query);
    }

    public SkyWebMessage FetchMailboxMessage(MailboxMsgID mid)
    {
      var channel = m_Channels[mid.MailboxID.Channel];
      if (channel==null) return null;

      return channel.FetchMailboxMessage(mid);
    }

    public Message.Attachment FetchMailboxMessageAttachment(MailboxMsgID mid, int attachmentIndex)
    {
      var channel = m_Channels[mid.MailboxID.Channel];
      if (channel==null) return null;

      return channel.FetchMailboxMessageAttachment(mid, attachmentIndex);
    }

    public void UpdateMailboxMessagePublication(MailboxMsgID mid, MsgPubStatus status, string oper, string description)
    {
      var channel = m_Channels[mid.MailboxID.Channel];
      if (channel==null) return;

      channel.UpdateMailboxMessagePublication(mid, status, oper, description);
    }

    public void UpdateMailboxMessageStatus(MailboxMsgID mid, MsgStatus status, string folders, string adornments)
    {
      var channel = m_Channels[mid.MailboxID.Channel];
      if (channel==null) return;

      channel.UpdateMailboxMessageStatus(mid, status, folders, adornments);
    }

    public void UpdateMailboxMessagesStatus(IEnumerable<MailboxMsgID> mids, MsgStatus status, string folders, string adornments)
    {
      var channelIdsPair = mids.ToLookup(m => m.MailboxID.Channel, m => m)
                               .ToDictionary(p => p.Key, p => p.AsEnumerable());

      foreach(var IdChannelPair in channelIdsPair)
      {
        var channel = m_Channels[IdChannelPair.Key];
        if (channel==null) continue;
        channel.UpdateMailboxMessagesStatus(IdChannelPair.Value, status, folders, adornments);
      }
    }

    #endregion

    #region Protected

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      deleteChannels();
      if (node==null || !node.Exists) return;

      m_Gateway.Configure(node[CONFIG_GATEWAY_SECTION]);//Configure gateway

      foreach(var cnode in node.Children.Where(cn => cn.IsSameName(CONFIG_CHANNEL_SECTION)))
      {
        var channel = FactoryUtils.MakeAndConfigure<Channel>(cnode, args: new object[] {this});
        if (!m_Channels.Register(channel)) throw new WebMessagingException(StringConsts.WM_SERVICE_NO_CHANNELS_ERROR.Args(GetType().Name));

      }
    }

    protected override void DoStart()
    {
      if (m_Channels.Count==0)
       throw new WebMessagingException(StringConsts.WM_SERVICE_DUPLICATE_CHANNEL_ERROR.Args(GetType().Name));

      m_Gateway.Start();

      m_Channels.ForEach( c => c.Start() );

      base.DoStart();
    }

    protected override void DoSignalStop()
    {
      base.DoSignalStop();
      m_Gateway.SignalStop();
      m_Channels.ForEach( c => c.SignalStop() );
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();

      m_Channels.ForEach( c => c.WaitForCompleteStop() );
      m_Gateway.WaitForCompleteStop();
    }

    #endregion

    #region .pvt

    private void deleteChannels()
    {
      m_Channels.ForEach( c => c.Dispose() );
      m_Channels.Clear();
    }

    private void log(Azos.Log.MessageType tp, string from, string text, Exception error = null, Guid? related = null)
    {
      App.Log.Write(new Azos.Log.Message
      {
        Type = tp,
        Topic = SysConsts.LOG_TOPIC_WMSG,
        From = "{0}.{1}".Args(GetType().Name, from),
        Text = text,
        Exception = error,
        RelatedTo = related ?? Guid.Empty
      });
    }

    #endregion
  }
}
