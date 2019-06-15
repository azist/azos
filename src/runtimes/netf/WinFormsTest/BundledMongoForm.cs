using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Azos.Data.Access.MongoDb;

namespace WinFormsTest
{
  public partial class BundledMongoForm : Form
  {
    public BundledMongoForm()
    {
      InitializeComponent();
    }

    private BundledMongoDb m_Bdb;

    private void btnStart_Click(object sender, EventArgs e)
    {
      if (m_Bdb==null)
      {
        m_Bdb = new BundledMongoDb(Azos.Apps.ExecutionContext.Application);
        m_Bdb.Mongo_directoyperdb = false;
        m_Bdb.Mongo_dbpath = @"c:\data\bundled";
      }
      m_Bdb.Start();
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
      m_Bdb.WaitForCompleteStop();
    }
  }
}
