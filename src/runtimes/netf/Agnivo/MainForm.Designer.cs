namespace Agnivo
{
  partial class MainForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.tabControl = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.statistics = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.DecodeBSONButton = new System.Windows.Forms.Button();
      this.SourceBSONText = new System.Windows.Forms.TextBox();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.ResultGDIDs = new System.Windows.Forms.TextBox();
      this.SourceGDID = new System.Windows.Forms.TextBox();
      this.tabControl.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl
      // 
      this.tabControl.Controls.Add(this.tabPage1);
      this.tabControl.Controls.Add(this.tabPage2);
      this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl.Location = new System.Drawing.Point(0, 0);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.Size = new System.Drawing.Size(594, 245);
      this.tabControl.TabIndex = 0;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.statistics);
      this.tabPage1.Controls.Add(this.label1);
      this.tabPage1.Controls.Add(this.DecodeBSONButton);
      this.tabPage1.Controls.Add(this.SourceBSONText);
      this.tabPage1.Location = new System.Drawing.Point(4, 25);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(586, 216);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "BSON";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // statistics
      // 
      this.statistics.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.statistics.AutoSize = true;
      this.statistics.Location = new System.Drawing.Point(9, 182);
      this.statistics.Name = "statistics";
      this.statistics.Size = new System.Drawing.Size(0, 16);
      this.statistics.TabIndex = 8;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(9, 12);
      this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(112, 16);
      this.label1.TabIndex = 7;
      this.label1.Text = "BSON in BASE64";
      // 
      // DecodeBSONButton
      // 
      this.DecodeBSONButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.DecodeBSONButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.DecodeBSONButton.Location = new System.Drawing.Point(438, 173);
      this.DecodeBSONButton.Margin = new System.Windows.Forms.Padding(4);
      this.DecodeBSONButton.Name = "DecodeBSONButton";
      this.DecodeBSONButton.Size = new System.Drawing.Size(139, 34);
      this.DecodeBSONButton.TabIndex = 6;
      this.DecodeBSONButton.Text = "Decode";
      this.DecodeBSONButton.UseVisualStyleBackColor = true;
      this.DecodeBSONButton.Click += new System.EventHandler(this.DecodeBSONButton_Click);
      // 
      // SourceBSONText
      // 
      this.SourceBSONText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.SourceBSONText.Location = new System.Drawing.Point(9, 35);
      this.SourceBSONText.Margin = new System.Windows.Forms.Padding(4);
      this.SourceBSONText.Multiline = true;
      this.SourceBSONText.Name = "SourceBSONText";
      this.SourceBSONText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.SourceBSONText.Size = new System.Drawing.Size(568, 130);
      this.SourceBSONText.TabIndex = 5;
      this.SourceBSONText.TextChanged += new System.EventHandler(this.SourceBSONText_TextChanged);
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.ResultGDIDs);
      this.tabPage2.Controls.Add(this.SourceGDID);
      this.tabPage2.Location = new System.Drawing.Point(4, 25);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(586, 216);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "GDID";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // ResultGDIDs
      // 
      this.ResultGDIDs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.ResultGDIDs.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.ResultGDIDs.Location = new System.Drawing.Point(9, 47);
      this.ResultGDIDs.Margin = new System.Windows.Forms.Padding(4);
      this.ResultGDIDs.Multiline = true;
      this.ResultGDIDs.Name = "ResultGDIDs";
      this.ResultGDIDs.ReadOnly = true;
      this.ResultGDIDs.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.ResultGDIDs.Size = new System.Drawing.Size(568, 160);
      this.ResultGDIDs.TabIndex = 9;
      // 
      // SourceGDID
      // 
      this.SourceGDID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.SourceGDID.Location = new System.Drawing.Point(9, 17);
      this.SourceGDID.Margin = new System.Windows.Forms.Padding(4);
      this.SourceGDID.Name = "SourceGDID";
      this.SourceGDID.Size = new System.Drawing.Size(568, 22);
      this.SourceGDID.TabIndex = 7;
      this.SourceGDID.TextChanged += new System.EventHandler(this.SourceGDID_TextChanged);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(594, 245);
      this.Controls.Add(this.tabControl);
      this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(4);
      this.MinimumSize = new System.Drawing.Size(520, 240);
      this.Name = "MainForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "Agnivo";
      this.tabControl.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.tabPage2.ResumeLayout(false);
      this.tabPage2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button DecodeBSONButton;
    private System.Windows.Forms.TextBox SourceBSONText;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.TextBox ResultGDIDs;
    private System.Windows.Forms.TextBox SourceGDID;
    private System.Windows.Forms.Label statistics;
  }
}

