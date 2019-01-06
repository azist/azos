/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
namespace WinFormsTestSky.Caching
{
  partial class CacheForm
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
      this.tmrStatus = new System.Windows.Forms.Timer(this.components);
      this.tbCount = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.tbKeyStart = new System.Windows.Forms.TextBox();
      this.chkAutoGet = new System.Windows.Forms.CheckBox();
      this.chkAutoPut = new System.Windows.Forms.CheckBox();
      this.chkAutoRemove = new System.Windows.Forms.CheckBox();
      this.tbAutoGet = new System.Windows.Forms.TextBox();
      this.tbAutoPut = new System.Windows.Forms.TextBox();
      this.tbAutoRemove = new System.Windows.Forms.TextBox();
      this.btnPut = new System.Windows.Forms.Button();
      this.btnGet = new System.Windows.Forms.Button();
      this.btnRemove = new System.Windows.Forms.Button();
      this.label3 = new System.Windows.Forms.Label();
      this.tbTable = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.tbParallel = new System.Windows.Forms.TextBox();
      this.btnGC = new System.Windows.Forms.Button();
      this.label5 = new System.Windows.Forms.Label();
      this.tbAbsoluteExpiration = new System.Windows.Forms.TextBox();
      this.label6 = new System.Windows.Forms.Label();
      this.tbMaxCapacity = new System.Windows.Forms.TextBox();
      this.chkAutoRandKey = new System.Windows.Forms.CheckBox();
      this.lblAutoStatus = new System.Windows.Forms.Label();
      this.chkAutoRandTbl = new System.Windows.Forms.CheckBox();
      this.tbBinFrom = new System.Windows.Forms.TextBox();
      this.tbBinTo = new System.Windows.Forms.TextBox();
      this.label7 = new System.Windows.Forms.Label();
      this.tbMaxAgeSec = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // tmrStatus
      // 
      this.tmrStatus.Enabled = true;
      this.tmrStatus.Interval = 1000;
      this.tmrStatus.Tick += new System.EventHandler(this.tmrStatus_Tick);
      // 
      // tbCount
      // 
      this.tbCount.Location = new System.Drawing.Point(176, 41);
      this.tbCount.Name = "tbCount";
      this.tbCount.Size = new System.Drawing.Size(78, 20);
      this.tbCount.TabIndex = 0;
      this.tbCount.Text = "1000";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(176, 25);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(35, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Count";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(313, 25);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(50, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "Key Start";
      // 
      // tbKeyStart
      // 
      this.tbKeyStart.Location = new System.Drawing.Point(316, 41);
      this.tbKeyStart.Name = "tbKeyStart";
      this.tbKeyStart.Size = new System.Drawing.Size(78, 20);
      this.tbKeyStart.TabIndex = 2;
      this.tbKeyStart.Text = "0";
      // 
      // chkAutoGet
      // 
      this.chkAutoGet.AutoSize = true;
      this.chkAutoGet.Location = new System.Drawing.Point(20, 226);
      this.chkAutoGet.Name = "chkAutoGet";
      this.chkAutoGet.Size = new System.Drawing.Size(68, 17);
      this.chkAutoGet.TabIndex = 4;
      this.chkAutoGet.Text = "Auto Get";
      this.chkAutoGet.UseVisualStyleBackColor = true;
      // 
      // chkAutoPut
      // 
      this.chkAutoPut.AutoSize = true;
      this.chkAutoPut.Location = new System.Drawing.Point(94, 226);
      this.chkAutoPut.Name = "chkAutoPut";
      this.chkAutoPut.Size = new System.Drawing.Size(67, 17);
      this.chkAutoPut.TabIndex = 5;
      this.chkAutoPut.Text = "Auto Put";
      this.chkAutoPut.UseVisualStyleBackColor = true;
      // 
      // chkAutoRemove
      // 
      this.chkAutoRemove.AutoSize = true;
      this.chkAutoRemove.Location = new System.Drawing.Point(167, 226);
      this.chkAutoRemove.Name = "chkAutoRemove";
      this.chkAutoRemove.Size = new System.Drawing.Size(91, 17);
      this.chkAutoRemove.TabIndex = 6;
      this.chkAutoRemove.Text = "Auto Remove";
      this.chkAutoRemove.UseVisualStyleBackColor = true;
      // 
      // tbAutoGet
      // 
      this.tbAutoGet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
      this.tbAutoGet.Location = new System.Drawing.Point(20, 249);
      this.tbAutoGet.Name = "tbAutoGet";
      this.tbAutoGet.Size = new System.Drawing.Size(46, 20);
      this.tbAutoGet.TabIndex = 7;
      this.tbAutoGet.Text = "10";
      // 
      // tbAutoPut
      // 
      this.tbAutoPut.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
      this.tbAutoPut.Location = new System.Drawing.Point(94, 249);
      this.tbAutoPut.Name = "tbAutoPut";
      this.tbAutoPut.Size = new System.Drawing.Size(46, 20);
      this.tbAutoPut.TabIndex = 8;
      this.tbAutoPut.Text = "10";
      // 
      // tbAutoRemove
      // 
      this.tbAutoRemove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
      this.tbAutoRemove.Location = new System.Drawing.Point(167, 249);
      this.tbAutoRemove.Name = "tbAutoRemove";
      this.tbAutoRemove.Size = new System.Drawing.Size(46, 20);
      this.tbAutoRemove.TabIndex = 9;
      this.tbAutoRemove.Text = "2";
      // 
      // btnPut
      // 
      this.btnPut.Location = new System.Drawing.Point(111, 112);
      this.btnPut.Name = "btnPut";
      this.btnPut.Size = new System.Drawing.Size(104, 36);
      this.btnPut.TabIndex = 10;
      this.btnPut.Text = "Put";
      this.btnPut.UseVisualStyleBackColor = true;
      this.btnPut.Click += new System.EventHandler(this.btnPut_Click);
      // 
      // btnGet
      // 
      this.btnGet.Location = new System.Drawing.Point(222, 112);
      this.btnGet.Name = "btnGet";
      this.btnGet.Size = new System.Drawing.Size(104, 36);
      this.btnGet.TabIndex = 11;
      this.btnGet.Text = "Get";
      this.btnGet.UseVisualStyleBackColor = true;
      this.btnGet.Click += new System.EventHandler(this.btnGet_Click);
      // 
      // btnRemove
      // 
      this.btnRemove.Location = new System.Drawing.Point(332, 112);
      this.btnRemove.Name = "btnRemove";
      this.btnRemove.Size = new System.Drawing.Size(104, 36);
      this.btnRemove.TabIndex = 12;
      this.btnRemove.Text = "Remove";
      this.btnRemove.UseVisualStyleBackColor = true;
      this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(19, 25);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(34, 13);
      this.label3.TabIndex = 14;
      this.label3.Text = "Table";
      // 
      // tbTable
      // 
      this.tbTable.Location = new System.Drawing.Point(22, 41);
      this.tbTable.Name = "tbTable";
      this.tbTable.Size = new System.Drawing.Size(78, 20);
      this.tbTable.TabIndex = 13;
      this.tbTable.Text = "table1";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(131, 25);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(43, 13);
      this.label4.TabIndex = 16;
      this.label4.Text = "Paralllel";
      // 
      // tbParallel
      // 
      this.tbParallel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
      this.tbParallel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbParallel.Location = new System.Drawing.Point(138, 41);
      this.tbParallel.Name = "tbParallel";
      this.tbParallel.Size = new System.Drawing.Size(32, 20);
      this.tbParallel.TabIndex = 15;
      this.tbParallel.Text = "4";
      this.tbParallel.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // btnGC
      // 
      this.btnGC.Location = new System.Drawing.Point(595, 175);
      this.btnGC.Name = "btnGC";
      this.btnGC.Size = new System.Drawing.Size(104, 36);
      this.btnGC.TabIndex = 17;
      this.btnGC.Text = "GC";
      this.btnGC.UseVisualStyleBackColor = true;
      this.btnGC.Click += new System.EventHandler(this.btnGC_Click);
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(418, 24);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(97, 13);
      this.label5.TabIndex = 19;
      this.label5.Text = "Absolute Expiration";
      // 
      // tbAbsoluteExpiration
      // 
      this.tbAbsoluteExpiration.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
      this.tbAbsoluteExpiration.Location = new System.Drawing.Point(415, 40);
      this.tbAbsoluteExpiration.Name = "tbAbsoluteExpiration";
      this.tbAbsoluteExpiration.Size = new System.Drawing.Size(100, 20);
      this.tbAbsoluteExpiration.TabIndex = 18;
      this.tbAbsoluteExpiration.Text = " ";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(25, 70);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(71, 13);
      this.label6.TabIndex = 21;
      this.label6.Text = "Max Capacity";
      // 
      // tbMaxCapacity
      // 
      this.tbMaxCapacity.BackColor = System.Drawing.Color.White;
      this.tbMaxCapacity.Location = new System.Drawing.Point(22, 86);
      this.tbMaxCapacity.Name = "tbMaxCapacity";
      this.tbMaxCapacity.Size = new System.Drawing.Size(68, 20);
      this.tbMaxCapacity.TabIndex = 20;
      this.tbMaxCapacity.Text = " 0";
      // 
      // chkAutoRandKey
      // 
      this.chkAutoRandKey.AutoSize = true;
      this.chkAutoRandKey.Location = new System.Drawing.Point(154, 275);
      this.chkAutoRandKey.Name = "chkAutoRandKey";
      this.chkAutoRandKey.Size = new System.Drawing.Size(100, 17);
      this.chkAutoRandKey.TabIndex = 22;
      this.chkAutoRandKey.Text = "Randomize Key";
      this.chkAutoRandKey.UseVisualStyleBackColor = true;
      // 
      // lblAutoStatus
      // 
      this.lblAutoStatus.AutoSize = true;
      this.lblAutoStatus.Location = new System.Drawing.Point(338, 249);
      this.lblAutoStatus.Name = "lblAutoStatus";
      this.lblAutoStatus.Size = new System.Drawing.Size(47, 13);
      this.lblAutoStatus.TabIndex = 23;
      this.lblAutoStatus.Text = "<status>";
      // 
      // chkAutoRandTbl
      // 
      this.chkAutoRandTbl.AutoSize = true;
      this.chkAutoRandTbl.Location = new System.Drawing.Point(22, 275);
      this.chkAutoRandTbl.Name = "chkAutoRandTbl";
      this.chkAutoRandTbl.Size = new System.Drawing.Size(114, 17);
      this.chkAutoRandTbl.TabIndex = 24;
      this.chkAutoRandTbl.Text = "Randomize Tables";
      this.chkAutoRandTbl.UseVisualStyleBackColor = true;
      // 
      // tbBinFrom
      // 
      this.tbBinFrom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
      this.tbBinFrom.Location = new System.Drawing.Point(577, 70);
      this.tbBinFrom.Name = "tbBinFrom";
      this.tbBinFrom.Size = new System.Drawing.Size(46, 20);
      this.tbBinFrom.TabIndex = 25;
      this.tbBinFrom.Text = "0";
      // 
      // tbBinTo
      // 
      this.tbBinTo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
      this.tbBinTo.Location = new System.Drawing.Point(629, 70);
      this.tbBinTo.Name = "tbBinTo";
      this.tbBinTo.Size = new System.Drawing.Size(46, 20);
      this.tbBinTo.TabIndex = 26;
      this.tbBinTo.Text = "0";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(526, 24);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(71, 13);
      this.label7.TabIndex = 28;
      this.label7.Text = "Max Age Sec";
      // 
      // tbMaxAgeSec
      // 
      this.tbMaxAgeSec.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
      this.tbMaxAgeSec.Location = new System.Drawing.Point(523, 40);
      this.tbMaxAgeSec.Name = "tbMaxAgeSec";
      this.tbMaxAgeSec.Size = new System.Drawing.Size(100, 20);
      this.tbMaxAgeSec.TabIndex = 27;
      this.tbMaxAgeSec.Text = " ";
      // 
      // CacheForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(711, 300);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.tbMaxAgeSec);
      this.Controls.Add(this.tbBinTo);
      this.Controls.Add(this.tbBinFrom);
      this.Controls.Add(this.chkAutoRandTbl);
      this.Controls.Add(this.lblAutoStatus);
      this.Controls.Add(this.chkAutoRandKey);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.tbMaxCapacity);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.tbAbsoluteExpiration);
      this.Controls.Add(this.btnGC);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.tbParallel);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.tbTable);
      this.Controls.Add(this.btnRemove);
      this.Controls.Add(this.btnGet);
      this.Controls.Add(this.btnPut);
      this.Controls.Add(this.tbAutoRemove);
      this.Controls.Add(this.tbAutoPut);
      this.Controls.Add(this.tbAutoGet);
      this.Controls.Add(this.chkAutoRemove);
      this.Controls.Add(this.chkAutoPut);
      this.Controls.Add(this.chkAutoGet);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.tbKeyStart);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.tbCount);
      this.Name = "CacheForm";
      this.Text = "CacheForm";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CacheForm_FormClosed);
      this.Load += new System.EventHandler(this.CacheForm_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Timer tmrStatus;
    private System.Windows.Forms.TextBox tbCount;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox tbKeyStart;
    private System.Windows.Forms.CheckBox chkAutoGet;
    private System.Windows.Forms.CheckBox chkAutoPut;
    private System.Windows.Forms.CheckBox chkAutoRemove;
    private System.Windows.Forms.TextBox tbAutoGet;
    private System.Windows.Forms.TextBox tbAutoPut;
    private System.Windows.Forms.TextBox tbAutoRemove;
    private System.Windows.Forms.Button btnPut;
    private System.Windows.Forms.Button btnGet;
    private System.Windows.Forms.Button btnRemove;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox tbTable;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox tbParallel;
    private System.Windows.Forms.Button btnGC;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.TextBox tbAbsoluteExpiration;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.TextBox tbMaxCapacity;
    private System.Windows.Forms.CheckBox chkAutoRandKey;
    private System.Windows.Forms.Label lblAutoStatus;
    private System.Windows.Forms.CheckBox chkAutoRandTbl;
    private System.Windows.Forms.TextBox tbBinFrom;
    private System.Windows.Forms.TextBox tbBinTo;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.TextBox tbMaxAgeSec;
  }
}