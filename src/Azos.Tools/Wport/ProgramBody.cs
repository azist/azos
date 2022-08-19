/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Reflection;

using Azos.Apps;
using Azos.Conf;
using Azos.IO.Console;
using Azos.Platform;
using Azos.Scripting;

namespace Azos.Tools.Wport
{
    /// <summary>
    /// Program body (entry point) for WPORT "web port connector" utility
  /// </summary>
  [Platform.ProcessActivation.ProgramBody("wport", Description = "Web port connector utility")]
  public static class ProgramBody
    {
      public static void Main(string[] args)
      {
        try
        {
          using(var app = new AzosApplication(true, args, null))
          {
            app.SetConsolePort(LocalConsolePort.Default);

            Console.CancelKeyPress += (_, e) =>
            {
              app.Stop();
              ((IApplicationImplementation)ExecutionContext.Application).Stop();
              e.Cancel = true;
            };

            System.Environment.ExitCode = run(app);
          }
        }
        catch(Exception error)
        {
          ConsoleUtils.Error(error.ToMessageWithType());
          ConsoleUtils.Warning(error.StackTrace);
          Console.WriteLine();
          System.Environment.ExitCode = -1;
        }
      }


      private static int run(AzosApplication app)
      {
        var config = app.CommandArgs;

        ConsoleUtils.WriteMarkupContent( typeof(ProgramBody).GetText("Welcome.txt") );


        if (config["?"].Exists ||
            config["h"].Exists ||
            config["help"].Exists)
        {
            ConsoleUtils.WriteMarkupContent( typeof(ProgramBody).GetText("Help.txt") );
            return 0;
        }


        var actionName = config.AttrByIndex(0).Value;
        var url = config.AttrByIndex(1).Value;

        if (actionName.IsNullOrWhiteSpace())
        {
          ConsoleUtils.Error("Missing action type name");
          return -2;
        }

        if (url.IsNullOrWhiteSpace())
        {
          ConsoleUtils.Error("Missing url");
          return -2;
        }

        var uri = new Uri(url);
        var tAction = actionName.IsOneOf("load") ? typeof(Actions.LoadAction) : Type.GetType(actionName);

        tAction.IsOfType<ActionBase>("{0} sub-type".Args(nameof(ActionBase)));

        using var action =  (ActionBase)Activator.CreateInstance(tAction, new object[]{app, uri});

        Console.ForegroundColor =  ConsoleColor.DarkGray;
        Console.Write("Action: ");
        Console.ForegroundColor =  ConsoleColor.Yellow;
        Console.WriteLine(action.GetType().DisplayNameWithExpandedGenericArgs());
        Console.ForegroundColor =  ConsoleColor.Gray;

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("URL: ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(uri);
        Console.ForegroundColor = ConsoleColor.Gray;

        action.Configure(config);
        action.Run();

        return  0;
      }
   }
}
