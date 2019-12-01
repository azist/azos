using System;
using System.Collections.Generic;
using System.Text;

using Azos.Apps;
using Azos.IO.Console;
using Azos.Platform;

namespace Azos.Wave.Tv
{
  /// <summary>
  /// Television program body
  /// </summary>
  public static class TvProgramBody
  {
    public static void Main(string[] args)
    {
      try
      {
        using (var app = new AzosApplication(true, args, null))
        {
          app.SetConsolePort(LocalConsolePort.Default);
          System.Environment.ExitCode = runServer(app);
        }
        ConsoleUtils.Info("{0} exit time".Args(DateTime.UtcNow));
      }
      catch (Exception error)
      {
        ConsoleUtils.Error(error.ToMessageWithType());
        ConsoleUtils.Warning(error.StackTrace);
        Console.WriteLine();
        System.Environment.ExitCode = -1;
      }
    }

    private static int runServer(AzosApplication app)
    {
      var config = app.CommandArgs;

      ConsoleUtils.WriteMarkupContent(typeof(TvProgramBody).GetText("Welcome.txt"));


      if (config["?"].Exists ||
          config["h"].Exists ||
          config["help"].Exists)
      {
        ConsoleUtils.WriteMarkupContent(typeof(TvProgramBody).GetText("Help.txt"));
        return 0;
      }

      using (var web = new WaveServer(app))
      {
        web.Configure(null);
        web.Start();
        ConsoleUtils.Info("{0} start time".Args(DateTime.UtcNow));

        Console.WriteLine("Server is listening on:");
        web.Prefixes.ForEach(p => Console.WriteLine("     {0}".Args(p)));
        Console.WriteLine();
        Console.WriteLine("Strike <ENTER> to terminate ...");

        new Azos.Log.Sinks.ConsoleSink((Azos.Log.Sinks.ISinkOwner)app.Log, "con", 0){  Colored =true }.Start();

        Console.ReadLine();

        Console.WriteLine("...terminating");
        Console.WriteLine();
      }

      return 0;
    }

 }
}
