/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;


using Azos.Platform.Abstraction;

namespace Azos.Platform
{
  /// <summary>
  /// Denotes primary OS Families: Win/Mac/Lin*nix
  /// </summary>
  public enum OSFamily
  {
    Undetermined = 0,
    Windows = 1,
    PosixSystems = 100,
    Linux = PosixSystems + 1,
    BSD = 200,
    Mac = 500,
    Unix = 1000
  }

  /// <summary>
  /// Provides current memory status snapshot
  /// </summary>
  [Serializable]
  public struct MemoryStatus
  {
    public uint LoadPct { get;  set; }

    public ulong TotalPhysicalBytes { get;  set; }
    public ulong AvailablePhysicalBytes { get;  set; }

    public ulong TotalPageFileBytes { get;  set; }
    public ulong AvailablePageFileBytes { get;  set; }

    public ulong TotalVirtBytes { get;  set; }
    public ulong AvailableVirtBytes { get;  set; }
  }


  /// <summary>
  /// Facilitates various computer-related tasks such as CPU usage, memory utilization etc.
  /// </summary>
  public static class Computer
  {

    static Computer()
    {
      //20210122 DKh #410
      _____SetHostName(null);//do this once in .ctor as it is very slow on every get
    }


    private static string s_HostName;

    /// <summary>
    /// Internal framework method which sets the logical name for local host.
    /// Not to be called by developers
    /// </summary>
    public static void _____SetHostName(string host)
    {
      s_HostName = host;
      //20210122 GIT#410 DKh MachineName.get is very slow on .Net 4.7/windows, hence the caching
      if (s_HostName.IsNullOrWhiteSpace()) s_HostName = Environment.MachineName;
    }

    /// <summary>
    /// Returns this host name. If the specific host name was not set, then local Envirionemnt.MachineName is returned
    /// </summary>
    public static string HostName => s_HostName;


    /// <summary>
    /// Returns current computer-wide CPU utilization percentage
    /// </summary>
    public static int CurrentProcessorUsagePct
    {
      get
      {
        return PlatformAbstractionLayer.MachineInfo.CurrentProcessorUsagePct;
      }
    }


    /// <summary>
    /// Returns current computer-wide RAM availability in mbytes
    /// </summary>
    public static int CurrentAvailableMemoryMb
    {
      get
      {
        return PlatformAbstractionLayer.MachineInfo.CurrentAvailableMemoryMb;
      }
    }


    private static OSFamily s_OSFamily;

    /// <summary>
    /// Rsturns OS family for this computer: Linux vs Win vs Mac
    /// </summary>
    public static OSFamily OSFamily
    {
      get
      {
        if (s_OSFamily != OSFamily.Undetermined) return s_OSFamily;

        switch (System.Environment.OSVersion.Platform)
        {
          case PlatformID.Unix:
            {
              // Need to check for Mac-specific root folders, because Mac may get reported as UNIX
              if (System.IO.Directory.Exists("/Applications")
                  && System.IO.Directory.Exists("/System")
                  && System.IO.Directory.Exists("/Users")
                  && System.IO.Directory.Exists("/Volumes"))
              {
                s_OSFamily = OSFamily.Mac;
                break;
              }
              else
              {
                s_OSFamily = OSFamily.Linux;
                break;
              }
            }

          case PlatformID.MacOSX: { s_OSFamily = OSFamily.Mac; break; }

          default: { s_OSFamily = OSFamily.Windows; break; };
        }

        return s_OSFamily;
      }

    }


    private static string s_UniqueNetworkSignature;

    /// <summary>
    /// Returns network signature for this machine which is unique in the enclosing network segment (MAC-based)
    /// </summary>
    public static string UniqueNetworkSignature
    {
      get
      {
        if (s_UniqueNetworkSignature == null)
          s_UniqueNetworkSignature = NetworkUtils.GetMachineUniqueMACSignature();

        return s_UniqueNetworkSignature;
      }
    }

    public static bool IsMono { get { return PlatformAbstractionLayer.MachineInfo.IsMono; } }


    public static MemoryStatus GetMemoryStatus()
    {
      return PlatformAbstractionLayer.MachineInfo.GetMemoryStatus();
    }

  }
}