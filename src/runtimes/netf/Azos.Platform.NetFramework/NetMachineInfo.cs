/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Diagnostics;

using System.Runtime.InteropServices;

namespace Azos.Platform.Abstraction.NetFramework
{
  internal class NetMachineInfo : IPALMachineInfo
  {
    public NetMachineInfo()
    {
      try
      {
        //see https://stackoverflow.com/questions/12435869/getting-the-cpu-usage-generates-category-does-not-exist-error
        //see https://superuser.com/questions/384554/whats-the-difference-between-processor-and-processor-information-in-perfmon-cou
        try
        {
          m_CPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
        }
        catch
        {
          m_CPUCounter = new PerformanceCounter("Processor Information", "% Processor Time", "_Total", true);
        }
      }
      catch
      {
        //no where to log
        m_CPUCounter = null;
      }

      try
      {
        m_RAMAvailableCounter = new PerformanceCounter("Memory", "Available MBytes", true);
      }
      catch
      {
        //no where to log
        m_RAMAvailableCounter = null;
      }

      m_IsMono = Type.GetType("Mono.Runtime") != null;
    }

    private bool m_IsMono;
    private PerformanceCounter m_CPUCounter;
    private PerformanceCounter m_RAMAvailableCounter;


    public bool IsPerformanceAvailable  => m_CPUCounter != null && m_RAMAvailableCounter != null;
    public int CurrentProcessorUsagePct => m_CPUCounter != null          ? (int)m_CPUCounter.NextValue() : 0;
    public int CurrentAvailableMemoryMb => m_RAMAvailableCounter != null ? (int)m_RAMAvailableCounter.NextValue() : 0;
    public bool IsMono => m_IsMono;


    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class MEMORYSTATUSEX
    {
      public uint dwLength;
      public uint dwMemoryLoad;
      public ulong ullTotalPhys;
      public ulong ullAvailPhys;
      public ulong ullTotalPageFile;
      public ulong ullAvailPageFile;
      public ulong ullTotalVirtual;
      public ulong ullAvailVirtual;
      public ulong ullAvailExtendedVirtual;
      public MEMORYSTATUSEX()
      {
        this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
      }
    }


    public MemoryStatus GetMemoryStatus()
    {
      if (IsMono) return new MemoryStatus();
      var stat = new MEMORYSTATUSEX();

      if (Computer.OSFamily == OSFamily.Windows)
        GlobalMemoryStatusEx(stat);

      return new MemoryStatus()
      {
        LoadPct = stat.dwMemoryLoad,

        TotalPhysicalBytes = stat.ullTotalPhys,
        AvailablePhysicalBytes = stat.ullAvailPhys,

        TotalPageFileBytes = stat.ullTotalPageFile,
        AvailablePageFileBytes = stat.ullAvailPageFile,

        TotalVirtBytes = stat.ullTotalVirtual,
        AvailableVirtBytes = stat.ullAvailVirtual
      };
    }
  }
}
