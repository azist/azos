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

namespace Azos.Sky.Fabric
{
  [Schema(Description = "Fiber start arguments")]
  [Bix("ac1affba-5829-4e13-a3fc-1334b8828644")]
  public sealed class FiberStartArgs : TransientModel
  {
    [Field(Required = true,
           Description = "Unique `FiberId` obtained from a call to `AllocateFiberId()`. This fiber must NOT have started yet")]
    public FiberId Id { get; set; }

    [Field(Required = true,
          Description = "Defines what cloud origin (cluster partition) this fiber belongs to and runs")]
    public Atom Origin { get; set; }

    [Field(Required = true,
           Description = "Uniquely identifies the type of process image which backs this fiber execution. " +
                         "In CLR runtime, this maps to a descendant type of a `Fiber` class via BIX mapping")]
    public Guid ImageTypeId { get; set; }

    [Field(Required = false,
           MaxLength = Constraints.MAX_STRAND_LEN,
           Description = "Optionally, ensures sequential processing of fibers within the same strand, that is:" +
                         " no more than a single fiber instance of the same strand ever executes in the system origin (cloud partition) concurrently." +
                         "Strands can only be defined at fiber start and are afterwards immutable")]
    public string Strand { get; set; }

    [Field(Required = false,
           MaxLength = Constraints.MAX_GROUP_LEN,
           Description = "Optionally, groups fibers by some correlation value. For example this can be used to group multiple fibers" +
                         " executing on behalf of master job fiber. Unlike Strands, Groups do not affect fiber execution order/concurrency. " +
                         "Group can only be defined at fiber start and is afterwards immutable")]
    public string Group { get; set; }

    [Field(Required = false,
           Min = Constraints.PRIORITY_MIN,
           Max = Constraints.PRIORITY_MAX,
           Description = "Optionally, sets a relative priority 0.001..100.0, default is 1.0f")]
    public float? Priority { get; set; }

    [Field(Required = false,
           MaxLength = Constraints.MAX_IMPERSONATE_LEN,
           Description = "Optionally, includes principal references, such as URI credentials which impersonate the fiber execution context. " +
                         "Do not include plain `IdPasswordCredentials`, as only password-less credentials are supported")]
    public EntityId? ImpersonateAs { get; set; }

    [Field(Required = false, Description = "When is the fiber scheduled to start execution, if null then fiber starts running ASAP")]
    public DateTime? ScheduledStartUtc { get; set; }

    [Field(Required = false,
           MaxLength = Constraints.MAX_INITIATOR_LEN,
           Description = "What has caused this fiber to be created")]
    public EntityId? Initiator { get; set; }

    [Field(Required = false,
           MaxLength = Constraints.MAX_OWNER_LEN,
           Description = "Optionally establishes an ownership of this fiber")]
    public EntityId? Owner { get; set; }

    [Field(Required = false,
           MaxLength = Constraints.MAX_DESCRIPTION_LEN,
           Description = "Optionally provides a short fiber description")]
    public string Description { get; set; }


    [Field(Required = true,
           Description = "Fiber start parameters. These values are immutable for the lifetime of a fiber instance " +
           "(not to be confused with Fiber STATE which IS mutable)")]
    public FiberParameters Parameters{ get; set; }

    /// <summary>
    /// Indexable tags used for future fiber searches. Tags are immutable beyond fiber start
    /// </summary>
    [Field(Required = true,
           MaxLength = Constraints.MAX_TAG_COUNT,
           Description = "Indexable tags used for future fiber searches. Tags are immutable beyond fiber start")]
    public List<Data.Adlib.Tag> Tags { get; set; }
  }
}
