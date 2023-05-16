/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Data;
using Azos.Data.Business;
using Azos.Log;
using Azos.Serialization.Bix;

namespace Azos.Sky.Chronicle
{
  [Bix("a06fb712-7e64-435e-8325-f28d67a544e3")]
  [Schema(Description = "Provides model for filtering log chronicles for facts")]
  public sealed class LogChronicleFactFilter : FilterModel<IEnumerable<Fact>>
  {
    [Field(required: true, description: "Log message filter which facts are based on")]
    public LogChronicleFilter LogFilter { get; set; }

    [InjectModule] ILogChronicle m_Chronicle;

    protected async override Task<SaveResult<IEnumerable<Fact>>> DoSaveAsync()
     => new SaveResult<IEnumerable<Fact>>(await m_Chronicle.GetFactsAsync(this).ConfigureAwait(false));
  }

}
