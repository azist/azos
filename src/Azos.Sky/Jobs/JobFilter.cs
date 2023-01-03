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

namespace Azos.Sky.Jobs
{
  [Schema(Description = "Job filter")]
  [Bix("5c24bf15-a4e0-4394-b9e5-a77af7d311a1")]
  public sealed class JobFilter : FilterModel<IEnumerable<JobInfo>>
  {
    [Field(Required = false, Description = "Job id: runspace and Gdid")]
    public JobId JobId { get; set; }


    [Field(description: "Tag filter expression tree")]
    public Expression TagFilter { get; set; }

    [InjectModule] IJobManagerLogic m_Logic;

    protected async override Task<SaveResult<IEnumerable<JobInfo>>> DoSaveAsync()
     => new SaveResult<IEnumerable<JobInfo>>(await m_Logic.GetJobListAsync(this).ConfigureAwait(false));
  }
}
