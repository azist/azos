/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.IO;

using Azos.Apps;
using Azos.Conf;
using Azos.Collections;
using Azos.IO;
using Azos.IO.FileSystem;
using Azos.IO.FileSystem.Local;
using Azos.IO.FileSystem.Packaging;
using Azos.Platform;

using Azos.Sky.Metabase;


namespace Azos.Sky.Tools.amm
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
      //????????????????????????????????????????
      SkySystem.MetabaseApplicationName = "WebApp";


      using (var app = new AzosApplication(allowNesting: true, args: args, rootConfig: null))
      {
        var silent = app.CommandArgs["s", "silent"].Exists;
        if (!silent)
        {
          ConsoleUtils.WriteMarkupContent(typeof(ProgramBody).GetText("Welcome.txt"));

          ConsoleUtils.Info("Build information:");
          Console.WriteLine(" Azos:     " + BuildInformation.ForFramework);
          Console.WriteLine(" Tool:     " + new BuildInformation(typeof(amm.ProgramBody).Assembly));
        }

        if (app.CommandArgs["?", "h", "help"].Exists)
        {
          ConsoleUtils.WriteMarkupContent(typeof(ProgramBody).GetText("Help.txt"));
          return;
        }


        var mbPath = app.CommandArgs
                        .AttrByIndex(0)
                        .ValueAsString(System.Environment.GetEnvironmentVariable(BootConfLoader.ENV_VAR_METABASE_FS_ROOT));

        if (!Directory.Exists(mbPath))
          throw new Exception("Specified metabase path not found");

        var fromHost = app.CommandArgs["host", "from"].AttrByIndex(0).Value;
        if (fromHost.IsNullOrWhiteSpace())
          fromHost = System.Environment.GetEnvironmentVariable(BootConfLoader.ENV_VAR_HOST_NAME);

        if (!silent)
        {
          ConsoleUtils.Info("Metabase path: " + mbPath);
          ConsoleUtils.Info("Host (this machine): " + fromHost);
        }

        var w = System.Diagnostics.Stopwatch.StartNew();

        using (var fs = new LocalFileSystem(app))
        using (var mb = new Metabank(fs, new FileSystemSessionConnectParams(), mbPath))
        {
          using (var skyApp = new SkyApplication(app, SystemApplicationType.Tool, mb, fromHost, allowNesting: false, args: null, rootConfig: null))
          {
            if (app.CommandArgs["gbm"].Exists)
              generateManifests(mb, silent);
            else
              validate(mb, silent);
          }
        }

        if (!silent)
        {
          Console.WriteLine();
          ConsoleUtils.Info("Run time: " + w.Elapsed.ToString());
        }
      }//using APP

    }

    private static void validate(Metabank mb, bool silent)
    {
      var output = new EventedList<MetabaseValidationMsg, object>();

      var ni = 0;
      var nw = 0;
      var ne = 0;

      output.GetReadOnlyEvent = (_) => false;
      output.ChangeEvent =
        (_, ct, p, i, item) =>
        {
          if (p != EventPhase.After) return;
          switch (item.Type)
          {
            case MetabaseValidationMessageType.Info: ni++; ConsoleUtils.Info(item.ToString(false), ni); break;
            case MetabaseValidationMessageType.Warning: nw++; ConsoleUtils.Warning(item.ToString(false), nw); break;
            case MetabaseValidationMessageType.Error: ne++; ConsoleUtils.Error(item.ToString(false), ne); break;
          }
          Console.WriteLine();
        };
      mb.Validate(output);

      if (!silent)
        Console.WriteLine("--------------------------------------------");

      Console.WriteLine("Infos: {0}   Warnings: {1}   Errors: {2}".Args(ni, nw, ne));

    }

    private static void generateManifests(Metabank mb, bool silent)
    {
      mb.fsAccess("amm.GenerateManifest()", Metabank.BIN_CATALOG,
        (session, dir) =>
        {
          foreach (var sdn in dir.SubDirectoryNames)
            using (var sdir = dir.GetSubDirectory(sdn))
            {
              var computed = ManifestUtils.GeneratePackagingManifest(sdir);
              var computedContent = computed.Configuration.ToLaconicString();

              if (!silent)
                ConsoleUtils.WriteMarkupContent(
                    "<push><f color=gray>Pckg: <f color=white>{0,-42}<f color=gray>  Mnfst.Sz: <f color=yellow>{1}<pop>\n".Args("'{0}'".Args(sdn), computedContent.Length));

              using (var file = sdir.CreateFile(ManifestUtils.MANIFEST_FILE_NAME))
              {
                file.WriteAllText(computedContent);
              }
            }

          return true;
        }
      );
    }
  }
}
