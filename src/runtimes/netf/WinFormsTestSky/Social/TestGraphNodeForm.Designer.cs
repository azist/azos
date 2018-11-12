namespace WinFormsTestSky.Social
{
  partial class TestGraphNodeForm
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
      this.btnStart = new System.Windows.Forms.Button();
      this.btnSTOP = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnViewSub = new System.Windows.Forms.Button();
      this.btnSubscribe = new System.Windows.Forms.Button();
      this.btnDelete = new System.Windows.Forms.Button();
      this.lbForSubscribe = new System.Windows.Forms.ListBox();
      this.btnLoad = new System.Windows.Forms.Button();
      this.lbNodes = new System.Windows.Forms.ListBox();
      this.btnCreateNode = new System.Windows.Forms.Button();
      this.panel2 = new System.Windows.Forms.Panel();
      this.btnDislike = new System.Windows.Forms.Button();
      this.btnLike = new System.Windows.Forms.Button();
      this.lbComments = new System.Windows.Forms.ListBox();
      this.btnReply = new System.Windows.Forms.Button();
      this.btnSendComment = new System.Windows.Forms.Button();
      this.tbComment = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.gbRating = new System.Windows.Forms.GroupBox();
      this.rb5 = new System.Windows.Forms.RadioButton();
      this.rb4 = new System.Windows.Forms.RadioButton();
      this.rb3 = new System.Windows.Forms.RadioButton();
      this.rb2 = new System.Windows.Forms.RadioButton();
      this.rb1 = new System.Windows.Forms.RadioButton();
      this.rb0 = new System.Windows.Forms.RadioButton();
      this.lbSubscribers = new System.Windows.Forms.ListBox();
      this.tmrMaim = new System.Windows.Forms.Timer(this.components);
      this.lbResponse = new System.Windows.Forms.ListBox();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.gbRating.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnStart
      // 
      this.btnStart.Location = new System.Drawing.Point(13, 13);
      this.btnStart.Name = "btnStart";
      this.btnStart.Size = new System.Drawing.Size(75, 23);
      this.btnStart.TabIndex = 0;
      this.btnStart.Text = "START";
      this.btnStart.UseVisualStyleBackColor = true;
      this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
      // 
      // btnSTOP
      // 
      this.btnSTOP.Location = new System.Drawing.Point(95, 13);
      this.btnSTOP.Name = "btnSTOP";
      this.btnSTOP.Size = new System.Drawing.Size(75, 23);
      this.btnSTOP.TabIndex = 1;
      this.btnSTOP.Text = "STOP";
      this.btnSTOP.UseVisualStyleBackColor = true;
      this.btnSTOP.Click += new System.EventHandler(this.btnSTOP_Click);
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnViewSub);
      this.panel1.Controls.Add(this.btnSubscribe);
      this.panel1.Controls.Add(this.btnDelete);
      this.panel1.Controls.Add(this.lbForSubscribe);
      this.panel1.Controls.Add(this.btnLoad);
      this.panel1.Controls.Add(this.lbNodes);
      this.panel1.Controls.Add(this.btnCreateNode);
      this.panel1.Location = new System.Drawing.Point(13, 43);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(612, 174);
      this.panel1.TabIndex = 2;
      // 
      // btnViewSub
      // 
      this.btnViewSub.Location = new System.Drawing.Point(340, 4);
      this.btnViewSub.Name = "btnViewSub";
      this.btnViewSub.Size = new System.Drawing.Size(75, 23);
      this.btnViewSub.TabIndex = 6;
      this.btnViewSub.Text = "VIEW SUBS";
      this.btnViewSub.UseVisualStyleBackColor = true;
      this.btnViewSub.Click += new System.EventHandler(this.btnViewSub_Click);
      // 
      // btnSubscribe
      // 
      this.btnSubscribe.Location = new System.Drawing.Point(248, 4);
      this.btnSubscribe.Name = "btnSubscribe";
      this.btnSubscribe.Size = new System.Drawing.Size(85, 23);
      this.btnSubscribe.TabIndex = 5;
      this.btnSubscribe.Text = "SUBSCRIBE";
      this.btnSubscribe.UseVisualStyleBackColor = true;
      this.btnSubscribe.Click += new System.EventHandler(this.btnSubscribe_Click);
      // 
      // btnDelete
      // 
      this.btnDelete.Location = new System.Drawing.Point(166, 4);
      this.btnDelete.Name = "btnDelete";
      this.btnDelete.Size = new System.Drawing.Size(75, 23);
      this.btnDelete.TabIndex = 4;
      this.btnDelete.Text = "DELETE";
      this.btnDelete.UseVisualStyleBackColor = true;
      this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
      // 
      // lbForSubscribe
      // 
      this.lbForSubscribe.FormattingEnabled = true;
      this.lbForSubscribe.Location = new System.Drawing.Point(300, 32);
      this.lbForSubscribe.Name = "lbForSubscribe";
      this.lbForSubscribe.Size = new System.Drawing.Size(309, 134);
      this.lbForSubscribe.TabIndex = 4;
      // 
      // btnLoad
      // 
      this.btnLoad.Location = new System.Drawing.Point(84, 3);
      this.btnLoad.Name = "btnLoad";
      this.btnLoad.Size = new System.Drawing.Size(75, 23);
      this.btnLoad.TabIndex = 3;
      this.btnLoad.Text = "LOAD";
      this.btnLoad.UseVisualStyleBackColor = true;
      this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
      // 
      // lbNodes
      // 
      this.lbNodes.FormattingEnabled = true;
      this.lbNodes.Location = new System.Drawing.Point(3, 32);
      this.lbNodes.Name = "lbNodes";
      this.lbNodes.Size = new System.Drawing.Size(291, 134);
      this.lbNodes.TabIndex = 1;
      this.lbNodes.SelectedIndexChanged += new System.EventHandler(this.lbNodes_SelectedIndexChanged);
      // 
      // btnCreateNode
      // 
      this.btnCreateNode.Location = new System.Drawing.Point(3, 3);
      this.btnCreateNode.Name = "btnCreateNode";
      this.btnCreateNode.Size = new System.Drawing.Size(75, 23);
      this.btnCreateNode.TabIndex = 2;
      this.btnCreateNode.Text = "CREATE NODE";
      this.btnCreateNode.UseVisualStyleBackColor = true;
      this.btnCreateNode.Click += new System.EventHandler(this.btnCreateNode_Click);
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.lbResponse);
      this.panel2.Controls.Add(this.btnDislike);
      this.panel2.Controls.Add(this.btnLike);
      this.panel2.Controls.Add(this.lbComments);
      this.panel2.Controls.Add(this.btnReply);
      this.panel2.Controls.Add(this.btnSendComment);
      this.panel2.Controls.Add(this.tbComment);
      this.panel2.Controls.Add(this.label1);
      this.panel2.Controls.Add(this.gbRating);
      this.panel2.Controls.Add(this.lbSubscribers);
      this.panel2.Location = new System.Drawing.Point(13, 224);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(612, 355);
      this.panel2.TabIndex = 3;
      // 
      // btnDislike
      // 
      this.btnDislike.Location = new System.Drawing.Point(86, 304);
      this.btnDislike.Name = "btnDislike";
      this.btnDislike.Size = new System.Drawing.Size(75, 23);
      this.btnDislike.TabIndex = 19;
      this.btnDislike.Text = "DISLIKE";
      this.btnDislike.UseVisualStyleBackColor = true;
      this.btnDislike.Click += new System.EventHandler(this.btnDislike_Click);
      // 
      // btnLike
      // 
      this.btnLike.Location = new System.Drawing.Point(4, 304);
      this.btnLike.Name = "btnLike";
      this.btnLike.Size = new System.Drawing.Size(75, 23);
      this.btnLike.TabIndex = 18;
      this.btnLike.Text = "LIKE";
      this.btnLike.UseVisualStyleBackColor = true;
      this.btnLike.Click += new System.EventHandler(this.btnLike_Click);
      // 
      // lbComments
      // 
      this.lbComments.FormattingEnabled = true;
      this.lbComments.Location = new System.Drawing.Point(4, 202);
      this.lbComments.Name = "lbComments";
      this.lbComments.Size = new System.Drawing.Size(290, 95);
      this.lbComments.TabIndex = 17;
      this.lbComments.SelectedIndexChanged += new System.EventHandler(this.lbComments_SelectedIndexChanged);
      // 
      // btnReply
      // 
      this.btnReply.Location = new System.Drawing.Point(86, 148);
      this.btnReply.Name = "btnReply";
      this.btnReply.Size = new System.Drawing.Size(75, 23);
      this.btnReply.TabIndex = 16;
      this.btnReply.Text = "REPLY";
      this.btnReply.UseVisualStyleBackColor = true;
      this.btnReply.Click += new System.EventHandler(this.btnReply_Click);
      // 
      // btnSendComment
      // 
      this.btnSendComment.Location = new System.Drawing.Point(4, 148);
      this.btnSendComment.Name = "btnSendComment";
      this.btnSendComment.Size = new System.Drawing.Size(75, 23);
      this.btnSendComment.TabIndex = 15;
      this.btnSendComment.Text = "SEND";
      this.btnSendComment.UseVisualStyleBackColor = true;
      this.btnSendComment.Click += new System.EventHandler(this.btnSendComment_Click);
      // 
      // tbComment
      // 
      this.tbComment.Location = new System.Drawing.Point(238, 109);
      this.tbComment.Multiline = true;
      this.tbComment.Name = "tbComment";
      this.tbComment.Size = new System.Drawing.Size(371, 62);
      this.tbComment.TabIndex = 14;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(238, 92);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(51, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "Comment";
      // 
      // gbRating
      // 
      this.gbRating.Controls.Add(this.rb5);
      this.gbRating.Controls.Add(this.rb4);
      this.gbRating.Controls.Add(this.rb3);
      this.gbRating.Controls.Add(this.rb2);
      this.gbRating.Controls.Add(this.rb1);
      this.gbRating.Controls.Add(this.rb0);
      this.gbRating.Location = new System.Drawing.Point(4, 92);
      this.gbRating.Name = "gbRating";
      this.gbRating.Size = new System.Drawing.Size(227, 49);
      this.gbRating.TabIndex = 7;
      this.gbRating.TabStop = false;
      this.gbRating.Text = "Rating";
      // 
      // rb5
      // 
      this.rb5.AutoSize = true;
      this.rb5.Location = new System.Drawing.Point(192, 20);
      this.rb5.Name = "rb5";
      this.rb5.Size = new System.Drawing.Size(31, 17);
      this.rb5.TabIndex = 13;
      this.rb5.TabStop = true;
      this.rb5.Text = "5";
      this.rb5.UseVisualStyleBackColor = true;
      // 
      // rb4
      // 
      this.rb4.AutoSize = true;
      this.rb4.Location = new System.Drawing.Point(154, 20);
      this.rb4.Name = "rb4";
      this.rb4.Size = new System.Drawing.Size(31, 17);
      this.rb4.TabIndex = 12;
      this.rb4.TabStop = true;
      this.rb4.Text = "4";
      this.rb4.UseVisualStyleBackColor = true;
      // 
      // rb3
      // 
      this.rb3.AutoSize = true;
      this.rb3.Location = new System.Drawing.Point(116, 20);
      this.rb3.Name = "rb3";
      this.rb3.Size = new System.Drawing.Size(31, 17);
      this.rb3.TabIndex = 11;
      this.rb3.TabStop = true;
      this.rb3.Text = "3";
      this.rb3.UseVisualStyleBackColor = true;
      // 
      // rb2
      // 
      this.rb2.AutoSize = true;
      this.rb2.Location = new System.Drawing.Point(78, 20);
      this.rb2.Name = "rb2";
      this.rb2.Size = new System.Drawing.Size(31, 17);
      this.rb2.TabIndex = 10;
      this.rb2.TabStop = true;
      this.rb2.Text = "2";
      this.rb2.UseVisualStyleBackColor = true;
      // 
      // rb1
      // 
      this.rb1.AutoSize = true;
      this.rb1.Location = new System.Drawing.Point(45, 20);
      this.rb1.Name = "rb1";
      this.rb1.Size = new System.Drawing.Size(31, 17);
      this.rb1.TabIndex = 9;
      this.rb1.TabStop = true;
      this.rb1.Text = "1";
      this.rb1.UseVisualStyleBackColor = true;
      // 
      // rb0
      // 
      this.rb0.AutoSize = true;
      this.rb0.Location = new System.Drawing.Point(7, 20);
      this.rb0.Name = "rb0";
      this.rb0.Size = new System.Drawing.Size(31, 17);
      this.rb0.TabIndex = 8;
      this.rb0.TabStop = true;
      this.rb0.Text = "0";
      this.rb0.UseVisualStyleBackColor = true;
      // 
      // lbSubscribers
      // 
      this.lbSubscribers.FormattingEnabled = true;
      this.lbSubscribers.Location = new System.Drawing.Point(3, 3);
      this.lbSubscribers.Name = "lbSubscribers";
      this.lbSubscribers.Size = new System.Drawing.Size(606, 82);
      this.lbSubscribers.TabIndex = 0;
      // 
      // tmrMaim
      // 
      this.tmrMaim.Enabled = true;
      this.tmrMaim.Tick += new System.EventHandler(this.tmrMaim_Tick);
      // 
      // lbResponse
      // 
      this.lbResponse.FormattingEnabled = true;
      this.lbResponse.Location = new System.Drawing.Point(300, 202);
      this.lbResponse.Name = "lbResponse";
      this.lbResponse.Size = new System.Drawing.Size(309, 95);
      this.lbResponse.TabIndex = 20;
      // 
      // TestGraphNodeForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(637, 591);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.btnSTOP);
      this.Controls.Add(this.btnStart);
      this.Name = "TestGraphNodeForm";
      this.Text = "TestGraphNodeForm";
      this.panel1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.gbRating.ResumeLayout(false);
      this.gbRating.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnStart;
    private System.Windows.Forms.Button btnSTOP;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.ListBox lbNodes;
    private System.Windows.Forms.Button btnCreateNode;
    private System.Windows.Forms.Button btnLoad;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.ListBox lbSubscribers;
    private System.Windows.Forms.ListBox lbForSubscribe;
    private System.Windows.Forms.Button btnDelete;
    private System.Windows.Forms.Button btnSubscribe;
    private System.Windows.Forms.Timer tmrMaim;
    private System.Windows.Forms.Button btnViewSub;
    private System.Windows.Forms.Button btnSendComment;
    private System.Windows.Forms.TextBox tbComment;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox gbRating;
    private System.Windows.Forms.RadioButton rb5;
    private System.Windows.Forms.RadioButton rb4;
    private System.Windows.Forms.RadioButton rb3;
    private System.Windows.Forms.RadioButton rb2;
    private System.Windows.Forms.RadioButton rb1;
    private System.Windows.Forms.RadioButton rb0;
    private System.Windows.Forms.ListBox lbComments;
    private System.Windows.Forms.Button btnReply;
    private System.Windows.Forms.Button btnDislike;
    private System.Windows.Forms.Button btnLike;
    private System.Windows.Forms.ListBox lbResponse;
  }
}