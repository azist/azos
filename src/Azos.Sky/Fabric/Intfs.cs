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
  /// Fiber execution statuses
  /// </summary>
  public enum FiberStatus
  {
    /// <summary>
    /// The fiber was created but ts status has not been determined yet
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// The fiber has been created but has not started (yet)
    /// </summary>
    Created,

    /// <summary>
    /// The fiber is running
    /// </summary>
    Started,

    /// <summary>
    /// Paused fiber skip their time slices but still react to signals (e.g. termination), contrast with Suspended mode
    /// </summary>
    Paused,

    /// <summary>
    /// Suspended fiber do not react to signals and do not execute their time slices
    /// </summary>
    Suspended,

    /// <summary>
    /// The fiber has finished normally
    /// </summary>
    Finished = 0x1FFFF,

    /// <summary>
    /// The fiber has finished abnormally with unhandled exception
    /// </summary>
    Crashed = -1,

    /// <summary>
    /// The fiber has finished abnormally due to signal intervention, e.g. manual termination or call to Abort("reason");
    /// </summary>
    Aborted = -2
  }


  /// <summary>
  /// Performs fiber management tasks such as starting and querying jobs, sending signals etc.
  /// </summary>
  public interface IFiberManager
  {
    /// <summary>
    /// Returns a list of runspaces which are allowed in the system, e.g. `sys`, `biz` etc..
    /// Runspaces partitions fiber into atom-addressable areas
    /// </summary>
    IEnumerable<Atom> GetRunspaces();

    /// <summary>
    /// Reserves a fiber id by runspace.
    /// You may need to know FiberIds before you start them, e.g. you may need
    /// to pass future FiberId into another data structure, and if it fails, abort creating a fiber
    /// </summary>
    FiberId AllocateFiberId(Atom runspace);//  sys-log:0:8:43647826346

    /// <summary>
    /// Starts a fiber
    /// </summary>
    Task<FiberInfo> StartFiberAsync(FiberStartArgs args);

    /// <summary>
    /// Filters fiber info list
    /// </summary>
    Task<IEnumerable<FiberInfo>> GetFiberListAsync(FiberFilter args);

    /// <summary>
    /// Returns fiber by id or null
    /// </summary>
    Task<FiberInfo>          GetFiberInfoAsync(FiberId idFiber);

    /// <summary>
    /// Returns fiber start parameters or null if fiber not found
    /// </summary>
    Task<FiberParameters>    GetFiberParametersAsync(FiberId idFiber);

    /// <summary>
    /// Gets fiber result or null
    /// </summary>
    Task<FiberResult>        GetFiberResultAsync(FiberId idFiber);

    /// <summary>
    /// Returns most current fiber state
    /// </summary>
    Task<FiberState>         GetFiberStateAsync(FiberId idFiber);

    /// <summary>
    /// Loads fiber state slot returning true on success, false if not found
    /// </summary>
    Task<bool>               LoadFiberStateSlotAsync(FiberId idFiber, FiberState.Slot slot);

    /// <summary>
    /// Sends fiber a signal
    /// </summary>
    Task<FiberSignalResult>  SendSignalAsync(FiberSignal signal);
  }

  public interface IFiberManagerLogic : IFiberManager, IModuleImplementation
  {
  }

  public interface IFiberRuntime
  {

  }




  //tbd in .Server
  public interface IWorkManager
  {
    Task CreateWorkItemAsync();


    ///// <summary>
    ///// Only one worker in a work set gets particular work
    ///// </summary>
    //Task<WorkItem[]> CheckoutNextWorkSegmentAsync(Worker worker);

    //////called by worker to update the store
    //Task CommitWorkItemAsync(Worker worker, WorkItem work);
  }


}
