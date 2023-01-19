/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;

namespace Azos.Sky.Fabric
{
  /// <summary>
  /// Performs fiber management tasks such as starting and querying fibers, sending signals etc.
  /// </summary>
  public interface IFiberManager
  {
    /// <summary>
    /// Returns a list of runspaces which are allowed in the system, e.g. `sys`, `biz` etc..
    /// Runspaces partitions fiber execution and storage zones
    /// </summary>
    IEnumerable<Atom> GetRunspaces();

    /// <summary>
    /// Reserves a new <see cref="FiberId"/> in a runspace.
    /// You may need to know FiberIds before you start them, e.g. you may need
    /// to pass future fiber identified by `FiberId` into another data structure, and if it fails, abort creating that fiber
    /// </summary>
    FiberId AllocateFiberId(Atom runspace);//  sys-log::a1->0:8:43647826346

    /// <summary>
    /// Starts a fiber
    /// </summary>
    Task<FiberInfo> StartFiberAsync(FiberStartArgs args);

    /// <summary>
    /// Filters fiber info list
    /// </summary>
    Task<IEnumerable<FiberInfo>> GetFiberListAsync(FiberFilter args);

    /// <summary>
    /// Returns fiber information by id or null if not found
    /// </summary>
    Task<FiberInfo>          GetFiberInfoAsync(FiberId idFiber);

    /// <summary>
    /// Returns fiber start parameters or null if fiber not found
    /// </summary>
    Task<FiberParameters>    GetFiberParametersAsync(FiberId idFiber);

    /// <summary>
    /// Gets fiber result object or null if fiber was not found or has not completed yet
    /// </summary>
    Task<FiberResult>        GetFiberResultAsync(FiberId idFiber);

    /// <summary>
    /// Returns most current fiber state as of the time of the call
    /// </summary>
    Task<FiberState>         GetFiberStateAsync(FiberId idFiber);

    /// <summary>
    /// Loads fiber state slot returning true on success, false if not found
    /// </summary>
    Task<bool>               LoadFiberStateSlotAsync(FiberId idFiber, FiberState.Slot slot);

    /// <summary>
    /// Sends fiber a signal returning the result or null if such fiber is not found
    /// </summary>
    Task<FiberSignalResponse>  SendSignalAsync(FiberSignal signal);

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
  }

  public interface IFiberManagerLogic : IFiberManager, IModuleImplementation
  {
  }

  /// <summary>
  /// Abstraction for the runtime processor component which executes fibers
  /// </summary>
  public interface IFiberRuntime : IApplicationComponent
  {
    /// <summary>
    /// Gets a version tag for this runtime.
    /// By convention it is an integer of a from `yyyymmdd` e.g. 20230118
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Returns an averaged current system load coefficient expressed as [0..1].
    /// 1.0 represents full system availability (e.g. low CPU/ram consumption),
    /// 0.0 represents system being maxed-out of resources.
    /// Fibers may sometimes query this number to postpone/reschedule processing of slices
    /// with heavy activity
    /// </summary>
    float SystemLoadCoefficient { get;}

    /// <summary>
    /// True while the runtime is active/running.
    /// You can use this as a polling cancellation token in fiber implementation.
    /// Fibers have to finish and yield control back ASAP when this becomes false
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// True when the runtime is launched for debugging, such as mock host.
    /// This should have limited use in business applications, as debugging code paths
    /// should be as similar as possible with real cases, however sometimes
    /// fiber implementations might need to know, for example, suppress real customer
    /// emails and direct emails at debugging endpoint instead, or add "TESTING" watermark
    /// </summary>
    bool IsDebugging { get; }
  }

}
