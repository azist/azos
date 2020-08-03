/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Data;
using Azos.Data.Business;
using Azos.Log;
using Azos.Serialization.Bix;
using Azos.Time;

namespace Azos.Sky.Chronicle
{
  [Bix("DDD07B97-355E-4CF2-8E82-496CE77A265E")]
  [Schema(Description = "Provides model for log batch upload")]
  public sealed class LogBatch : PersistedModel<ChangeResult>
  {
    [Field(isArow: true, required: true, backendName: "d", description: "Data to upload")]
    public Message[] Data{ get; set; }

    [Inject] ILogChronicleLogic m_Chronicle;

    protected async override Task<SaveResult<ChangeResult>> DoSaveAsync()
    {
      var t = Timeter.StartNew();
      await m_Chronicle.WriteAsync(this);
      return new SaveResult<ChangeResult>(new ChangeResult(ChangeResult.ChangeType.Inserted, Data.Length, $"Done in {t.ElapsedMs} ms", null));
    }
  }

}
