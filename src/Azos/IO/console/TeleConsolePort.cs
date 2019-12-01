using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azos.Collections;
using Azos.Conf;
using Azos.Web;

namespace Azos.IO.Console
{
  /// <summary>
  /// Provides a base abstraction for tele (remote) console port implementations.
  /// The communication is done on the dedicated system thread
  /// </summary>
  public abstract class TeleConsolePort : DisposableObject, IConsolePort
  {
    public const string DEFAULT_CONSOLE_NAME = "*";

    public TeleConsolePort(IApplication app, string name = null)
    {
      m_App = app.NonNull(nameof(app));
      if (name.IsNullOrWhiteSpace()) name = Guid.NewGuid().ToString();
      m_Name = name;
      m_Default = new TeleConsoleOut(this, DEFAULT_CONSOLE_NAME);
      m_Outs.Register(m_Default);
      m_Signal = new ManualResetEvent(false);
      m_Thread = new Thread(threadBody);
      m_Thread.Name = GetType().Name;
      m_Thread.IsBackground = false;
      m_Thread.Start();
    }

    protected override void Destructor()
    {
      m_Signal.Set();
      m_Thread.Join();
      m_Signal.Dispose();
      m_Outs.ForEach( o => o.Dispose() );
      base.Destructor();
    }

    private IApplication m_App;
    private string m_Name;
    private TeleConsoleOut m_Default;
    private Registry<TeleConsoleOut> m_Outs = new Registry<TeleConsoleOut>();
    private CappedQueue<TeleConsoleMsg> m_Queue = new CappedQueue<TeleConsoleMsg>(m => m.Size);
    private Thread m_Thread;
    private ManualResetEvent m_Signal;

    private HttpClient m_Http;


    public IApplication App => m_App;
    public string Name => m_Name;
    public IConsoleOut this[string name] => m_Outs[name] ?? m_Default;
    public bool IsRemote => true;
    public IConsoleOut DefaultConsole => m_Default;

    public IConsoleOut GetOrCreate(string name)
     =>  m_Outs.GetOrRegister(name, n => new TeleConsoleOut(this, n));

    internal void _Unregister(TeleConsoleOut console)
    {
      m_Outs.Unregister(console);
      if (m_Default==console) m_Default = null;
    }


    internal void Emit(TeleConsoleMsg msg)
    {
      if (Disposed) return;//can not enqueue anymore, do not throw, the console just disconnected
      m_Queue.TryEnqueue(msg);
    }

    private void threadBody()
    {
      while(!Disposed)
      {
        m_Signal.WaitOne(75);
        try
        {
          drainAll();
        }
        catch(Exception error)
        {
          m_App.Log.Write( new Log.Message{
             Type = Log.MessageType.Error,
             App = m_App.AppId,
             Exception = error,
             From = "threadBody()",
             Topic = CoreConsts.APPLICATION_TOPIC,
             Text = "Leaked: "+error.ToMessageWithType()
          });
          if (!Disposed) m_Signal.WaitOne(2000);//throttle down
        }
      }
    }

    private void drainAll()
    {
      const int MAX_BATCH_SIZE = 64 * 1024;

      while(true)
      {

        var batch = new List<TeleConsoleMsg>();
        for(int i = 0, totalSize = 0; totalSize < MAX_BATCH_SIZE; i++)
        {
          if (!m_Queue.TryDequeue(out var msg)) break;
          totalSize += msg.Size;
          batch.Add(msg);
        }

        if (batch.Count == 0) break;

        var frame = new TeleConsoleMsgBatch
        {
          App = m_App.AppId,
          TimestampUtc = m_App.TimeSource.UTCNow,
          Data = batch
        };

        send(frame);
      }//while
    }

    private void send(TeleConsoleMsgBatch batch)
    {
      var retry = Disposed ? 1 : 5;

      for(var i=0; i<retry; i++)
      {
        try
        {
          SendMsgBatchOnce(batch);
          return;
        }
        catch(Exception error)
        {
          m_App.Log.Write(new Log.Message
          {
            Type = Log.MessageType.Error,
            App = m_App.AppId,
            Exception = error,
            From = nameof(send),
            Topic = CoreConsts.APPLICATION_TOPIC,
            Text = "Leaked: " + error.ToMessageWithType()
          });
          if (!Disposed) m_Signal.WaitOne((i + 1) * 2000);//throttle down
        }
      }//for
    }

    /// <summary>
    /// Override to perform physical message sending to the remote port
    /// </summary>
    protected abstract void SendMsgBatchOnce(TeleConsoleMsgBatch batch);

  }

}
