/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Text;
using System.Windows.Forms;


using Azos;
using Azos.Serialization.JSON;
using Azos.Sky.Locking;
using Azos.Sky.Locking.Server;

namespace WinFormsTestSky.Locker
{
  public partial class MDSARLocking : Form
  {
    public MDSARLocking()
    {
      InitializeComponent();
    }


    private LockServerService m_Server;

    private void MDSARLocking_Load(object sender, EventArgs e)
    {
      m_Server = new LockServerService(null);
      m_Server.Start();
    }

    private void MDSARLocking_FormClosed(object sender, FormClosedEventArgs e)
    {
      m_Server.Dispose();
    }


    private void btnSessionAdd_Click(object sender, EventArgs e)
    {
      var name = "{0}-{1}".Args(tbSession.Text, DateTime.Now);

      var session = new LockSessionData( new LockSessionID(null), name, 1000);
      lbSessions.Items.Add( session );
      log("Added session "+session);
    }

    private void btnSessionDelete_Click(object sender, EventArgs e)
    {
      var session = lbSessions.SelectedItem as LockSessionData;
      if (session==null) return;

      var result = m_Server.EndLockSession( session.ID );


      lbSessions.Items.Remove( session );
      log("Removed session {0}. \r\n Server returned: [{1}] ".Args(session, result));
    }

    private void log (string msg)
    {
      tb.AppendText( "{0}  {1}\r\n".Args(DateTime.Now, msg) );
    }


    private LockTransaction mdsEnterStart(string facility, string patient, string mds)
    {
      return new LockTransaction("MDS Entry for {0}@'{1}'".Args(patient, facility), "Clinical", 0, 0.0d,
                 LockOp.Assert( LockOp.Not(  LockOp.Exists("Month-End", facility)  )),
                 LockOp.Assert( LockOp.Not(  LockOp.Exists("MDS-Review", facility, patient+mds)  )),
                 LockOp.Assert( LockOp.SetVar("MDS-Entry", facility,  patient+mds, allowDuplicates: true) )
               );
    }

    private LockTransaction mdsEnterStop(string facility, string patient, string mds)
    {
      return new LockTransaction("MDS Entry for {0}@'{1}' is done".Args(patient, facility), "Clinical", 0, 0.0d,
                    LockOp.Assert( LockOp.DeleteVar("MDS-Entry", facility, patient+mds) )
               );
    }


    private LockTransaction mdsReviewStart(string facility, string patient, string mds)
    {
      return  new LockTransaction("MDS Review for {0}@'{1}'".Args(patient, facility), "Clinical", 0, 0.0d,
               LockOp.Assert( LockOp.Not(  LockOp.Exists("Month-End", facility)  )),
               LockOp.Assert( LockOp.Not(  LockOp.Exists("MDS-Entry", facility, patient+mds)  )),
               LockOp.Assert( LockOp.SetVar("MDS-Review", facility, patient+mds, allowDuplicates: false) )
               );
    }

    private LockTransaction mdsReviewStop(string facility, string patient, string mds)
    {
      return  new LockTransaction("MDS Review for {0}@'{1}' is done".Args(patient, facility), "Clinical", 0, 0.0d,
                    LockOp.Assert( LockOp.DeleteVar("MDS-Review", facility, patient+mds) )
               );
    }


    private LockTransaction arCloseStart(string facility)
    {
      return  new LockTransaction("Month End Close@'{0}'".Args(facility), "Clinical", 0, 0.0d,
                LockOp.Assert( LockOp.Not(  LockOp.Exists("MDS-Entry", facility)  )),
                LockOp.Assert( LockOp.Not(  LockOp.Exists("MDS-Review", facility)  )),
                LockOp.Assert( LockOp.SetVar("Month-End", facility, null) )
              );
    }

    private LockTransaction arCloseStop(string facility)
    {
      return  new LockTransaction("Month End Close@'{0}' is done".Args(facility), "Clinical", 0, 0.0d,
                LockOp.Assert( LockOp.DeleteVar("Month-End", facility) )
               );
    }

    private LockTransaction select(string facility, string varName)
    {
      return new LockTransaction("Test select var", "Clinical", 0, 0.0d,
                LockOp.SelectVarValue("VAR-RESULT", varName, facility, chkIgnoreSession.Checked)
               );
    }

    private void logTranResult(LockTransactionResult result)
    {
       var sb = new StringBuilder();

       sb.AppendFormat("Transaction: {0}  Server Host: {1} \r\n", result.TransactionID, result.ServerHost);
       sb.AppendFormat("Server Trust: {0}  Runtime Sec: {1}  \r\n", result.ServerTrustLevel, result.ServerRuntimeSec);
       sb.AppendFormat("Status: {0} {1} \r\n", result.Status, result.Status!= LockStatus.TransactionOK ? "Error Cause: {0}   Failed: {1}".Args(result.ErrorCause, result.FailedStatement): "");
       sb.AppendFormat("Data: {0} \r\n", result.ToJSON(JSONWritingOptions.PrettyPrint ) );
       sb.AppendLine("---------------------------------------------------------");
       log(sb.ToString());
    }

    private void btnLOCKBUTTON_Click(object sender, EventArgs e)
    {
      var session = lbSessions.SelectedItem as LockSessionData;
      if (session==null) return;

      var tran =
         sender == btnMDSEnter ? mdsEnterStart( tbFacility.Text, tbPatient.Text, tbMDS.Text)
         : sender == btnMDSEnterStop ? mdsEnterStop( tbFacility.Text, tbPatient.Text, tbMDS.Text)
         : sender == btnMDSReview ?  mdsReviewStart( tbFacility.Text, tbPatient.Text, tbMDS.Text)
         : sender == btnMDSReviewStop ?  mdsReviewStop( tbFacility.Text, tbPatient.Text, tbMDS.Text)
         : sender == btnARLock ?  arCloseStart( tbFacility.Text )
         : sender == btnARLockStop ? arCloseStop( tbFacility.Text )
         : select(tbFacility.Text, cmbVarName.Text) ;

      var tranResult = m_Server.ExecuteLockTransaction(session, tran);
      logTranResult( tranResult );
    }





  }
}
