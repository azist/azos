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

using Azos;

namespace Agnivo
{
  public partial class MainForm : AgnivoFormBase
  {
    private const string STATISTICS = "{0} bytes";

    public MainForm()
    {
      InitializeComponent();
      InitializeFromConfig();
      statistics.Text = STATISTICS.Args(0);
    }

    private void DecodeBSONButton_Click(object sender, EventArgs e)
    {
      var bsonText = SourceBSONText.Text;
      if (bsonText.IsNullOrWhiteSpace()) return;

      try
      {
        var json = AgnivoHelper.Base64BSONToJSON(bsonText);
        var form = new DecodedBSONForm(json);
        form.Show();
      }
      catch (Exception error)
      {
        MessageBox.Show(error.ToMessageWithType());
      }
    }

    private void SourceGDID_TextChanged(object sender, EventArgs e)
    {
      var str = SourceGDID.Text.Trim();
      if (str.IsNullOrEmpty())
      {
        ResultGDIDs.Text = string.Empty;
        return;
      }

      try
      {
        ResultGDIDs.Text = AgnivoHelper.ParseGDID(str);
      }
      catch (Exception error)
      {
        ResultGDIDs.Text = error.Message;
      }
    }

    private void SourceBSONText_TextChanged(object sender, EventArgs e)
    {
      var bson = ((TextBox)sender).Text;
      try
      {
        var bytes = System.Text.ASCIIEncoding.ASCII.GetByteCount(bson);
        statistics.Text = STATISTICS.Args(bytes);
      }
      catch (Exception error)
      {
        statistics.Text = error.Message;
      }
    }

    private void btnBase64Json_Click(object sender, EventArgs e)
    {
      var jsonBsonText = tbBase64Json.Text;
      if (jsonBsonText.IsNullOrWhiteSpace()) return;

      try
      {
        var json = AgnivoHelper.Base64ToJson(jsonBsonText);
        var form = new DecodedBSONForm(json);
        form.Show();
      }
      catch (Exception error)
      {
        MessageBox.Show(error.ToMessageWithType());
      }
    }

    private void tbAtomNumber_TextChanged(object sender, EventArgs e)
    {
      try
      {
        if (tbAtomNumber.Text.IsNullOrWhiteSpace())
        {
          tbAtomValue.Text = "ZERO";
          return;
        }

        Atom atom;

        if (ulong.TryParse(tbAtomNumber.Text, out var id))
        {
          atom = new Atom(id);
          tbAtomValue.Text = ("{0} = `{1}` \r\n" +
                              "{2} chars \r\n" +
                              "valid: {3}").Args(atom.ID, atom.Value, atom.Length, atom.IsValid);
          return;
        }

        if (Atom.TryEncodeValueOrId(tbAtomNumber.Text, out atom))
        {
          tbAtomValue.Text = ("{0} = `{1}` \r\n" +
                              "{2} chars \r\n" +
                              "valid: {3}").Args(atom.ID, atom.Value, atom.Length, atom.IsValid);
          return;
        }
      }
      catch (Exception error)
      {
        tbAtomValue.Text = "Invalid: " + error.ToMessageWithType();
      }
    }
  }
}
