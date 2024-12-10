/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using Azos.Client;
using Azos.Glue.Protocol;
using Azos.Web;
using Azos.Serialization.JSON;

namespace Azos.Sky.Messaging.Sinks
{
  class TwilioSmsSink : TwilioSinkBase
  {
    const string FROM_PHONE_NUMBER = "+18555450552";

    public TwilioSmsSink(MessageDaemon director) : base(director)
    {
    }

    public override MsgChannels SupportedChannels => MsgChannels.SMS;

    protected override bool DoSendMsg(Message message)
    {
      if (!checkMessage(message))
      {
        WriteLogFromHere(Azos.Log.MessageType.Warning,
                 "SMS message data not qualified for Twilio API",
                 related: message.Id,
                 pars: new { msg = message.ShortBody.TakeFirstChars(64, ".."), t = message.AddressTo }.ToJson(JsonWritingOptions.CompactASCII)
                );
        return false;
      }

      // Twilio requires that each message is its own call
      foreach (var recipient in message.AddressToBuilder.GetMatchesForChannels(SupportedChannelNames))
      {

        // Twilio didn't seem to like it when you give them raw JSON as a body.
        var smsContent = new FormUrlEncodedContent(new[]
        {
          new KeyValuePair<string, string>("To", recipient.Address),
          new KeyValuePair<string, string>("From", FROM_PHONE_NUMBER),
          new KeyValuePair<string, string>("Body", message.ShortBody),
        });

        var task = m_Twilio.Call(TwilioServiceAddress,
                         nameof(TwilioSmsSink),
                         new ShardKey(message.Id), //sharding
                         (http, ct) => http.Client.PostAndGetJsonMapAsync("send", smsContent));

        task.Await(); //complete synchronously by design
      }

      return true;
    }

    private bool checkMessage(Message message)
    {
      var toAddresses = message.AddressToBuilder.GetMatchesForChannels(SupportedChannelNames);
      return message.ShortBody.IsNotNullOrWhiteSpace() && toAddresses.Any();
    }
  }
}
