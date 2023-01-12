/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Collections;


namespace Azos.Sky.Fabric.Server
{
  /// <summary>
  /// Abstraction of fiber persisted store - where the system stores the state of fibers
  /// </summary>
  public interface IFiberStoreShard : INamed, IApplicationComponent
  {
    /// <summary>
    /// Shard Id. Must be immutable for lifetime of shard
    /// </summary>
    Atom Id { get; }

    /// <summary>
    /// Checks-out next pending fiber work item within a runspace and takes the lock
    /// for the specified processor. Null if there is nothing pending
    /// </summary>
    Task<FiberMemory> CheckOutNextPendingAsync(Atom runspace, Atom procId);

    /// <summary>
    /// Tries to check-out a specific fiber by its id, or null if it is not found.
    /// Check the <see cref="FiberMemory.Status"/> to make sure it is not locked, e.g. when
    /// a signal is being applied a fiber may be in locked state and the caller may need to wait/abort
    /// </summary>
    Task<FiberMemory> TryCheckOutSpecificAsync(FiberId idFiber, Atom procId);

    /// <summary>
    /// Checks-in fiber data and releases the lock
    /// </summary>
    Task<bool> CheckInAsync(FiberMemoryDelta fiber);
  }
}
