/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;

using Azos.Conf;

namespace Azos.Log.Sinks
{

    /// <summary>
    /// Implements log sink that sends emails
    /// </summary>
    public class SMTPSink : Sink
    {
        #region CONSTS


             public const int DEFAULT_SMTP_PORT = 587;



        #endregion



        #region .ctor

            /// <summary>
            /// Creates a new instance of destination that sends EMails
            /// </summary>
            public SMTPSink() : base(null)
            {
              SmtpPort = DEFAULT_SMTP_PORT;
            }

            /// <summary>
            /// Creates a new instance of destination that sends EMails
            /// </summary>
            public SMTPSink(string name, string host, int port, bool ssl) : base(name)
            {
              SmtpHost = host;
              SmtpPort = port;
              SmtpSSL = ssl;
            }

            protected override void Destructor()
            {
              base.Destructor();
            }
      #endregion


      #region Private Fields

        private SmtpClient m_Smtp;

      #endregion


      #region Properties

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

      #endregion

      #region Public


       public override void Open()
       {
          base.Open();

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

       public override void Close()
       {
           if (m_Smtp!=null)
           {
             m_Smtp.Dispose();
             m_Smtp = null;
           }
           base.Close();
       }

      #endregion


      #region Protected

        protected internal override void DoSend(Message entry)
        {
           if (string.IsNullOrEmpty(ToAddress)) return;

           var from = new MailAddress(this.FromAddress, this.FromName);
           var to = new MailAddress(ToAddress);


           using (var email = new MailMessage(from, to))
           {

                email.Subject = this.Subject ?? entry.Topic;
                email.Body = (this.Body??string.Empty) + entry.ToString();//for now
                email.CC.Add(ToAddress);

                m_Smtp.Send(email);
           }
        }

      #endregion

    }
}
