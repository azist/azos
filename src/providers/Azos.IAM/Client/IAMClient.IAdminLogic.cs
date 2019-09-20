using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Data.Business;
using Azos.IAM.Protocol;
using Azos.Serialization.JSON;
using Azos.Web;

namespace Azos.IAM.Client
{
  public sealed partial class IAMClient
  {

    public async Task<TResult> FilterOneAsync<TResult>(FilterModel<TResult> filter) where TResult : TypedDoc
      => await CallWithRetry(async server =>
      {
        var wrap = await server.GetClient().PostAndGetJsonMapAsync("filter", filter);
        var raw = wrap.UnwrapPayloadMap();
        var result = JsonReader.ToDoc<TResult>(raw);
        return result;
      });

    public async Task<IEnumerable<TResult>> FilterListAsync<TResult>(FilterModel<IEnumerable<TResult>> filter) where TResult : TypedDoc
      => await CallWithRetry(async server =>
      {
        var wrap = await server.GetClient().PostAndGetJsonMapAsync("filter", filter);
        var raw = wrap.UnwrapPayloadArray();
        var result = raw.Select(d => d is JsonDataMap dmap ? JsonReader.ToDoc<TResult>(dmap) : null);
        return result;
      });

    public Task<SaveResult<ChangeResult>> ApplyChangeAsync<TChangeForm>(TChangeForm form) where TChangeForm : ChangeForm
    {
      throw new NotImplementedException();
    }


    public Task<TEntityBody> GetEntityBodyAsync<TEntityBody>(GDID id) where TEntityBody : EntityBody
    {
      throw new NotImplementedException();
    }
  }
}
