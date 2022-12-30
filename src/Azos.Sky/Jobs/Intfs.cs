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

namespace Azos.Sky.Jobs
{
  public interface IJobManager
  {
    void AllocJobId();

    // Task<JobId> StartJob(JobStartArgs args)
    // Task<JobInfo> QueryJob(JobQueryArgs args)

    // Task<JobId> SendSignal(Signal signal)
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



  public class JobStartArgs
  {
    [Field(Description = "Uniquely identifies the type of process image which backs this job execution. " +
                         "In CLR runtime, this maps to a descendant type of a `Job` class via BIX mapping")]
    public Guid ProcessImage { get; set; }


    [Field(Description = "Optionally, ensures sequential processing of jobs within the same strand, that is:" +
                         " no more than a single job instance of the same strand ever executes in the system origin (cloud partition) concurrently." +
                         "Strands can only be defined at job start and are afterwards immutable")]
    public string Strand { get; set; }

    //tags are immutable once job is launched - see adlib/formflow
    //public tag[] Tags
  }


}
