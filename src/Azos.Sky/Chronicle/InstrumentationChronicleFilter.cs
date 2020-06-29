using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Data;
using Azos.Data.AST;
using Azos.Data.Business;
using Azos.Instrumentation;
using Azos.Log;
using Azos.Serialization.Bix;
using Azos.Time;

namespace Azos.Sky.Chronicle
{
  [Bix("5843A744-54CF-4E22-99CF-A91F0C2CE08C")]
  [Schema(Description = "Provides model for filtering log chronicles")]
  public sealed class InstrumentationChronicleFilter : FilterModel<IEnumerable<Datum>>
  {
    [Field(isArow: true, backendName: "gdid", description: "Measurement datum GDID")]
    public GDID Gdid { get; set; }

    [Field(isArow: true, backendName: "names", description: "Semicolon-separated instrument/type names")]
    public string Names {  get; set;}

    [Field(isArow: true, backendName: "utcr", description: "Log message start/end date UTC time range")]
    public DateRange? TimeRange { get; set; }


    [Field(isArow: true, backendName: "af", description: "Advanced filter, whcih can be used for filter by archive dimensions")]
    public Expression AdvancedFilter{  get; set;}

    [Inject] IInstrumentationChronicle m_Chronicle;

    protected async override Task<SaveResult<IEnumerable<Datum>>> DoSaveAsync()
     => new SaveResult<IEnumerable<Datum>>(await m_Chronicle.GetAsync(this));
  }

}
