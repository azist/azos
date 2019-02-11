/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;


namespace Azos.Platform.Abstraction
{
  /// <summary>
  /// Provides functions for getting Machine/OS info like CPU and RAM usage
  /// </summary>
  public interface IPALMachineInfo
  {
    /// <summary>
    /// Returns true if the process is hosted by Mono runtime
    /// </summary>
    bool IsMono { get; }

    /// <summary>
    /// Returns true if the process was able to get access to system-wide CPU/RAM descriptors,
    /// (e.g. PerformanceCounters may be used on Windows)
    /// </summary>
    /// <remarks>
    /// If this member returns false, then the PAL could not obtain system information and
    /// consequently CurrentProcessorUsagePct and CurrentAvailableMemoryMb may not be relied upon
    /// </remarks>
    bool IsPerformanceAvailable {  get; }
    int CurrentProcessorUsagePct { get; }
    int CurrentAvailableMemoryMb { get; }

    MemoryStatus GetMemoryStatus();
  }
}
