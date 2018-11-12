using System;
using System.Windows.Forms;

using Azos;
using Azos.Data;
using Azos.Sky.Kdb;

namespace WinFormsTestSky.KDB
{
  public partial class KDBForm : System.Windows.Forms.Form
  {
    public KDBForm()
    {
    InitializeComponent();
    }

    private const string TBL = "MT_TBL";

    DefaultKdbStore m_Store;

    private void tmrReactor_Tick(Object sender,EventArgs e)
    {
      tbConfig.Enabled = m_Store == null;
      btnMount.Enabled = m_Store == null;
      btnUnmount.Enabled = m_Store != null;

      btnGet.Enabled = m_Store != null;
      btnPut.Enabled = m_Store != null;
      btnDelete.Enabled = m_Store != null;

      tbPutCount.Enabled = m_Store != null;
      tbGetCount.Enabled = m_Store != null;
      tbDeleteCount.Enabled = m_Store != null;
    }

    private void btnMount_Click(Object sender,EventArgs e)
    {
      var conf = tbConfig.Text.AsLaconicConfig();
      if (conf == null)
      {
        MessageBox.Show("Error in conf");
        return;
      }

      m_Store = new DefaultKdbStore();
      m_Store.Configure(conf);
      m_Store.Start();
    }

    private void btnUnmount_Click(Object sender,EventArgs e)
    {
      DisposableObject.DisposeAndNull(ref m_Store);
    }

    private void btnPut_Click(Object sender,EventArgs e)
    {
      var cnt = tbPutCount.Text.AsInt(0);
      for(var i = 0; i < cnt; i++)
        m_Store.PutRaw(TBL, i.ToString().ToUTF8Bytes(), new byte[2*12]);
    }

    private void btnGet_Click(Object sender,EventArgs e)
    {
      var cnt = tbPutCount.Text.AsInt(0);
      for(var i = 0; i < cnt; i++)
        m_Store.GetRaw(TBL, i.ToString().ToUTF8Bytes());
    }

    private void btnDelete_Click(Object sender,EventArgs e)
    {
      var cnt = tbPutCount.Text.AsInt(0);
      for(var i = 0; i < cnt; i++)
        m_Store.Delete(TBL, i.ToString().ToUTF8Bytes());
    }
  }
}
