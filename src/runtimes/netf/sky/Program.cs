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
    //kept here so we can control per-platform
    private static readonly Dictionary<string, Action<string[]>> MAPPINGS = new Dictionary<string, Action<string[]>>(StringComparer.OrdinalIgnoreCase)
    {
      {"ntc",   args => Azos.Tools.Ntc.ProgramBody.Main(args)   },
      {"phash", args => Azos.Tools.Phash.ProgramBody.Main(args) },
      {"gluec", args => Azos.Tools.Gluec.ProgramBody.Main(args) },
      {"rsc",   args => Azos.Tools.Rsc.ProgramBody.Main(args)   },
      {"trun",  args => Azos.Tools.Trun.ProgramBody.Main(args)  },
      {"tv",    args => Azos.Wave.Tv.TvProgramBody.Main(args)   },
      {"getdatetime",    args => Azos.Tools.Getdatetime.ProgramBody.Main(args)   },
      {"arow",    args => Azos.Tools.Arow.ProgramBody.Main(args)   },
    };

    private static Action<string[]> GetProcessEntryPoint(string processName)
    {
      if (MAPPINGS.TryGetValue(processName, out var body)) return body;
      return null;
    }

    private static IEnumerable<string> GetCommonProcessNames()
     => MAPPINGS.Keys.OrderBy( _=>_ );


    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
      if (args.Length < 1 || args[0].IsNullOrWhiteSpace())
      {
        Azos.IO.Console.ConsoleUtils.Error("Missing target process name: `$ sky process [arguments]`");
        Console.WriteLine("Common processes: ");
        foreach(var p in GetCommonProcessNames()) Console.WriteLine("  $ sky {0}".Args(p));
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
