
using System;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Data;
using Azos.Data.Business;
using Azos.Serialization.JSON;

namespace Azos.IAM.Protocol
{
  /// <summary>
  /// Base class for entity bodies
  /// </summary>
  public abstract class EntityBody : FragmentModel
  {
    [Field(key: true, storeFlag: StoreFlag.OnlyLoad, description: "Global Distributed ID of this Entity")]
    public GDID GDID { get; set; }

    [Field(storeFlag: StoreFlag.OnlyLoad, description: "Version time stamp")]
    public DateTime? VersionTimestamp { get; set; }

    [Field(storeFlag: StoreFlag.OnlyLoad, description: "Version status")]
    public char? VersionStatus { get; set; }

    [Field(storeFlag: StoreFlag.OnlyLoad, description: "Actor/User who authored the version")]
    public GDID G_VersionActor { get; set; }
  }

  /// <summary>
  /// EntityBody which have properties
  /// </summary>
  public abstract class EntityBodyWithProperties : EntityBody
  {
    [Field(maxLength: Sizes.DESCRIPTION_MAX, description: "Provides an optional description")]
    public string Description { get; set; }

    [Field(maxLength: Sizes.PROPERTY_COUNT_MAX, description: "Properties")]
    public JsonDataMap PropertyData { get; set; }
  }

  /// <summary>
  /// EntotyBodies with Rights
  /// </summary>
  public abstract class EntityBodyWithRights : EntityBody
  {
    private string m_RightsData;

    [NonSerialized]
    private ConfigSectionNode m_Rights;

    /// <summary>
    /// The Audit* family of columns provide the minimum history in case of Audit log deletion/purge
    /// </summary>
    [Field(storeFlag: StoreFlag.OnlyLoad, description: "Captures the original create date. This value never changes after that")]
    public DateTime? AuditCreateDate { get; set; }

    /// <summary>
    /// The Audit* family of columns provide the minimum history in case of Audit log deletion/purge
    /// </summary>
    [Field(storeFlag: StoreFlag.OnlyLoad, description: "Captures the actor/user account who created this entity. This value never changes after that")]
    public string AuditCreateActorTitle { get; set; }

    /// <summary>
    /// The Audit* family of columns provide the minimum history in case of Audit log deletion/purge
    /// </summary>
    [Field(storeFlag: StoreFlag.OnlyLoad, description: "Captures the last modification date")]
    public DateTime? AuditLastModifyDate { get; set; }

    /// <summary>
    /// The Audit* family of columns provide the minimum history in case of Audit log deletion/purge
    /// </summary>
    [Field(storeFlag: StoreFlag.OnlyLoad, description: "Captures the actor/user account title who has performed the last modifications")]
    public string AuditLastModifyActorTitle { get; set; }


    [Field(required: true, description: "Contains a list of time periods during which this entity is valid. An entity is considered as invalid/non-existent one outside of these time spans")]
    public TimePeriod[] ValidPeriods { get; set; }

    /// <summary>
    /// The locking has different effect on different entities: a locked role just does not get mixed-in as if it never existed.
    /// Locked group disallows any access to any entities directly or indirectly under it.
    /// Locked Account disables all logins, and locked login disables just that login
    /// </summary>
    [Field(description: "Optional temporary lock UTC timestamp, beyond which  an entity is considered to be invalid")]
    public DateTime? LockDate { get; set; }

    /// <summary>
    /// If this date is specified and the the current UTC is over this date then Lock considers to be auto-lifted
    /// </summary>
    [Field(description: "If this date is specified and the the current UTC is over this date then Lock considers to be auto-lifted")]
    public DateTime? LockAutoResetDate { get; set; }

    [Field(maxLength: Sizes.NOTE_MAX,
           description: "Optional note associated with optional temporary lock timestamp. A note may contain a reason why entity is locked-out")]
    public string LockNote { get; set; }


    [Field(maxLength: Sizes.RIGHTS_DATA_MAX,
           description: "Access rights")]
    public string RightsData
    {
      get => m_RightsData;
      set
      {
        if (!m_RightsData.EqualsOrdSenseCase(value)) m_Rights = null;
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
        if (m_Rights == null)
        {
          if (m_RightsData.IsNotNullOrWhiteSpace())
            m_Rights = m_RightsData.AsJSONConfig(handling: ConvertErrorHandling.Throw);
        }
        return m_Rights;
      }
      set
      {
        m_Rights = value;

        if (value == null || !value.Exists)
          m_RightsData = null;
        else
          m_RightsData = value.ToJSONString(JsonWritingOptions.PrettyPrintRowsAsMap);
      }
    }
  }

}
