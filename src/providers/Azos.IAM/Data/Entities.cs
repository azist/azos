using System;

using Azos.Conf;
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
    [Field(typeof(Entity), nameof(VersionTimestamp), TMONGO, backendName: "_vt")]
    public DateTime? VersionTimestamp { get; set; }

    [Field(required: true, description: "Version status", valueList: "U:Update,C:Create,D:Delete")]
    [Field(typeof(Entity), nameof(VersionStatus), TMONGO, backendName: "_vs")]
    public char? VersionStatus {  get; set; }

    [Field(required: true, description: "Actor/User who caused the change")]//not indexed, use Audit for searches instead
    [Field(typeof(Entity), nameof(VersionActor), TMONGO, backendName: "_va")]
    public GDID VersionActor { get; set; }

    [Field(required: true, description: "Properties")]
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


    [Field(required: true, description: "Access rights")]
    [Field(typeof(EntityWithRights), nameof(RightsData), TMONGO, backendName: "rights")]
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
