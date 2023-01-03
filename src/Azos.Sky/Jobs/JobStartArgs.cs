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

namespace Azos.Sky.Jobs
{
  [Schema(Description = "Job start arguments")]
  [Bix("ac1affba-5829-4e13-a3fc-1334b8828644")]
  public sealed class JobStartArgs : TransientModel
  {
    [Field(Required = true,
           Description = "Unique JobId obtained from a call to `AllocateJobId()`. This job must not have started yet")]
    public JobId JobId { get; set; }

    [Field(Required = true,
          Description = "Defines what cloud origin (cluster partition) this job belongs to and runs")]
    public Atom Origin { get; set; }

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


    [Field(Required = false, Description = "When is the job scheduled to start execution, if null then job runs asap")]
    public DateTime? ScheduledStartUtc { get; set; }


    [Field(Required = false,
           Description = "What has caused this job to be created")]
    public EntityId? Initiator { get; set; }

    [Field(Required = false,
           Description = "Optionally establishes an ownership of this job")]
    public EntityId? Owner { get; set; }

    [Field(Required = false,
           MaxLength = Constraints.MAX_DESCRIPTION_LEN,
           Description = "Optionally provides a short job description")]
    public string Description { get; set; }


    [Field(Required = true,
           Description = "Job start parameters. These values are immutable for the lifetime of a job instance " +
           "(not to be confused with Job STATE which IS mutable)")]
    public JobParameters Parameters{ get; set; }

    /// <summary>
    /// Indexable tags used for future flow job searches. Tags are immutable beyond job start
    /// </summary>
    [Field(required: true, maxLength: Constraints.MAX_TAG_COUNT, Description = "Indexable tags used for future flow job searches. Tags are immutable beyond job start")]
    public List<Data.Adlib.Tag> Tags { get; set; }
  }
}
