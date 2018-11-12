namespace WinFormsTestSky.Workers
{
  partial class ProcessForm
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
      this.components = new System.ComponentModel.Container();
      this.tmr = new System.Windows.Forms.Timer(this.components);
      this.btnServerStart = new System.Windows.Forms.Button();
      this.btnServerStop = new System.Windows.Forms.Button();
      this.pnl = new System.Windows.Forms.Panel();
      this.lstProcess = new System.Windows.Forms.ListBox();
      this.btnSpawn = new System.Windows.Forms.Button();
      this.pnl.SuspendLayout();
      this.SuspendLayout();
      // 
      // tmr
      // 
      this.tmr.Enabled = true;
      this.tmr.Interval = 250;
      this.tmr.Tick += new System.EventHandler(this.tmr_Tick);
      // 
      // btnServerStart
      // 
      this.btnServerStart.AutoSize = true;
      this.btnServerStart.Location = new System.Drawing.Point(12, 12);
      this.btnServerStart.Name = "btnServerStart";
      this.btnServerStart.Size = new System.Drawing.Size(75, 27);
      this.btnServerStart.TabIndex = 0;
      this.btnServerStart.Text = "START";
      this.btnServerStart.UseVisualStyleBackColor = true;
      this.btnServerStart.Click += new System.EventHandler(this.btnServerStart_Click);
      // 
      // btnServerStop
      // 
      this.btnServerStop.AutoSize = true;
      this.btnServerStop.Location = new System.Drawing.Point(93, 12);
      this.btnServerStop.Name = "btnServerStop";
      this.btnServerStop.Size = new System.Drawing.Size(75, 27);
      this.btnServerStop.TabIndex = 1;
      this.btnServerStop.Text = "STOP";
      this.btnServerStop.UseVisualStyleBackColor = true;
      this.btnServerStop.Click += new System.EventHandler(this.btnServerStop_Click);
      // 
      // pnl
      // 
      this.pnl.Controls.Add(this.lstProcess);
      this.pnl.Controls.Add(this.btnSpawn);
      this.pnl.Location = new System.Drawing.Point(13, 45);
      this.pnl.Name = "pnl";
      this.pnl.Size = new System.Drawing.Size(757, 444);
      this.pnl.TabIndex = 2;
      // 
      // lstProcess
      // 
      this.lstProcess.FormattingEnabled = true;
      this.lstProcess.ItemHeight = 16;
      this.lstProcess.Location = new System.Drawing.Point(131, 4);
      this.lstProcess.Name = "lstProcess";
      this.lstProcess.Size = new System.Drawing.Size(623, 436);
      this.lstProcess.TabIndex = 1;
      // 
      // btnSpawn
      // 
      this.btnSpawn.AutoSize = true;
      this.btnSpawn.Location = new System.Drawing.Point(3, 3);
      this.btnSpawn.Name = "btnSpawn";
      this.btnSpawn.Size = new System.Drawing.Size(121, 27);
      this.btnSpawn.TabIndex = 0;
      this.btnSpawn.Text = "Spawn One";
      this.btnSpawn.UseVisualStyleBackColor = true;
      this.btnSpawn.Click += new System.EventHandler(this.btnSpawn_Click);
      // 
      // ProcessForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(782, 753);
      this.Controls.Add(this.pnl);
      this.Controls.Add(this.btnServerStop);
      this.Controls.Add(this.btnServerStart);
      this.Name = "ProcessForm";
      this.Text = "ProcessForm";
      this.pnl.ResumeLayout(false);
      this.pnl.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Timer tmr;
    private System.Windows.Forms.Button btnServerStart;
    private System.Windows.Forms.Button btnServerStop;
    private System.Windows.Forms.Panel pnl;
    private System.Windows.Forms.Button btnSpawn;
    private System.Windows.Forms.ListBox lstProcess;
  }
}