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

namespace Azos.Sky.Jobs
{
  /// <summary>
  /// Job statuses
  /// </summary>
  public enum JobStatus
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


  /// <summary>
  /// Performs job management tasks such as starting and querying jobs, sending signals etc.
  /// </summary>
  public interface IJobManager
  {
    /// <summary>
    /// Reserves a job id by runspace.
    /// You may need to know JobIds before you start them, e.g. you may need
    /// to pass future JobId into another data structure, and if it fails, abort creating a job
    /// </summary>
    JobId AllocateJobId(Atom runspace);//  sys-log:0:8:43647826346

    Task<JobInfo> StartJobAsync(JobStartArgs args);

  //  Task<IEnumerable<JobInfo>> GetJobListAsync(JobFilter args);

    Task<JobInfo>       GetJobInfoAsync(JobId idJob);
    Task<JobParameters> GetJobParametersAsync(JobId idJob);
    Task<JobResult>     GetJobResultAsync(JobId idJob);
    Task<State>         GetJobStateAsync(JobId idJob);
    Task                LoadJobStateSlotAsync(JobId idJob, State.Slot slot);
    Task<SignalResult>  SendSignalAsync(Signal signal);
  }

  public interface IJobManagerLogic : IJobManager, IModuleImplementation
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
