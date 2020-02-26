
using System.Linq;

using Azos.Serialization.JSON;

namespace Azos.IAM
{
  /// <summary>
  /// Facilitates structured property access
  /// </summary>
  public static class Properties
  {
    public const string PRIMARY = "1";
    public const string SECONDARY = "2";

    public const string FIRST_NAME = "fnm";
    public const string MIDDLE_NAME = "mnm";
    public const string LAST_NAME = "lnm";
    public const string SUFFIX_NAME = "sfx";
    public const string EMAIL = "eml";
    public const string PHONE_VOICE = "tel";
    public const string PHONE_TEXT = "txt";

    public const string HOME = "hom";
    public const string WORK = "wrk";
    public const string MOBIL = "mob";

    public const string RELATIVES = "relatives";

    public const string ADDRESS = "addr";

    public const string COUNTRY = "country";
    public const string REGION = "reg";
    public const string CITY = "city";
    public const string STATE = "state";//state/province
    public const string PCODE = "code";//e.g. zip
    public const string LINE1 = "1";
    public const string LINE2 = "2";

    public const string NAME = "name";
    public const string DESCRIPTION = "dscr";
    public const string UNIT = "unit";
    public const string ORG = "org";
    public const string EMPLOYER = "empl";
    public const string GROUP = "group";
    public const string PARENT = "parent";

    public const string BUSINESS = "bis";
    public const string EMERGENCY = "ems";
    public const string FINANCIAL = "fin";
    public const string MEDICAL = "med";
    public const string LEGAL = "leg";
    public const string ACCOUNTING = "accnt";
    public const string HUMAN_RESOURCES = "hr";
    public const string SALES = "sls";
    public const string MARKTING = "mrkt";
    public const string IT = "it";
    public const string SUPPORT = "sup";
    public const string MANAGEMENT = "man";


    /// <summary>
    /// Facilitates structured property access
    /// </summary>
    public static PropertySection Access(this JsonDataMap data, params string[] nameSegments)
    =>  new PropertySection(data.NonNull(nameof(data)), string.Join(":", nameSegments.Where(s => s.IsNotNullOrWhiteSpace())).NonBlank(nameof(nameSegments)));

    /// <summary>
    /// Facilitates structured property access
    /// </summary>
    public sealed class PropertySection
    {
      internal PropertySection(JsonDataMap data, string name)
      {
        m_Data = data;
        m_Name = name;
      }
      private JsonDataMap m_Data;
      public string m_Name;


      public string Name => m_Name;

      public string Value
      {
        get => m_Data[m_Name]?.ToString();
        set => m_Data[m_Name] = value?.ToString();
      }
    }
  }
}
