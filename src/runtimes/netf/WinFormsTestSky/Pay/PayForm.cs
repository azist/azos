using System;
using System.Windows.Forms;

using Azos;
using Azos.Data;
using Azos.Financial;
using Azos.Web.Pay;

namespace WinFormsTestSky.Pay
{
  public partial class PayForm : System.Windows.Forms.Form
  {
    #region Inner Classes

      private class AccountCmbItem
      {
        public Account Account;
        public string DisplayName;

        public override string ToString()
        {
          return DisplayName;
        }
      }

    #endregion

    public PayForm()
    {
      InitializeComponent();
    }

    #region Events

      protected override void OnLoad(EventArgs e)
      {
        cmbChargeCurrencyISO.SelectedIndex = 0;

        foreach (var ps in PaySystem.Instances){
          cmbChargePaySystem.Items.Add(ps.Name);
          cmbTransferPaySystem.Items.Add(ps.Name);
        }

        if (cmbChargePaySystem.Items.Count > 0) cmbChargePaySystem.SelectedIndex = cmbChargePaySystem.Items.Count - 1;
        if (cmbTransferPaySystem.Items.Count > 0) cmbTransferPaySystem.SelectedIndex = cmbTransferPaySystem.Items.Count - 1;


        cmbChargeAccounts.Items.Add(new AccountCmbItem {
          DisplayName = "Credit Card Correct",
          Account = FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT
        });

        cmbChargeAccounts.Items.Add(new AccountCmbItem {
          DisplayName = "Credit Card Declined",
          Account = FakePaySystemHost.CARD_DECLINED
        });

        if (cmbChargeAccounts.Items.Count > 0) cmbChargeAccounts.SelectedIndex = 0;


        cmbTransferAccounts.Items.Add(new AccountCmbItem {
          DisplayName = "Debit Card Correct",
          Account = FakePaySystemHost.CARD_DEBIT_ACCOUNT_STRIPE_CORRECT
        });

        cmbTransferAccounts.Items.Add(new AccountCmbItem {
          DisplayName = "Bank Correct",
          Account = FakePaySystemHost.BANK_ACCOUNT_STRIPE_CORRECT
        });

        cmbTransferAccounts.Items.Add(new AccountCmbItem {
          DisplayName = "Debit Card Correct Addr.",
          Account = FakePaySystemHost.CARD_DEBIT_ACCOUNT_STRIPE_CORRECT_WITH_ADDRESS
        });

        if (cmbTransferAccounts.Items.Count > 0) cmbTransferAccounts.SelectedIndex = 0;

        SyncControlsEnabled(selectedTransaction);
      }

      private void lstTransactions_SelectedValueChanged(object sender, EventArgs e)
      {
        SyncControlsEnabled(selectedTransaction);
      }

      private void SyncControlsEnabled(Transaction ta)
      {
        if (ta == null)
        {
          grpCapture.Enabled = false;
          grpRefund.Enabled = false;
          return;
        }

        grpCapture.Enabled = ta.CanCapture;
        grpRefund.Enabled = ta.CanRefund;
      }

      private void btnCharge_Click(object sender, EventArgs e)
      {
        charge();
      }

      private void btnCapture_Click(object sender, EventArgs e)
      {
        capture();
      }

      private void btnRefund_Click(object sender, EventArgs e)
      {
        refund();
      }

      private void btnTransfer_Click(object sender, EventArgs e)
      {
        transfer();
      }

    #endregion

    #region Tests

      #region Charge

        private void charge()
        {
          try
          {
            var psName = (string)cmbChargePaySystem.SelectedItem;
            var ps = PaySystem.Instances[psName];

            using (var pss = ps.StartSession())
            {
              var acc = ((AccountCmbItem)cmbChargeAccounts.SelectedItem).Account;

              var amountValue = nudChargeValue.Value;
              var amountCurrencyISO = cmbChargeCurrencyISO.SelectedItem.AsString();
              var amount = new Amount(amountCurrencyISO, amountValue);

              var capture = chkChargeCapture.Checked;

              var descr = txtChargeDescription.Text;

              var ta = charge(pss, acc, amount, capture, descr);

              lstTransactions.Items.Add(ta);
            }
          }
          catch (AzosException ex)
          {
            lstTransactions.Items.Add(ex.Message);
          }
        }

        private Transaction charge(PaySession pss, Account acc, Amount amount, bool capture, string descr)
        {
          var ta = pss.Charge(acc, Account.EmptyInstance, amount, capture, descr);
          return ta;
        }

      #endregion

      #region Capture

        private void capture()
        {
          var ta = selectedTransaction;
          if (ta == null || !ta.CanCapture) return;

          try
          {
            ta.Capture();
            SyncControlsEnabled(selectedTransaction);
          }
          catch (AzosException ex)
          {
            lstTransactions.Items.Add(ex.Message);
          }
        }

      #endregion

      #region Refund

        private void refund()
        {
          var ta = selectedTransaction;
          if (ta == null || !ta.CanRefund) return;

          var refundAmountVal = nudRefundValue.Value;

          try
          {
            ta.Refund(amount: refundAmountVal > 0M ? refundAmountVal : (decimal?)null);
            SyncControlsEnabled(selectedTransaction);
          }
          catch (AzosException ex)
          {
            lstTransactions.Items.Add(ex.Message);
          }
        }

      #endregion

      #region Transfer

        private void transfer()
        {
          try
          {
            var psName = (string)cmbTransferPaySystem.SelectedItem;
            var ps = PaySystem.Instances[psName];

            using (var pss = ps.StartSession())
            {
              var acc = ((AccountCmbItem)cmbTransferAccounts.SelectedItem).Account;

              var amountValue = nudTransferValue.Value;
              var amountCurrencyISO = cmbTransferCurrencyISO.SelectedItem.AsString();
              var amount = new Amount(amountCurrencyISO, amountValue);

              var descr = txtTransferDescription.Text;

              var ta = transfer(pss, acc, amount, descr);

              lstTransactions.Items.Add(ta);
            }
          }
          catch (NFXException ex)
          {
            lstTransactions.Items.Add(ex.Message);
          }
        }

        private Transaction transfer(PaySession pss, Account acc, Amount amount, string descr)
        {
          return pss.Transfer(Account.EmptyInstance, acc, amount, descr);
        }

      #endregion

      #region Helpers

        private Transaction selectedTransaction { get { return lstTransactions.SelectedItem as Transaction; } }

      #endregion

    #endregion

  }
}
