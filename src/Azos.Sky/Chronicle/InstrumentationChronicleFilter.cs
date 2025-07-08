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

namespace Azos.Sky.Chronicle
{
  /// <summary>
  /// Provides model for filtering and retrieving time series data from instrumentation chronicles
  /// </summary>
  [Bix("5843a744-54cf-4e22-99cf-a91f0c2ce08c")]
  [Schema(Description = "Provides model for filtering and retrieving time series data from instrumentation chronicles")]
  public sealed class InstrumentationChronicleFilter : FilterModel<IEnumerable<JsonDataMap>>
  {
    public const int TERM_DISJUNCTION_MAX_LEN = 128;

    [Field(required: true, description: "Instrumentation fetch starting from UTC point in time. It is always required")]
    public DateTime StartUtc { get; set; }

    [Field(required: true, description: "Instrumentation fetch time range length in seconds")]
    public int RangeLengthSec{ get; set; }

    [Field(required: false, description: "Optional GDID unique id as of which to start scrolling time series data after StartUtc")]
    public GDID StartGdid { get; set; }

    [Field(required: false, maxLength: TERM_DISJUNCTION_MAX_LEN, description: "Optional disjunction of host names to consider")]
    public string[] HostNames { get; set; }

    [Field(required: false, maxLength: TERM_DISJUNCTION_MAX_LEN, description: "Optional disjunction of instrument namespaces hashes. Every instrument belongs to one and only one namespace at a time")]
    public ulong[] NamespaceHashes { get; set; }

    [Field(required: false, maxLength: TERM_DISJUNCTION_MAX_LEN, description: "Optional instrument type ID disjunction")]
    public Guid[] InstrumentTypes { get; set;}

    [Field(required: false, maxLength: TERM_DISJUNCTION_MAX_LEN, description: "Optional app id disjunction")]
    public Atom[] AppTypes { get; set; }

    [Field(required: false, description: "Optional lower limit for RefValue")]
    public double? MinRefValue { get; set; }

    [Field(required: false, description: "Optional high limit for RefValue")]
    public double? MaxRefValue { get; set; }

    [Field(required: false, description: "Optional advanced filter expression tree")]
    public Expression AdvancedFilter{  get; set;}

    [Inject] IInstrumentationChronicle m_Chronicle;

    protected async override Task<SaveResult<IEnumerable<JsonDataMap>>> DoSaveAsync()
     => new SaveResult<IEnumerable<JsonDataMap>>(await m_Chronicle.GetAsync(this).ConfigureAwait(false));
  }

}
