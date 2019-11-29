using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Azos.Collections;
using Azos.Conf;

namespace Azos.IO.Connectivity
{
  /// <summary>
  /// Provides tele (remote) console port implementation
  /// </summary>
  public sealed class TeleConsolePort : DisposableObject, IConsolePort
  {

    public TeleConsolePort(string name = null)
    {
      if (name.IsNullOrWhiteSpace()) name = Guid.NewGuid().ToString();
      m_Name = name;
      m_Default = new TeleConsoleOut(this, "Default");
    }

    protected override void Destructor()
    {
      m_Outs.ForEach( o => o.Dispose() );
      base.Destructor();
    }


    private string m_Name;
    private TeleConsoleOut m_Default;
    private Registry<TeleConsoleOut> m_Outs = new Registry<TeleConsoleOut>();
    private CappedQueue<TeleConsoleMsg> m_Queue = new CappedQueue<TeleConsoleMsg>(m => m.Size);


    public string Name => m_Name;
    public IConsoleOut this[string name] => m_Outs[name] ?? m_Default;
    public bool IsRemote => true;
    public IConsoleOut DefaultConsole => m_Default;

    public IConsoleOut GetOrCreate(string name)
    {
      throw new NotImplementedException();
    }

    internal void _Register(TeleConsoleOut console) => m_Outs.Register(console);
    internal void _Unregister(TeleConsoleOut console) => m_Outs.Unregister(console);


    internal void Emit(TeleConsoleMsg msg)
    {
      if (Disposed) return;//can not enqueue anymore, do not throw, the console just disconnected
      m_Queue.TryEnqueue(msg);
    }

    private bool sendBatch()
    {
      const int MAX_BATCH_SIZE = 64 * 1024;

      var batch = new List<TeleConsoleMsg>();
      for(int i = 0, totalSize = 0; totalSize < MAX_BATCH_SIZE && m_Queue.TryDequeue(out var msg); i++)
      {
        totalSize += msg.Size;
        batch.Add(msg);
      }
      return true;
    }
  }


  public sealed class TeleConsoleOut : DisposableObject, IConsoleOut
  {
    internal TeleConsoleOut(TeleConsolePort port, string name)
    {
      m_Port = port;
      m_Name = name;
      m_Port._Register(this);
    }

    protected override void Destructor()
    {
      m_Port._Unregister(this);
      base.Destructor();
    }

    private string m_Name;
    private TeleConsolePort m_Port;

    private ConsoleColor m_BackgroundColor;
    private ConsoleColor m_ForegroundColor;


    public string Name => m_Name;
    public IConsolePort Port => m_Port;

    public ConsoleColor BackgroundColor { get => m_BackgroundColor; set => m_BackgroundColor = value; }
    public ConsoleColor ForegroundColor { get => m_ForegroundColor; set => m_ForegroundColor = value; }


    public void Beep()
     => m_Port.Emit(TeleConsoleMsg.Beep(m_Name));

    public void Clear()
     => m_Port.Emit(TeleConsoleMsg.Clear(m_Name));

    public void ResetColor()
     => m_Port.Emit(TeleConsoleMsg.Reset(m_Name));

    public void Write(string content)
     => m_Port.Emit(TeleConsoleMsg.Write(m_Name, content, BackgroundColor, ForegroundColor));

    public void WriteLine()
     => m_Port.Emit(TeleConsoleMsg.WriteLine(m_Name, string.Empty, BackgroundColor, ForegroundColor));

    public void WriteLine(string content)
     => m_Port.Emit(TeleConsoleMsg.WriteLine(m_Name, content, BackgroundColor, ForegroundColor));
  }

}
