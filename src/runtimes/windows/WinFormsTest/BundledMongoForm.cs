using System;
using System.Collections.Generic;
using System.Linq;

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

        m_Bdb.MongoBinPath = @"C:\mongo\4.0\bin";

        m_Bdb.Mongo_directoryperdb = false;
        m_Bdb.Mongo_dbpath = @"c:\data\bundled";

        m_Bdb.ComponentLogLevel = Azos.Log.MessageType.Debug;
      }
      m_Bdb.Start();
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
      m_Bdb.WaitForCompleteStop();
    }
  }
}
