using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Security;

namespace Azos.IAM.Protocol
{
  /// <summary>
  /// Denotes IAM entity types
  /// </summary>
  public enum EntityType{ Group, Account, Login, Role, Audit }


  /// <summary>
  /// Provides IAM administration services
  /// </summary>
  public interface IAdministrator
  {
    //Task<List<GroupInfo>>     GetGroupListAsync  (GroupListFilter   filter);
    //Task<List<AccountInfo>>   GetAccountListAsync(AccountListFilter filter);
    //Task<List<LoginInfo>>     GetLoginListAsync  (LoginListFilter   filter);
    //Task<List<RoleInfo>>      GetRoleListAsync   (RoleListFilter    filter);
    //Task<List<AuditInfo>>     GetAuditLogAsync   (AuditLogFilter    filter);

    //Task<Group>   GetGroupAsync(GDID id);
    //Task<Account> GetAccountAsync(GDID id);
    //Task<Login>   GetLoginListAsync(GDID id);
    //Task<Role>    GetRoleAsync(GDID id);

    //Task<Group>   SaveGroupAsync(Group form, ActionDescriptor action);
    //Task<Account> SaveAccountAsync(Account form, ActionDescriptor action);
    //Task<Login>   SaveLoginListAsync(Login form, ActionDescriptor action);
    //Task<Role>    SaveRoleAsync(Role form, ActionDescriptor action);

    //Task<Group>   DeleteGroupAsync(GDID id, ActionDescriptor action);
    //Task<Account> DeleteAccountAsync(GDID id, ActionDescriptor action);
    //Task<Login>   DeleteLoginListAsync(GDID id, ActionDescriptor action);
    //Task<Role>    DeleteRoleAsync(GDID id, ActionDescriptor action);

    //Task<bool>  LockEntityAsync(EntityType entityType, GDID gEntity, DateTime utcLockDate, ActionDescriptor action);
    //Task<bool> SetEntityRightsAsync(EntityType entityType, GDID gEntity, string rightsData, ActionDescriptor action);
  }
}
