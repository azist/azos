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
using Azos.Data.AST;
using Azos.Data.Business;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;
using Azos.Time;

namespace Azos.Sky.Chronicle
{
  [Bix("5843A744-54CF-4E22-99CF-A91F0C2CE08C")]
  [Schema(Description = "Provides model for filtering instrumentation chronicles")]
  public sealed class InstrumentationChronicleFilter : FilterModel<IEnumerable<JsonDataMap>>
  {
    [Field(isArow: true, backendName: "gdid", description: "Measurement datum GDID to fetch a specific measurement or ZERO")]
    public GDID Gdid { get; set; }

    [Field(isArow: true, backendName: "tps", description: "An array of instrument type IDs")]
    public Guid[] InstrumentTypes {  get; set;}

    [Field(isArow: true, backendName: "utcr", description: "Log message start/end date UTC time range")]
    public DateRange? TimeRange { get; set; }


    [Field(isArow: true, backendName: "af", description: "Advanced filter, which can be used for filter by archive dimensions")]
    public Expression AdvancedFilter{  get; set;}

    [Inject] IInstrumentationChronicle m_Chronicle;

    protected async override Task<SaveResult<IEnumerable<JsonDataMap>>> DoSaveAsync()
     => new SaveResult<IEnumerable<JsonDataMap>>(await m_Chronicle.GetAsync(this).ConfigureAwait(false));
  }

}
