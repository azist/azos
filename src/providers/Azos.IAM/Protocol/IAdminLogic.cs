using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Data.Business;

namespace Azos.IAM.Protocol
{
  /// <summary>
  /// Provides IAM administration services
  /// </summary>
  public interface IAdminLogic : IBusinessLogic
  {
    Task<TResult> FilterOneAsync<TResult>(FilterModel<TResult> filter) where TResult : TypedDoc;
    Task<IEnumerable<TResult>> FilterListAsync<TResult>(FilterModel<IEnumerable<TResult>> filter) where TResult : TypedDoc;
    Task<TEntityBody> GetEntityBodyAsync<TEntityBody>(GDID id) where TEntityBody : EntityBody;
    Task<SaveResult<ChangeResult>> ApplyChangeAsync<TChangeForm>(TChangeForm form) where TChangeForm : ChangeForm;

    //Task<bool>  LockEntityAsync(EntityType entityType, GDID gEntity, DateTime utcLockDate, ActionDescriptor action);
    //Task<bool> SetEntityRightsAsync(EntityType entityType, GDID gEntity, string rightsData, ActionDescriptor action);
  }
}
