/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Collections;


namespace Azos.Sky.Fabric.Server
{
  /// <summary>
  /// Abstraction of fiber persisted store - where the system stores the state of fibers
  /// </summary>
  public interface IFiberStoreShard : IAtomNamed, IApplicationComponent
  {
    /// <summary>
    /// Starts a fiber
    /// </summary>
    Task<FiberInfo> StartFiberAsync(FiberStartArgs args);

    /// <summary>
    /// Filters fibers of this shard info a list
    /// </summary>
    Task<IEnumerable<FiberInfo>> GetFiberListAsync(FiberFilter args);

    /// <summary>
    /// Returns fiber information by id or null if not found; doe not take lock as the data for view-only
    /// </summary>
    Task<FiberInfo> GetFiberInfoAsync(FiberId idFiber);

    /// <summary>
    /// Returns fiber start parameters or null if fiber not found
    /// </summary>
    Task<FiberParameters> GetFiberParametersAsync(FiberId idFiber);

    /// <summary>
    /// Gets fiber result object or null if fiber was not found or has not completed yet
    /// </summary>
    Task<FiberResult> GetFiberResultAsync(FiberId idFiber);

    /// <summary>
    /// Returns most current fiber state as of the time of the call
    /// </summary>
    Task<FiberState> GetFiberStateAsync(FiberId idFiber);

    /// <summary>
    /// Loads fiber state slot returning true on success, false if not found
    /// </summary>
    Task<bool> LoadFiberStateSlotAsync(FiberId idFiber, FiberState.Slot slot);


    /// <summary>
    /// Sets a new priority 0.01 .. 100.00. Null returned if fiber is not found
    /// </summary>
    Task<FiberInfo> SetPriorityAsync(FiberId idFiber, float priority, string statusDescription);

    /// <summary>
    /// Pauses fibers. Null returned if fiber is not found.
    /// A paused fiber does not execute its next scheduled slice, however it still responds to signals.
    /// Contrast with <see cref="SuspendAsync(FiberId, string)"/> which suspends fibers
    /// </summary>
    Task<FiberInfo> PauseAsync(FiberId idFiber, string statusDescription);

    /// <summary>
    /// Suspends fibers. Null returned if fiber is not found.
    /// A suspended fiber does not respond to signals or run scheduled slices. Contrast with <see cref="PauseAsync(FiberId, string)"/>
    /// which only prevents fibers from running scheduled slices but still reacts to signals
    /// </summary>
    Task<FiberInfo> SuspendAsync(FiberId idFiber, string statusDescription);

    /// <summary>
    /// Resumes fibers in <see cref="FiberStatus.Paused"/> or <see cref="FiberStatus.Suspended"/> states.
    /// Null returned if fiber is not found
    /// </summary>
    Task<FiberInfo> ResumeAsync(FiberId idFiber, string statusDescription);

    /// <summary>
    /// Aborts fiber. Null returned if fiber is not found.
    /// An abort is an ABNORMAL deterministic termination of fiber execution which prevents it
    /// from running time slices or responding to signals.
    /// Abort is different from normal completion in its intent, e.g.
    /// you can abort a fiber which represents a business activity which is no longer needed to run
    /// </summary>
    Task<FiberInfo> AbortAsync(FiberId idFiber, string statusDescription);

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

    /// <summary>
    /// Undo checkout
    /// </summary>
    Task UndoCheckoutAsync(FiberId idFiber);
  }

  public interface IFiberStoreShardLogic : IFiberStoreShard, IModuleImplementation
  {

  }
}
