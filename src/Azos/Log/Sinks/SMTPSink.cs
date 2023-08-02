/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Net;
using System.Net.Mail;

using Azos.Conf;

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Implements log sink that sends emails using SMTP protocol
  /// </summary>
  public class SmtpSink : Sink
  {
    public const int DEFAULT_SMTP_PORT = 587;


    public SmtpSink(ISinkOwner owner) : base(owner){ }
    public SmtpSink(ISinkOwner owner, string name, int order) : base(owner, name, order){ }

    private SmtpClient m_Smtp;

    [Config]
    public string SmtpHost { get; set; }

    [Config(Default = DEFAULT_SMTP_PORT)]
    public int SmtpPort { get; set; }

    [Config]
    public bool SmtpSSL { get; set; }

    [Config]
    public string DropFolder { get; set; }

    [Config]
    public string FromAddress { get; set; }
    [Config]
    public string FromName { get; set; }

    [Config]
    public string ToAddress { get; set; }


    [Config]
    public string CredentialsID { get; set; }
    [Config]
    public string CredentialsPassword { get; set; }


    [Config]
    public string Subject { get; set; }

    [Config]
    public string Body { get; set; }


    protected override void DoConfigureLockedDaemon(IConfigSectionNode fromNode)
    {
      using(var scope = new Security.SecurityFlowScope(Security.TheSafe.SAFE_ACCESS_FLAG))
      {
        base.DoConfigureLockedDaemon(fromNode);
      }
    }

    protected override void DoStart()
    {
      base.DoStart();

      if (DropFolder.IsNotNullOrWhiteSpace() && System.IO.Directory.Exists(DropFolder))
      {
        m_Smtp =
                new SmtpClient
                {
                  DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                  PickupDirectoryLocation = DropFolder
                };
      }
      else
      {
        m_Smtp =
                new SmtpClient
                {
                  Host = this.SmtpHost,
                  Port = this.SmtpPort,
                  EnableSsl = this.SmtpSSL,
                  DeliveryMethod = SmtpDeliveryMethod.Network,
                  UseDefaultCredentials = false,
                  Credentials = new NetworkCredential(this.CredentialsID, this.CredentialsPassword)
                };
      }
    }

    protected override void DoWaitForCompleteStop()
    {
        DisposeAndNull(ref m_Smtp);
        base.DoWaitForCompleteStop();
    }


    protected internal override void DoSend(Message entry)
    {
      var smtp = m_Smtp;
      if (smtp==null) return;

      if (string.IsNullOrEmpty(ToAddress)) return;

      var from = new MailAddress(this.FromAddress, this.FromName);
      var to = new MailAddress(ToAddress);


      using (var email = new MailMessage(from, to))
      {
        email.Subject = this.Subject ?? entry.Topic;
        email.Body = (this.Body??string.Empty) + entry.ToString();//for now
        email.CC.Add(ToAddress);

        smtp.Send(email);
      }
    }
  }
}
