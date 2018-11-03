using System;

using Azos.IO;
using Azos.Conf;
using Azos.Apps;
using Azos.Platform;
using Azos.Serialization.JSON;

using Azos.Sky.Identification;

namespace Azos.Sky.Tools.agm
{
  public static class ProgramBody
  {
    public static void Main(string[] args)
    {
      try
      {
        run(args);
      }
      catch (Exception error)
      {
        ConsoleUtils.Error(error.ToMessageWithType());
        ConsoleUtils.Info("Exception details:");
        Console.WriteLine(error.ToString());

        Environment.ExitCode = -1;
      }
    }

    static void run(string[] args)
    {
      using (var app = new ServiceBaseApplication(args, null))
      {
        var silent = app.CommandArgs["s", "silent"].Exists;
        if (!silent)
        {
          ConsoleUtils.WriteMarkupContent(typeof(ProgramBody).GetText("Welcome.txt"));

          ConsoleUtils.Info("Build information:");
          Console.WriteLine(" Azos:     " + BuildInformation.ForFramework);
          Console.WriteLine(" Tool:     " + new BuildInformation(typeof(agm.ProgramBody).Assembly));
        }

        if (app.CommandArgs["?", "h", "help"].Exists)
        {
          ConsoleUtils.WriteMarkupContent(typeof(ProgramBody).GetText("Help.txt"));
          return;
        }

        var authority = app.CommandArgs.AttrByIndex(0).Value;
        var connectToAuthority = authority.ToResolvedServiceNode(false).ConnectString;
        var scope = app.CommandArgs.AttrByIndex(1).Value;
        var seq = app.CommandArgs.AttrByIndex(2).Value;
        var bsize = app.CommandArgs.AttrByIndex(3).ValueAsInt(1);

        if (!silent)
        {
          ConsoleUtils.Info("Authority:  " + authority);
          ConsoleUtils.Info("Connect to: " + connectToAuthority);
          ConsoleUtils.Info("Scope:      " + scope);
          ConsoleUtils.Info("Sequence:   " + seq);
          ConsoleUtils.Info("Block Size: " + bsize);
        }



        var w = System.Diagnostics.Stopwatch.StartNew();

        var generator = new GdidGenerator();
        generator.AuthorityHosts.Register(new GdidGenerator.AuthorityHost(connectToAuthority));


        var json = app.CommandArgs["j", "json"].Exists;
        var arr = app.CommandArgs["array"].Exists;


        if (arr) Console.WriteLine("[");

        for (var i = 0; i < bsize; i++)
        {
          var gdid = generator.GenerateOneGdid(scope, seq, bsize - i, noLWM: true);
          string line;

          if (json)
            line = new
            {
              Era = gdid.Era,
              ID = gdid.ID,
              Authority = gdid.Authority,
              Counter = gdid.Counter
            }.ToJSON(JSONWritingOptions.Compact);
          else
            line = "{0}:  {1}".Args(i, gdid);


          Console.Write(line);
          Console.WriteLine(arr && i != bsize - 1 ? ", " : " ");
        }

        if (arr) Console.WriteLine("]");

        if (!silent)
        {
          Console.WriteLine();
          ConsoleUtils.Info("Run time: " + w.Elapsed.ToString());
        }
      }//using APP

    }
  }
}
