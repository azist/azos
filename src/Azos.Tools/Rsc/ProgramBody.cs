/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;
using System.IO;

using Azos.Data.Modeling;
using Azos.Conf;
using Azos.IO;
using Azos.Platform;

namespace Azos.Tools.Rsc
{
    /// <summary>
    /// Program body (entry point) for Relation Schema Compiler tool
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

          if (!config.Root.AttrByIndex(0).Exists)
            throw new Exception("Schema file missing");

          var schemaFileName = config.Root.AttrByIndex(0).Value;
          if (string.IsNullOrWhiteSpace(schemaFileName))
            throw new Exception("Schema empty path");

          schemaFileName = Path.GetFullPath(schemaFileName);

          ConsoleUtils.Info("Trying to load schema: " + schemaFileName);

          var schema = new Schema(schemaFileName, new string[] { Path.GetDirectoryName(schemaFileName) });

          ConsoleUtils.Info("Schema file loaded OK");

          var tcompiler = typeof(MsSqlCompiler);
          var tcname = config.Root["c", "compiler"].AttrByIndex(0).Value;

          if (!string.IsNullOrWhiteSpace(tcname))
            tcompiler = Type.GetType(tcname, true);

          var compiler = Activator.CreateInstance(tcompiler, new object[] { schema }) as Compiler;

          if (compiler==null) throw new Exception("Could not create compiler type");



          compiler.OutputPath = Path.GetDirectoryName(schemaFileName);

          var options = config.Root["o","opt","options"];
          if (options.Exists)
            compiler.Configure(options);



          ConsoleUtils.Info("Compiler information:");
          Console.WriteLine("   Type={0}\n   Name={1}\n   Target={2}".Args(compiler.GetType().FullName, compiler.Name, compiler.Target) );
          if (compiler is RDBMSCompiler)
            Console.WriteLine("   DomainsSearchPath={0}".Args(((RDBMSCompiler)compiler).DomainSearchPaths) );
          Console.WriteLine("   OutPath={0}".Args(compiler.OutputPath) );
          Console.WriteLine("   OutPrefix={0}".Args(compiler.OutputPrefix) );
          Console.WriteLine("   CaseSensitivity={0}".Args(compiler.NameCaseSensitivity) );

          compiler.Compile();



          foreach(var error in compiler.CompileErrors)
            ConsoleUtils.Error(error.ToMessageWithType());


          if (compiler.CompileException!=null)
          {
            ConsoleUtils.Warning("Compile exception thrown");
            throw compiler.CompileException;
          }
        }

    }
}
