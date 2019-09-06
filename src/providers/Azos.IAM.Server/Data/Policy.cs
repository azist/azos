using System;
using System.Collections.Generic;
using System.Text;

using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.IAM.Server.Data
{
  /// <summary>
  /// Stores named config vectors which contain policy/settings applied to groups/accounts
  /// </summary>
  public sealed class Policy : EntityWithProperties
  {
    /// <summary>
    /// Provides policy settings in a time span
    /// </summary>
    public sealed class PolicyPeriod : TimePeriod
    {
      private string m_PolicyData;

      [NonSerialized]
      private ConfigSectionNode m_Policy;

      [Field(required: true, description: "Policy Json content")]
      [Field(typeof(PolicyPeriod), nameof(PolicyContent), TMONGO, backendName: "r")]
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


    [Field(required: true,
           kind: DataKind.ScreenName,
           description: "Unique Policy ID",
           metadata: "idx{name='main' order=0 unique=true dir=asc}")]
    [Field(typeof(Policy), nameof(ID), TMONGO, backendName: "id")]
    public string ID { get; set; }

    [Field(required: true, description: "Contains a list of time periods with policy data")]
    [Field(typeof(Policy), nameof(Periods), TMONGO, backendName: "periods")]
    public PolicyPeriod[] Periods { get; set; }

  }
}
