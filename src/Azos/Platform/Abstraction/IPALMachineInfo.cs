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
    bool IsMono { get; }
    int CurrentProcessorUsagePct { get; }
    int CurrentAvailableMemoryMb { get; }

    MemoryStatus GetMemoryStatus();
  }
}
