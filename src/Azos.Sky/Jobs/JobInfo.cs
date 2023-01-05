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
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Sky.Jobs
{
  [Schema(Description = "Provides job header information")]
  [Bix("aec8ff3e-d555-46e2-8eaa-ea7d8f7cceee")]
  public sealed class JobInfo : TransientModel
  {
    [Field(Required = true, Description = "Unique JobId initially obtained from a call to `AllocateJobId()`")]
    public JobId JobId { get; set; }

    [Field(Required = true, Description = "Job Guid used for log and other correlation token")]
    public Guid JobGuid { get; set; }

    [Field(Required = true,
          Description = "Defines what cloud origin (cluster partition) this job belongs to and runs")]
    public Atom Origin { get; set; }

    [Field(Required = true, Description = "Current job status")]
    public JobStatus JobStatus { get; set; }

    [Field(Required = false,
           MaxLength = Constraints.MAX_DESCRIPTION_LEN,
           Description = "Optionally provides current job status description")]
    public string StatusDescription { get; set; }


    [Field(Required = true, Description = "Current job status")]
    public int ExitCode { get; set; }


    [Field(Required = false, Description = "Exception data in case of failure or null")]
    public WrappedExceptionData Exception { get; set; }


    [Field(Required = true,
           Description = "Uniquely identifies the type of process image which backs this job execution. " +
                         "In CLR runtime, this maps to a descendant type of a `Job` class via BIX mapping")]
    public Guid ImageTypeId { get; set; }


    [Field(Required = false,
           MaxLength = Constraints.MAX_STRAND_LEN,
           Description = "Optionally, ensures sequential processing of jobs within the same strand, that is:" +
                         " no more than a single job instance of the same strand ever executes in the system origin (cloud partition) concurrently." +
                         "Strands can only be defined at job start and are afterwards immutable")]
    public string Strand { get; set; }

    [Field(Required = false,
           MaxLength = Constraints.MAX_DESCRIPTION_LEN,
           Description = "Describes who execution session is impersonated as or null if it is executed under a system account. " +
                         "Note: this is a descriptive string for humans, not a token which can be used to perform any auth functionality")]
    public string ImpersonationTitle{ get; set; }


    [Field(Required = true,
           MaxLength = Constraints.MAX_DESCRIPTION_LEN,
           Description = "Optionally provides a short job description")]
    public string Description { get; set; }

    [Field(Required = false,
           Description = "What/who has caused this job to be created")]
    public EntityId? Initiator { get; set; }

    [Field(Required = false,
           Description = "Optionally establishes an ownership of this job")]
    public EntityId? Owner { get; set; }

    [Field(Required = true, Description = "When job was created")]
    public DateTime CreateUtc { get; set; }

    [Field(Required = true, Description = "When is the job scheduled to run its next time slice")]
    public DateTime NextSliceUtc { get; set; }

    [Field(Required = true, Description = "How long the last execution slice took")]
    public int LastSliceDurationMs { get; set; }

    /// <summary>
    /// Indexable tags used for future flow job searches. Tags are immutable beyond job start
    /// </summary>
    [Field(required: true,
           maxLength: Constraints.MAX_TAG_COUNT,
           Description = "Indexable tags used for future flow job searches. Tags are immutable beyond job start")]
    public List<Data.Adlib.Tag> Tags { get; set; }

  }
}
