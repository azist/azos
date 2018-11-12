namespace Agnivo
{
  partial class DecodedBSONForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DecodedBSONForm));
      this.DecodedBSONText = new System.Windows.Forms.RichTextBox();
      this.SuspendLayout();
      // 
      // DecodedBSONText
      // 
      this.DecodedBSONText.Dock = System.Windows.Forms.DockStyle.Fill;
      this.DecodedBSONText.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.DecodedBSONText.Location = new System.Drawing.Point(0, 0);
      this.DecodedBSONText.Name = "DecodedBSONText";
      this.DecodedBSONText.ReadOnly = true;
      this.DecodedBSONText.Size = new System.Drawing.Size(617, 405);
      this.DecodedBSONText.TabIndex = 0;
      this.DecodedBSONText.Text = "";
      this.DecodedBSONText.WordWrap = false;
      // 
      // DecodedBSONForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(617, 405);
      this.Controls.Add(this.DecodedBSONText);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MinimumSize = new System.Drawing.Size(500, 300);
      this.Name = "DecodedBSONForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "Result in JSON";
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.RichTextBox DecodedBSONText;
  }
}