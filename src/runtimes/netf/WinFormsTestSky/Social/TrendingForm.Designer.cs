namespace WinFormsTestSky.Social
{
  partial class TrendingForm
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
      this.btnServerStop = new System.Windows.Forms.Button();
      this.btnServerStart = new System.Windows.Forms.Button();
      this.btnRead = new System.Windows.Forms.Button();
      this.tmr = new System.Windows.Forms.Timer(this.components);
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.cbUserEntity = new System.Windows.Forms.CheckBox();
      this.cbProductEntity = new System.Windows.Forms.CheckBox();
      this.tbAge = new System.Windows.Forms.TextBox();
      this.tbSex = new System.Windows.Forms.TextBox();
      this.tbCountry = new System.Windows.Forms.TextBox();
      this.tbSize = new System.Windows.Forms.TextBox();
      this.tbColor = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.tbUserValue = new System.Windows.Forms.TextBox();
      this.label8 = new System.Windows.Forms.Label();
      this.tbProductValue = new System.Windows.Forms.TextBox();
      this.label9 = new System.Windows.Forms.Label();
      this.bStartSend = new System.Windows.Forms.Button();
      this.tmrSend = new System.Windows.Forms.Timer(this.components);
      this.dtpSB = new System.Windows.Forms.DateTimePicker();
      this.dtpED = new System.Windows.Forms.DateTimePicker();
      this.nCol = new System.Windows.Forms.NumericUpDown();
      this.tmrMain = new System.Windows.Forms.Timer(this.components);
      this.btnFilter = new System.Windows.Forms.Button();
      this.tbfAge = new System.Windows.Forms.TextBox();
      this.tbfSex = new System.Windows.Forms.TextBox();
      this.tbfCountry = new System.Windows.Forms.TextBox();
      this.tbfSize = new System.Windows.Forms.TextBox();
      this.tbfColor = new System.Windows.Forms.TextBox();
      ((System.ComponentModel.ISupportInitialize)(this.nCol)).BeginInit();
      this.SuspendLayout();
      // 
      // btnServerStop
      // 
      this.btnServerStop.Location = new System.Drawing.Point(91, 11);
      this.btnServerStop.Name = "btnServerStop";
      this.btnServerStop.Size = new System.Drawing.Size(75, 23);
      this.btnServerStop.TabIndex = 3;
      this.btnServerStop.Text = "STOP";
      this.btnServerStop.UseVisualStyleBackColor = true;
      this.btnServerStop.Click += new System.EventHandler(this.btnServerStop_Click);
      // 
      // btnServerStart
      // 
      this.btnServerStart.Location = new System.Drawing.Point(10, 11);
      this.btnServerStart.Name = "btnServerStart";
      this.btnServerStart.Size = new System.Drawing.Size(75, 23);
      this.btnServerStart.TabIndex = 2;
      this.btnServerStart.Text = "START";
      this.btnServerStart.UseVisualStyleBackColor = true;
      this.btnServerStart.Click += new System.EventHandler(this.btnServerStart_Click);
      // 
      // btnRead
      // 
      this.btnRead.Location = new System.Drawing.Point(13, 400);
      this.btnRead.Margin = new System.Windows.Forms.Padding(2);
      this.btnRead.Name = "btnRead";
      this.btnRead.Size = new System.Drawing.Size(75, 23);
      this.btnRead.TabIndex = 28;
      this.btnRead.Text = "READ";
      this.btnRead.UseVisualStyleBackColor = true;
      this.btnRead.Click += new System.EventHandler(this.btnCheckTrendings_Click);
      // 
      // tmr
      // 
      this.tmr.Enabled = true;
      this.tmr.Interval = 250;
      this.tmr.Tick += new System.EventHandler(this.tmr_Tick);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(10, 359);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(30, 13);
      this.label1.TabIndex = 7;
      this.label1.Text = "From";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(10, 376);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(20, 13);
      this.label2.TabIndex = 8;
      this.label2.Text = "To";
      // 
      // cbUserEntity
      // 
      this.cbUserEntity.AutoSize = true;
      this.cbUserEntity.Location = new System.Drawing.Point(13, 41);
      this.cbUserEntity.Name = "cbUserEntity";
      this.cbUserEntity.Size = new System.Drawing.Size(98, 17);
      this.cbUserEntity.TabIndex = 9;
      this.cbUserEntity.Text = "USER ENTITY";
      this.cbUserEntity.UseVisualStyleBackColor = true;
      // 
      // cbProductEntity
      // 
      this.cbProductEntity.AutoSize = true;
      this.cbProductEntity.Location = new System.Drawing.Point(13, 172);
      this.cbProductEntity.Name = "cbProductEntity";
      this.cbProductEntity.Size = new System.Drawing.Size(121, 17);
      this.cbProductEntity.TabIndex = 14;
      this.cbProductEntity.Text = "PRODUCT ENTITY";
      this.cbProductEntity.UseVisualStyleBackColor = true;
      // 
      // tbAge
      // 
      this.tbAge.Location = new System.Drawing.Point(13, 65);
      this.tbAge.Name = "tbAge";
      this.tbAge.Size = new System.Drawing.Size(100, 20);
      this.tbAge.TabIndex = 10;
      // 
      // tbSex
      // 
      this.tbSex.Location = new System.Drawing.Point(13, 92);
      this.tbSex.Name = "tbSex";
      this.tbSex.Size = new System.Drawing.Size(100, 20);
      this.tbSex.TabIndex = 11;
      // 
      // tbCountry
      // 
      this.tbCountry.Location = new System.Drawing.Point(13, 119);
      this.tbCountry.Name = "tbCountry";
      this.tbCountry.Size = new System.Drawing.Size(100, 20);
      this.tbCountry.TabIndex = 12;
      // 
      // tbSize
      // 
      this.tbSize.Location = new System.Drawing.Point(13, 196);
      this.tbSize.Name = "tbSize";
      this.tbSize.Size = new System.Drawing.Size(100, 20);
      this.tbSize.TabIndex = 15;
      // 
      // tbColor
      // 
      this.tbColor.Location = new System.Drawing.Point(13, 223);
      this.tbColor.Name = "tbColor";
      this.tbColor.Size = new System.Drawing.Size(100, 20);
      this.tbColor.TabIndex = 16;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(119, 65);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(26, 13);
      this.label3.TabIndex = 16;
      this.label3.Text = "Age";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(120, 92);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(25, 13);
      this.label4.TabIndex = 17;
      this.label4.Text = "Sex";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(120, 119);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(43, 13);
      this.label5.TabIndex = 18;
      this.label5.Text = "Country";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(120, 196);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(27, 13);
      this.label6.TabIndex = 19;
      this.label6.Text = "Szie";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(120, 223);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(31, 13);
      this.label7.TabIndex = 20;
      this.label7.Text = "Color";
      // 
      // tbUserValue
      // 
      this.tbUserValue.Location = new System.Drawing.Point(13, 146);
      this.tbUserValue.Name = "tbUserValue";
      this.tbUserValue.Size = new System.Drawing.Size(100, 20);
      this.tbUserValue.TabIndex = 13;
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(120, 146);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(34, 13);
      this.label8.TabIndex = 22;
      this.label8.Text = "Value";
      // 
      // tbProductValue
      // 
      this.tbProductValue.Location = new System.Drawing.Point(13, 250);
      this.tbProductValue.Name = "tbProductValue";
      this.tbProductValue.Size = new System.Drawing.Size(100, 20);
      this.tbProductValue.TabIndex = 17;
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(120, 250);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(34, 13);
      this.label9.TabIndex = 24;
      this.label9.Text = "Value";
      // 
      // bStartSend
      // 
      this.bStartSend.Location = new System.Drawing.Point(9, 277);
      this.bStartSend.Name = "bStartSend";
      this.bStartSend.Size = new System.Drawing.Size(104, 23);
      this.bStartSend.TabIndex = 18;
      this.bStartSend.Text = "START SEND";
      this.bStartSend.UseVisualStyleBackColor = true;
      this.bStartSend.Click += new System.EventHandler(this.bStartSend_Click);
      // 
      // tmrSend
      // 
      this.tmrSend.Enabled = true;
      this.tmrSend.Interval = 1000;
      this.tmrSend.Tick += new System.EventHandler(this.tmrSend_Tick);
      // 
      // dtpSB
      // 
      this.dtpSB.Location = new System.Drawing.Point(46, 353);
      this.dtpSB.Name = "dtpSB";
      this.dtpSB.Size = new System.Drawing.Size(200, 20);
      this.dtpSB.TabIndex = 26;
      // 
      // dtpED
      // 
      this.dtpED.Location = new System.Drawing.Point(46, 375);
      this.dtpED.Name = "dtpED";
      this.dtpED.Size = new System.Drawing.Size(200, 20);
      this.dtpED.TabIndex = 27;
      // 
      // nCol
      // 
      this.nCol.Location = new System.Drawing.Point(46, 327);
      this.nCol.Name = "nCol";
      this.nCol.Size = new System.Drawing.Size(120, 20);
      this.nCol.TabIndex = 25;
      this.nCol.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
      // 
      // tmrMain
      // 
      this.tmrMain.Enabled = true;
      this.tmrMain.Interval = 250;
      this.tmrMain.Tick += new System.EventHandler(this.tmrMain_Tick);
      // 
      // btnFilter
      // 
      this.btnFilter.Location = new System.Drawing.Point(160, 277);
      this.btnFilter.Name = "btnFilter";
      this.btnFilter.Size = new System.Drawing.Size(100, 23);
      this.btnFilter.TabIndex = 29;
      this.btnFilter.Text = "FILTER";
      this.btnFilter.UseVisualStyleBackColor = true;
      this.btnFilter.Click += new System.EventHandler(this.btnFilter_Click);
      // 
      // tbfAge
      // 
      this.tbfAge.Location = new System.Drawing.Point(160, 64);
      this.tbfAge.Name = "tbfAge";
      this.tbfAge.Size = new System.Drawing.Size(100, 20);
      this.tbfAge.TabIndex = 30;
      // 
      // tbfSex
      // 
      this.tbfSex.Location = new System.Drawing.Point(160, 91);
      this.tbfSex.Name = "tbfSex";
      this.tbfSex.Size = new System.Drawing.Size(100, 20);
      this.tbfSex.TabIndex = 31;
      // 
      // tbfCountry
      // 
      this.tbfCountry.Location = new System.Drawing.Point(160, 119);
      this.tbfCountry.Name = "tbfCountry";
      this.tbfCountry.Size = new System.Drawing.Size(100, 20);
      this.tbfCountry.TabIndex = 32;
      // 
      // tbfSize
      // 
      this.tbfSize.Location = new System.Drawing.Point(160, 196);
      this.tbfSize.Name = "tbfSize";
      this.tbfSize.Size = new System.Drawing.Size(100, 20);
      this.tbfSize.TabIndex = 34;
      // 
      // tbfColor
      // 
      this.tbfColor.Location = new System.Drawing.Point(160, 223);
      this.tbfColor.Name = "tbfColor";
      this.tbfColor.Size = new System.Drawing.Size(100, 20);
      this.tbfColor.TabIndex = 35;
      // 
      // TrendingForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(453, 485);
      this.Controls.Add(this.tbfColor);
      this.Controls.Add(this.tbfSize);
      this.Controls.Add(this.tbfCountry);
      this.Controls.Add(this.tbfSex);
      this.Controls.Add(this.tbfAge);
      this.Controls.Add(this.btnFilter);
      this.Controls.Add(this.nCol);
      this.Controls.Add(this.dtpED);
      this.Controls.Add(this.dtpSB);
      this.Controls.Add(this.bStartSend);
      this.Controls.Add(this.label9);
      this.Controls.Add(this.tbProductValue);
      this.Controls.Add(this.label8);
      this.Controls.Add(this.tbUserValue);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.tbColor);
      this.Controls.Add(this.tbSize);
      this.Controls.Add(this.tbCountry);
      this.Controls.Add(this.tbSex);
      this.Controls.Add(this.tbAge);
      this.Controls.Add(this.cbProductEntity);
      this.Controls.Add(this.cbUserEntity);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.btnRead);
      this.Controls.Add(this.btnServerStop);
      this.Controls.Add(this.btnServerStart);
      this.Margin = new System.Windows.Forms.Padding(2);
      this.Name = "TrendingForm";
      this.Text = "TrendingForm";
      ((System.ComponentModel.ISupportInitialize)(this.nCol)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnServerStop;
    private System.Windows.Forms.Button btnServerStart;
    private System.Windows.Forms.Button btnRead;
    private System.Windows.Forms.Timer tmr;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.CheckBox cbUserEntity;
    private System.Windows.Forms.CheckBox cbProductEntity;
    private System.Windows.Forms.TextBox tbAge;
    private System.Windows.Forms.TextBox tbSex;
    private System.Windows.Forms.TextBox tbCountry;
    private System.Windows.Forms.TextBox tbSize;
    private System.Windows.Forms.TextBox tbColor;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.TextBox tbUserValue;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.TextBox tbProductValue;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Button bStartSend;
    private System.Windows.Forms.Timer tmrSend;
    private System.Windows.Forms.DateTimePicker dtpSB;
    private System.Windows.Forms.DateTimePicker dtpED;
    private System.Windows.Forms.NumericUpDown nCol;
    private System.Windows.Forms.Timer tmrMain;
    private System.Windows.Forms.Button btnFilter;
    private System.Windows.Forms.TextBox tbfAge;
    private System.Windows.Forms.TextBox tbfSex;
    private System.Windows.Forms.TextBox tbfCountry;
    private System.Windows.Forms.TextBox tbfSize;
    private System.Windows.Forms.TextBox tbfColor;
  }
}