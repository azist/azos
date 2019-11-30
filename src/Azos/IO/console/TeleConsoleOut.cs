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
  /// Implements IConsoleOut for derivatives of TeleConsolePort
  /// </summary>
  public sealed class TeleConsoleOut : DisposableObject, IConsoleOut
  {
    internal TeleConsoleOut(TeleConsolePort port, string name)
    {
      m_Port = port;
      m_Name = name;
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
