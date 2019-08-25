using Azos.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.IAM.Data
{
  /// <summary>
  /// Represents a set of users, a unit of user hierarchy organization.
  /// </summary>
  public sealed class Group : EntityWithRights
  {
    // Parent Role/or null if top-most
    [Field]
    public GDID G_PARENT
    {
      get; set;
    }
  }


  /*
  Account may be in multiple different groups.
  Group assignment is temporal (in data range)
  ACL get applied in order

  For simplicity:
  Roles are not to be linked via table, but referenced in config:  no way to know what users/groups use THIS role



   */

  public sealed class GroupAccount : Entity
  {
    public GDID Group{ get; set;}//index
    public GDID Account { get; set; }//index
    public int? Order{  get; set;}//

    public DateTime? StartDate{  get; set;}
    public DateTime? EndDate { get; set; }

  }

}
