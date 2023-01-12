/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azos.Sky.Fabric.Server
{
  /// <summary>
  /// Represents a changeset made to <see cref="FiberMemory"/> object.
  /// This changeset is obtained from a call to <see cref="FiberMemory.MakeDeltaSnapshot"/>
  /// and gets commited back into <see cref="IFiberStoreShard"/> using <see cref="IFiberStoreShard.CheckInAsync(FiberMemoryDelta)"/>
  /// </summary>
  public sealed class FiberMemoryDelta
  {
    public FiberId Id { get; set; }
    public int ExitCode { get; set; }
    public FiberResult Result { get; set; }

    public FiberState.Slot[] StateSlotChanges { get; set; }
  }
}
