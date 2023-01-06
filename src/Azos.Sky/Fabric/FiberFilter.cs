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
  [Schema(Description = "Job filter")]
  [Bix("5c24bf15-a4e0-4394-b9e5-a77af7d311a1")]
  public sealed class FiberFilter : FilterModel<IEnumerable<FiberInfo>>
  {
    [Field(Required = false, Description = "Job id: runspace and Gdid")]
    public FiberId? Id { get; set; }

    [Field(description: "Job Guid used for log and other correlation token")]
    public Guid? FiberGuid { get; set; }

    [Field(description: "Defines what cloud origin (cluster partition) this job belongs to and runs")]
    public Atom? Origin { get; set; }

    [Field(description: "Job strand")]
    public string Strand { get; set; }

    [Field(description: "Job status")]
    public FiberStatus? Status { get; set; }

    [Field(description: "Uniquely identifies the type of process image which backs this job execution. " +
                        "In CLR runtime, this maps to a descendant type of a `Job` class via BIX mapping")]
    public Guid? ImageTypeId { get; set; }

    [Field(description: "When job was created")]
    public DateRange? CreateUtc { get; set; }

    [Field(description: "What/who has caused this job to be created")]
    public EntityId? Initiator { get; set; }

    [Field(description: "Optionally establishes an ownership of this job")]
    public EntityId? Owner { get; set; }

    [Field(description: "Minimum duration of last slice exec")]
    public int? MinLastSliceDurationMs { get; set; }

    [Field(description: "Maximum duration of last slice exec")]
    public int? MaxLastSliceDurationMs { get; set; }

    [Field(description: "Time range of next exec slice")]
    public DateRange? NextSliceUtc { get; set; }

    [Field(description: "Tag filter expression tree")]
    public Expression TagFilter { get; set; }

    [InjectModule] IFiberManagerLogic m_Logic;

    protected async override Task<SaveResult<IEnumerable<FiberInfo>>> DoSaveAsync()
     => new SaveResult<IEnumerable<FiberInfo>>(await m_Logic.GetFiberListAsync(this).ConfigureAwait(false));
  }
}
