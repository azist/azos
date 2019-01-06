/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Conf;
using Azos.Instrumentation;
using Azos.Web.Messaging;

using Azos.Sky.Contracts;

namespace Azos.Sky.WebMessaging
{
  /// <summary>
  /// Dispatches instances of SkyWebMessage into the remote IWebMessageSystem
  /// </summary>
  public class SkyWebMessageSink : MessageSink
  {
    #region CONSTS
    public const string CONFIG_HOST_ATTR = "host";

    #endregion

    public SkyWebMessageSink(MessageDaemon director) : base(director)
    {
    }

    private string m_HostName;

    //todo: Refactor to use hosts sets with load balancing
    private IWebMessageSystemClient m_Client;


    /// <summary>
    /// Specifies the name of the host where the messages are sent
    /// </summary>
    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_MESSAGING)]
    public string Host { get; set; }

    public override MsgChannels SupportedChannels { get { return MsgChannels.All; } }


	protected override bool Filter(Message msg)
    {
      return true;
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      //throws on bad host spec
      App.GetMetabase().CatalogReg.NavigateHost(Host);
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();
      DisposeAndNull(ref m_Client);
    }


    protected override bool DoSendMsg(Message msg)
    {
      var amsg = msg as SkyWebMessage;
      if (amsg == null) return false;

      try
      {
        ensureClient();
        m_Client.SendMessage(amsg);
      }
      catch (Exception error)
      {
        throw new WebMessagingException("{0}.DoSend: {1}".Args(GetType().Name, error.ToMessageWithType()), error);
      }
      return true;
    }

    private void ensureClient()
    {
      var hn = this.Host;
      if (m_Client == null && !hn.EqualsOrdIgnoreCase(m_HostName))
      {
        m_Client = App.GetServiceClientHub().MakeNew<IWebMessageSystemClient>(hn);
        m_HostName = hn;
      }
    }
  }
}
