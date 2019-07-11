/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Platform.Abstraction.NetCore
{
  internal class CoreMachineInfo : IPALMachineInfo
  {
    public CoreMachineInfo()
    {
      //https://stackoverflow.com/questions/3769405/determining-cpu-utilization
    }

    public bool IsPerformanceAvailable  => false;
    public int CurrentProcessorUsagePct => 0;
    public int CurrentAvailableMemoryMb => 0;
    public bool IsMono => false;




    public MemoryStatus GetMemoryStatus()
    {
      return new MemoryStatus();//finish
    }
  }
}
