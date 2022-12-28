using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;

namespace Azos.Sky.Jobs
{
  public interface IJobManager
  {
   // Task<JobId> StartJob(JobStartArgs args)
   // Task<JobInfo> QueryJob(JobQueryArgs args)
  }

  public interface IJobManagerLogic : IJobManager, IModuleImplementation
  {
  }

  public interface IJobStateStore
  {
    //IEnumerable<WorkItem> GetWorkSegment(int worker);

    ////called by worker to update the store
    //void CommitWorkItem(int worker, JobId id, Datime nextSlice, JsonDataMap state);
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


  public struct JobId
  {
    public readonly GDID Gdid;

  }


}
