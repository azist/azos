using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Log;
using Azos.Web;

namespace Azos.IO.Console
{
  /// <summary>
  /// Provides a base abstraction for tele (remote) console port implementations.
  /// The communication is done on the dedicated system thread
  /// </summary>
  public abstract class TeleConsolePort : ApplicationComponent, IConsolePort
  {
    public const string DEFAULT_CONSOLE_NAME = "*";

    public TeleConsolePort(IApplication app, string name = null) : base(app)
    {
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

    private string m_Name;
    private TeleConsoleOut m_Default;
    private Registry<TeleConsoleOut> m_Outs = new Registry<TeleConsoleOut>();
    private CappedQueue<TeleConsoleMsg> m_Queue = new CappedQueue<TeleConsoleMsg>(m => m.Size);
    private Thread m_Thread;
    private ManualResetEvent m_Signal;

    public override string ComponentLogTopic => CoreConsts.APPLICATION_TOPIC;
    public string Name => m_Name;
    public IConsoleOut this[string name] => m_Outs[name] ?? m_Default;
    public bool IsRemote => true;
    public IConsoleOut DefaultConsole => m_Default;

    /// <summary>
    /// Controls the level of detail in error logging.
    /// 0 =no logging, 1 = only severe errors, 2 = all errors
    /// </summary>
    [Config(Default = 99)]
    public int LogDetail {  get; set; } = 99;//log all


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

      var ok = m_Queue.TryEnqueue(msg);

      if (!ok && LogDetail>1)
        WriteLog(MessageType.Error, ".Emit()", "Queue limit reached");
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
          WriteLog(MessageType.CatastrophicError, ".threadBody()", "Leaked: " + error.ToMessageWithType(), error);
          if (!Disposed) m_Signal.WaitOne(2000);//throttle down
        }
      }
    }

    private void drainAll()
    {
      const int MAX_BATCH_SIZE = 64 * 1024;

      while(true)
      {
        var list = new List<TeleConsoleMsg>();
        for(int i = 0, totalSize = 0; totalSize < MAX_BATCH_SIZE; i++)
        {
          if (!m_Queue.TryDequeue(out var msg)) break;
          totalSize += msg.Size;
          list.Add(msg);
        }

        if (list.Count == 0) break;

        var batch = new TeleConsoleMsgBatch(App.AppId, App.TimeSource.UTCNow, list);
        send(batch);
      }//while
    }

    private void send(TeleConsoleMsgBatch batch)
    {
      var retry = Disposed ? 1 : 5; //on dispose we DO NOT retry at all

      for(var i=0; i<retry; i++)
      {
        if (i > 0) m_Signal.WaitOne(i * 2000);//throttle down
        try
        {
          SendMsgBatchOnce(batch);
          return;
        }
        catch(Exception error)
        {
          if (LogDetail>1) WriteLog(MessageType.Error, ".send().for(retry)", "Leaked: " + error.ToMessageWithType(), error);
        }
      }//for
      if (LogDetail>0) WriteLog(MessageType.CriticalAlert, ".send()", "Lost batch after {0} retries".Args(retry));
    }

    /// <summary>
    /// Override to perform physical message sending to the remote port
    /// </summary>
    protected abstract void SendMsgBatchOnce(TeleConsoleMsgBatch batch);

  }

}
