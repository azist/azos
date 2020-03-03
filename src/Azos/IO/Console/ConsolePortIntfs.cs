/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.IO.Console
{
  /// <summary>
  /// Represents a "port"/connection to an external console which maintains a registry of IConsoleOut objects, each uniquely named.
  /// IConsolePort instances owns all IConsoleOut instances it created, which are all disposed on IConsolePort dispose.
  /// The implementation must be thread-safe
  /// </summary>
  public interface IConsolePort : Collections.INamed, IDisposable
  {
    /// <summary>
    /// Returns true for ports that involve out-of-process/socket communication, such as Remote ports on a different machine or another process
    /// </summary>
    bool IsRemote{ get; }

    /// <summary>
    /// Returns console by name or Default if a console with such name was not found
    /// </summary>
    IConsoleOut this[string name]{ get; }

    /// <summary>
    /// Returns a default IConsoleOut instance
    /// </summary>
    IConsoleOut DefaultConsole {  get; }

    /// <summary>
    /// Returns an existing IConsoleOut by name, or creates one if it does not exist.
    /// Note: In cases of console ports which do not support multi-named consoles, this method may not create another
    /// instance and return a DefaultConsole instead
    /// </summary>
    IConsoleOut GetOrCreate(string name);
  }



  /// <summary>
  /// Represents an abstraction of a console output - a named "window" on a remote machine or local console.
  /// This is used to output/dump data into local or remote console.
  /// The 'Name' property is typically used to identify a "panel/tab/window" on a remote debug/trace viewer
  /// </summary>
  public interface IConsoleOut : Collections.INamed, IDisposable
  {
    /// <summary>
    /// References IConsolePort which has created this IConsoleOut
    /// </summary>
    IConsolePort Port { get; }

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
