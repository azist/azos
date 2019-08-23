using System;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.IAM.Data
{
  public abstract class BaseDoc : AmorphousTypedDoc
  {
    public const string TMONGO = "mongo";
  }

  /// <summary>
  /// Provides base for replicating data entities
  /// </summary>
  public abstract class Entity : BaseDoc
  {
    [Field(key: true, required: true, description: "Primary key which identifies this entity")]
    [Field(typeof(Entity), nameof(GDID), TMONGO, backendName: "_id")]
    public GDID GDID{ get; set;}

    [Field(required: true, description: "Version time stamp", metadata: "idx{name='vts' order='0' dir=asc}")]
    [Field(typeof(Entity), nameof(VersionTimestamp), TMONGO, backendName: "_vts")]
    public DateTime? VersionTimestamp { get; set; }

    [Field(required: true, description: "Actor/User who caused the change")]//not indexed, use Audit for searches instead
    [Field(typeof(Entity), nameof(VersionActor), TMONGO, backendName: "_vts")]
    public GDID VersionActor { get; set; }

    [Field(required: true, description: "Properties")]
    [Field(typeof(Entity), nameof(PropertyData), TMONGO, backendName: "props")]
    public JsonDataMap PropertyData { get; set; }//JsonDataMap is used because it is supported by all ser frameworks, but we only store strings


  }
}
