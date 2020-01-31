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
using System.Threading.Tasks;

using Azos.Conf;

namespace Azos.Apps.Terminal.Cmdlets
{

  public enum ToilType { Cpu, Ram, RamCpu }

  public class Toil : Cmdlet
  {
    public const string CONFIG_TYPE_ATTR = "type";
    public const string CONFIG_TASKS_ATTR = "tasks";
    public const string CONFIG_DURATION_ATTR = "duration";
    public const string CONFIG_RAM_ATTR = "ram";

    public Toil(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
    {

    }

    public override string Execute()
    {
      var watch = Stopwatch.StartNew();
      var type = m_Args.AttrByName(CONFIG_TYPE_ATTR).ValueAsEnum<ToilType>(ToilType.Cpu);
      switch(type)
      {
          case ToilType.Ram:
          {
              doWork(false, true);
              break;
          }

          case ToilType.RamCpu:
          {
              doWork(true, true);
              break;
          }

          default:
          {
              doWork(true, false);
              break;
          }
      }

      return "Elapsed: {0} ms.".Args( watch.ElapsedMilliseconds );
    }

    public override string GetHelp()
    {
        return
@"Loads machine with heavy work that simulates real conditions.
        Pass <f color=yellow>type<f color=gray>={<f color=darkRed>Cpu<f color=gray>|<f color=darkRed>Ram<f color=gray>|<f color=darkRed>RamCpu<f color=gray>} to specify the type of work.
        Parameters:
        <f color=yellow>tasks=int<f color=gray> - specifies how many parallel tasks to run
        <f color=yellow>duration=int_ms<f color=gray> - for how long to toil the system
        <f color=yellow>ram=int_mbytes<f color=gray> - how many mbytes to allocate
";
    }


    private void doWork(bool doCPU, bool doRAM)
    {
      var tasks = m_Args.AttrByName(CONFIG_TASKS_ATTR).ValueAsInt(4);
      var duration = m_Args.AttrByName(CONFIG_DURATION_ATTR).ValueAsInt(3000);
      var ram = m_Args.AttrByName(CONFIG_RAM_ATTR).ValueAsInt(64);

      if (tasks<1 || duration <1 || ram <1) return;

      Task[] tarr = new Task[tasks];

      var watch = Stopwatch.StartNew();
      for(int i=0; i<tasks; i++)
      tarr[i] = Task.Factory.StartNew( () =>
      {
        while(watch.ElapsedMilliseconds < duration)
        {
          if (doRAM)
          {
              var list = new List<byte[]>();
              for(int count =0; count < ram; count++)
                list.Add( new byte[1024*1024]);
          }
          if (!doCPU) System.Threading.Thread.Sleep(250);
        }
      });
      Task.WaitAll(tarr);
    }
  }
}
