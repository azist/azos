/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Apps;
using Azos.Conf;
using Azos.IO.Console;
using Azos.Security;
using Azos.Glue;
using Azos.Glue.Protocol;
using Azos.Platform;


namespace Azos.AuthKit.Tools.idp
{
  [Platform.ProcessActivation.ProgramBody("idp", Description = "AuthKit IDentity Provider CLI tool")]
  public static class ProgramBody
  {
    public static void Main(string[] args)
    {
      try
      {
        using (var app = new AzosApplication(args, null))
        {
          run(app);
        }
      }
      catch (Exception error)
      {
        ConsoleUtils.Error(error.ToMessageWithType());
        ConsoleUtils.Info("Exception details:");
        Console.WriteLine(error.ToString());

        if (error is RemoteException)
          TerminalUtils.ShowRemoteException((RemoteException)error);

        Environment.ExitCode = -1;
      }
    }

    static void run(IApplication app)
    {
      var silent = app.CommandArgs["s", "silent"].Exists;
      if (!silent)
      {
        ConsoleUtils.WriteMarkupContent(typeof(ProgramBody).GetText("Welcome.txt"));

        ConsoleUtils.Info("Build:  " + BuildInformation.ForFramework);
        ConsoleUtils.Info("App description:  " + app.Description);
      }

      if (app.CommandArgs["?", "h", "help"].Exists)
      {
        ConsoleUtils.WriteMarkupContent(typeof(ProgramBody).GetText("Help.txt"));
        return;
      }

    }//run

  }
}
