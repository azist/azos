using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;

namespace Azos.IAM.Data
{
  /// <summary>
  /// Represents a named set of permissions
  /// </summary>
  public class Role : Entity
  {
    /// <summary>
    /// Parent Role/or null if top-most
    /// </summary>
    [Field]
    public GDID G_PARENT{  get; set;}

    //ACL
  }
}
