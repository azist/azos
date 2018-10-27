/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NFX;
using NFX.Web.Messaging;
using WinFormsTest.Properties;

namespace WinFormsTest
{
    public partial class MailForm : Form
    {
        public MailForm()
        {
            InitializeComponent();
        }

        private void btSend_Click(object sender, EventArgs e)
        {
          // note that ToAddress has complex structure - laconic config, look MessageBuilder.Addressee
          var message = new NFX.Web.Messaging.Message(null)
          {
            AddressFrom = tbFROMAddress.Text,
            AddressTo = tbTOAddress.Text,
            Subject = tbSubject.Text,
            Body = tbBody.Text,
            RichBody = tbHTML.Text
          };

          if (includeAttachments.Checked)
          {
            ImageConverter converter = new ImageConverter();
            var imageBytes = (byte[])converter.ConvertTo(Resources._20140601_204233, typeof(byte[]));

            message.Attachments = new NFX.Web.Messaging.Message.Attachment[]
                          {new NFX.Web.Messaging.Message.Attachment("photo1", imageBytes, NFX.Web.ContentType.JPEG), new NFX.Web.Messaging.Message.Attachment("photo2", imageBytes, NFX.Web.ContentType.JPEG)};
          }

          MessageService.Instance.SendMsg(message);
        }
    }
}
