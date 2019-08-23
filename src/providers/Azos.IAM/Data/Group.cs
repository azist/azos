using Azos.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.IAM.Data
{
  /// <summary>
  /// Represents a set of users, a unit of user hierarchy organization.
  /// </summary>
  class Group
  {
    /// Parent Role/or null if top-most
    /// </summary>
    [Field]
    public GDID G_PARENT
    {
      get; set;
    }
  }
}
