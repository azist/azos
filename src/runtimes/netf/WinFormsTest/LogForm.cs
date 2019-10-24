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
using System.Windows.Forms;

using Azos;
using Azos.Log;
using Azos.WinForms;

namespace WinFormsTest
{
    public partial class LogForm : Form
    {
        const int CNT = 1;

        public LogForm()
        {
            InitializeComponent();
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            write(MessageType.Info);
        }



        private void write(MessageType t)
        {
          for(var i=0; i<CNT; i++)
             FormsAmbient.App.Log.Write( new Azos.Log.Message{Type = t, Topic = "LogForm", From = tbFrom.Text, Text = tbText.Text + i.ToString(), Exception = new AzosException("AZ5 was pressed")});
        }

        private void btnTrace_Click(object sender, EventArgs e)
        {
            write(MessageType.Trace);
        }

        private void btnWarning_Click(object sender, EventArgs e)
        {
              write(MessageType.Warning);
        }

        private void btnError_Click(object sender, EventArgs e)
        {
              write(MessageType.Error);
        }

        private void btnCatastrophy_Click(object sender, EventArgs e)
        {
              write(MessageType.CatastrophicError);
        }

        private void btnDebug_Click(object sender, EventArgs e)
        {
              write(MessageType.Debug);
        }

    private void btnWebCall_Click(object sender, EventArgs e)
    {
      try
      {

        throw new Azos.Web.WebCallException("Custom exception text", "/zhaba/push", "GET", 403, "Access denied", "Abazh is prohibited");
      }
      catch(Exception error)
      {
      FormsAmbient.App.Log.Write(new Azos.Log.Message { Type = MessageType.Error, Topic = "WebCall", From = tbFrom.Text, Text = tbText.Text,
       Exception = error });
      }
    }
  }
}
