using System;
using System.Windows.Forms;

using Azos;
using Azos.Data;

using Azos.Sky.Social.Graph;
using Azos.Sky.Social.Graph.Server;
using Azos.Sky.Workers.Server.Queue;

namespace WinFormsTestSky.Social
{
  public partial class TestGraphNodeForm : System.Windows.Forms.Form
  {
    public TestGraphNodeForm()
    {
      InitializeComponent();
    }

    private const string DIMENSION = "TEST";

    private GraphSystemService m_Server;
    private TodoQueueService m_ServerTodo;

    private GraphNodeSystemClient m_NodeClient;

    private GraphEventSystemClient m_EventClient;
    private GraphCommentSystemClient m_CommentClient;

    private void btnStart_Click(object sender, EventArgs e)
    {
      m_Server = new GraphSystemService(null);
      var cfg = @"
      service 
      {
        name=GraphNodeSvc
        type='Azos.Sky.Social.Graph.Server.GraphSystemService, Azos.Sky.Social'

        graph-host
        {
          type='WinFormsTestSky.Social.TestGraphHost, WinFormsTestSky'
        }

        graph-comment-fetch-strategy
        {
          type=' Azos.Sky.Social.Graph.Server.GraphCommentFetchDefaultStrategy, Azos.Sky.Social'
        }

      }
      ".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      m_ServerTodo = new TodoQueueService();

      var cfg1 = @"
      service
      {
        name=socialgraphtodoqueue

        startup-delay-sec = 1

        queue { name='esub-manage' mode=ParallelByKey}
        queue { name='esub-deliver' mode=ParallelByKey}
        queue { name='esub-remove' mode=ParallelByKey}
        queue { name='esub-manage' mode=ParallelByKey}

        queue-store
        {
          type='Azos.Sky.Workers.Server.Queue.MongoTodoQueueStore, Azos.Sky.MongoDB'
          mongo='mongo{server=\'localhost:27017\' db=\'social-tezt\'}'
          fetch-by=4000
        }
      }
      ".AsLaconicConfig(handling: ConvertErrorHandling.Throw);


      m_ServerTodo.Configure(cfg1);
      m_ServerTodo.Start();

      m_Server.Configure(cfg);
      m_Server.Start();

      m_NodeClient = new GraphNodeSystemClient("async://localhost:{0}".Args(App.ConfigRoot.Navigate("/gv/services/$async-graphnode").Value));
      m_EventClient = new GraphEventSystemClient("async://localhost:{0}".Args(App.ConfigRoot.Navigate("/gv/services/$async-graphevent").Value));
      m_CommentClient = new GraphCommentSystemClient("async://localhost:{0}".Args(App.ConfigRoot.Navigate("/gv/services/$async-graphcomment").Value));
    }

    private void btnSTOP_Click(object sender, EventArgs e)
    {
      DisposableObject.DisposeAndNull(ref m_Server);
    }

    private void btnCreateNode_Click(object sender, EventArgs e)
    {
      GraphNode graphNode = GraphNode.MakeNew("TEST", GDID.Zero, GDID.Zero, "TEST{0}".Args(App.Random.NextScaledRandomInteger(10000)), null, FriendVisibility.Anyone);
      var status = m_NodeClient.SaveNode(graphNode);
      if(status == GraphChangeStatus.Added)
      {
        lbNodes.Items.Add(graphNode);
        lbForSubscribe.Items.Add(graphNode);
      }

    }

