/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
namespace WinFormsTestSky.Locker
{
  partial class MDSARLocking
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
      this.tbPatient = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.tbFacility = new System.Windows.Forms.TextBox();
      this.tb = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.tbMDS = new System.Windows.Forms.TextBox();
      this.btnMDSEnter = new System.Windows.Forms.Button();
      this.btnMDSReview = new System.Windows.Forms.Button();
      this.btnARLock = new System.Windows.Forms.Button();
      this.label4 = new System.Windows.Forms.Label();
      this.lbSessions = new System.Windows.Forms.ListBox();
      this.btnSessionAdd = new System.Windows.Forms.Button();
      this.btnSessionDelete = new System.Windows.Forms.Button();
      this.tbSession = new System.Windows.Forms.TextBox();
      this.btnARLockStop = new System.Windows.Forms.Button();
      this.btnMDSReviewStop = new System.Windows.Forms.Button();
      this.btnMDSEnterStop = new System.Windows.Forms.Button();
      this.btnSelectVar = new System.Windows.Forms.Button();
      this.cmbVarName = new System.Windows.Forms.ComboBox();
      this.chkIgnoreSession = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // tbPatient
      // 
      this.tbPatient.Location = new System.Drawing.Point(745, 69);
      this.tbPatient.Name = "tbPatient";
      this.tbPatient.Size = new System.Drawing.Size(100, 20);
      this.tbPatient.TabIndex = 1;
      this.tbPatient.Text = "ANUA-29";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(745, 49);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(85, 13);
      this.label1.TabIndex = 2;
      this.label1.Text = "Resident Patient";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(626, 49);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(39, 13);
      this.label2.TabIndex = 4;
      this.label2.Text = "Facility";
      // 
      // tbFacility
      // 
      this.tbFacility.Location = new System.Drawing.Point(626, 69);
      this.tbFacility.Name = "tbFacility";
      this.tbFacility.Size = new System.Drawing.Size(100, 20);
      this.tbFacility.TabIndex = 3;
      this.tbFacility.Text = "RollingAcres-1";
      // 
      // tb
      // 
      this.tb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tb.BackColor = System.Drawing.Color.DimGray;
      this.tb.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
      this.tb.Location = new System.Drawing.Point(4, 196);
      this.tb.Multiline = true;
      this.tb.Name = "tb";
      this.tb.ReadOnly = true;
      this.tb.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.tb.Size = new System.Drawing.Size(1138, 392);
      this.tb.TabIndex = 5;
      this.tb.TabStop = false;
      this.tb.WordWrap = false;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(876, 47);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(90, 13);
      this.label3.TabIndex = 7;
      this.label3.Text = "MDS Assessment";
      // 
      // tbMDS
      // 
      this.tbMDS.Location = new System.Drawing.Point(876, 68);
      this.tbMDS.Name = "tbMDS";
      this.tbMDS.Size = new System.Drawing.Size(100, 20);
      this.tbMDS.TabIndex = 6;
      this.tbMDS.Text = "439801";
      // 
      // btnMDSEnter
      // 
      this.btnMDSEnter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnMDSEnter.ForeColor = System.Drawing.Color.Green;
      this.btnMDSEnter.Location = new System.Drawing.Point(629, 104);
      this.btnMDSEnter.Name = "btnMDSEnter";
      this.btnMDSEnter.Size = new System.Drawing.Size(86, 51);
      this.btnMDSEnter.TabIndex = 8;
      this.btnMDSEnter.Text = "MDS Enter";
      this.btnMDSEnter.UseVisualStyleBackColor = true;
      this.btnMDSEnter.Click += new System.EventHandler(this.btnLOCKBUTTON_Click);
      // 
      // btnMDSReview
      // 
      this.btnMDSReview.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnMDSReview.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
      this.btnMDSReview.Location = new System.Drawing.Point(721, 104);
      this.btnMDSReview.Name = "btnMDSReview";
      this.btnMDSReview.Size = new System.Drawing.Size(86, 51);
      this.btnMDSReview.TabIndex = 9;
      this.btnMDSReview.Text = "MDS Review";
      this.btnMDSReview.UseVisualStyleBackColor = true;
      this.btnMDSReview.Click += new System.EventHandler(this.btnLOCKBUTTON_Click);
      // 
      // btnARLock
      // 
      this.btnARLock.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnARLock.ForeColor = System.Drawing.Color.Fuchsia;
      this.btnARLock.Location = new System.Drawing.Point(841, 104);
      this.btnARLock.Name = "btnARLock";
      this.btnARLock.Size = new System.Drawing.Size(86, 51);
      this.btnARLock.TabIndex = 10;
      this.btnARLock.Text = "AR Month End Close";
      this.btnARLock.UseVisualStyleBackColor = true;
      this.btnARLock.Click += new System.EventHandler(this.btnLOCKBUTTON_Click);
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(12, 9);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(44, 13);
      this.label4.TabIndex = 12;
      this.label4.Text = "Session";
      // 
      // lbSessions
      // 
      this.lbSessions.BackColor = System.Drawing.SystemColors.InactiveBorder;
      this.lbSessions.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbSessions.ForeColor = System.Drawing.Color.Blue;
      this.lbSessions.FormattingEnabled = true;
      this.lbSessions.Location = new System.Drawing.Point(15, 38);
      this.lbSessions.Name = "lbSessions";
      this.lbSessions.Size = new System.Drawing.Size(557, 147);
      this.lbSessions.TabIndex = 13;
      // 
      // btnSessionAdd
      // 
      this.btnSessionAdd.Location = new System.Drawing.Point(195, 3);
      this.btnSessionAdd.Name = "btnSessionAdd";
      this.btnSessionAdd.Size = new System.Drawing.Size(59, 29);
      this.btnSessionAdd.TabIndex = 14;
      this.btnSessionAdd.Text = "Add";
      this.btnSessionAdd.UseVisualStyleBackColor = true;
      this.btnSessionAdd.Click += new System.EventHandler(this.btnSessionAdd_Click);
      // 
      // btnSessionDelete
      // 
      this.btnSessionDelete.Location = new System.Drawing.Point(260, 3);
      this.btnSessionDelete.Name = "btnSessionDelete";
      this.btnSessionDelete.Size = new System.Drawing.Size(59, 29);
      this.btnSessionDelete.TabIndex = 15;
      this.btnSessionDelete.Text = "Delete";
      this.btnSessionDelete.UseVisualStyleBackColor = true;
      this.btnSessionDelete.Click += new System.EventHandler(this.btnSessionDelete_Click);
      // 
      // tbSession
      // 
      this.tbSession.Location = new System.Drawing.Point(89, 7);
      this.tbSession.Name = "tbSession";
      this.tbSession.Size = new System.Drawing.Size(100, 20);
      this.tbSession.TabIndex = 16;
      this.tbSession.Text = "MySession-";
      // 
      // btnARLockStop
      // 
      this.btnARLockStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnARLockStop.ForeColor = System.Drawing.Color.Fuchsia;
      this.btnARLockStop.Location = new System.Drawing.Point(841, 161);
      this.btnARLockStop.Name = "btnARLockStop";
      this.btnARLockStop.Size = new System.Drawing.Size(86, 21);
      this.btnARLockStop.TabIndex = 19;
      this.btnARLockStop.Text = "END";
      this.btnARLockStop.UseVisualStyleBackColor = true;
      this.btnARLockStop.Click += new System.EventHandler(this.btnLOCKBUTTON_Click);
      // 
      // btnMDSReviewStop
      // 
      this.btnMDSReviewStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnMDSReviewStop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
      this.btnMDSReviewStop.Location = new System.Drawing.Point(721, 161);
      this.btnMDSReviewStop.Name = "btnMDSReviewStop";
      this.btnMDSReviewStop.Size = new System.Drawing.Size(86, 21);
      this.btnMDSReviewStop.TabIndex = 18;
      this.btnMDSReviewStop.Text = "END";
      this.btnMDSReviewStop.UseVisualStyleBackColor = true;
      this.btnMDSReviewStop.Click += new System.EventHandler(this.btnLOCKBUTTON_Click);
      // 
      // btnMDSEnterStop
      // 
      this.btnMDSEnterStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnMDSEnterStop.ForeColor = System.Drawing.Color.Green;
      this.btnMDSEnterStop.Location = new System.Drawing.Point(629, 161);
      this.btnMDSEnterStop.Name = "btnMDSEnterStop";
      this.btnMDSEnterStop.Size = new System.Drawing.Size(86, 21);
      this.btnMDSEnterStop.TabIndex = 17;
      this.btnMDSEnterStop.Text = "END";
      this.btnMDSEnterStop.UseVisualStyleBackColor = true;
      this.btnMDSEnterStop.Click += new System.EventHandler(this.btnLOCKBUTTON_Click);
      // 
      // btnSelectVar
      // 
      this.btnSelectVar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnSelectVar.ForeColor = System.Drawing.Color.RoyalBlue;
      this.btnSelectVar.Location = new System.Drawing.Point(1005, 131);
      this.btnSelectVar.Name = "btnSelectVar";
      this.btnSelectVar.Size = new System.Drawing.Size(86, 51);
      this.btnSelectVar.TabIndex = 10;
      this.btnSelectVar.Text = "Select Var";
      this.btnSelectVar.UseVisualStyleBackColor = true;
      this.btnSelectVar.Click += new System.EventHandler(this.btnLOCKBUTTON_Click);
      // 
      // cmbVarName
      // 
      this.cmbVarName.FormattingEnabled = true;
      this.cmbVarName.Items.AddRange(new object[] {
            "MDS-Entry",
            "MDS-Review",
            "Month-End"});
      this.cmbVarName.Location = new System.Drawing.Point(1005, 67);
      this.cmbVarName.Name = "cmbVarName";
      this.cmbVarName.Size = new System.Drawing.Size(121, 21);
      this.cmbVarName.TabIndex = 20;
      // 
      // chkIgnoreSession
      // 
      this.chkIgnoreSession.AutoSize = true;
      this.chkIgnoreSession.Checked = true;
      this.chkIgnoreSession.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkIgnoreSession.Location = new System.Drawing.Point(1005, 96);
      this.chkIgnoreSession.Name = "chkIgnoreSession";
      this.chkIgnoreSession.Size = new System.Drawing.Size(119, 17);
      this.chkIgnoreSession.TabIndex = 21;
      this.chkIgnoreSession.Text = "Ignore This Session";
      this.chkIgnoreSession.UseVisualStyleBackColor = true;
      // 
      // MDSARLocking
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1143, 589);
      this.Controls.Add(this.chkIgnoreSession);
      this.Controls.Add(this.cmbVarName);
      this.Controls.Add(this.btnARLockStop);
      this.Controls.Add(this.btnMDSReviewStop);
      this.Controls.Add(this.btnMDSEnterStop);
      this.Controls.Add(this.tbSession);
      this.Controls.Add(this.btnSessionDelete);
      this.Controls.Add(this.btnSessionAdd);
      this.Controls.Add(this.lbSessions);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.btnSelectVar);
      this.Controls.Add(this.btnARLock);
      this.Controls.Add(this.btnMDSReview);
      this.Controls.Add(this.btnMDSEnter);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.tbMDS);
      this.Controls.Add(this.tb);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.tbPatient);
      this.Controls.Add(this.tbFacility);
      this.Name = "MDSARLocking";
      this.Text = "Clinical MDS &  Financial AR Locking";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MDSARLocking_FormClosed);
      this.Load += new System.EventHandler(this.MDSARLocking_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox tbPatient;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox tbFacility;
    private System.Windows.Forms.TextBox tb;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox tbMDS;
    private System.Windows.Forms.Button btnMDSEnter;
    private System.Windows.Forms.Button btnMDSReview;
    private System.Windows.Forms.Button btnARLock;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.ListBox lbSessions;
    private System.Windows.Forms.Button btnSessionAdd;
    private System.Windows.Forms.Button btnSessionDelete;
    private System.Windows.Forms.TextBox tbSession;
    private System.Windows.Forms.Button btnARLockStop;
    private System.Windows.Forms.Button btnMDSReviewStop;
    private System.Windows.Forms.Button btnMDSEnterStop;
    private System.Windows.Forms.Button btnSelectVar;
    private System.Windows.Forms.ComboBox cmbVarName;
    private System.Windows.Forms.CheckBox chkIgnoreSession;
  }
}