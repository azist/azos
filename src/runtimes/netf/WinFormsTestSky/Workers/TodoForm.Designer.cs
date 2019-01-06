/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
namespace WinFormsTestSky.Workers
{
  partial class TodoForm
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
      if(disposing && (components != null))
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
      this.components = new System.ComponentModel.Container();
      this.btnServerStart = new System.Windows.Forms.Button();
      this.btnServerStop = new System.Windows.Forms.Button();
      this.tmr = new System.Windows.Forms.Timer(this.components);
      this.btnSendOne = new System.Windows.Forms.Button();
      this.tbPersonID = new System.Windows.Forms.TextBox();
      this.tbPersonName = new System.Windows.Forms.TextBox();
      this.tbPersonDOB = new System.Windows.Forms.TextBox();
      this.tbCount = new System.Windows.Forms.TextBox();
      this.btnSendMany = new System.Windows.Forms.Button();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.lblProcessed = new System.Windows.Forms.ToolStripStatusLabel();
      this.tbCorrelationKey = new System.Windows.Forms.TextBox();
      this.btnSendCorrelatedOne = new System.Windows.Forms.Button();
      this.tbCorrelationCounter = new System.Windows.Forms.TextBox();
      this.btnSendEMail = new System.Windows.Forms.Button();
      this.tbWho = new System.Windows.Forms.TextBox();
      this.statusStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnServerStart
      // 
      this.btnServerStart.Location = new System.Drawing.Point(12, 12);
      this.btnServerStart.Name = "btnServerStart";
      this.btnServerStart.Size = new System.Drawing.Size(75, 23);
      this.btnServerStart.TabIndex = 0;
      this.btnServerStart.Text = "START";
      this.btnServerStart.UseVisualStyleBackColor = true;
      this.btnServerStart.Click += new System.EventHandler(this.btnServerStart_Click);
      // 
      // btnServerStop
      // 
      this.btnServerStop.Location = new System.Drawing.Point(12, 41);
      this.btnServerStop.Name = "btnServerStop";
      this.btnServerStop.Size = new System.Drawing.Size(75, 23);
      this.btnServerStop.TabIndex = 1;
      this.btnServerStop.Text = "STOP";
      this.btnServerStop.UseVisualStyleBackColor = true;
      this.btnServerStop.Click += new System.EventHandler(this.btnServerStop_Click);
      // 
      // tmr
      // 
      this.tmr.Enabled = true;
      this.tmr.Interval = 250;
      this.tmr.Tick += new System.EventHandler(this.tmr_Tick);
      // 
      // btnSendOne
      // 
      this.btnSendOne.Location = new System.Drawing.Point(12, 218);
      this.btnSendOne.Name = "btnSendOne";
      this.btnSendOne.Size = new System.Drawing.Size(81, 28);
      this.btnSendOne.TabIndex = 2;
      this.btnSendOne.Text = "Send One";
      this.btnSendOne.UseVisualStyleBackColor = true;
      this.btnSendOne.Click += new System.EventHandler(this.btnSendOne_Click);
      // 
      // tbPersonID
      // 
      this.tbPersonID.Location = new System.Drawing.Point(12, 132);
      this.tbPersonID.Name = "tbPersonID";
      this.tbPersonID.Size = new System.Drawing.Size(100, 20);
      this.tbPersonID.TabIndex = 3;
      this.tbPersonID.Text = "ID-43762";
      // 
      // tbPersonName
      // 
      this.tbPersonName.Location = new System.Drawing.Point(12, 158);
      this.tbPersonName.Name = "tbPersonName";
      this.tbPersonName.Size = new System.Drawing.Size(100, 20);
      this.tbPersonName.TabIndex = 4;
      this.tbPersonName.Text = "Alex Kozloff";
      // 
      // tbPersonDOB
      // 
      this.tbPersonDOB.Location = new System.Drawing.Point(12, 184);
      this.tbPersonDOB.Name = "tbPersonDOB";
      this.tbPersonDOB.Size = new System.Drawing.Size(100, 20);
      this.tbPersonDOB.TabIndex = 5;
      this.tbPersonDOB.Text = "01/23/1982";
      // 
      // tbCount
      // 
      this.tbCount.Location = new System.Drawing.Point(142, 184);
      this.tbCount.Name = "tbCount";
      this.tbCount.Size = new System.Drawing.Size(100, 20);
      this.tbCount.TabIndex = 6;
      this.tbCount.Text = "1000";
      // 
      // btnSendMany
      // 
      this.btnSendMany.Location = new System.Drawing.Point(142, 218);
      this.btnSendMany.Name = "btnSendMany";
      this.btnSendMany.Size = new System.Drawing.Size(81, 28);
      this.btnSendMany.TabIndex = 7;
      this.btnSendMany.Text = "Send Many";
      this.btnSendMany.UseVisualStyleBackColor = true;
      this.btnSendMany.Click += new System.EventHandler(this.btnSendMany_Click);
      // 
      // statusStrip1
      // 
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblProcessed});
      this.statusStrip1.Location = new System.Drawing.Point(0, 518);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size(661, 22);
      this.statusStrip1.TabIndex = 8;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // lblProcessed
      // 
      this.lblProcessed.Name = "lblProcessed";
      this.lblProcessed.Size = new System.Drawing.Size(72, 17);
      this.lblProcessed.Text = "Processed: 0";
      // 
      // tbCorrelationKey
      // 
      this.tbCorrelationKey.Location = new System.Drawing.Point(12, 295);
      this.tbCorrelationKey.Name = "tbCorrelationKey";
      this.tbCorrelationKey.Size = new System.Drawing.Size(100, 20);
      this.tbCorrelationKey.TabIndex = 10;
      this.tbCorrelationKey.Text = "AAA";
      // 
      // btnSendCorrelatedOne
      // 
      this.btnSendCorrelatedOne.Location = new System.Drawing.Point(12, 353);
      this.btnSendCorrelatedOne.Name = "btnSendCorrelatedOne";
      this.btnSendCorrelatedOne.Size = new System.Drawing.Size(81, 28);
      this.btnSendCorrelatedOne.TabIndex = 9;
      this.btnSendCorrelatedOne.Text = "Send One";
      this.btnSendCorrelatedOne.UseVisualStyleBackColor = true;
      this.btnSendCorrelatedOne.Click += new System.EventHandler(this.btnSendCorrelatedOne_Click);
      // 
      // tbCorrelationCounter
      // 
      this.tbCorrelationCounter.Location = new System.Drawing.Point(12, 321);
      this.tbCorrelationCounter.Name = "tbCorrelationCounter";
      this.tbCorrelationCounter.Size = new System.Drawing.Size(100, 20);
      this.tbCorrelationCounter.TabIndex = 11;
      this.tbCorrelationCounter.Text = "1";
      // 
      // btnSendEMail
      // 
      this.btnSendEMail.Location = new System.Drawing.Point(499, 353);
      this.btnSendEMail.Name = "btnSendEMail";
      this.btnSendEMail.Size = new System.Drawing.Size(81, 28);
      this.btnSendEMail.TabIndex = 13;
      this.btnSendEMail.Text = "Send Many";
      this.btnSendEMail.UseVisualStyleBackColor = true;
      this.btnSendEMail.Click += new System.EventHandler(this.btnSendEMail_Click);
      // 
      // tbWho
      // 
      this.tbWho.Location = new System.Drawing.Point(499, 319);
      this.tbWho.Name = "tbWho";
      this.tbWho.Size = new System.Drawing.Size(100, 20);
      this.tbWho.TabIndex = 12;
      this.tbWho.Text = "Ded pixto";
      // 
      // TodoForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(661, 540);
      this.Controls.Add(this.btnSendEMail);
      this.Controls.Add(this.tbWho);
      this.Controls.Add(this.tbCorrelationCounter);
      this.Controls.Add(this.tbCorrelationKey);
      this.Controls.Add(this.btnSendCorrelatedOne);
      this.Controls.Add(this.statusStrip1);
      this.Controls.Add(this.btnSendMany);
      this.Controls.Add(this.tbCount);
      this.Controls.Add(this.tbPersonDOB);
      this.Controls.Add(this.tbPersonName);
      this.Controls.Add(this.tbPersonID);
      this.Controls.Add(this.btnSendOne);
      this.Controls.Add(this.btnServerStop);
      this.Controls.Add(this.btnServerStart);
      this.Name = "TodoForm";
      this.Text = "TodoForm";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TodoForm_FormClosed);
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnServerStart;
    private System.Windows.Forms.Button btnServerStop;
    private System.Windows.Forms.Timer tmr;
    private System.Windows.Forms.Button btnSendOne;
    private System.Windows.Forms.TextBox tbPersonID;
    private System.Windows.Forms.TextBox tbPersonName;
    private System.Windows.Forms.TextBox tbPersonDOB;
    private System.Windows.Forms.TextBox tbCount;
    private System.Windows.Forms.Button btnSendMany;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripStatusLabel lblProcessed;
    private System.Windows.Forms.TextBox tbCorrelationKey;
    private System.Windows.Forms.Button btnSendCorrelatedOne;
    private System.Windows.Forms.TextBox tbCorrelationCounter;
    private System.Windows.Forms.Button btnSendEMail;
    private System.Windows.Forms.TextBox tbWho;
  }
}