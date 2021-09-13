/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Client;
using Azos.Serialization.JSON;

namespace Azos.Web.Messaging.Sinks
{
  /// <summary>
  /// Implements EMail sending logic based on Twilio SendGrid SMTP APIs
  /// </summary>
  /// <remarks>
  /// See:
  /// https://www.twilio.com/sendgrid/email-api
  /// https://sendgrid.com/docs/for-developers/sending-email/api-getting-started/
  /// </remarks>
  public sealed class TwilioEmailSink : TwilioSinkBase
  {
    public TwilioEmailSink(MessageDaemon director) : base(director)
    {
    }

    public override MsgChannels SupportedChannels => MsgChannels.EMail;

    protected override bool DoSendMsg(Message msg)
    {
      var tw = toTwilioEmail(msg);

      var task = m_Twilio.Call(TwilioServiceAddress,
                              nameof(TwilioEmailSink),
                              new ShardKey(msg.Id), //sharding
                              (http, ct) => http.Client.PostAndGetJsonMapAsync("send", tw));

      var response = task.GetAwaiter().GetResult();//complete synchronously by design

      //how do we write the response which has Twilio-specific assigned id etc...

      return true;
    }

    private JsonDataMap toTwilioEmail(Message msg)
    {
      var result = new JsonDataMap();
      result["a"] = 1;
      return result;
    }

  }
}
