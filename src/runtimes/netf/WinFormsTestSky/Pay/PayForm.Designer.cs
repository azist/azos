namespace WinFormsTestSky.Pay
{
  partial class PayForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.btnCharge = new System.Windows.Forms.Button();
      this.nudChargeValue = new System.Windows.Forms.NumericUpDown();
      this.cmbChargePaySystem = new System.Windows.Forms.ComboBox();
      this.label4 = new System.Windows.Forms.Label();
      this.cmbChargeAccounts = new System.Windows.Forms.ComboBox();
      this.cmbChargeCurrencyISO = new System.Windows.Forms.ComboBox();
      this.chkChargeCapture = new System.Windows.Forms.CheckBox();
      this.txtChargeDescription = new System.Windows.Forms.TextBox();
      this.lstTransactions = new System.Windows.Forms.ListBox();
      this.btnRefund = new System.Windows.Forms.Button();
      this.btnCapture = new System.Windows.Forms.Button();
      this.nudRefundValue = new System.Windows.Forms.NumericUpDown();
      this.lblRefund = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.cmbRefundCurrencyISO = new System.Windows.Forms.ComboBox();
      this.grpRefund = new System.Windows.Forms.GroupBox();
      this.grpCapture = new System.Windows.Forms.GroupBox();
      this.grpCharge = new System.Windows.Forms.GroupBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.txtRefundDescription = new System.Windows.Forms.TextBox();
      this.grpTransfer = new System.Windows.Forms.GroupBox();
      this.label6 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.cmbTransferPaySystem = new System.Windows.Forms.ComboBox();
      this.btnTransfer = new System.Windows.Forms.Button();
      this.nudTransferValue = new System.Windows.Forms.NumericUpDown();
      this.label9 = new System.Windows.Forms.Label();
      this.txtTransferDescription = new System.Windows.Forms.TextBox();
      this.cmbTransferAccounts = new System.Windows.Forms.ComboBox();
      this.cmbTransferCurrencyISO = new System.Windows.Forms.ComboBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.panel2 = new System.Windows.Forms.Panel();
      ((System.ComponentModel.ISupportInitialize)(this.nudChargeValue)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudRefundValue)).BeginInit();
      this.grpRefund.SuspendLayout();
      this.grpCapture.SuspendLayout();
      this.grpCharge.SuspendLayout();
      this.grpTransfer.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudTransferValue)).BeginInit();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnCharge
      // 
      this.btnCharge.Location = new System.Drawing.Point(214, 129);
      this.btnCharge.Name = "btnCharge";
      this.btnCharge.Size = new System.Drawing.Size(75, 23);
      this.btnCharge.TabIndex = 0;
      this.btnCharge.Text = "Charge";
      this.btnCharge.UseVisualStyleBackColor = true;
      this.btnCharge.Click += new System.EventHandler(this.btnCharge_Click);
      // 
      // nudChargeValue
      // 
      this.nudChargeValue.DecimalPlaces = 2;
      this.nudChargeValue.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.nudChargeValue.Location = new System.Drawing.Point(82, 91);
      this.nudChargeValue.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
      this.nudChargeValue.Name = "nudChargeValue";
      this.nudChargeValue.Size = new System.Drawing.Size(63, 20);
      this.nudChargeValue.TabIndex = 1;
      this.nudChargeValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.nudChargeValue.ThousandsSeparator = true;
      this.nudChargeValue.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
      // 
      // cmbChargePaySystem
      // 
      this.cmbChargePaySystem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbChargePaySystem.FormattingEnabled = true;
      this.cmbChargePaySystem.Location = new System.Drawing.Point(82, 19);
      this.cmbChargePaySystem.Name = "cmbChargePaySystem";
      this.cmbChargePaySystem.Size = new System.Drawing.Size(121, 21);
      this.cmbChargePaySystem.TabIndex = 8;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(11, 22);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(62, 13);
      this.label4.TabIndex = 9;
      this.label4.Text = "Pay System";
      // 
      // cmbChargeAccounts
      // 
      this.cmbChargeAccounts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbChargeAccounts.FormattingEnabled = true;
      this.cmbChargeAccounts.Location = new System.Drawing.Point(82, 53);
      this.cmbChargeAccounts.Name = "cmbChargeAccounts";
      this.cmbChargeAccounts.Size = new System.Drawing.Size(121, 21);
      this.cmbChargeAccounts.TabIndex = 10;
      // 
      // cmbChargeCurrencyISO
      // 
      this.cmbChargeCurrencyISO.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbChargeCurrencyISO.FormattingEnabled = true;
      this.cmbChargeCurrencyISO.Items.AddRange(new object[] {
            "usd",
            "eur",
            "rub",
            "uah"});
      this.cmbChargeCurrencyISO.Location = new System.Drawing.Point(227, 91);
      this.cmbChargeCurrencyISO.Name = "cmbChargeCurrencyISO";
      this.cmbChargeCurrencyISO.Size = new System.Drawing.Size(59, 21);
      this.cmbChargeCurrencyISO.TabIndex = 11;
      // 
      // chkChargeCapture
      // 
      this.chkChargeCapture.AutoSize = true;
      this.chkChargeCapture.Location = new System.Drawing.Point(2, 133);
      this.chkChargeCapture.Name = "chkChargeCapture";
      this.chkChargeCapture.Size = new System.Drawing.Size(63, 17);
      this.chkChargeCapture.TabIndex = 12;
      this.chkChargeCapture.Text = "Capture";
      this.chkChargeCapture.UseVisualStyleBackColor = true;
      // 
      // txtChargeDescription
      // 
      this.txtChargeDescription.Location = new System.Drawing.Point(71, 131);
      this.txtChargeDescription.Name = "txtChargeDescription";
      this.txtChargeDescription.Size = new System.Drawing.Size(135, 20);
      this.txtChargeDescription.TabIndex = 13;
      this.txtChargeDescription.Text = "test charge";
      // 
      // lstTransactions
      // 
      this.lstTransactions.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lstTransactions.FormattingEnabled = true;
      this.lstTransactions.Location = new System.Drawing.Point(0, 0);
      this.lstTransactions.Name = "lstTransactions";
      this.lstTransactions.Size = new System.Drawing.Size(551, 543);
      this.lstTransactions.TabIndex = 14;
      this.lstTransactions.SelectedValueChanged += new System.EventHandler(this.lstTransactions_SelectedValueChanged);
      // 
      // btnRefund
      // 
      this.btnRefund.Location = new System.Drawing.Point(189, 57);
      this.btnRefund.Name = "btnRefund";
      this.btnRefund.Size = new System.Drawing.Size(75, 23);
      this.btnRefund.TabIndex = 15;
      this.btnRefund.Text = "Refund";
      this.btnRefund.UseVisualStyleBackColor = true;
      this.btnRefund.Click += new System.EventHandler(this.btnRefund_Click);
      // 
      // btnCapture
      // 
      this.btnCapture.Location = new System.Drawing.Point(189, 19);
      this.btnCapture.Name = "btnCapture";
      this.btnCapture.Size = new System.Drawing.Size(75, 23);
      this.btnCapture.TabIndex = 16;
      this.btnCapture.Text = "Capture";
      this.btnCapture.UseVisualStyleBackColor = true;
      this.btnCapture.Click += new System.EventHandler(this.btnCapture_Click);
      // 
      // nudRefundValue
      // 
      this.nudRefundValue.DecimalPlaces = 2;
      this.nudRefundValue.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.nudRefundValue.Location = new System.Drawing.Point(59, 22);
      this.nudRefundValue.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
      this.nudRefundValue.Name = "nudRefundValue";
      this.nudRefundValue.Size = new System.Drawing.Size(63, 20);
      this.nudRefundValue.TabIndex = 17;
      this.nudRefundValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.nudRefundValue.ThousandsSeparator = true;
      // 
      // lblRefund
      // 
      this.lblRefund.AutoSize = true;
      this.lblRefund.Location = new System.Drawing.Point(9, 24);
      this.lblRefund.Name = "lblRefund";
      this.lblRefund.Size = new System.Drawing.Size(43, 13);
      this.lblRefund.TabIndex = 18;
      this.lblRefund.Text = "Amount";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(132, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(67, 13);
      this.label1.TabIndex = 19;
      this.label1.Text = "CurrencyISO";
      // 
      // cmbRefundCurrencyISO
      // 
      this.cmbRefundCurrencyISO.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbRefundCurrencyISO.FormattingEnabled = true;
      this.cmbRefundCurrencyISO.Items.AddRange(new object[] {
            "usd",
            "eur",
            "rub",
            "uah"});
      this.cmbRefundCurrencyISO.Location = new System.Drawing.Point(205, 21);
      this.cmbRefundCurrencyISO.Name = "cmbRefundCurrencyISO";
      this.cmbRefundCurrencyISO.Size = new System.Drawing.Size(59, 21);
      this.cmbRefundCurrencyISO.TabIndex = 20;
      // 
      // grpRefund
      // 
      this.grpRefund.Controls.Add(this.txtRefundDescription);
      this.grpRefund.Controls.Add(this.cmbRefundCurrencyISO);
      this.grpRefund.Controls.Add(this.lblRefund);
      this.grpRefund.Controls.Add(this.btnRefund);
      this.grpRefund.Controls.Add(this.label1);
      this.grpRefund.Controls.Add(this.nudRefundValue);
      this.grpRefund.Location = new System.Drawing.Point(5, 250);
      this.grpRefund.Name = "grpRefund";
      this.grpRefund.Size = new System.Drawing.Size(275, 100);
      this.grpRefund.TabIndex = 21;
      this.grpRefund.TabStop = false;
      this.grpRefund.Text = "Refund";
      // 
      // grpCapture
      // 
      this.grpCapture.Controls.Add(this.btnCapture);
      this.grpCapture.Location = new System.Drawing.Point(3, 186);
      this.grpCapture.Name = "grpCapture";
      this.grpCapture.Size = new System.Drawing.Size(275, 58);
      this.grpCapture.TabIndex = 22;
      this.grpCapture.TabStop = false;
      this.grpCapture.Text = "Capture";
      // 
      // grpCharge
      // 
      this.grpCharge.Controls.Add(this.label5);
      this.grpCharge.Controls.Add(this.label3);
      this.grpCharge.Controls.Add(this.label2);
      this.grpCharge.Controls.Add(this.cmbChargePaySystem);
      this.grpCharge.Controls.Add(this.btnCharge);
      this.grpCharge.Controls.Add(this.nudChargeValue);
      this.grpCharge.Controls.Add(this.label4);
      this.grpCharge.Controls.Add(this.txtChargeDescription);
      this.grpCharge.Controls.Add(this.cmbChargeAccounts);
      this.grpCharge.Controls.Add(this.chkChargeCapture);
      this.grpCharge.Controls.Add(this.cmbChargeCurrencyISO);
      this.grpCharge.Location = new System.Drawing.Point(3, 3);
      this.grpCharge.Name = "grpCharge";
      this.grpCharge.Size = new System.Drawing.Size(310, 168);
      this.grpCharge.TabIndex = 23;
      this.grpCharge.TabStop = false;
      this.grpCharge.Text = "Charge";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(11, 56);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(47, 13);
      this.label2.TabIndex = 14;
      this.label2.Text = "Account";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(156, 94);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(67, 13);
      this.label3.TabIndex = 20;
      this.label3.Text = "CurrencyISO";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(11, 94);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(43, 13);
      this.label5.TabIndex = 20;
      this.label5.Text = "Amount";
      // 
      // txtRefundDescription
      // 
      this.txtRefundDescription.Location = new System.Drawing.Point(14, 57);
      this.txtRefundDescription.Name = "txtRefundDescription";
      this.txtRefundDescription.Size = new System.Drawing.Size(121, 20);
      this.txtRefundDescription.TabIndex = 21;
      this.txtRefundDescription.Text = "test refund";
      // 
      // grpTransfer
      // 
      this.grpTransfer.Controls.Add(this.label6);
      this.grpTransfer.Controls.Add(this.label7);
      this.grpTransfer.Controls.Add(this.label8);
      this.grpTransfer.Controls.Add(this.cmbTransferPaySystem);
      this.grpTransfer.Controls.Add(this.btnTransfer);
      this.grpTransfer.Controls.Add(this.nudTransferValue);
      this.grpTransfer.Controls.Add(this.label9);
      this.grpTransfer.Controls.Add(this.txtTransferDescription);
      this.grpTransfer.Controls.Add(this.cmbTransferAccounts);
      this.grpTransfer.Controls.Add(this.cmbTransferCurrencyISO);
      this.grpTransfer.Location = new System.Drawing.Point(7, 365);
      this.grpTransfer.Name = "grpTransfer";
      this.grpTransfer.Size = new System.Drawing.Size(310, 168);
      this.grpTransfer.TabIndex = 24;
      this.grpTransfer.TabStop = false;
      this.grpTransfer.Text = "Transfer";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(11, 94);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(43, 13);
      this.label6.TabIndex = 20;
      this.label6.Text = "Amount";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(156, 94);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(67, 13);
      this.label7.TabIndex = 20;
      this.label7.Text = "CurrencyISO";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(11, 56);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(47, 13);
      this.label8.TabIndex = 14;
      this.label8.Text = "Account";
      // 
      // cmbTransferPaySystem
      // 
      this.cmbTransferPaySystem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbTransferPaySystem.FormattingEnabled = true;
      this.cmbTransferPaySystem.Location = new System.Drawing.Point(82, 19);
      this.cmbTransferPaySystem.Name = "cmbTransferPaySystem";
      this.cmbTransferPaySystem.Size = new System.Drawing.Size(121, 21);
      this.cmbTransferPaySystem.TabIndex = 8;
      // 
      // btnTransfer
      // 
      this.btnTransfer.Location = new System.Drawing.Point(214, 129);
      this.btnTransfer.Name = "btnTransfer";
      this.btnTransfer.Size = new System.Drawing.Size(75, 23);
      this.btnTransfer.TabIndex = 0;
      this.btnTransfer.Text = "Transfer";
      this.btnTransfer.UseVisualStyleBackColor = true;
      this.btnTransfer.Click += new System.EventHandler(this.btnTransfer_Click);
      // 
      // nudTransferValue
      // 
      this.nudTransferValue.DecimalPlaces = 2;
      this.nudTransferValue.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.nudTransferValue.Location = new System.Drawing.Point(82, 91);
      this.nudTransferValue.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
      this.nudTransferValue.Name = "nudTransferValue";
      this.nudTransferValue.Size = new System.Drawing.Size(63, 20);
      this.nudTransferValue.TabIndex = 1;
      this.nudTransferValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.nudTransferValue.ThousandsSeparator = true;
      this.nudTransferValue.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(11, 22);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(62, 13);
      this.label9.TabIndex = 9;
      this.label9.Text = "Pay System";
      // 
      // txtTransferDescription
      // 
      this.txtTransferDescription.Location = new System.Drawing.Point(85, 131);
      this.txtTransferDescription.Name = "txtTransferDescription";
      this.txtTransferDescription.Size = new System.Drawing.Size(121, 20);
      this.txtTransferDescription.TabIndex = 13;
      this.txtTransferDescription.Text = "test transfer";
      // 
      // cmbTransferAccounts
      // 
      this.cmbTransferAccounts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbTransferAccounts.FormattingEnabled = true;
      this.cmbTransferAccounts.Location = new System.Drawing.Point(82, 53);
      this.cmbTransferAccounts.Name = "cmbTransferAccounts";
      this.cmbTransferAccounts.Size = new System.Drawing.Size(121, 21);
      this.cmbTransferAccounts.TabIndex = 10;
      // 
      // cmbTransferCurrencyISO
      // 
      this.cmbTransferCurrencyISO.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbTransferCurrencyISO.FormattingEnabled = true;
      this.cmbTransferCurrencyISO.Items.AddRange(new object[] {
            "usd",
            "eur",
            "rub",
            "uah"});
      this.cmbTransferCurrencyISO.Location = new System.Drawing.Point(227, 91);
      this.cmbTransferCurrencyISO.Name = "cmbTransferCurrencyISO";
      this.cmbTransferCurrencyISO.Size = new System.Drawing.Size(59, 21);
      this.cmbTransferCurrencyISO.TabIndex = 11;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.grpCharge);
      this.panel1.Controls.Add(this.grpTransfer);
      this.panel1.Controls.Add(this.grpRefund);
      this.panel1.Controls.Add(this.grpCapture);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(324, 543);
      this.panel1.TabIndex = 25;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.lstTransactions);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(324, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(551, 543);
      this.panel2.TabIndex = 26;
      // 
      // PayForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(875, 543);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.Name = "PayForm";
      this.Text = "PayForm";
      ((System.ComponentModel.ISupportInitialize)(this.nudChargeValue)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudRefundValue)).EndInit();
      this.grpRefund.ResumeLayout(false);
      this.grpRefund.PerformLayout();
      this.grpCapture.ResumeLayout(false);
      this.grpCharge.ResumeLayout(false);
      this.grpCharge.PerformLayout();
      this.grpTransfer.ResumeLayout(false);
      this.grpTransfer.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudTransferValue)).EndInit();
      this.panel1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnCharge;
    private System.Windows.Forms.NumericUpDown nudChargeValue;
    private System.Windows.Forms.ComboBox cmbChargePaySystem;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.ComboBox cmbChargeAccounts;
    private System.Windows.Forms.ComboBox cmbChargeCurrencyISO;
    private System.Windows.Forms.CheckBox chkChargeCapture;
    private System.Windows.Forms.TextBox txtChargeDescription;
    private System.Windows.Forms.ListBox lstTransactions;
    private System.Windows.Forms.Button btnRefund;
    private System.Windows.Forms.Button btnCapture;
    private System.Windows.Forms.NumericUpDown nudRefundValue;
    private System.Windows.Forms.Label lblRefund;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox cmbRefundCurrencyISO;
    private System.Windows.Forms.GroupBox grpRefund;
    private System.Windows.Forms.GroupBox grpCapture;
    private System.Windows.Forms.GroupBox grpCharge;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox txtRefundDescription;
    private System.Windows.Forms.GroupBox grpTransfer;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.ComboBox cmbTransferPaySystem;
    private System.Windows.Forms.Button btnTransfer;
    private System.Windows.Forms.NumericUpDown nudTransferValue;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.TextBox txtTransferDescription;
    private System.Windows.Forms.ComboBox cmbTransferAccounts;
    private System.Windows.Forms.ComboBox cmbTransferCurrencyISO;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel2;
  }
}