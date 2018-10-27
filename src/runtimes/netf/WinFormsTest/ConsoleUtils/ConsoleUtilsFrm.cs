/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Text;
using System.IO;
using System.Windows.Forms;

using Azos.Platform;

namespace WinFormsTest.ConsoleUtils
{
  public partial class ConsoleUtilsFrm : Form
  {
    public ConsoleUtilsFrm()
    {
      InitializeComponent();
    }

    private void m_btnGenerate_Click(object sender, EventArgs e)
    {
      WriteHTMLFromContentPath("Help.txt", m_cmbBase.SelectedItem.ToString());
    }

    private void button1_Click(object sender, EventArgs e)
    {
      WriteHTMLFromContentPath("Welcome.txt", m_cmbBase.SelectedItem.ToString());
    }

    private static void WriteHTMLFromContentPath(string contentPath, string baseName)
    {
      var content = typeof(ConsoleUtilsFrm).GetText(contentPath);
      WriteHTML(content, baseName);
    }

    private static void WriteHTML(string content, string baseName)
    {
      var baseHtm = typeof(ConsoleUtilsFrm).GetText(baseName);

      using (var output = new MemoryStream())
      {
        Azos.IO.ConsoleUtils.WriteMarkupContentAsHTML(output, content);

        var outStr = Encoding.UTF8.GetString(output.GetBuffer(), 0, (int)output.Position);

        var processedHtm = baseHtm.Replace("[BODY]", outStr);

        using (var fs = new FileStream("ConsoleHtml.htm", FileMode.Create, FileAccess.Write))
        {
          using (var fwri = new StreamWriter(fs, Encoding.UTF8))
          {
            fwri.Write(processedHtm);
          }
        }
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
      var d = new OpenFileDialog() { Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*" };

      if (d.ShowDialog() == DialogResult.OK)
        m_txtFile.Text = d.FileName;
    }

    private void m_btnGenerateOpenedFile_Click(object sender, EventArgs e)
    {
      try
      {
        var contet = File.ReadAllText(m_txtFile.Text);
        WriteHTML(contet, m_cmbBase.SelectedItem.ToString());
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

  }
}
