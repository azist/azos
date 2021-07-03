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

    public static int ConsoleMain(BootArgs args)
    {
      var result = consoleMainBody(args);

      if (System.Diagnostics.Debugger.IsAttached)
      {
        Console.WriteLine("Press any key to stop the debugging...");
        Console.ReadKey(true);
      }

      return result;
    }

    private static int consoleMainBody(BootArgs args)
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
          Console.WriteLine("Rev 1.0 Nov 2020 / Radio-86RK");
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

    //note: this is a static class because it uses process-wide console hooks and
    //process return codes. There is no need to make it an instance class for any practical reason
    private static BootArgs s_Args;
    private static GovernorSipcClient s_Sipc;
    private static AzosApplication s_Application;
    private static Daemon s_Server;

    //need to protect with crash dump into file
    public static void Start(BootArgs args)
    {
      s_Args = args;
      if (args.IsGoverned)
      {
        //todo: Establish SIPC
        s_Sipc = new GovernorSipcClient(args.GovernorPort);
      }

      s_Application = new AzosApplication(args.ForApplication, null);
      var nBoot = s_Application.ConfigRoot[CONFIG_BOOT_SECTION];
      s_Server = FactoryUtils.MakeAndConfigureComponent<Daemon>(s_Application, nBoot, typeof(WaveServer));
      s_Server.Start();
    }

    public static void Stop()
    {
      DisposableObject.DisposeAndNull(ref s_Server);
      DisposableObject.DisposeAndNull(ref s_Application);

      if (s_Sipc != null)
      {
      //todo: Disconnect SIPC
        DisposableObject.DisposeAndNull(ref s_Sipc);
      }
    }
  }
}
