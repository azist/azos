/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Client;
using Azos.Serialization.JSON;
using Azos.Web;

namespace Azos.Sky.Messaging.Sinks
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
      if (tw == null)
      {
        WriteLogFromHere(Azos.Log.MessageType.Warning,
                         "Email not qualified for SendGrid API",
                         related: msg.Id,
                         pars: new { sbj = msg.Subject.TakeFirstChars(64, ".."), f = msg.AddressFrom, t = msg.AddressTo }.ToJson(JsonWritingOptions.CompactASCII)
                        );
        return false;
      }

      var task = m_Twilio.Call(TwilioServiceAddress,
                               nameof(TwilioEmailSink),
                               new ShardKey(msg.Id), //sharding
                               (http, ct) => http.Client.PostAndGetJsonMapAsync("send", tw));

      task.Await();//complete synchronously by design

      return true;
    }

    private JsonDataMap toTwilioEmail(Message message)
    {
      if (!checkMessagePreflight(message)) return null;
      var twilioMessage = constructEmailContent(message);
      return twilioMessage;
    }

    #region 20241122 GMK Twilio Integration
    private bool checkMessagePreflight(Message msg)
    {
      if (msg == null) return false;

      bool hasMinimumInfoForSendGrid = msg.AddressToBuilder.All.Any() && msg.AddressFromBuilder.All.Any();
      if (!hasMinimumInfoForSendGrid) return false;

      var fromAddress = msg.AddressFromBuilder.GetMatchesForChannels(SupportedChannelNames);
      var toAddresses = msg.AddressToBuilder.GetMatchesForChannels(SupportedChannelNames);
      var ccAddresses = msg.AddressCCBuilder.GetMatchesForChannels(SupportedChannelNames);
      var bccAddresses = msg.AddressBCCBuilder.GetMatchesForChannels(SupportedChannelNames);

      if (!toAddresses.Any() || !fromAddress.Any()) return false;
      // SendGrid claims subject isn't required but will reject if you leave it out!
      if (msg.Subject.IsNullOrEmpty()) return false;
      // Any overlap in to, cc, bcc address lists will be rejected by SendGrid
      if (toAddresses.Intersect(ccAddresses).Intersect(bccAddresses).Count() != 0) return false;

      return true;
    }

    private JsonDataMap constructEmailContent(Message msg)
    {
      var emailContent = new JsonDataMap();

      var personalizations = constructPersonalizations(msg);
      var attachments = constructAttachments(msg);
      bool contentPresent = msg.Body != null || msg.RichBody != null;

      if (personalizations.Count > 0)
      {
        emailContent.Add("personalizations", new[] { personalizations });
      }
      if (msg.AddressFromBuilder.All.Any())
      {
        emailContent.Add("from", new { email = msg.AddressFromBuilder.GetFirstOrDefaultMatchForChannels(SupportedChannelNames).Address });
      }
      if (msg.Subject.IsNotNullOrWhiteSpace())
      {
        emailContent.Add("subject", msg.Subject);
      }
      if (contentPresent)
      {
        emailContent.Add("content", new[] { constructMessageContent(msg) });
      }
      if (msg.AddressReplyToBuilder.All.Any())
      {
        emailContent.Add("reply_to_list", constructEmailList(msg.AddressReplyToBuilder));
      }
      if (attachments.Length > 0)
      {
        emailContent.Add("attachments", attachments);
      }

      return emailContent;
    }

    private JsonDataMap constructPersonalizations(Message msg)
    {
      var personalizations = new JsonDataMap();

      if (msg.AddressToBuilder.All.Any())
      {
        personalizations.Add("to", constructEmailList(msg.AddressToBuilder));
      }
      if (msg.AddressCCBuilder.All.Any())
      {
        personalizations.Add("cc", constructEmailList(msg.AddressCCBuilder));
      }
      if (msg.AddressBCCBuilder.All.Any())
      {
        personalizations.Add("bcc", constructEmailList(msg.AddressBCCBuilder));
      }

      return personalizations;
    }

    private JsonDataMap[] constructAttachments(Message msg)
    {
      if (msg.Attachments == null)
      {
        return new JsonDataMap[0];
      }

      var attachmentList = new List<JsonDataMap>();
      foreach (var attachment in msg.Attachments)
      {
        var attachmentMap = new JsonDataMap();

        if (attachment.Content != null && attachment.Content.Length >= 1)
        {
          attachmentMap.Add("content", attachment.Content.ToWebSafeBase64());
        }
        if (attachment.ContentType.IsNotNullOrWhiteSpace())
        {
          attachmentMap.Add("type", attachment.ContentType);
        }
        if (attachment.Name.IsNotNullOrWhiteSpace())
        {
          attachmentMap.Add("filename", attachment.Name);
        }

        ////// Here is an example how to read a custom attribute from a attachment-specific header
        //////msg.Headers.....
        //////attachment.Headers.Node.ValOf("cid");

        // implement inline attachments here
        attachmentList.Add(attachmentMap);
      }

      return attachmentList.ToArray();
    }

    private object constructMessageContent(Message msg)
    {
      // use richbody and rich content type unless unavailable, in which case use plaintext body
      if (msg.RichBody == null || msg.RichBodyContentType.IsNullOrWhiteSpace())
      {
        return new { type = "text/plain", value = msg.Body };
      }
      else
      {
        return new { type = msg.RichBodyContentType, value = msg.RichBody };
      }
    }

    private JsonDataArray constructEmailList(MessageAddressBuilder addressBuilder)
    {
      var matches = addressBuilder.GetMatchesForChannels(SupportedChannelNames).Select(one => one.Address).ToArray();
      var emails = new JsonDataArray();
      foreach (var match in matches)
      {
        emails.Add(new { email = match });
      }
      return emails;
    }

    #endregion
  }
}
