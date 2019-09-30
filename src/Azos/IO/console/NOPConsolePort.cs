/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.IO
{
  /// <summary>
  /// Implements IConsolePort which does not do anything
  /// </summary>
  public sealed class NOPConsolePort : IConsolePort
  {
    /// <summary>
    /// Default instance
    /// </summary>
    public static readonly NOPConsolePort Default = new NOPConsolePort("NOP");

    private NOPConsolePort(string name = null)
    {
      if (name.IsNullOrWhiteSpace()) name = Guid.NewGuid().ToString();
      Name = name;
      DefaultConsole = new NOPConsoleOut(this, "NOP");
    }


    public string Name { get; private set; }
    public bool IsRemote => false;
    public IConsoleOut DefaultConsole { get; private set;}
    public IConsoleOut this[string name] => DefaultConsole;


    public IConsoleOut GetOrCreate(string name) => DefaultConsole;

    public void Dispose(){ }
  }


  /// <summary>
  /// Implements IConsoleOut which does not do anything
  /// </summary>
  public sealed class NOPConsoleOut : IConsoleOut
  {
    internal NOPConsoleOut(NOPConsolePort port, string name)
    {
      Port = port;
      if (name.IsNullOrWhiteSpace()) name = Guid.NewGuid().ToString();
      Name = name;
    }

    public void Dispose(){ }

    public ConsoleColor BackgroundColor { get => ConsoleColor.Black; set {} }
    public ConsoleColor ForegroundColor { get => ConsoleColor.White; set {} }


    public IConsolePort Port { get; private set;}

    public string Name { get; private set; }

    public void Beep() { }
    public void Clear() { }

    public void ResetColor() { }
    public void Write(string content) { }
    public void WriteLine() { }
    public void WriteLine(string content) { }
  }
}
