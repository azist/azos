using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;

namespace Azos.IAM.Server.Data
{
  /// <summary>
  /// Roles represent named ACL sets of permissions
  /// Roles are addressable by their immutable name and
  /// are used in other ACLs as a mix-in using "assign{role=role-id}"
  /// Unlike groups/accounts, roles do not link to entities via a separate link table
  /// </summary>
  public class Role : EntityWithRights
  {

    [Field(required: true, kind: DataKind.ScreenName, metadata: "idx{name='id' unique=true dir=asc}")]
    public string ID { get; set;}

    //Description
  }
}
