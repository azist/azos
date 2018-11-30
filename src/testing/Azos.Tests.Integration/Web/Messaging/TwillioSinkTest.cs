/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Linq;
using Azos.Scripting;

using Azos.Apps;
using Azos.Data;
using Azos.Web.Messaging;

namespace Azos.Tests.Integration.Web.Messaging
{
  [Runnable]
  public class TwilioSinkTest : IRunnableHook
  {
    public const string CONFIG =
    @"nfx
      {
        messaging
        {
          sink
          {
            type='Azos.Web.Messaging.TwilioSink, Azos.Web'
            name='Twilio'
            account-sid=$(~TWILIO_ACCOUNT_SID)
            auth-token=$(~TWILIO_AUTH_TOKEN)
            from='+15005550006'
          }
        }
      }";

    private AzosApplication m_App;
    private TwilioSink m_Sink;

    void IRunnableHook.Prologue(Runner runner, FID id)
    {
      var config = CONFIG.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
      m_App = new AzosApplication(null, config);

      m_Sink = (TwilioSink)((MessageDaemon)MessageDaemon.Instance).Sink;
      Aver.IsNotNull(m_Sink);

      Aver.IsTrue(m_Sink.Name.EqualsOrdIgnoreCase("Twilio"));
      Aver.IsTrue(m_Sink.SupportedChannels == MsgChannels.SMS);
      Aver.IsTrue(m_Sink.SupportedChannelNames.Contains("Twilio"));

      Aver.IsTrue(m_Sink.Running);
    }

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
    {
      DisposableObject.DisposeAndNull(ref m_App);
      return false;
    }

    [Run]
    public void SendSMS()
    {
      var sms = new Message(null)
      {
        AddressTo = "as { a { nm=Nick cn=Twilio ca=+15005550005 } }",
        Body = "Test SMS. Тестирование SMS"
      };
      var sent = m_Sink.SendMsg(sms);
      Aver.IsTrue(sent);
    }

    [Run]
    public void BadRequest_Throw()
    {
      try
      {
        var sms = new Message(null)
        {
          AddressTo = "as { a { nm=Nick cn=Twilio ca=+15005550009 } }",
          Body = "Test SMS"
        };
        var sent = m_Sink.SendMsg(sms);
      }
      catch (Exception error)
      {
        Aver.IsTrue(error.Message.EqualsOrdIgnoreCase("Sending message on sink 'Twilio' has not succeeded"));
      }
    }

    [Run]
    public void BadRequest_Ignore()
    {
      var oldMode = m_Sink.ErrorHandlingMode;
      m_Sink.ErrorHandlingMode = SendMessageErrorHandling.Ignore;

      var sms = new Message(null)
      {
        AddressTo = "as { a { nm=Nick cn=Twilio ca=+15005550009 } }",
        Body = "Test SMS"
      };
      var sent = m_Sink.SendMsg(sms);
      Aver.IsFalse(sent);
      m_Sink.ErrorHandlingMode = oldMode;
    }
  }
}
