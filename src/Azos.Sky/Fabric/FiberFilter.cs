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
using Azos.Apps.Injection;
using Azos.Data;
using Azos.Data.AST;
using Azos.Data.Business;
using Azos.Serialization.Bix;
using Azos.Time;

namespace Azos.Sky.Fabric
{
  [Schema(Description = "Fiber filter")]
  [Bix("5c24bf15-a4e0-4394-b9e5-a77af7d311a1")]
  public sealed class FiberFilter : FilterModel<IEnumerable<FiberInfo>>
  {
    [Field(description: "Runspace id: runspace")]
    public Atom? Runspace { get; set; }

    [Field(description: "Fiber id: runspace and Gdid")]
    public FiberId? Id { get; set; }

    [Field(description: "Fiber Guid used for log and other correlation token")]
    public Guid? InstanceGuid { get; set; }

    [Field(description: "Defines what cloud origin (cluster partition) this fiber belongs to and runs")]
    public Atom? Origin { get; set; }

    [Field(maxLength: Constraints.MAX_GROUP_LEN,
           description: "Fiber group")]
    public string Group { get; set; }

    [Field(description: "Fiber execution status")]
    public FiberStatus? Status { get; set; }

    [Field(description: "Uniquely identifies the type of process image which backs this fiber execution. " +
                        "In CLR runtime, this maps to a descendant type of a `Fiber` class via BIX mapping")]
    public Guid? ImageTypeId { get; set; }

    [Field(description: "When fiber was created")]
    public DateRange? CreateUtc { get; set; }

    [Field(maxLength: Constraints.MAX_INITIATOR_LEN,
           description: "What has caused this fiber to be created")]
    public EntityId? Initiator { get; set; }

    [Field(maxLength: Constraints.MAX_OWNER_LEN,
           description: "Optionally establishes an ownership of this fiber")]
    public EntityId? Owner { get; set; }

    [Field(description: "Average execution latency minimum")]
    public int? MinAvgLatencySec { get; set; }

    [Field(description: "Average execution latency maximum")]
    public int? MaxAvgLatencySec { get; set; }

    [Field(description: "Minimum duration of average slice exec")]
    public int? MinAvgSliceDurationMs { get; set; }

    [Field(description: "Maximum duration of average slice exec")]
    public int? MaxAvgSliceDurationMs { get; set; }

    [Field(description: "Time range of next exec slice")]
    public DateRange? NextSliceUtc { get; set; }

    [Field(description: "Tag filter expression tree")]
    public Expression TagFilter { get; set; }

    [Field(description: "State tag filter expression tree")]
    public Expression StateTagFilter { get; set; }

    [InjectModule] IFiberManagerLogic m_Logic;

    protected async override Task<SaveResult<IEnumerable<FiberInfo>>> DoSaveAsync()
     => new SaveResult<IEnumerable<FiberInfo>>(await m_Logic.GetFiberListAsync(this).ConfigureAwait(false));
  }
}
