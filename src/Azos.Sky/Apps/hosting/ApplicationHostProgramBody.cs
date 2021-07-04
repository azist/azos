/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps.Terminal;
using Azos.Conf;
using Azos.IO.Console;
using Azos.Security;
using Azos.Serialization.JSON;
using Azos.Wave;

namespace Azos.Apps.Hosting
{
  /// <summary>
  /// Provides program body for generic application host which boots a Daemon upon start.
  /// The configuration is file based (not a metabase-based app).
  /// The daemon config is under `/boot` section. If one does not exist, then WaveServer is booted
  /// of the default '/wave' section instead
  /// </summary>
  public static class ApplicationHostProgramBody
  {
    public const string CONFIG_BOOT_SECTION = "boot";

    #region Interactive console
    /// <summary>
    /// Called for interactive console process model which is NOT daemon and NOT governed
    /// </summary>
    public static int InteractiveConsoleMain(BootArgs args)
    {
      var result = interactiveConsoleMainBody(args);

      if (System.Diagnostics.Debugger.IsAttached)
      {
        Console.WriteLine("Press any key to stop the debugging...");
        Console.ReadKey(true);
      }

      return result;
    }

    private static int interactiveConsoleMainBody(BootArgs args)
    {
      Console.CancelKeyPress += (_, e) => {
          var app = s_Application;//capture
          if (app != null)
          {
            app.Stop();
            e.Cancel = true;
          }
      };

      try
      {
        try
        {
          Console.WriteLine("Azos Sky Application Host Process");
          Console.WriteLine("Rev 2.0 July 3 2021 / Radio-86RK");
          Console.WriteLine();
          Console.WriteLine("Booting server daemon...");
          Start(args);
          Console.WriteLine("...server started");
          Console.WriteLine("Application: {0} / `{1}`".Args(s_Application.AppId, s_Application.Name));
          Console.WriteLine("Boot daemon: {0} / `{1}`".Args(s_Server.GetType().Name, s_Server.Name));
          Console.WriteLine("Environment: {0}".Args(s_Application.EnvironmentName));
          Console.WriteLine("Services provided: ");
          Console.WriteLine("  " +s_Server.ServiceDescription.Default("n/a"));
          Console.WriteLine();
          Console.WriteLine("To stop the process enter 'exit' or 'stop' commands or hit <CTL+C>");

          using (var term = new AppRemoteTerminal())
          {
            s_Application.InjectInto(term);

            //Impersonate the current call flow as SYSTEM
            ExecutionContext.__SetThreadLevelSessionContext(new BaseSession(Guid.NewGuid(), 123)
            {
              User = new User(BlankCredentials.Instance, new SysAuthToken(), UserStatus.System, "sys", "Implicit grant", Rights.None)
            });

            while (s_Application.Active)
            {
              Console.Write("{0}@{1}\n$ ".Args(s_Application.AppId, Azos.Platform.Computer.HostName));
              var cmd = Console.ReadLine();
              if (cmd.IsNullOrWhiteSpace()) continue;
              if (cmd.IsOneOf("quit", "exit", "stop")) break;

              try
              {
                var response = term.Execute(cmd);

                if (response.StartsWith(AppRemoteTerminal.MARKUP_PRAGMA))
                  ConsoleUtils.WriteMarkupContent(response);
                else
                  Console.WriteLine(response);
              }
              catch (Exception eterm)
              {
                ConsoleUtils.Error("Terminal error: ");
                var wrap = new WrappedExceptionData(eterm, true);
                Console.WriteLine(wrap.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap));
              }
            }
          }
          Console.WriteLine("...shutting down now");
          Console.WriteLine();
        }
        finally
        {
          Stop();
        }

        return 0;
      }
      catch (Exception error)
      {
        ConsoleUtils.Error("App Root exception, details: ");
        var wrap = new WrappedExceptionData(error, true);
        Console.WriteLine(wrap.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap));

        return -100;
      }
    }
    #endregion

    #region Governed console
    /// <summary>
    /// Called for governed console process model which is NOT daemon but governed by governor process at the specified port
    /// </summary>
    public static int GovernedConsoleMain(BootArgs args)
    {
      var result = governedConsoleMainBody(args);
      return result;
    }

    private static int governedConsoleMainBody(BootArgs args)
    {
      Console.CancelKeyPress += (_, e) => {
        var app = s_Application;//capture
        if (app != null)
        {
          app.Stop();
          e.Cancel = true;
        }
      };

      try
      {
        try
        {
          Start(args);

          //blocks until application is running
//          s_Application.AsTask.Wait();

        }
        finally
        {
          Stop();
        }

        return 0;
      }
      catch (Exception error)
      {
        var wrap = new WrappedExceptionData(error, true);
        var errorContent = "App Root exception, details: \n" + wrap.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);
        var crashFile = "{0:yyyyMMdd-HHmmssff}-{1}-{2}.crash.log".Args(
                               DateTime.Now,
                               System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                               s_AppId.Default("unset"));

        try
        {
          System.IO.File.WriteAllText(crashFile, errorContent);
          Console.WriteLine(errorContent);
        }
        catch{ }

        return -100;
      }
    }
    #endregion

    #region Start/Stop/Daemon
    //note: this is a static class because it uses process-wide console hooks and
    //process return codes. There is no need to make it an instance class for any practical reason
    private static BootArgs s_Args;
    private static GovernorSipcClient s_Sipc;
    private static AzosApplication s_Application;
    private static string s_AppId;
    private static Daemon s_Server;

    /// <summary>
    /// Starts the application container, used by daemons directly
    /// </summary>
    public static void Start(BootArgs args)
    {
      s_Args = args;
      if (args.IsGoverned)
      {
        s_Sipc = new GovernorSipcClient(args.GovernorPort, () => s_Application);
        s_Sipc.Start();
      }

      s_Application = new AzosApplication(args.ForApplication, null);
      s_AppId = s_Application.AppId.ToString();
      var nBoot = s_Application.ConfigRoot[CONFIG_BOOT_SECTION];
      s_Server = FactoryUtils.MakeAndConfigureComponent<Daemon>(s_Application, nBoot, typeof(WaveServer));
      s_Server.Start();
    }

    /// <summary>
    /// Starts the application container, used by daemons directly
    /// </summary>
    public static void Stop()
    {
      DisposableObject.DisposeAndNull(ref s_Server);
      DisposableObject.DisposeAndNull(ref s_Application);

      if (s_Sipc != null)
      {
        DisposableObject.DisposeAndNull(ref s_Sipc);
      }
    }
    #endregion
  }
}