    private void btnLoad_Click(object sender, EventArgs e)
    {
      if (lbNodes.SelectedItem != null)
      {
        GraphNode gn = (GraphNode)lbNodes.SelectedItem;
        var graphNode = m_NodeClient.GetNode(gn.GDID);
        MessageBox.Show(graphNode.ToString(), "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      else
      {
        MessageBox.Show("Select graph node", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
      if (lbNodes.SelectedItem != null)
      {
        GraphNode gn = (GraphNode)lbNodes.SelectedItem;
        var status = m_NodeClient.DeleteNode(gn.GDID);
        if(status == GraphChangeStatus.Deleted)
        {
          MessageBox.Show("{0} - deleted".Args(gn), "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
          lbNodes.Items.Remove(gn);
          lbSubscribers.Items.Remove(gn);
        }
      }
      else
      {
        MessageBox.Show("Select graph node", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    private void btnSubscribe_Click(object sender, EventArgs e)
    {
      if (lbNodes.SelectedItem != null && lbForSubscribe.SelectedItem != null)
      {
        GraphNode emitter = (GraphNode)lbForSubscribe.SelectedItem;
        GraphNode subscriber = (GraphNode)lbNodes.SelectedItem;

        m_EventClient.Subscribe(subscriber.GDID, emitter.GDID, null);

      }
      else
      {
        MessageBox.Show("Select graph node and subscribe node", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    private void tmrMaim_Tick(object sender, EventArgs e)
    {
      bool isServerStart = m_Server != null;
      btnStart.Enabled = !isServerStart;
      btnSTOP.Enabled = isServerStart;
      btnCreateNode.Enabled = isServerStart;
      btnLoad.Enabled = isServerStart;
      btnDelete.Enabled = isServerStart;
      btnSubscribe.Enabled = isServerStart;
    }

    private void btnViewSub_Click(object sender, EventArgs e)
    {
      viewSubscribers();
    }

    private void btnSendComment_Click(object sender, EventArgs e)
    {
      if (lbNodes.SelectedItem != null && lbForSubscribe.SelectedItem != null)
      {
        var rating = getRating();
        var comment = tbComment.Text;
        GraphNode author = (GraphNode)lbForSubscribe.SelectedItem;
        GraphNode target = (GraphNode)lbNodes.SelectedItem;

        var commentId = m_CommentClient.Create(author.GDID, target.GDID, DIMENSION, comment, null, PublicationState.Public, rating, null);

        clsRetingMsg();

        viewComments();
      }
      else
      {
        MessageBox.Show("Select graph node and subscribe node", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    private RatingValue getRating()
    {
      if(rb1.Checked) return RatingValue.Star1;
      if(rb2.Checked) return RatingValue.Star2;
      if(rb3.Checked) return RatingValue.Star3;
      if(rb4.Checked) return RatingValue.Star5;
      if(rb5.Checked) return RatingValue.Star5;
      return RatingValue.Undefined;
    }

    private void lbNodes_SelectedIndexChanged(object sender, EventArgs e)
    {
      viewSubscribers();
      viewComments();
    }

    private void viewSubscribers()
    {
      if (lbNodes.SelectedItem != null)
      {
        GraphNode gn = (GraphNode)lbNodes.SelectedItem;
        var subscribers = m_EventClient.GetSubscribers(gn.GDID, 0, 10);
        lbSubscribers.Items.Clear();
        subscribers.ForEach(item => lbSubscribers.Items.Add(item));
      }
      else
      {
        MessageBox.Show("Select graph node", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    private void viewComments()
    {
      if (lbNodes.SelectedItem != null)
      {
        GraphNode gn = (GraphNode)lbNodes.SelectedItem;
        var qry = CommentQuery.MakeNew(gn.GDID, DIMENSION, CommentOrderType.ByDate, false, DateTime.UtcNow, 0);
        var comments = m_CommentClient.Fetch(qry);
        lbComments.Items.Clear();
        comments.ForEach(comment => lbComments.Items.Add(comment));
      }
      else
      {
        MessageBox.Show("Select graph node", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    private void btnReply_Click(object sender, EventArgs e)
    {
      if (lbComments.SelectedItem != null && lbForSubscribe.SelectedItem != null)
      {
        var comment = (Comment) lbComments.SelectedItem;
        GraphNode author = (GraphNode)lbForSubscribe.SelectedItem;
        var msg = tbComment.Text;
        var status = m_CommentClient.Respond(author.GDID, comment.ID, msg, null);
        viewComments();
        clsRetingMsg();
      }
      else
      {
        MessageBox.Show("Select author node and comment", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    private void btnLike_Click(object sender, EventArgs e)
    {
      if (lbComments.SelectedItem != null)
      {
        var comment = (Comment) lbComments.SelectedItem;
        var status = m_CommentClient.Like(comment.ID, 1, 0);
        viewComments();
      }
      else
      {
        MessageBox.Show("Select graph node and comment", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    private void btnDislike_Click(object sender, EventArgs e)
    {
      if (lbComments.SelectedItem != null)
      {
        var comment = (Comment) lbComments.SelectedItem;
        var status = m_CommentClient.Like(comment.ID, 0, 1);
        viewComments();
      }
      else
      {
        MessageBox.Show("Select graph node and comment", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    private void clsRetingMsg()
    {
      rb0.Checked = false;
      rb1.Checked = false;
      rb2.Checked = false;
      rb3.Checked = false;
      rb4.Checked = false;
      rb5.Checked = false;

      tbComment.Text = "";
    }

    private void lbComments_SelectedIndexChanged(object sender, EventArgs e)
    {

    }
  }
}
