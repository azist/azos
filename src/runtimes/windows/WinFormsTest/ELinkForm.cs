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
using System.Text;
using System.Windows.Forms;

using NFX.DataAccess.Distributed;

using NFX;
using NFX.IO;
using NFX.Security.CAPTCHA;
using NFX.Security;

namespace WinFormsTest
{
    public partial class ELinkForm : Form
    {
        public ELinkForm()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
          var lnk = new ELink(new GDID((uint)sbEra.Value, 12,(ulong)tbID.Text.AsLong(0)), null);
          tbELink.Text = lnk.Link;

          var lnk2 = new ELink( lnk.Link );

          tbResult.Text = lnk2.GDID.ToString();
        }

        private void sb_Scroll(object sender, ScrollEventArgs e)
        {
            tbID.Text = sb.Value.ToString();
            btnCalculate_Click(null ,null);
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            var lnk = new ELink((ulong)tbID.Text.AsLong(0), null);
            tbELink.Text = lnk.Link;

            var lnk2 = new ELink( lnk.Link );

            tbResult.Text = lnk2.ID.ToString();
        }

        private void btnBigID_Click(object sender, EventArgs e)
        {
            ulong id = 182500000000;// ulong.MaxValue;

            var lnk = new ELink(id, null);// new byte[]{ 123, 18});
            tbELink.Text = lnk.Link;
        }

        private void btnDecode_Click(object sender, EventArgs e)
        {
            var lnk2 = new ELink( tbELink.Text );

            tbResult.Text = lnk2.GDID.ToString();
        }

        private void tbPassword_TextChanged(object sender, EventArgs e)
        {
          var pm = App.SecurityManager.PasswordManager;
          var bytes = Encoding.UTF8.GetBytes(tbPassword.Text);
          using (var password = new SecureBuffer(bytes.Length))
          {
            foreach (var b in bytes)
              password.Push(b);
            Array.Clear(bytes, 0, bytes.Length);
            password.Seal();

            var hash = pm.ComputeHash(PasswordFamily.Text, password);

            Text = "Score: {0}   Easy%: {1}  Normal%: {2} Hard%: {3}".Args(
              pm.CalculateStrenghtScore(PasswordFamily.Text, password),
              pm.CalculateStrenghtPercent(PasswordFamily.Text, password, DefaultPasswordManager.TOP_SCORE_MINIMUM),
              pm.CalculateStrenghtPercent(PasswordFamily.Text, password, DefaultPasswordManager.TOP_SCORE_NORMAL),
              pm.CalculateStrenghtPercent(PasswordFamily.Text, password, DefaultPasswordManager.TOP_SCORE_MAXIMUM));

            tbJSON.Text = hash.ToString();
          }
        }


        private void sbv_Scroll(object sender, ScrollEventArgs e)
        {
          var img = cb2.Checked ? pb3.Image : pb1.Image;
          pb2.Image = img.NormalizeCenteredImage(sbh.Value, sbv.Value);
          Text = "{0} x {1}".Args(sbh.Value, sbv.Value);
        }

        private void ELinkForm_Load(object sender, EventArgs e)
        {
          sbv_Scroll(null, null);
        }

        private void sbEra_Scroll(object sender, ScrollEventArgs e)
        {
         button1_Click(null, null);
        }

        private void button2_Click(object sender, EventArgs e)
        {
           var lnk = new ELink((ulong)tbID.Text.AsLong(0), null);
           lnk.Encode(1);
           tbELink.Text = lnk.Link;

           var lnk2 = new ELink( lnk.Link );

           tbResult.Text = lnk2.ID.ToString();
        }


        public static string a1 = ExternalRandomGenerator.Instance.NextRandomWebSafeString().Substring(0, 2);
        string a2;

        private void btnPuzzle_Click(object sender, EventArgs e)
        {
           var pk = new PuzzleKeypad(NFX.Parsing.NaturalTextGenerator.Generate(16));
           var img = pk.DefaultRender(Color.White, false);
           pic.Image = img;

          // a1 = "a";
           a2 = tbPassword.Text;

           Text = "'{0}' ref eq '{1}' is {2}, == is {3} ".Args(a1, a2, object.ReferenceEquals(a1,a2), a1==a2);
        }


    }
}
