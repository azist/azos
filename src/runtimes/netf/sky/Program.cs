/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos;

namespace sky
{
  class Program
  {
    private static readonly Dictionary<string, Action<string[]>> MAPPINGS = new Dictionary<string, Action<string[]>>(StringComparer.OrdinalIgnoreCase)
    {
      {"ntc", args => Azos.Tools.Ntc.ProgramBody.Main(args) },
    };

    private static Action<string[]> GetProcessEntryPoint(string processName)
    {
      if (MAPPINGS.TryGetValue(processName, out var body)) return body;
      return null;
    }


    static void Main(string[] args)
    {
       new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
       if (args.Length < 1 || args[0].IsNullOrWhiteSpace())
       {
         Azos.IO.Console.ConsoleUtils.Error("Missing target process name: `$sky process [arguments]`");
         Environment.ExitCode = -1;
         return;
       }

       Ambient.SetProcessName(args[0]);

       var entryPoint = GetProcessEntryPoint(Ambient.ProcessName);
       if (entryPoint == null)
       {
        Azos.IO.Console.ConsoleUtils.Error("Unknown process `{0}`".Args(Ambient.ProcessName));
        Environment.ExitCode = -1;
        return;
      }

       var processArgs = args.Skip(1).ToArray();
       entryPoint(processArgs);
    }
  }
}
