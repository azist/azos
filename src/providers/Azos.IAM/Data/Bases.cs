using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;

namespace Azos.IAM.Data
{
  public abstract class BaseDoc : AmorphousTypedDoc
  {
    public const string TMONGO = "mongo";
  }

  public abstract class Entity : BaseDoc
  {
    [Field(key: true, required: true, description: "Primary key which identifies this entity")]
    [Field(typeof(Entity), nameof(GDID), TMONGO, backendName: "_id")]
    public GDID GDID{ get; set;}

  }
}
