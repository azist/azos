/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Client;
using Azos.Conf;

namespace Azos.Web.Messaging.Sinks
{
  /// <summary>
  /// Provides base for making Twilio HTTP service calls
  /// </summary>
  /// <remarks>
  /// See:
  /// https://www.twilio.com/sendgrid/email-api
  /// https://sendgrid.com/docs/for-developers/sending-email/api-getting-started/
  /// </remarks>
  public abstract class TwilioSinkBase : MessageSink
  {
    public const string CONFIG_SERVICE_SECTION = "twilio-service";

    public TwilioSinkBase(MessageDaemon director) : base(director)
    {
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Twilio);
      base.Destructor();
    }

    protected HttpService m_Twilio;
    private string m_TwilioServiceAddress;


    /// <summary>
    /// Logical service address of Twilio services
    /// </summary>
    [Config]
    public string TwilioServiceAddress
    {
      get => m_TwilioServiceAddress;
      set
      {
        CheckDaemonInactive();
        m_TwilioServiceAddress = value;
      }
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      DisposeAndNull(ref m_Twilio);
      if (node == null) return;

      var nServer = node[CONFIG_SERVICE_SECTION];
      m_Twilio = FactoryUtils.MakeDirectedComponent<HttpService>(this,
                                                                 nServer,
                                                                 typeof(HttpService),
                                                                 new object[] { nServer });
    }

    protected override void DoStart()
    {
      m_Twilio.NonNull("`{0}` is not configured".Args(CONFIG_SERVICE_SECTION));
      TwilioServiceAddress.NonBlank("`{0}` is not set".Args(nameof(TwilioServiceAddress)));
      base.DoStart();
    }

  }
}
