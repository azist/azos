using System;

namespace Azos.IO
{
  /// <summary>
  /// Implements IConsoleOut using local console
  /// </summary>
  public sealed class LocalConsoleOut : IConsoleOut
  {
    public static readonly LocalConsoleOut DEFAULT = new LocalConsoleOut("Default");

    public LocalConsoleOut(string name = null)
    {
      if (name.IsNullOrWhiteSpace()) name = Guid.NewGuid().ToString();
      Name = name;
    }

    public ConsoleColor BackgroundColor { get => Console.BackgroundColor; set => Console.BackgroundColor = value; }
    public ConsoleColor ForegroundColor { get => Console.ForegroundColor; set => Console.ForegroundColor = value; }

    public string Name {get; private set;}

    public void Beep()                    => Console.Beep();
    public void Clear()                   => Console.Clear();
    public void ResetColor()              => Console.ResetColor();
    public void Write(string content)     => Console.Write(content);
    public void WriteLine()               => Console.WriteLine();
    public void WriteLine(string content) => Console.WriteLine(content);
  }
}
