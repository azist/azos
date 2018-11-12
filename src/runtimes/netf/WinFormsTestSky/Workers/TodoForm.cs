using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

using Azos;
using Azos.Data;
using Azos.Sky.Coordination;
using Azos.Sky.Workers;
using Azos.Sky.Workers.Server.Queue;

namespace WinFormsTestSky.Workers
{
  public partial class TodoForm : System.Windows.Forms.Form
  {
    public TodoForm()
    {
      InitializeComponent();
    }

    private TodoQueueService m_Server;

    private void btnSendOne_Click(object sender, EventArgs e)
    {
      var todo = Todo.MakeNew<TeztTodo>();
      todo.PersonID = tbPersonID.Text;
      todo.PersonName = tbPersonName.Text;
      todo.PersonDOB = tbPersonDOB.Text.AsDateTime();

      m_Server.Enqueue( new[] { todo });
    }

    private void btnSendMany_Click(object sender, EventArgs e)
    {
      var cnt = tbCount.Text.AsInt(10);

      var uiId = tbPersonID.Text;
      var uiPName = tbPersonName.Text;
      var uiDt = tbPersonDOB.Text.AsDateTime();



      var sw = Stopwatch.StartNew();
      Parallel.For(0, cnt, new ParallelOptions { MaxDegreeOfParallelism = 10 },
      (i) =>
      {
        if (true)//(i & 1) == 0)
        {
          var todo = Todo.MakeNew<TeztTodo>();
          todo.PersonID = i.ToString() + uiId;
          todo.PersonName = uiPName;
          todo.PersonDOB = uiDt;
          m_Server.Enqueue(new[] { todo });
        }
        //else
        //{
        //  var todo2 = Todo.MakeNew<EmailXTimesTodo>();
        //  todo2.Count = 0;
        //  todo2.IntervalSec = 1;
        //  m_Server.Enqueue(new[] { todo2 });
        //}
      } );
      var el = sw.ElapsedMilliseconds;
      Text = "Did {0} in {1} ms. at {2} ops/sec".Args(cnt, el, cnt / (el / 1000d));
    }

    private void btnSendCorrelatedOne_Click(object sender, EventArgs e)
    {
      try
      {
        Parallel.For(0, 1, (i) =>///1000
        {
           var todo = Todo.MakeNew<CorrelatedTeztTodo>();
           todo.SysCorrelationKey = tbCorrelationKey.Text;
           todo.Counter = tbCorrelationCounter.Text.AsInt();

           todo.SysStartDate = App.TimeSource.UTCNow.AddSeconds(3);
           m_Server.Enqueue( new[] { todo });
        });
      }
      catch(Exception error)
      {
        MessageBox.Show(error.ToMessageWithType());
      }
    }

    private void btnSendEMail_Click(object sender, EventArgs e)
    {
      var todo = Todo.MakeNew<EmailXTimesTodo>();
      todo.Who = tbWho.Text;
      todo.Count = 7;
      todo.IntervalSec = 2;
      m_Server.Enqueue( new[] { todo });
    }

    private void btnServerStart_Click(object sender, EventArgs e)
    {
      m_Server = new TodoQueueService();

      var cfg= @"
srv
{
  startup-delay-sec = 1

  queue { name='tezt' batch-size=1024  mode=Parallel}
  queue { name='email' batch-size=1024  mode=Parallel}
  queue-store
  {
    type='Azos.Sky.Workers.Server.Queue.MongoTodoQueueStore, Azos.Sky.MongoDB'
    mongo='mongo{server=\'localhost:27017\' db=\'queue-tezt\'}'
    fetch-by=4000
  }
}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);


      m_Server.Configure( cfg );
      m_Server.Start();
    }

    private void btnServerStop_Click(object sender, EventArgs e)
    {
      DisposableObject.DisposeAndNull(ref m_Server);
    }

    private void tmr_Tick(object sender, EventArgs e)
    {
      btnServerStart.Enabled = m_Server == null;
      btnServerStop.Enabled = m_Server != null;
      btnSendOne.Enabled = m_Server != null;
      btnSendMany.Enabled = m_Server != null;
      btnSendCorrelatedOne.Enabled = m_Server != null;
      btnSendEMail.Enabled = m_Server != null;

      lblProcessed.Text = "Processed: {0} Correlated: {1}".Args(TeztTodo.TotalProcessed, CorrelatedTeztTodo.TotalProcessed);
    }

    private void TodoForm_FormClosed(object sender, FormClosedEventArgs e)
    {
      DisposableObject.DisposeAndNull(ref m_Server);
    }

  }
}
