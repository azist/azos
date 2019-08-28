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


}
