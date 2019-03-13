/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;

using Azos.IO;
using Azos.Conf;
using Azos.Platform;
using Azos.Serialization.Arow;

namespace Azos.Tools.Arow
{
  public static class ProgramBody
  {
    public static void Main(string[] args)
    {
      try
      {
        var w = Stopwatch.StartNew();
        run(args);
        w.Stop();
        ConsoleUtils.Info("Runtime: "+w.Elapsed);
        System.Environment.ExitCode = 0;
      }
      catch(Exception error)
      {
        ConsoleUtils.Error(error.ToMessageWithType());
        System.Environment.ExitCode = -1;
      }
    }


    private static void run(string[] args)
    {
      var config = new CommandArgsConfiguration(args);


      ConsoleUtils.WriteMarkupContent( typeof(ProgramBody).GetText("Welcome.txt") );

      if (config.Root["?", "h", "help"].Exists)
      {
          ConsoleUtils.WriteMarkupContent( typeof(ProgramBody).GetText("Help.txt") );
          return;
      }


      var asmFileName = config.Root.AttrByIndex(0).Value;
      var path = config.Root.AttrByIndex(1).Value;

      if (asmFileName.IsNullOrWhiteSpace()) throw new Exception("Assembly missing");
      if (path.IsNullOrWhiteSpace()) throw new Exception("Output path is missing");

      if (!File.Exists(asmFileName)) throw new Exception("Assembly file does not exist");
      if (!Directory.Exists(path)) throw new Exception("Output path does not exist");

      var assembly = Assembly.LoadFrom(asmFileName);

      using(var generator = new CodeGenerator())
      {
        generator.RootPath = path;
        generator.CodeSegregation = config.Root["c", "code"].AttrByIndex(0).ValueAsEnum(CodeGenerator.GeneratedCodeSegregation.FilePerNamespace);
        generator.Generate( assembly );
      }
    }
  }
}
