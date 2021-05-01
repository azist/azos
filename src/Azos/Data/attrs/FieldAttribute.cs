/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Text;

using Azos.Conf;
using Azos.Serialization.JSON;

namespace Azos.Data
{
  /// <summary>
  /// Decorates data document fields, providing metadata for targeted Schema.FiedDef objects
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple=true, Inherited=false)]
  public sealed partial class FieldAttribute : TargetedAttribute
  {
    /// <summary>
    /// Decorates data document fields, providing metadata for targeted Schema.FiedDef objects
    /// </summary>
    public FieldAttribute(
          string targetName   = ANY_TARGET,
          StoreFlag storeFlag = StoreFlag.LoadAndStore,
          bool key            = false,
          DataKind kind       = DataKind.Text,
          bool required       = false,
          bool visible        = true,
          string valueList    = null,
          object dflt         = null,
          object min          = null,
          object max          = null,
          int minLength       = 0,
          int maxLength       = 0,
          CharCase charCase   = CharCase.AsIs,
          string backendName  = null,
          string backendType  = null,
          string description  = null,
          string metadata = null,
          bool nonUI = false,
          string formatRegExp = null,
          string formatDescr  = null,
          string displayFormat = null,
          bool isArow         = false
      ) : base(targetName, metadata)
    {
      m_Description = description;
      m_StoreFlag = storeFlag;
      m_BackendName = backendName;
      m_BackendType = backendType;
      m_Key = key;
      m_Kind = kind;
      m_Required = required;
      m_Visible = visible;
      m_Min = min;
      m_Max = max;
      m_Default = dflt;
      m_MinLength = minLength;
      m_MaxLength = maxLength;
      m_CharCase = charCase;
      m_ValueList = valueList;
      m_NonUI = nonUI;
      m_FormatRegExp = formatRegExp;
      m_FormatDescription = formatDescr;
      m_DisplayFormat = displayFormat;
      m_IsArow = isArow;
    }

    /// <summary>
    /// Decorates data document fields, providing metadata for targeted Schema.FiedDef objects. This .cotr is used for injection of pre-parsed value list
    /// </summary>
    public FieldAttribute(
          JsonDataMap valueList,
          string targetName   = ANY_TARGET,
          StoreFlag storeFlag = StoreFlag.LoadAndStore,
          bool key            = false,
          DataKind kind       = DataKind.Text,
          bool required       = false,
          bool visible        = true,
          object dflt         = null,
          object min          = null,
          object max          = null,
          int minLength       = 0,
          int maxLength       = 0,
          CharCase charCase   = CharCase.AsIs,
          string backendName  = null,
          string backendType  = null,
          string description  = null,
          string metadata = null,
          bool nonUI = false,
          string formatRegExp = null,
          string formatDescr  = null,
          string displayFormat = null
      ) : base(targetName, metadata)
    {
      if (valueList == null)
        throw new DataException("FieldAttribute(JSONDataMap valueList==null)");

      m_Description = description;
      m_StoreFlag = storeFlag;
      m_BackendName = backendName;
      m_BackendType = backendType;
      m_Key = key;
      m_Kind = kind;
      m_Required = required;
      m_Visible = visible;
      m_Min = min;
      m_Max = max;
      m_Default = dflt;
      m_MinLength = minLength;
      m_MaxLength = maxLength;
      m_CharCase = charCase;
      m_NonUI = nonUI;
      m_FormatRegExp = formatRegExp;
      m_FormatDescription = formatDescr;
      m_DisplayFormat = displayFormat;

      m_CacheValueListPresetInCtor = true;
      m_CacheValueList_Insensitive = valueList;
      m_CacheValueList_Sensitive = valueList;
      m_ValueList = null;
    }

    /// <summary>
    /// Decorates data document fields, providing metadata for targeted Schema.FiedDef objects.
    /// This .ctor clones the field as a whole from another TypedDocument type. This constructor is typically used
    /// to borrow field definitions between layers, such as form model borrows it from data access layer
    /// </summary>
    public FieldAttribute(Type cloneFromDocType): base(ANY_TARGET, null)
    {
      if (cloneFromDocType == null || !typeof(TypedDoc).IsAssignableFrom(cloneFromDocType))
        throw new DataException("FieldAttribute(tClone isnt TypedDoc)");
      CloneFromDocType = cloneFromDocType;
    }

    /// <summary>
    /// Decorates data document fields, providing metadata for targeted Schema.FiedDef objects.
    /// This .ctor creates a field definition attribute which inherits all of the attributes of another field attribute declared on the same
    /// data document property with the specified `deriveFromTargetName` target. An error is thrown if such field def attribute with the specified target is not found.
    /// Circular dependencies are prohibited. This constructor has no effect on dynamic (non-typed) data documents as the inheritance is ignored for
    /// dynamic schemas
    /// </summary>
    /// <param name="targetName">Target name, may not be null or blank</param>
    /// <param name="deriveFromTargetName">From target name, may be null which indicates ANY_TARGET</param>
    public FieldAttribute(string targetName, string deriveFromTargetName) : base(targetName.NonBlank(nameof(targetName)))
    {
      DeriveFromTargetName = deriveFromTargetName.IsNullOrWhiteSpace() ? ANY_TARGET : deriveFromTargetName;
    }

    /// <summary>
    /// Decorates data document fields, providing metadata for targeted Schema.FiedDef objects.
    /// This .ctor creates a field def attribute cloning all fields from prototype/proto field name then optionally overriding one by one.
    /// You can not reference the same `protoType` as cyclical references are not allowed
    /// </summary>
    public FieldAttribute(
          Type protoType,
          string protoFieldName, //Schema:Field
          string targetName   = ANY_TARGET,
          object storeFlag    = null,
          object key          = null,
          object kind         = null,
          object required     = null,
          object visible      = null,
          string valueList    = null,
          object dflt         = null,
          object min          = null,
          object max          = null,
          object minLength    = null,
          object maxLength    = null,
          object charCase     = null,
          string backendName  = null,
          string backendType  = null,
          string description  = null,
          string metadata     = null,
          object nonUI        = null,
          string formatRegExp = null,
          string formatDescr  = null,
          string displayFormat = null,
          object isArow        = null
      ) : base(targetName, null)
    {
      if (protoType==null || protoFieldName.IsNullOrWhiteSpace()) throw new DataException(StringConsts.ARGUMENT_ERROR+"FieldAttr.ctor(protoType|protoFieldName=null|empty)");
      try
      {
        var schema = Schema.GetForTypedDoc(protoType);
        var protoTargetName = targetName;
        var segs = protoFieldName.Split(':');
        if (segs.Length>1)
        {
          protoTargetName = segs[0].Trim();
          protoFieldName = segs[1].Trim();
        }
        if (protoTargetName.IsNullOrWhiteSpace()) throw new Exception("Wrong target syntax");
        if (protoFieldName.IsNullOrWhiteSpace()) throw new Exception("Wrong field syntax");

        var protoFieldDef = schema[protoFieldName];
        if (protoFieldDef==null) throw new Exception("Prototype '{0}' field '{1}' not found".Args(protoType.FullName, protoFieldName));
        var protoAttr = protoFieldDef[protoTargetName];

        try
        {
          m_StoreFlag        = storeFlag    == null? protoAttr.StoreFlag   : (StoreFlag)storeFlag;
          m_BackendName      = backendName  == null? protoAttr.BackendName : backendName;
          m_BackendType      = backendType  == null? protoAttr.BackendType : backendType;
          m_Key              = key          == null? protoAttr.Key         : (bool)key;
          m_Kind             = kind         == null? protoAttr.Kind        : (DataKind)kind;
          m_Required         = required     == null? protoAttr.Required    : (bool)required;
          m_Visible          = visible      == null? protoAttr.Visible     : (bool)visible;
          m_Min              = min          == null? protoAttr.Min         : min;
          m_Max              = max          == null? protoAttr.Max         : max;
          m_Default          = dflt         == null? protoAttr.Default     : dflt;
          m_MinLength        = minLength    == null? protoAttr.MinLength   : (int)minLength;
          m_MaxLength        = maxLength    == null? protoAttr.MaxLength   : (int)maxLength;
          m_CharCase         = charCase     == null? protoAttr.CharCase    : (CharCase)charCase;
          m_ValueList        = valueList    == null? protoAttr.ValueList   : valueList;
          m_Description      = description  == null? protoAttr.Description : description;
          m_NonUI            = nonUI        == null? protoAttr.NonUI       : (bool)nonUI;
          m_FormatRegExp     = formatRegExp == null? protoAttr.FormatRegExp: formatRegExp;
          m_FormatDescription= formatDescr  == null? protoAttr.FormatDescription: formatDescr;
          m_DisplayFormat    = displayFormat== null? protoAttr.DisplayFormat : displayFormat;
          m_IsArow           = isArow       == null? protoAttr.IsArow        : (bool)isArow;


          if (metadata.IsNullOrWhiteSpace())
            MetadataContent = protoAttr.MetadataContent;
          else
          if (protoAttr.MetadataContent.IsNullOrWhiteSpace()) MetadataContent = metadata;
          else
          {
            var callSite = $"Proto of `{protoType.Name}`.`{protoFieldDef.Name}`";
            var conf1 = ParseMetadataContent(protoAttr.MetadataContent, callSite);
            var conf2 = ParseMetadataContent(metadata, callSite);

            var merged = new LaconicConfiguration();
            merged.CreateFromMerge(conf1, conf2);
            m_MetadataContent = merged.SaveToString();
          }
        }
        catch(Exception err)
        {
          throw new Exception("Invalid assignment of prototype override value: " + err.ToMessageWithType());
        }
      }
      catch(Exception error)
      {
        throw new DataException(StringConsts.CRUD_FIELD_ATTR_PROTOTYPE_CTOR_ERROR.Args(error.Message));
      }
    }

    /// <summary>
    /// When set, points to a Typed-Doc derivative that is used as a full clone
    /// </summary>
    public Type CloneFromDocType { get; private set; }

    /// <summary>
    /// When set, points to another attribute declaration on the same member with the specified targetName effectively "inheriting" this attribute
    /// from another
    /// </summary>
    public string DeriveFromTargetName { get; private set; }

    private StoreFlag m_StoreFlag;
    /// <summary>
    /// Determines whether field should be loaded/stored from/to storage
    /// </summary>
    public StoreFlag StoreFlag { get => m_StoreFlag; set => m_StoreFlag = AssignState(value); }

    private string m_BackendName;
    /// <summary>
    /// Provides an overridden name for this field
    /// </summary>
    public string BackendName { get => m_BackendName; set => m_BackendName = AssignState(value); }

    private string m_BackendType;
    /// <summary>
    /// Provides an overridden type for this field in backend,
    /// i.e. CLR string may be stored as ErlPid in Erlang
    /// </summary>
    public string BackendType { get => m_BackendType; set => m_BackendType = AssignState(value); }

    private bool m_Key;
    /// <summary>
    /// Determines whether this field is a part of the primary key
    /// </summary>
    public bool Key { get => m_Key; set => m_Key = AssignState(value); }

    private DataKind m_Kind;
    /// <summary>
    /// Provides hint/classification for textual field data
    /// </summary>
    public DataKind Kind { get => m_Kind; set => m_Kind = AssignState(value); }

    private bool m_Required;
    /// <summary>
    /// Determines whether the field must have data
    /// </summary>
    public bool Required { get => m_Required; set => m_Required = AssignState(value); }

    private bool m_Visible;
    /// <summary>
    /// Determines whether the field is shown to user (e.g. as a grid column)
    /// </summary>
    public bool Visible { get => m_Visible; set => m_Visible = AssignState(value); }

    private string m_ValueList;
    /// <summary>
    /// Returns a ";/,/|"-delimited list of permitted field values - used for lookup validation
    /// </summary>
    public string ValueList
    {
      get => m_ValueList;
      set
      {
        m_ValueList = AssignState(value);
        m_CacheValueList_Insensitive = null;
        m_CacheValueList_Sensitive = null;
      }
    }

    /// <summary>
    /// Returns true if the value list is set or internal JSONDataMap is set
    /// </summary>
    public bool HasValueList => ValueList.IsNotNullOrWhiteSpace() || m_CacheValueList_Sensitive != null;

                  private bool m_CacheValueListPresetInCtor;
                  private JsonDataMap m_CacheValueList_Sensitive;
                  private JsonDataMap m_CacheValueList_Insensitive;

    /// <summary>
    /// Returns a ValueList parsed into key values as:  val1: descr1,val2: desc2...
    /// </summary>
    public JsonDataMap ParseValueList(bool caseSensitiveKeys = false)
    {
      if (caseSensitiveKeys)
      {
        if (m_CacheValueList_Sensitive==null)
          m_CacheValueList_Sensitive = ParseValueListString(ValueList, true);

        return m_CacheValueList_Sensitive;
      }
      else
      {
        if (m_CacheValueList_Insensitive==null)
          m_CacheValueList_Insensitive = ParseValueListString(ValueList, false);

        return m_CacheValueList_Insensitive;
      }
    }

    public const char VALUE_LIST_SEPARATOR_1 = ',';
    public const char VALUE_LIST_SEPARATOR_2 = ';';
    public const char VALUE_LIST_ALT_KEY_SEPARATOR = '|';

    private static readonly char[] VALUE_LIST_SEPARATORS = new[]{ VALUE_LIST_SEPARATOR_1 , VALUE_LIST_SEPARATOR_2 };
    private static readonly char[] VALUE_LIST_ALT_KEY_SEPARATORS = new[] { VALUE_LIST_ALT_KEY_SEPARATOR };

    /// <summary>
    /// Returns a string parsed into key values as:  'val1: descr1, val2: desc2...'
    /// The spec: The values are separated with comma, pipe or semicolon having key names separated by colon from descriptions
    /// </summary>
    public static JsonDataMap ParseValueListString(string valueList, bool caseSensitiveKeys = false)
    {
      var result = new JsonDataMap(caseSensitiveKeys);
      if (valueList.IsNullOrWhiteSpace()) return result;

      var segs = valueList.Split(VALUE_LIST_SEPARATORS, StringSplitOptions.RemoveEmptyEntries);
      foreach(var seg in segs)
      {
        if (seg.IsNullOrWhiteSpace()) continue;

        var i = seg.LastIndexOf(':');
        var key = seg.Trim();
        var val = key;

        if (i>0 && i<seg.Length-1)
        {
          key = seg.Substring(0, i).Trim();
          val = seg.Substring(i+1).Trim();
        }

        var altkeys = key.Split(VALUE_LIST_ALT_KEY_SEPARATORS, StringSplitOptions.RemoveEmptyEntries);
        foreach(var altkey in altkeys)
        {
          if (altkey.IsNullOrWhiteSpace()) continue;

          var ak = altkey.Trim();

          if (result.ContainsKey(ak))
            throw new DataException(StringConsts.CRUD_FIELD_VALUE_LIST_DUP_ERROR.Args(valueList.TakeFirstChars(32), ak));

          result[ak] = val;
        }
      }

      return result;
    }

    /// <summary>
    /// Returns a string representation of JsonDataMap per value list specification:
    /// The values are separated with comma, pipe or semicolon having key names separated by colon from descriptions
    /// Example: 'key1: value1, key2: value2.... '
    /// </summary>
    public static string BuildValueListString(JsonDataMap map)
    {
      if (map==null) return null;
      var sb = new StringBuilder();
      var first = true;
      foreach(var kvp in map)
      {
        if (!first) sb.Append("; ");
        first = false;
        sb.AppendFormat("{0}:{1}", kvp.Key, kvp.Value);
      }
      return sb.ToString();
    }

    private object m_Min;
    /// <summary>
    /// Provides low-bound validation check
    /// </summary>
    public object Min { get => m_Min; set => m_Min = AssignState(value); }

    private object m_Max;
    /// <summary>
    /// Provides high-bound validation check
    /// </summary>
    public object Max { get => m_Max; set => m_Max = AssignState(value); }

    private object m_Default;
    /// <summary>
    /// Provides default value
    /// </summary>
    public object Default { get => m_Default; set => m_Default = AssignState(value); }

    private int m_MinLength;
    /// <summary>
    /// Imposes a limit on minimum amount of characters in a textual field
    /// </summary>
    public int MinLength { get => m_MinLength; set => m_MinLength = AssignState(value); }

    private int m_MaxLength;
    /// <summary>
    /// Imposes a limit on maximum amount of characters in a textual field
    /// </summary>
    public int MaxLength { get => m_MaxLength; set => m_MaxLength = AssignState(value); }

    private CharCase m_CharCase;
    /// <summary>
    /// Controls character casing of textual fields
    /// </summary>
    public CharCase CharCase { get => m_CharCase; set => m_CharCase = AssignState(value); }

    private string m_FormatRegExp;
    /// <summary>
    /// Regular expression used for field format validation if set
    /// </summary>
    public string FormatRegExp { get => m_FormatRegExp; set => m_FormatRegExp = AssignState(value); }

    private string m_FormatDescription;
    /// <summary>
    /// Description for regular expression used for field format validation if set
    /// </summary>
    public string FormatDescription { get => m_FormatDescription; set => m_FormatDescription = AssignState(value); }

    private string m_DisplayFormat;
    /// <summary>
    /// Display format string or null
    /// </summary>
    public string DisplayFormat { get => m_DisplayFormat; set => m_DisplayFormat = AssignState(value); }

    private bool m_NonUI;
    /// <summary>
    /// If true indicates that this field is ignored when generating UI and ignored when UI supplies the value to the server.
    /// Pass true to protect server-only structures from being modified by client
    /// </summary>
    public bool NonUI { get => m_NonUI; set => m_NonUI = AssignState(value); }

    private bool m_IsArow;
    /// <summary>
    /// True if this field definition is used by Arow serializer. This used here for convenience not to repeat voluminous field attributes for
    /// Arow serialization as field def already contains all data see Azos.Serialization.Arow
    /// </summary>
    public bool IsArow { get => m_IsArow; set => m_IsArow = AssignState(value); }

    public override int GetHashCode() => base.GetHashCode() ^ (int)Kind ^ (int)StoreFlag ^ (Required ? 0b10101010 : 0b01010101);

    public override bool Equals(object obj)
    {
      var other = obj as FieldAttribute;
      if (other==null) return false;

      if (!base.Equals(other)) return false; //target metadataContent description

      var equ =
          StoreFlag   == other.StoreFlag &&
          BackendName.EqualsOrdSenseCase(other.BackendName) &&
          BackendType.EqualsOrdSenseCase(other.BackendType) &&
          Key         == other.Key &&
          Kind        == other.Kind &&
          Required    == other.Required &&
          Visible     == other.Visible &&

          (
            (Min==null && other.Min==null) ||
            (Min!=null && other.Min!=null && Min.Equals(other.Min))
          ) &&

          (
            (Max==null && other.Max==null) ||
            (Max!=null && other.Max!=null && Max.Equals(other.Max))
          ) &&

          (
            (Default==null && other.Default==null) ||
            (Default!=null && other.Default!=null && Default.Equals(other.Default))
          ) &&

          MinLength   == other.MinLength &&
          MaxLength   == other.MaxLength &&
          CharCase    == other.CharCase &&
          ValueList.EqualsOrdSenseCase(other.ValueList) &&
          NonUI == other.NonUI &&
          FormatRegExp.EqualsOrdSenseCase(other.FormatRegExp) &&
          FormatDescription.EqualsOrdSenseCase(other.FormatDescription)&&
          DisplayFormat.EqualsOrdSenseCase(other.DisplayFormat) &&
          IsArow == other.IsArow &&
          (
              (!m_CacheValueListPresetInCtor)||
              (m_CacheValueList_Sensitive==null && other.m_CacheValueList_Sensitive==null) ||
              (
                m_CacheValueList_Sensitive!=null && other.m_CacheValueList_Sensitive!=null &&
                object.ReferenceEquals(m_CacheValueList_Sensitive, other.m_CacheValueList_Sensitive)
              )
          );

      return equ;
    }

    protected internal override void ExpandResourceReferencesRelativeTo(Type tDoc, string entity)
    {
      if (tDoc==null) return;

      base.ExpandResourceReferencesRelativeTo(tDoc, entity);

      FormatDescription = ExpandOneResourceReferences(tDoc, entity, "format-description", FormatDescription);

      if (!m_CacheValueListPresetInCtor)
      {
        ValueList = ExpandOneResourceReferences(tDoc, entity, "value-list", ValueList);
        m_CacheValueList_Insensitive = null;
        m_CacheValueList_Sensitive = null;
      }
    }

  }
}
