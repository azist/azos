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
  /// Memory statuses
  /// </summary>
  public enum MemoryStatus
  {
    Unset = 0,

    /// <summary>
    /// The memory is locked for the caller - the caller may mutate the memory and then call <see cref="IFiberStoreShard.CheckInAsync(FiberMemory)"/>
    /// </summary>
    LockedForCaller = 1,

    /// <summary>
    /// The memory is locked by some other processor and memory representation may be incomplete.
    /// You may NOT change this memory at this time
    /// </summary>
    LockedSomeoneElse
  }

  /// <summary>
  /// Represents a piece of TRANSITIVE virtual persisted memory which backs-up fiber execution: its start parameters, transitive state
  /// and result object. FiberMemory gets operated on by processors.
  /// </summary>
  public sealed class FiberMemory
  {
    private MemoryStatus m_Status;
    private Atom m_ShardId;

    public MemoryStatus Status => m_Status;

    /// <summary>
    /// Which cluster shard the data resides in.
    /// The sharding may NOT change during origin system runtime
    /// </summary>
    public Atom ShardId => m_ShardId;

  }
}
