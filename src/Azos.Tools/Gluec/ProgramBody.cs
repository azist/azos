/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using Azos.Conf;
using Azos.Platform;
using Azos.IO;
using Azos.Glue.Tools;

namespace Azos.Tools.Gluec
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

          if (!config.Root.AttrByIndex(0).Exists)
            throw new Exception("Assembly is missing");

          var afn = config.Root.AttrByIndex(0).Value;
          if (string.IsNullOrWhiteSpace(afn))
            throw new Exception("Assembly empty path");

          afn = Path.GetFullPath(afn);

          ConsoleUtils.Info("Trying to load assembly: " + afn);

          var asm = Assembly.LoadFile(afn);

          ConsoleUtils.Info("Assembly file loaded OK");

          var tcompiler = typeof(CSharpGluecCompiler);
          var tcname = config.Root["c", "compiler"].AttrByIndex(0).Value;

          if (!string.IsNullOrWhiteSpace(tcname))
            tcompiler = Type.GetType(tcname, true);

          var compiler = Activator.CreateInstance(tcompiler, new object[] { asm }) as GluecCompiler;

          if (compiler==null) throw new Exception("Could not create compiler type");

          compiler.OutPath = Path.GetDirectoryName(afn);
          compiler.FilePerContract = true;
          compiler.NamespaceFilter = config.Root["f", "flt", "filter"].AttrByIndex(0).Value;

          ConfigAttribute.Apply(compiler, config.Root["o", "opt", "options"]);

          ConsoleUtils.Info("Namespace filter: " + compiler.NamespaceFilter);
          compiler.Compile();


        }


    }
}
