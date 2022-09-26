/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Azos.Conf;
using Azos.Platform;

namespace Azos.Apps.Terminal.Cmdlets
{

  public sealed class Perf : Cmdlet
  {
    public const string CONFIG_DURATION_ATTR = "duration";
    public const string CONFIG_SAMPLE_ATTR = "sample";


    public Perf(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
    {

    }

    public override string Execute()
    {
      var duration = m_Args.AttrByName(CONFIG_DURATION_ATTR).ValueAsInt(8000);
      var sample = m_Args.AttrByName(CONFIG_SAMPLE_ATTR).ValueAsInt(500);

      if (duration<1000 || sample <100) return "Sampling rate must be > 100ms and duration >1000ms";


      var watch = Stopwatch.StartNew();

      var lst = new List<Tuple<int, int, int>>();

      while(watch.ElapsedMilliseconds<duration)
      {
          var cpu = Computer.CurrentProcessorUsagePct;
          var ram = Computer.GetMemoryStatus().LoadPct;

          lst.Add(Tuple.Create((int)watch.ElapsedMilliseconds, cpu, (int)ram));
          System.Threading.Thread.Sleep(sample);
      }

      var result = new StringBuilder(1024);
      const int maxw = 70;

      for(int i=0; i<lst.Count; i++)
      {
          var cpu = (int)(maxw * (lst[i].Item2 / 100d));
          var ram = (int)(maxw * (lst[i].Item3 / 100d));
          var l = "";
          if (ram<=cpu)
          {
              l = l.PadLeft(ram, '#').PadRight(cpu-ram, '=');
          }
          else
              l = l.PadLeft(cpu, '#').PadRight(ram-cpu, '|');

          l = l.PadRight(maxw, '·');

          l = (lst[i].Item1 / 1000d).ToString("F1").PadLeft(5) + l;

          result.AppendLine( l );
      }

      result.AppendLine(@"____________________________________________________________________________");
      result.AppendLine(@" Cpu: '==='    Ram: '|||'    Cpu+Ram: '###'    Available: {0} Mb.".Args(Computer.CurrentAvailableMemoryMb));

      return result.ToString();
    }

    public override string GetHelp()
    {
      return
@"Measures machine performance during the specified interval.
        Parameters:
        <f color=yellow>duration=int_ms<f color=gray> - specifies measurement interval length.
            The value must be >1000ms
        <f color=yellow>sample=int_ms<f color=gray> - specifies measurement sampling rate.
            The value must be > 100ms
";
    }
  }

}
