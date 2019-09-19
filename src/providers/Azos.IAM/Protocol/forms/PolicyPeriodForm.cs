
using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;
using System;

namespace Azos.IAM.Protocol
{

  public sealed class PolicyPeriodForm : ChangeForm<GroupEntityBody>{ }

  public sealed class PolicyPeriodEntityBody : EntityBodyWithRights
  {
    [Field(required: true,
           kind: DataKind.ScreenName,
           minLength: Sizes.ENTITY_ID_MIN,
           maxLength: Sizes.ENTITY_ID_MAX,
           description: "Unique Policy ID")]
    public string ID { get; set; }

    [Field(required: true,
           minLength: Sizes.NAME_MIN,
           maxLength: Sizes.NAME_MAX,
           description: "Provides a short name for this policy period of time, e.g. 'Christmas 2018'")]
    public string Name { get; set; }

    [Field(required: true, description: "Period start timestamp in UTC")]
    public DateTime? SD { get; set; }

    [Field(required: true, description: "Period end timestamp in UTC")]
    public DateTime? ED { get; set; }

    private string m_PolicyData;

    [NonSerialized]
    private ConfigSectionNode m_Policy;

    [Field(required: true, description: "Policy Json content")]
    public string PolicyContent
    {
      get => m_PolicyData;
      set
      {
        if (!m_PolicyData.EqualsOrdSenseCase(value)) m_Policy = null;
        m_PolicyData = value;
      }
    }

    /// <summary>
    /// Provides structured policy data
    /// </summary>
    public ConfigSectionNode Policy
    {
      get
      {
        if (m_Policy == null)
        {
          if (m_PolicyData.IsNotNullOrWhiteSpace())
            m_Policy = m_PolicyData.AsJSONConfig(handling: ConvertErrorHandling.Throw);
        }
        return m_Policy;
      }
      set
      {
        m_Policy = value;

        if (value == null || !value.Exists)
          m_PolicyData = null;
        else
          m_PolicyData = value.ToJSONString(JsonWritingOptions.PrettyPrintRowsAsMap);
      }
    }

  }


}
