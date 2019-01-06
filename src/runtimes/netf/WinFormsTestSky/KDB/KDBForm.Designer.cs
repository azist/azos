/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
namespace WinFormsTestSky.KDB
{
  partial class KDBForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KDBForm));
      this.tbConfig = new System.Windows.Forms.TextBox();
      this.btnMount = new System.Windows.Forms.Button();
      this.btnUnmount = new System.Windows.Forms.Button();
      this.tmrReactor = new System.Windows.Forms.Timer(this.components);
      this.btnPut = new System.Windows.Forms.Button();
      this.btnGet = new System.Windows.Forms.Button();
      this.btnDelete = new System.Windows.Forms.Button();
      this.tbPutCount = new System.Windows.Forms.TextBox();
      this.tbGetCount = new System.Windows.Forms.TextBox();
      this.tbDeleteCount = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // tbConfig
      // 
      this.tbConfig.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbConfig.Location = new System.Drawing.Point(12, 12);
      this.tbConfig.Multiline = true;
      this.tbConfig.Name = "tbConfig";
      this.tbConfig.Size = new System.Drawing.Size(938, 414);
      this.tbConfig.TabIndex = 0;
      this.tbConfig.Text = resources.GetString("tbConfig.Text");
      // 
      // btnMount
      // 
      this.btnMount.Location = new System.Drawing.Point(12, 448);
      this.btnMount.Name = "btnMount";
      this.btnMount.Size = new System.Drawing.Size(75, 23);
      this.btnMount.TabIndex = 1;
      this.btnMount.Text = "Mount";
      this.btnMount.UseVisualStyleBackColor = true;
      this.btnMount.Click += new System.EventHandler(this.btnMount_Click);
      // 
      // btnUnmount
      // 
      this.btnUnmount.Location = new System.Drawing.Point(13, 478);
      this.btnUnmount.Name = "btnUnmount";
      this.btnUnmount.Size = new System.Drawing.Size(75, 23);
      this.btnUnmount.TabIndex = 2;
      this.btnUnmount.Text = "Unmount";
      this.btnUnmount.UseVisualStyleBackColor = true;
      this.btnUnmount.Click += new System.EventHandler(this.btnUnmount_Click);
      // 
      // tmrReactor
      // 
      this.tmrReactor.Enabled = true;
      this.tmrReactor.Tick += new System.EventHandler(this.tmrReactor_Tick);
      // 
      // btnPut
      // 
      this.btnPut.Location = new System.Drawing.Point(633, 448);
      this.btnPut.Name = "btnPut";
      this.btnPut.Size = new System.Drawing.Size(75, 23);
      this.btnPut.TabIndex = 3;
      this.btnPut.Text = "Put";
      this.btnPut.UseVisualStyleBackColor = true;
      this.btnPut.Click += new System.EventHandler(this.btnPut_Click);
      // 
      // btnGet
      // 
      this.btnGet.Location = new System.Drawing.Point(633, 478);
      this.btnGet.Name = "btnGet";
      this.btnGet.Size = new System.Drawing.Size(75, 23);
      this.btnGet.TabIndex = 4;
      this.btnGet.Text = "Get";
      this.btnGet.UseVisualStyleBackColor = true;
      this.btnGet.Click += new System.EventHandler(this.btnGet_Click);
      // 
      // btnDelete
      // 
      this.btnDelete.Location = new System.Drawing.Point(633, 508);
      this.btnDelete.Name = "btnDelete";
      this.btnDelete.Size = new System.Drawing.Size(75, 23);
      this.btnDelete.TabIndex = 5;
      this.btnDelete.Text = "Delete";
      this.btnDelete.UseVisualStyleBackColor = true;
      this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
      // 
      // tbPutCount
      // 
      this.tbPutCount.Location = new System.Drawing.Point(715, 450);
      this.tbPutCount.Name = "tbPutCount";
      this.tbPutCount.Size = new System.Drawing.Size(100, 20);
      this.tbPutCount.TabIndex = 6;
      this.tbPutCount.Text = "100";
      // 
      // tbGetCount
      // 
      this.tbGetCount.Location = new System.Drawing.Point(715, 480);
      this.tbGetCount.Name = "tbGetCount";
      this.tbGetCount.Size = new System.Drawing.Size(100, 20);
      this.tbGetCount.TabIndex = 7;
      this.tbGetCount.Text = "100";
      // 
      // tbDeleteCount
      // 
      this.tbDeleteCount.Location = new System.Drawing.Point(715, 507);
      this.tbDeleteCount.Name = "tbDeleteCount";
      this.tbDeleteCount.Size = new System.Drawing.Size(100, 20);
      this.tbDeleteCount.TabIndex = 8;
      this.tbDeleteCount.Text = "100";
      // 
      // KDBForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(962, 551);
      this.Controls.Add(this.tbDeleteCount);
      this.Controls.Add(this.tbGetCount);
      this.Controls.Add(this.tbPutCount);
      this.Controls.Add(this.btnDelete);
      this.Controls.Add(this.btnGet);
      this.Controls.Add(this.btnPut);
      this.Controls.Add(this.btnUnmount);
      this.Controls.Add(this.btnMount);
      this.Controls.Add(this.tbConfig);
      this.Name = "KDBForm";
      this.Text = "KDBForm";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox tbConfig;
    private System.Windows.Forms.Button btnMount;
    private System.Windows.Forms.Button btnUnmount;
    private System.Windows.Forms.Timer tmrReactor;
    private System.Windows.Forms.Button btnPut;
    private System.Windows.Forms.Button btnGet;
    private System.Windows.Forms.Button btnDelete;
    private System.Windows.Forms.TextBox tbPutCount;
    private System.Windows.Forms.TextBox tbGetCount;
    private System.Windows.Forms.TextBox tbDeleteCount;
  }
}