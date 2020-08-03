/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.Apps;
using Azos.IO.Console;
using Azos.Platform;
using Azos.Tools.Crun.Logic;

namespace Azos.Tools.Crun
{
  /// <summary>
  /// Program body (entry point) for CRUN "command runner" utility which allows
  /// for a sub command option argument that will target a specific attributed method
  /// to be called from the "Commands" CmdBase derived class.
  /// </summary>
  /// <remarks>
  /// For example specifying "echo -r text="jello world" ru="mir zhele"" will execute
  /// the Commands.cs runEcho method attributed with the following:  [Cmd("echo")]
  /// </remarks>
  public static class ProgramBody
  {
    public static void Main(string[] args)
    {
      try
      {
        Environment.ExitCode = run(args);
      }
      catch (Exception error)
      {
        ConsoleUtils.Error(error.ToMessageWithType());
        ConsoleUtils.Warning(error.StackTrace);
        Console.WriteLine();
        Environment.ExitCode = -1;
      }
    }

    /// <summary>
    /// The run method tries to match the args[0] item to an CmdAttribute
    /// decorated method and invokes that method with the remaining args.
    /// </summary>
    private static int run(string[] args)
    {
      var cmds = new Commands();
      if (cmds.TryGetCommand(args[0], out Cmd cmd))
      {
        var newargs = args.Skip(1).ToArray();
        using (var app = new AzosApplication(true, newargs, null))
        {
          app.SetConsolePort(LocalConsolePort.Default);
          Console.CancelKeyPress += (_, e) => { app.Stop(); e.Cancel = true; };

          var config = app.CommandArgs;
          var welcome = cmd.Attribute.WelcomeFile ?? "Welcome.txt";
          ConsoleUtils.WriteMarkupContent(typeof(ProgramBody).GetText(welcome));

          if (config["?"].Exists ||
              config["h"].Exists ||
              config["help"].Exists)
          {
            var help = cmd.Attribute.HelpFile ?? "Help.txt";
            ConsoleUtils.WriteMarkupContent(typeof(ProgramBody).GetText(help));
            Environment.ExitCode = 0;
          }
          var result = cmds.InvokeCommand(app, cmd.Method);
          if (result is int) return (int)result;
          return 0;
        }
      }
      ConsoleUtils.Error("No matching command option specified or found");
      return -2;
    }
  }
}
