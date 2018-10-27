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
  internal class PALMachineInfo : IPALMachineInfo
  {
    public PALMachineInfo()
    {
      m_CPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
      m_RAMAvailableCounter = new PerformanceCounter("Memory", "Available MBytes", true);
      m_IsMono = Type.GetType("Mono.Runtime") != null;
    }

    private bool m_IsMono;
    private PerformanceCounter m_CPUCounter;
    private PerformanceCounter m_RAMAvailableCounter;


    public int CurrentProcessorUsagePct { get => (int)m_CPUCounter.NextValue(); }
    public int CurrentAvailableMemoryMb { get => (int)m_RAMAvailableCounter.NextValue(); }
    public bool IsMono { get => m_IsMono; }


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
