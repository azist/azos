/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Data;
using Azos.Data.AST;
using Azos.Data.Business;
using Azos.Log;
using Azos.Serialization.Bix;
using Azos.Time;

namespace Azos.Sky.Chronicle
{
  [Bix("57015390-9684-4065-9492-9087D9AC0025")]
  [Schema(Description = "Provides model for filtering log chronicles")]
  public sealed class LogChronicleFilter : FilterModel<IEnumerable<Message>>
  {
    [Field(isArow: true, backendName: "gdid", description: "Log message GDID")]
    public GDID Gdid { get; set; }

    [Field(isArow: true, backendName: "guid", description: "Log message GUID")]
    public Guid? Id { get; set; }

    [Field(isArow: true, backendName: "relguid", description: "Log message related to GUID")]
    public Guid? RelId { get; set; }

    [Field(isArow: true, backendName: "chn", description: "Log message channel")]
    public Atom? Channel { get; set; }

    [Field(isArow: true, backendName: "app", description: "Log emitter app id")]
    public Atom? Application { get; set; }

    [Field(isArow: true, backendName: "utcr", description: "Log message start/end date UTC time range")]
    public DateRange? TimeRange { get; set; }

    [Field(isArow: true, backendName: "mintp", description: "Log message min type")]
    public MessageType? MinType { get; set; }

    [Field(isArow: true, backendName: "maxtp", description: "Log message max type")]
    public MessageType? MaxType { get; set; }

    [Field(isArow: true, backendName: "af", description: "Advanced filter, which can be used for filter by archive dimensions")]
    public Expression AdvancedFilter{  get; set;}

    [Inject] ILogChronicle m_Chronicle;

    protected async override Task<SaveResult<IEnumerable<Message>>> DoSaveAsync()
     => new SaveResult<IEnumerable<Message>>(await m_Chronicle.GetAsync(this));
  }

}
