/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
namespace WinFormsTestSky.IDGen
{
  partial class GDIDForm
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
      this.tbSequence = new System.Windows.Forms.TextBox();
      this.tbNamespace = new System.Windows.Forms.TextBox();
      this.btnGenerateOne = new System.Windows.Forms.Button();
      this.tbManyCount = new System.Windows.Forms.TextBox();
      this.btnMany = new System.Windows.Forms.Button();
      this.tbOutput = new System.Windows.Forms.TextBox();
      this.chkAuto = new System.Windows.Forms.CheckBox();
      this.tmrAuto = new System.Windows.Forms.Timer(this.components);
      this.SuspendLayout();
      // 
      // tbSequence
      // 
      this.tbSequence.Location = new System.Drawing.Point(36, 137);
      this.tbSequence.Name = "tbSequence";
      this.tbSequence.Size = new System.Drawing.Size(327, 20);
      this.tbSequence.TabIndex = 0;
      this.tbSequence.Text = "seq";
      // 
      // tbNamespace
      // 
      this.tbNamespace.Location = new System.Drawing.Point(36, 99);
      this.tbNamespace.Name = "tbNamespace";
      this.tbNamespace.Size = new System.Drawing.Size(327, 20);
      this.tbNamespace.TabIndex = 1;
      this.tbNamespace.Text = "ns";
      // 
      // btnGenerateOne
      // 
      this.btnGenerateOne.Location = new System.Drawing.Point(36, 181);
      this.btnGenerateOne.Name = "btnGenerateOne";
      this.btnGenerateOne.Size = new System.Drawing.Size(75, 37);
      this.btnGenerateOne.TabIndex = 2;
      this.btnGenerateOne.Text = "Gen One";
      this.btnGenerateOne.UseVisualStyleBackColor = true;
      this.btnGenerateOne.Click += new System.EventHandler(this.btnGenerateOne_Click);
      // 
      // tbManyCount
      // 
      this.tbManyCount.Location = new System.Drawing.Point(145, 188);
      this.tbManyCount.Name = "tbManyCount";
      this.tbManyCount.Size = new System.Drawing.Size(53, 20);
      this.tbManyCount.TabIndex = 3;
      this.tbManyCount.Text = "10";
      // 
      // btnMany
      // 
      this.btnMany.Location = new System.Drawing.Point(221, 179);
      this.btnMany.Name = "btnMany";
      this.btnMany.Size = new System.Drawing.Size(75, 37);
      this.btnMany.TabIndex = 4;
      this.btnMany.Text = "Gen Many";
      this.btnMany.UseVisualStyleBackColor = true;
      this.btnMany.Click += new System.EventHandler(this.btnMany_Click);
      // 
      // tbOutput
      // 
      this.tbOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbOutput.BackColor = System.Drawing.SystemColors.ControlDark;
      this.tbOutput.ForeColor = System.Drawing.Color.Yellow;
      this.tbOutput.Location = new System.Drawing.Point(640, 1);
      this.tbOutput.Multiline = true;
      this.tbOutput.Name = "tbOutput";
      this.tbOutput.ReadOnly = true;
      this.tbOutput.Size = new System.Drawing.Size(304, 434);
      this.tbOutput.TabIndex = 5;
      this.tbOutput.Text = "ns";
      // 
      // chkAuto
      // 
      this.chkAuto.AutoSize = true;
      this.chkAuto.Location = new System.Drawing.Point(57, 249);
      this.chkAuto.Name = "chkAuto";
      this.chkAuto.Size = new System.Drawing.Size(48, 17);
      this.chkAuto.TabIndex = 6;
      this.chkAuto.Text = "Auto";
      this.chkAuto.UseVisualStyleBackColor = true;
      this.chkAuto.CheckedChanged += new System.EventHandler(this.chkAuto_CheckedChanged);
      // 
      // tmrAuto
      // 
      this.tmrAuto.Tick += new System.EventHandler(this.tmrAuto_Tick);
      // 
      // GDIDForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(947, 436);
      this.Controls.Add(this.chkAuto);
      this.Controls.Add(this.tbOutput);
      this.Controls.Add(this.btnMany);
      this.Controls.Add(this.tbManyCount);
      this.Controls.Add(this.btnGenerateOne);
      this.Controls.Add(this.tbNamespace);
      this.Controls.Add(this.tbSequence);
      this.Name = "GDIDForm";
      this.Text = "GDIDForm";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GDIDForm_FormClosed);
      this.Load += new System.EventHandler(this.GDIDForm_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox tbSequence;
    private System.Windows.Forms.TextBox tbNamespace;
    private System.Windows.Forms.Button btnGenerateOne;
    private System.Windows.Forms.TextBox tbManyCount;
    private System.Windows.Forms.Button btnMany;
    private System.Windows.Forms.TextBox tbOutput;
    private System.Windows.Forms.CheckBox chkAuto;
    private System.Windows.Forms.Timer tmrAuto;
  }
}