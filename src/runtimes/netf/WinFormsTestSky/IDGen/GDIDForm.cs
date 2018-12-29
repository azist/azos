using System;
using System.Windows.Forms;
using System.Diagnostics;


using Azos;
using Azos.Data;
using Azos.Sky.Identification;

namespace WinFormsTestSky.IDGen
{
  public partial class GDIDForm : System.Windows.Forms.Form
  {
    public GDIDForm()
    {
      InitializeComponent();
    }


    private GdidGenerator m_Generator;


    private void GDIDForm_Load(object sender, EventArgs e)
    {
      m_Generator = new GdidGenerator(Azos.WinForms.FormsAmbient.App);
      m_Generator.TestingAuthorityNode = "sync://localhost:4000";//"async://localhost:4001";
    }

    private void GDIDForm_FormClosed(object sender, FormClosedEventArgs e)
    {
      DisposableObject.DisposeAndNull(ref m_Generator);
    }




    private void chkAuto_CheckedChanged(object sender, EventArgs e)
    {
      tmrAuto.Enabled = ! tmrAuto.Enabled;
    }


    private void btnGenerateOne_Click(object sender, EventArgs e)
    {
      tbOutput.Text = "";

      var sw = Stopwatch.StartNew();
      var gdid = m_Generator.GenerateOneGdid(tbNamespace.Text, tbSequence.Text, tbManyCount.Text.AsInt(0));

      var elapsedMs = sw.ElapsedTicks / ((double)Stopwatch.Frequency / 1000d);

      tbOutput.AppendText("Elapsed {0:n3}msec".Args(elapsedMs));


      tbOutput.AppendText("{0}\r\n".Args( gdid ));

    }

    private void btnMany_Click(object sender, EventArgs e)
    {
      tbOutput.Text = "";

      var sw = Stopwatch.StartNew();
      var many = m_Generator.TryGenerateManyConsecutiveGdids(tbNamespace.Text, tbSequence.Text, tbManyCount.Text.AsInt(10));

      var elapsedMs = sw.ElapsedTicks / ((double)Stopwatch.Frequency / 1000d);

      tbOutput.AppendText("Elapsed {0:n3}msec".Args(elapsedMs));

      for(var i =0; i<many.Length; i++)
      {
        tbOutput.AppendText("{0} {1}\r\n".Args(i, many[i]));
      }
    }

    private void tmrAuto_Tick(object sender, EventArgs e)
    {
      btnMany_Click(null, null);
    }






  }
}
