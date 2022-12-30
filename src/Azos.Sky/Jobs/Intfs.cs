using System;
using System.Collections.Generic;
using System.Text;
/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Data.Business;

namespace Azos.Sky.Jobs
{
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


 //   Task<JobInfo> StartJobAsync(JobStartArgs args);

//     Task<IEnumerable<JobInfo>> GetJobListAsync(JobFilter args);

//     Task<JobInfo> SendSignalAsync(JobId idJob, Signal signal);
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
