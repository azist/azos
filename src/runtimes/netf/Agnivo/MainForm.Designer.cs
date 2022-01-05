/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
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
      this.tamMisc = new System.Windows.Forms.TabControl();
      this.tabBson = new System.Windows.Forms.TabPage();
      this.label1 = new System.Windows.Forms.Label();
      this.DecodeBSONButton = new System.Windows.Forms.Button();
      this.SourceBSONText = new System.Windows.Forms.TextBox();
      this.tabGdid = new System.Windows.Forms.TabPage();
      this.ResultGDIDs = new System.Windows.Forms.TextBox();
      this.SourceGDID = new System.Windows.Forms.TextBox();
      this.tabJson = new System.Windows.Forms.TabPage();
      this.label2 = new System.Windows.Forms.Label();
      this.btnBase64Json = new System.Windows.Forms.Button();
      this.tbBase64Json = new System.Windows.Forms.TextBox();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.label3 = new System.Windows.Forms.Label();
      this.tbAtomValue = new System.Windows.Forms.TextBox();
      this.tbAtomNumber = new System.Windows.Forms.TextBox();
      this.tamMisc.SuspendLayout();
      this.tabBson.SuspendLayout();
      this.tabGdid.SuspendLayout();
      this.tabJson.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tamMisc
      // 
      this.tamMisc.Controls.Add(this.tabBson);
      this.tamMisc.Controls.Add(this.tabGdid);
      this.tamMisc.Controls.Add(this.tabJson);
      this.tamMisc.Controls.Add(this.tabPage1);
      this.tamMisc.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tamMisc.Location = new System.Drawing.Point(0, 0);
      this.tamMisc.Name = "tamMisc";
      this.tamMisc.SelectedIndex = 0;
      this.tamMisc.Size = new System.Drawing.Size(594, 245);
      this.tamMisc.TabIndex = 0;
      // 
      // tabBson
      // 
      this.tabBson.Controls.Add(this.label1);
      this.tabBson.Controls.Add(this.DecodeBSONButton);
      this.tabBson.Controls.Add(this.SourceBSONText);
      this.tabBson.Location = new System.Drawing.Point(4, 26);
      this.tabBson.Name = "tabBson";
      this.tabBson.Padding = new System.Windows.Forms.Padding(3);
      this.tabBson.Size = new System.Drawing.Size(586, 215);
      this.tabBson.TabIndex = 0;
      this.tabBson.Text = "BSON";
      this.tabBson.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(9, 12);
      this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(125, 18);
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
      // tabGdid
      // 
      this.tabGdid.Controls.Add(this.ResultGDIDs);
      this.tabGdid.Controls.Add(this.SourceGDID);
      this.tabGdid.Location = new System.Drawing.Point(4, 26);
      this.tabGdid.Name = "tabGdid";
      this.tabGdid.Padding = new System.Windows.Forms.Padding(3);
      this.tabGdid.Size = new System.Drawing.Size(586, 215);
      this.tabGdid.TabIndex = 1;
      this.tabGdid.Text = "GDID";
      this.tabGdid.UseVisualStyleBackColor = true;
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
      this.SourceGDID.Size = new System.Drawing.Size(568, 24);
      this.SourceGDID.TabIndex = 7;
      this.SourceGDID.TextChanged += new System.EventHandler(this.SourceGDID_TextChanged);
      // 
      // tabJson
      // 
      this.tabJson.Controls.Add(this.label2);
      this.tabJson.Controls.Add(this.btnBase64Json);
      this.tabJson.Controls.Add(this.tbBase64Json);
      this.tabJson.Location = new System.Drawing.Point(4, 26);
      this.tabJson.Name = "tabJson";
      this.tabJson.Padding = new System.Windows.Forms.Padding(3);
      this.tabJson.Size = new System.Drawing.Size(586, 215);
      this.tabJson.TabIndex = 2;
      this.tabJson.Text = "Base64 Json";
      this.tabJson.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(9, 10);
      this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(123, 18);
      this.label2.TabIndex = 10;
      this.label2.Text = "JSON in BASE64";
      // 
      // btnBase64Json
      // 
      this.btnBase64Json.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnBase64Json.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnBase64Json.Location = new System.Drawing.Point(438, 171);
      this.btnBase64Json.Margin = new System.Windows.Forms.Padding(4);
      this.btnBase64Json.Name = "btnBase64Json";
      this.btnBase64Json.Size = new System.Drawing.Size(139, 34);
      this.btnBase64Json.TabIndex = 9;
      this.btnBase64Json.Text = "Decode";
      this.btnBase64Json.UseVisualStyleBackColor = true;
      this.btnBase64Json.Click += new System.EventHandler(this.btnBase64Json_Click);
      // 
      // tbBase64Json
      // 
      this.tbBase64Json.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbBase64Json.Location = new System.Drawing.Point(9, 33);
      this.tbBase64Json.Margin = new System.Windows.Forms.Padding(4);
      this.tbBase64Json.Multiline = true;
      this.tbBase64Json.Name = "tbBase64Json";
      this.tbBase64Json.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.tbBase64Json.Size = new System.Drawing.Size(568, 130);
      this.tbBase64Json.TabIndex = 8;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.label3);
      this.tabPage1.Controls.Add(this.tbAtomValue);
      this.tabPage1.Controls.Add(this.tbAtomNumber);
      this.tabPage1.Location = new System.Drawing.Point(4, 26);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(586, 215);
      this.tabPage1.TabIndex = 3;
      this.tabPage1.Text = "Misc";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(8, 17);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(51, 18);
      this.label3.TabIndex = 2;
      this.label3.Text = "ATOM";
      // 
      // tbAtomValue
      // 
      this.tbAtomValue.Location = new System.Drawing.Point(252, 14);
      this.tbAtomValue.Multiline = true;
      this.tbAtomValue.Name = "tbAtomValue";
      this.tbAtomValue.Size = new System.Drawing.Size(292, 163);
      this.tbAtomValue.TabIndex = 1;
      // 
      // tbAtomNumber
      // 
      this.tbAtomNumber.Location = new System.Drawing.Point(74, 14);
      this.tbAtomNumber.Name = "tbAtomNumber";
      this.tbAtomNumber.Size = new System.Drawing.Size(152, 24);
      this.tbAtomNumber.TabIndex = 0;
      this.tbAtomNumber.Text = "000000";
      this.tbAtomNumber.TextChanged += new System.EventHandler(this.tbAtomNumber_TextChanged);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(594, 245);
      this.Controls.Add(this.tamMisc);
      this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(4);
      this.MinimumSize = new System.Drawing.Size(520, 240);
      this.Name = "MainForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "Agnivo";
      this.tamMisc.ResumeLayout(false);
      this.tabBson.ResumeLayout(false);
      this.tabBson.PerformLayout();
      this.tabGdid.ResumeLayout(false);
      this.tabGdid.PerformLayout();
      this.tabJson.ResumeLayout(false);
      this.tabJson.PerformLayout();
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl tamMisc;
    private System.Windows.Forms.TabPage tabBson;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button DecodeBSONButton;
    private System.Windows.Forms.TextBox SourceBSONText;
    private System.Windows.Forms.TabPage tabGdid;
    private System.Windows.Forms.TextBox ResultGDIDs;
    private System.Windows.Forms.TextBox SourceGDID;
    private System.Windows.Forms.Label statistics;
    private System.Windows.Forms.TabPage tabJson;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button btnBase64Json;
    private System.Windows.Forms.TextBox tbBase64Json;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox tbAtomValue;
    private System.Windows.Forms.TextBox tbAtomNumber;
  }
}

