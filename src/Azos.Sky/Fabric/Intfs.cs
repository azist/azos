/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Data.Business;
using Azos.Serialization.JSON;

namespace Azos.Sky.Fabric
{
  /// <summary>
  /// Job statuses
  /// </summary>
  public enum FiberStatus
  {
    /// <summary>
    /// The job was created but ts status has not been determined yet
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// The job has been created but has not started (yet)
    /// </summary>
    Created,

    /// <summary>
    /// The job is running
    /// </summary>
    Started,

    /// <summary>
    /// Paused jobs skip their time slices but still react to signals (e.g. termination), contrast with Suspended mode
    /// </summary>
    Paused,

    /// <summary>
    /// Suspended jobs do not react to signals and do not execute their time slices
    /// </summary>
    Suspended,

    /// <summary>
    /// The job has finished normally
    /// </summary>
    Finished = 0x1FFFF,

    /// <summary>
    /// The job has finished abnormally with unhandled exception
    /// </summary>
    Crashed = -1,

    /// <summary>
    /// The job has finished abnormally due to signal intervention, e.g. manual termination or call to Abort("reason");
    /// </summary>
    Aborted = -2
  }



  //?????????????????????????????????
  //Who jobs execute as?  Should we have impersonate prinicpal in job args?
  //?????????????????????????????????
  //?????????????????????????????????
  //?????????????????????????????????
  //?????????????????????????????????
  //?????????????????????????????????
  //?????????????????????????????????
  //Security depends on JOB state, for example an operator X may not be able to query job which deal with payroll of their superior boss
  // IMpersonateAs and signals use caller identity




  /// <summary>
  /// Performs job management tasks such as starting and querying jobs, sending signals etc.
  /// </summary>
  public interface IFiberManager
  {
    /// <summary>
    /// Returns a list of runspaces which are allowed in the system, e.g. `sys`, `biz` etc..
    /// Runspaces partitions jobs into atom-addressable areas
    /// </summary>
    IEnumerable<Atom> GetRunspaces();

    /// <summary>
    /// Reserves a job id by runspace.
    /// You may need to know JobIds before you start them, e.g. you may need
    /// to pass future JobId into another data structure, and if it fails, abort creating a job
    /// </summary>
    FiberId AllocateFiberId(Atom runspace);//  sys-log:0:8:43647826346

    Task<FiberInfo> StartFiberAsync(FiberStartArgs args);

    Task<IEnumerable<FiberInfo>> GetFiberListAsync(FiberFilter args);

    Task<FiberInfo>          GetFiberInfoAsync(FiberId idFiber);
    Task<FiberParameters>    GetFiberParametersAsync(FiberId idFiber);
    Task<FiberResult>        GetFiberResultAsync(FiberId idFiber);
    Task<FiberState>         GetFiberStateAsync(FiberId idFiber);
    Task                     LoadFiberStateSlotAsync(FiberId idFiber, FiberState.Slot slot);
    Task<FiberSignalResult>  SendSignalAsync(FiberSignal signal);
  }

  public interface IFiberManagerLogic : IFiberManager, IModuleImplementation
  {
  }

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
