namespace WinFormsTest
{
  partial class EMAForm
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
      this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
      this.btnEMA = new System.Windows.Forms.Button();
      this.btn = new System.Windows.Forms.Button();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.tbFactor = new System.Windows.Forms.TextBox();
      this.btnCpu = new System.Windows.Forms.Button();
      this.btnRAM = new System.Windows.Forms.Button();
      this.chkInstant = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // hScrollBar1
      // 
      this.hScrollBar1.Location = new System.Drawing.Point(9, 420);
      this.hScrollBar1.Name = "hScrollBar1";
      this.hScrollBar1.Size = new System.Drawing.Size(782, 21);
      this.hScrollBar1.TabIndex = 0;
      this.hScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBar1_Scroll);
      // 
      // btnEMA
      // 
      this.btnEMA.Location = new System.Drawing.Point(297, 116);
      this.btnEMA.Name = "btnEMA";
      this.btnEMA.Size = new System.Drawing.Size(85, 36);
      this.btnEMA.TabIndex = 1;
      this.btnEMA.Text = "btn";
      this.btnEMA.UseVisualStyleBackColor = true;
      // 
      // btn
      // 
      this.btn.Location = new System.Drawing.Point(297, 168);
      this.btn.Name = "btn";
      this.btn.Size = new System.Drawing.Size(85, 36);
      this.btn.TabIndex = 2;
      this.btn.Text = "btn";
      this.btn.UseVisualStyleBackColor = true;
      // 
      // timer1
      // 
      this.timer1.Enabled = true;
      this.timer1.Interval = 500;
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // tbFactor
      // 
      this.tbFactor.Location = new System.Drawing.Point(492, 339);
      this.tbFactor.Name = "tbFactor";
      this.tbFactor.Size = new System.Drawing.Size(35, 25);
      this.tbFactor.TabIndex = 3;
      this.tbFactor.Text = "0.1";
      this.tbFactor.TextChanged += new System.EventHandler(this.tbFactor_TextChanged);
      // 
      // btnCpu
      // 
      this.btnCpu.Location = new System.Drawing.Point(297, 221);
      this.btnCpu.Name = "btnCpu";
      this.btnCpu.Size = new System.Drawing.Size(85, 36);
      this.btnCpu.TabIndex = 4;
      this.btnCpu.Text = "CPU";
      this.btnCpu.UseVisualStyleBackColor = true;
      // 
      // btnRAM
      // 
      this.btnRAM.Location = new System.Drawing.Point(297, 263);
      this.btnRAM.Name = "btnRAM";
      this.btnRAM.Size = new System.Drawing.Size(85, 33);
      this.btnRAM.TabIndex = 5;
      this.btnRAM.Text = "RAM";
      this.btnRAM.UseVisualStyleBackColor = true;
      // 
      // chkInstant
      // 
      this.chkInstant.AutoSize = true;
      this.chkInstant.Location = new System.Drawing.Point(662, 346);
      this.chkInstant.Name = "chkInstant";
      this.chkInstant.Size = new System.Drawing.Size(65, 21);
      this.chkInstant.TabIndex = 6;
      this.chkInstant.Text = "Instant";
      this.chkInstant.UseVisualStyleBackColor = true;
      // 
      // EMAForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(800, 450);
      this.Controls.Add(this.chkInstant);
      this.Controls.Add(this.btnRAM);
      this.Controls.Add(this.btnCpu);
      this.Controls.Add(this.tbFactor);
      this.Controls.Add(this.btn);
      this.Controls.Add(this.btnEMA);
      this.Controls.Add(this.hScrollBar1);
      this.Name = "EMAForm";
      this.Text = "EMAForm";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.HScrollBar hScrollBar1;
    private System.Windows.Forms.Button btnEMA;
    private System.Windows.Forms.Button btn;
    private System.Windows.Forms.Timer timer1;
    private System.Windows.Forms.TextBox tbFactor;
    private System.Windows.Forms.Button btnCpu;
    private System.Windows.Forms.Button btnRAM;
    private System.Windows.Forms.CheckBox chkInstant;
  }
}