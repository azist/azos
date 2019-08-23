using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;

namespace Azos.IAM.Data
{
  public sealed class Account : Entity
  {

    public string CName {  get; set; }

    /// <summary>
    /// Account Name/Title. For human users this is set to FirstName+LastName
    /// </summary>
    public string Title {  get; set; }

    /// <summary>
    /// Human, Process, Robot, Org, System
    /// </summary>
  //  public AccountTypes Type { get; set; }

  }
}
