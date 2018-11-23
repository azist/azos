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
using Azos.IO;
using Azos.Platform;
using Azos.Scripting;
using Azos.Templatization;

namespace Azos.Tools.Trun
{
    public static class ProgramBody
    {
        public static void Main(string[] args)
        {
          try
          {
           using(var app = new ServiceBaseApplication(true, args, null))
           {
             Console.CancelKeyPress += (_, e) => { app.Stop(); e.Cancel = true;};

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


        private static int run(ServiceBaseApplication app)
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


          var assemblies = config.Attributes
                                 .Select( a => Assembly.LoadFrom(a.Value))
                                 .ToArray();

          if (assemblies.Length==0)
          {
            ConsoleUtils.Error("No assemblies to run");
            return -2;
          }


          Console.ForegroundColor =  ConsoleColor.DarkGray;
          Console.Write("Platform runtime: ");
          Console.ForegroundColor =  ConsoleColor.Yellow;
          Console.WriteLine(Platform.Abstraction.PlatformAbstractionLayer.PlatformName);
          Console.ForegroundColor =  ConsoleColor.Gray;

          var hnode = config["host"];
          var rnode = config["r", "runner"];

          var errors = 0;
          using(var host =  FactoryUtils.MakeAndConfigureComponent<IRunnerHost>(app, hnode, typeof(TestRunnerConsoleHost)))
          {
            Console.ForegroundColor =  ConsoleColor.DarkGray;
            Console.Write("Runner host: ");
            Console.ForegroundColor =  ConsoleColor.Yellow;
            Console.WriteLine(host.GetType().DisplayNameWithExpandedGenericArgs());
            Console.ForegroundColor =  ConsoleColor.Gray;

            foreach(var asm in assemblies)
            {
              using(var runner =  FactoryUtils.MakeDirectedComponent<Runner>(host, rnode, typeof(Runner), new object[]{asm, rnode}))
              {
                Console.WriteLine("Assembly: {0}".Args(asm));
                Console.WriteLine("Runner: {0}".Args(runner.GetType().DisplayNameWithExpandedGenericArgs()));
                Console.WriteLine();

                runner.Run();
                errors += host.TotalErrors;

                Console.WriteLine();
                Console.WriteLine();
              }
            }
          }//using host

          return errors > 0 ? -1 : 0;
        }



    }
}
