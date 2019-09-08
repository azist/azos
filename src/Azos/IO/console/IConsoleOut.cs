using System;

namespace Azos.IO
{
  /// <summary>
  /// Represents an abstraction of a console output - a named "port" to local or remote console.
  /// This is used to output/dump data into local or remote console
  /// </summary>
  public interface IConsoleOut : Collections.INamed
  {
    void Clear();
    void Beep();
    void Write(string content);
    void WriteLine();
    void WriteLine(string content);

    void ResetColor();
    ConsoleColor BackgroundColor{ get; set; }
    ConsoleColor ForegroundColor{ get; set; }
  }
}
