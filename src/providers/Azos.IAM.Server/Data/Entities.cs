using System;

using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.IAM.Server.Data
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
    public const string CHANGE_TYPE_VALUE_LIST = "U:Update,C:Create,D:Delete";


    [Field(key: true, required: true, description: "Primary key which identifies this entity")]
    [Field(typeof(Entity), nameof(GDID), TMONGO, backendName: "_id")]
    public GDID GDID{ get; set;}

    [Field(required: true, description: "Version time stamp", metadata: "idx{name='vts' order='0' dir=asc}")]
    [Field(typeof(Entity), nameof(VersionTimestamp), TMONGO, backendName: "_vt")]
    public DateTime? VersionTimestamp { get; set; }

    [Field(required: true, description: "Version status", valueList: CHANGE_TYPE_VALUE_LIST)]
    [Field(typeof(Entity), nameof(VersionStatus), TMONGO, backendName: "_vs")]
    public char? VersionStatus {  get; set; }

    [Field(required: true, description: "Actor/User who caused the change")]//not indexed, use Audit for searches instead
    [Field(typeof(Entity), nameof(VersionActor), TMONGO, backendName: "_va")]
    public GDID VersionActor { get; set; }

    [Field(required: false, description: "Properties")]
    [Field(typeof(Entity), nameof(PropertyData), TMONGO, backendName: "props")]
    public JsonDataMap PropertyData { get; set; }//JsonDataMap is used because it is supported by all ser frameworks, but we only store strings
  }

  /// <summary>
  /// Represents entity with access rights
  /// </summary>
  public abstract class EntityWithRights : Entity
  {
    private string m_RightsData;

    [NonSerialized]
    private ConfigSectionNode m_Rights;


    [Field(required: true, description: "Validity start UTC timestamp. An entity is considered invalid/non-existent before this point in time")]
    [Field(typeof(EntityWithRights), nameof(ValidSD), TMONGO, backendName: "vsd")]
    public DateTime? ValidSD  { get; set; }

    [Field(required: true, description: "Validity end UTC timestamp, beyond which  an entity is considered invalid")]
    [Field(typeof(EntityWithRights), nameof(ValidED), TMONGO, backendName: "ved")]
    public DateTime? ValidED  { get; set; }

    /// <summary>
    /// The locking has different effect on different entities: a locked role just does not get mixed-in as if it never existed.
    /// Locked group disallows any access to any entities directly or indirectly under it.
    /// Locked Account disables all logins, and locked login disables just that login
    /// </summary>
    [Field(description: "Optional temporary lock UTC timestamp, beyond which  an entity is considered to be invalid")]
    [Field(typeof(EntityWithRights), nameof(LockDate), TMONGO, backendName: "lckd")]
    public DateTime? LockDate { get; set; }

    [Field(description: "Optional note associated with optional temporary lock timestamp. A note may contain a reason why entity is locked-out")]
    [Field(typeof(EntityWithRights), nameof(LockNote), TMONGO, backendName: "lckn")]
    public string    LockNote { get; set; }



    [Field(required: true, description: "Access rights")]
    [Field(typeof(EntityWithRights), nameof(RightsData), TMONGO, backendName: "r")]
    public string RightsData
    {
      get => m_RightsData;
      set
      {
        if (m_RightsData != value) m_Rights = null;
        m_RightsData = value;
      }
    }

    /// <summary>
    /// Provides structured rights representation of this nodes RightData
    /// </summary>
    public ConfigSectionNode Rights
    {
      get
      {
        if (m_Rights==null)
        {
          if (m_RightsData.IsNotNullOrWhiteSpace())
            m_Rights = m_RightsData.AsJSONConfig(handling: ConvertErrorHandling.Throw);
        }
        return m_Rights;
      }
      set
      {
        m_Rights = value;

        if (value==null || !value.Exists)
          m_RightsData = null;
        else
          m_RightsData = value.ToJSONString(JsonWritingOptions.PrettyPrintRowsAsMap);

      }
    }
  }


}
