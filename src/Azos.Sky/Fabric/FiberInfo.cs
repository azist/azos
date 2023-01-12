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

namespace Azos.Sky.Fabric
{
  [Schema(Description = "Provides fiber information")]
  [Bix("aec8ff3e-d555-46e2-8eaa-ea7d8f7cceee")]
  public sealed class FiberInfo : TransientModel
  {
    [Field(Required = true, Description = "Unique FiberId initially obtained from a call to `AllocateFiberId()`")]
    public FiberId Id { get; set; }

    [Field(Required = true, Description = "Fiber Guid used for log and other correlation token")]
    public Guid FiberGuid { get; set; }

    [Field(Required = true,
          Description = "Defines what cloud origin (cluster partition) this fiber belongs to and runs")]
    public Atom Origin { get; set; }

    [Field(Required = true, Description = "Current fiber status")]
    public FiberStatus Status { get; set; }

    [Field(Required = false,
           MaxLength = Constraints.MAX_DESCRIPTION_LEN,
           Description = "Optionally provides current fiber status description")]
    public string StatusDescription { get; set; }


    [Field(Required = true, Description = "Current fiber status")]
    public int ExitCode { get; set; }


    [Field(Required = false, Description = "Exception data in case of failure or null")]
    public WrappedExceptionData Exception { get; set; }


    [Field(Required = true,
           Description = "Uniquely identifies the type of process image which backs this fiber execution. " +
                         "In CLR runtime, this maps to a descendant type of a `Fiber` class via BIX mapping")]
    public Guid ImageTypeId { get; set; }


    [Field(Required = false,
           MaxLength = Constraints.MAX_GROUP_LEN,
           Description = "Optionally, groups fibers by some correlation value. For example this can be used to group multiple fibers" +
                         " executing on behalf of master job fiber. Unlike Strands, Groups do not affect Fiber execution order/concurrency. " +
                         "Group can only be defined at fiber start and is afterwards immutable")]
    public string Group { get; set; }

    [Field(Required = false,
           Min = Constraints.PRIORITY_MIN,
           Max = Constraints.PRIORITY_MAX,
           Description = "Relative priority 0.001..100.0, default is 1.0f")]
    public float? Priority { get; set; }

    [Field(Required = false,
           MaxLength = Constraints.MAX_DESCRIPTION_LEN,
           Description = "Describes who execution session is impersonated as or null if it is executed under a system account. " +
                         "Note: this is a descriptive string for humans, not a token which can be used to perform any auth functionality")]
    public string ImpersonationTitle{ get; set; }


    [Field(Required = true,
           MaxLength = Constraints.MAX_DESCRIPTION_LEN,
           Description = "Optionally provides a short fiber description")]
    public string Description { get; set; }

    [Field(Required = false,
           Description = "What/who has caused this fiber to be created")]
    public EntityId? Initiator { get; set; }

    [Field(Required = false,
           Description = "Optionally establishes an ownership of this fiber")]
    public EntityId? Owner { get; set; }

    [Field(Required = true, Description = "When fiber was created")]
    public DateTime CreateUtc { get; set; }

    [Field(Required = true, Description = "When is the fiber scheduled to run its next time slice")]
    public DateTime NextSliceUtc { get; set; }

    [Field(Required = true,
           Description = "Last execution latency - a difference before scheduled and actual slice start")]
    public int LastLatencySec { get; set; }

    [Field(Required = true,
           Description = "How long the last execution slice took")]
    public int LastSliceDurationMs { get; set; }

    [Field(Required = true,
           Description = "Average execution latency - a difference before scheduled and actual slice start")]
    public int AvgLatencySec { get; set; }

    [Field(Required = true,
           Description = "How long the average execution slice takes")]
    public int AvgSliceDurationMs { get; set; }

    /// <summary>
    /// Indexable tags used for future fiber searches. Tags are immutable beyond fiber start
    /// </summary>
    [Field(required: true,
           maxLength: Constraints.MAX_TAG_COUNT,
           Description = "Indexable tags used for future flow fiber searches. Tags are immutable beyond fiber start")]
    public List<Data.Adlib.Tag> Tags { get; set; }

  }
}
