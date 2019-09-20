using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Data.Business;
using Azos.IAM.Protocol;

namespace Azos.IAM.Client
{
  public sealed partial class IAMClient
  {

    public Task<TResult> FilterAsync<TResult>(FilterModel<TResult> filter)
    {
      throw new NotImplementedException();
    }

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
