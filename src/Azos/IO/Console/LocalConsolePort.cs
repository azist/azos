/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Syscon=System.Console;

namespace Azos.IO.Console
{
  /// <summary>
  /// Implements IConsolePort using local in-process Console
  /// </summary>
  public sealed class LocalConsolePort : IConsolePort
  {
    /// <summary>
    /// Default instance
    /// </summary>
    public static readonly LocalConsolePort Default = new LocalConsolePort("Default");

    private LocalConsolePort(string name = null)
    {
      if (name.IsNullOrWhiteSpace()) name = Guid.NewGuid().ToString();
      Name = name;
      DefaultConsole = new LocalConsoleOut(this, "Default");
    }


    public string Name { get; private set; }
    public bool IsRemote => false;
    public IConsoleOut DefaultConsole { get; private set;}
    public IConsoleOut this[string name] => DefaultConsole;


    public IConsoleOut GetOrCreate(string name) => DefaultConsole;

    public void Dispose()
    {
      //does nothing for this class
    }
  }


  /// <summary>
  /// Implements IConsoleOut using local console
  /// </summary>
  public sealed class LocalConsoleOut : IConsoleOut
  {
    internal LocalConsoleOut(LocalConsolePort port, string name)
    {
      Port = port;
      if (name.IsNullOrWhiteSpace()) name = Guid.NewGuid().ToString();
      Name = name;
    }

    public void Dispose()
    {
      //does nothing for this class
    }

    public ConsoleColor BackgroundColor { get => Syscon.BackgroundColor; set => Syscon.BackgroundColor = value; }
    public ConsoleColor ForegroundColor { get => Syscon.ForegroundColor; set => Syscon.ForegroundColor = value; }


    public IConsolePort Port { get; private set;}

    public string Name { get; private set; }

    public void Beep() => Syscon.Beep();
    public void Clear() => Syscon.Clear();

    public void ResetColor() => Syscon.ResetColor();
    public void Write(string content) => Syscon.Write(content);
    public void WriteLine() => Syscon.WriteLine();
    public void WriteLine(string content) => Syscon.WriteLine(content);
  }
}
