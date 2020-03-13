/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;

using Azos.IO.Console;
using Azos.Conf;
using Azos.Platform;
using Azos.Serialization.Arow;
using Azos.Serialization.JSON;
using System.Linq;
using Azos.Data;

namespace Azos.Tools.Arow
{
  /// <summary>
  /// Amorphous Row (Arow) serializer generator
  /// </summary>
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

      //20191106 DKh
      //var assembly = Assembly.LoadFrom(asmFileName);
      Assembly assembly;
      try
      {
        assembly = Assembly.LoadFrom(asmFileName);

        var allTypes = CodeGenerator.DefaultScanForAllRowTypes(assembly);

        //this throws on invalid loader exception as
        var schemas = allTypes.Select(t => Schema.GetForTypedDoc(t)).ToArray();//it touches all type/schemas because of .ToArray()
        ConsoleUtils.Info("Assembly contains {0} data document schemas".Args(schemas.Length));
      }
      catch(Exception asmerror)
      {
        ConsoleUtils.Error("Could not load assembly: `{0}`".Args(asmFileName));
        ConsoleUtils.Warning("Exception: ");
        ConsoleUtils.Warning(asmerror.ToMessageWithType());
        Console.WriteLine();
        ConsoleUtils.Warning(new WrappedExceptionData(asmerror).ToJson(JsonWritingOptions.PrettyPrintASCII));
        throw;
      }

      using(var generator = new CodeGenerator())
      {
        generator.RootPath = path;
        generator.CodeSegregation = config.Root["c", "code"].AttrByIndex(0).ValueAsEnum(CodeGenerator.GeneratedCodeSegregation.FilePerNamespace);
        generator.HeaderDetailLevel = config.Root["hdr", "header", "hl"].AttrByIndex(0).ValueAsInt(255);
        generator.Generate( assembly );
      }
    }
  }
}
