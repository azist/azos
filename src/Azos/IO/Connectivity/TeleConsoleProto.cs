using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.IO.Connectivity
{

  public struct TeleConsoleMsg
  {
    public enum CmdType { Write, WriteLine, Beep, Clear, Reset }

    public static TeleConsoleMsg Beep(string name) => new TeleConsoleMsg(name, CmdType.Beep, null, null, null);
    public static TeleConsoleMsg Clear(string name) => new TeleConsoleMsg(name, CmdType.Clear, null, null, null);
    public static TeleConsoleMsg Reset(string name) => new TeleConsoleMsg(name, CmdType.Reset, null, null, null);
    public static TeleConsoleMsg Write(string name, string text, ConsoleColor bg, ConsoleColor fg) => new TeleConsoleMsg(name, CmdType.Write, text, bg, fg);
    public static TeleConsoleMsg WriteLine(string name, string text, ConsoleColor bg, ConsoleColor fg) => new TeleConsoleMsg(name, CmdType.WriteLine, text, bg, fg);

    private TeleConsoleMsg(string name, CmdType  cmd, string text, ConsoleColor? bg, ConsoleColor? fg)
    {
      Name = name;
      Cmd = cmd;
      Text = text;
      Background = bg;
      Foreground = fg;
    }

    public string Name { get; private set; }
    public CmdType Cmd {  get; private set; }
    public string Text { get; private set; }
    public ConsoleColor? Background { get; private set; }
    public ConsoleColor? Foreground { get; private set; }


    /// <summary> Returns an approximate size in relative units (e.g. `chars`) used to estimate memory requirements</summary>
    public int Size => Text != null ? Text.Length : 0;
  }



}
