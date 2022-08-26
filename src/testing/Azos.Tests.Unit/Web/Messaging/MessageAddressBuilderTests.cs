/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Linq;

using Azos.Scripting;
using Azos.Sky.Messaging;

namespace Azos.Tests.Unit.Web.Messaging
{
  [Runnable(TRUN.BASE, 6)]
  public class MessageAddressBuilderTests
  {
    [Run]
    public void BuildMessageAddress()
    {
      var config =
      @"[
          {
            n: 'Peter',
            c: 'Twilio',
            a: '+15005550005'
          }
          ,
          {
            n: 'Nick',
            c: 'Mailgun',
            a: 'nick@example.com'
          }
        ]";

      var builder = new MessageAddressBuilder(config);
      var addressees = builder.All.ToArray();

      Aver.AreEqual(addressees.Count(), 2);
      Aver.AreEqual(builder.ToString(),
                    @"[{""n"":""Peter"",""c"":""Twilio"",""a"":""+15005550005""},{""n"":""Nick"",""c"":""Mailgun"",""a"":""nick@example.com""}]");

      Aver.AreEqual(addressees[0].Name, "Peter");
      Aver.AreEqual(addressees[0].Channel, "Twilio");
      Aver.AreEqual(addressees[0].Address, "+15005550005");

      Aver.AreEqual(addressees[1].Name, "Nick");
      Aver.AreEqual(addressees[1].Channel, "Mailgun");
      Aver.AreEqual(addressees[1].Address, "nick@example.com");

      var ann = new MessageAddressBuilder.Addressee
      (
        "Ann",
        "SMTP",
        "ann@example.com"
      );
      builder.Add(ann);

      addressees = builder.All.ToArray();
      Aver.AreEqual(addressees.Count(), 3);
      var str = builder.ToString();
      Aver.AreEqual(builder.ToString(),
                    @"[{""n"":""Peter"",""c"":""Twilio"",""a"":""+15005550005""},{""n"":""Nick"",""c"":""Mailgun"",""a"":""nick@example.com""},{""n"":""Ann"",""c"":""SMTP"",""a"":""ann@example.com""}]");

      Aver.AreEqual(addressees[2].Name, "Ann");
      Aver.AreEqual(addressees[2].Channel, "SMTP");
      Aver.AreEqual(addressees[2].Address, "ann@example.com");

      builder = new MessageAddressBuilder(null);
      builder.Add(ann);
      Aver.AreEqual(builder.ToString(), @"[{""n"":""Ann"",""c"":""SMTP"",""a"":""ann@example.com""}]");
      Aver.AreEqual(builder.All.Count(), 1);

      builder = new MessageAddressBuilder("[]");
      Aver.AreEqual(builder.All.Count(), 0);
      Aver.AreEqual(builder.ToString(), "[]");
    }

    [Run]
    public void MatchNames()
    {
      var config =
      @"[
          {
            n: 'Peter',
            c: 'Twilio',
            a: '+15005550005'
          }
          ,
          {
            n: 'Nick',
            c: 'Mailgun',
            a: 'nick@example.com'
          }
          ,
          {
            n: 'Ann',
            c: 'SMTP',
            a: 'ann@example.com'
          }
        ]";
      var builder = new MessageAddressBuilder(config);

      var names = new string[] {"smtp"};
      Aver.IsTrue(builder.MatchNamedChannel(names));
      var matches = builder.GetMatchesForChannels(names).ToArray();
      Aver.AreEqual(matches.Length, 1);
      Aver.AreEqual(matches[0].Name, "Ann");

      names = new string[] {"Twilio", "MailGun"};
      Aver.IsTrue(builder.MatchNamedChannel(names));

      matches = builder.GetMatchesForChannels(names).ToArray();
      Aver.AreEqual(matches.Length, 2);
      Aver.AreEqual(matches[0].Name, "Peter");
      Aver.AreEqual(matches[1].Name, "Nick");
      var first = builder.GetFirstOrDefaultMatchForChannels(names);
      Aver.IsNotNull(first);
      Aver.IsTrue(first.Assigned);
      Aver.AreEqual(first.Name, "Peter");

      names = new string[] {"Skype"};
      Aver.IsFalse(builder.MatchNamedChannel(names));
      matches = builder.GetMatchesForChannels(names).ToArray();
      Aver.AreEqual(matches.Length, 0);
      first = builder.GetFirstOrDefaultMatchForChannels(names);
      Aver.IsNotNull(first);
      Aver.IsFalse(first.Assigned);
    }
  }
}
