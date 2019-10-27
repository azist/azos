/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;
using Azos.Platform;
using Azos.Serialization.JSON;

namespace Azos.Data
{
  /// <summary>
  /// Provides information about table schema that this typed row is a part of
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple=true, Inherited=false)]
  public sealed class FieldAttribute : TargetedAttribute
  {
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
      Description = description;
      StoreFlag = storeFlag;
      BackendName = backendName;
      BackendType = backendType;
      Key = key;
      Kind = kind;
      Required = required;
      Visible = visible;
      Min = min;
      Max = max;
      Default = dflt;
      MinLength = minLength;
      MaxLength = maxLength;
      CharCase = charCase;
      ValueList = valueList;
      NonUI = nonUI;
      FormatRegExp = formatRegExp;
      FormatDescription = formatDescr;
      DisplayFormat = displayFormat;
      IsArow = isArow;
    }

    /// <summary>
    /// Used for injection of pre-parsed value list
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

      Description = description;
      StoreFlag = storeFlag;
      BackendName = backendName;
      BackendType = backendType;
      Key = key;
      Kind = kind;
      Required = required;
      Visible = visible;
      Min = min;
      Max = max;
      Default = dflt;
      MinLength = minLength;
      MaxLength = maxLength;
      CharCase = charCase;
      NonUI = nonUI;
      FormatRegExp = formatRegExp;
      FormatDescription = formatDescr;
      DisplayFormat = displayFormat;

      m_CacheValueListPresetInCtor = true;
      m_CacheValueList_Insensitive = valueList;
      m_CacheValueList_Sensitive = valueList;
      ValueList = null;
    }


    public FieldAttribute(Type cloneFromDocType): base(ANY_TARGET, null)
    {
      if (cloneFromDocType == null || !typeof(TypedDoc).IsAssignableFrom(cloneFromDocType))
        throw new DataException("FieldAttribute(tClone isnt TypedDoc)");
      CloneFromDocType = cloneFromDocType;
    }


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
          StoreFlag        = storeFlag    == null? protoAttr.StoreFlag   : (StoreFlag)storeFlag;
          BackendName      = backendName  == null? protoAttr.BackendName : backendName;
          BackendType      = backendType  == null? protoAttr.BackendType : backendType;
          Key              = key          == null? protoAttr.Key         : (bool)key;
          Kind             = kind         == null? protoAttr.Kind        : (DataKind)kind;
          Required         = required     == null? protoAttr.Required    : (bool)required;
          Visible          = visible      == null? protoAttr.Visible     : (bool)visible;
          Min              = min          == null? protoAttr.Min         : min;
          Max              = max          == null? protoAttr.Max         : max;
          Default          = dflt         == null? protoAttr.Default     : dflt;
          MinLength        = minLength    == null? protoAttr.MinLength   : (int)minLength;
          MaxLength        = maxLength    == null? protoAttr.MaxLength   : (int)maxLength;
          CharCase         = charCase     == null? protoAttr.CharCase    : (CharCase)charCase;
          ValueList        = valueList    == null? protoAttr.ValueList   : valueList;
          Description      = description  == null? protoAttr.Description : description;
          NonUI            = nonUI        == null? protoAttr.NonUI       : (bool)nonUI;
          FormatRegExp     = formatRegExp == null? protoAttr.FormatRegExp: formatRegExp;
          FormatDescription= formatDescr  == null? protoAttr.FormatDescription: formatDescr;
          DisplayFormat    = displayFormat== null? protoAttr.DisplayFormat : displayFormat;
          IsArow           = isArow       == null? protoAttr.IsArow        : (bool)isArow;



          if (metadata.IsNullOrWhiteSpace())
            MetadataContent = protoAttr.MetadataContent;
          else
          if (protoAttr.MetadataContent.IsNullOrWhiteSpace()) MetadataContent = metadata;
          else
          {
            var conf1 = ParseMetadataContent(protoAttr.MetadataContent);
            var conf2 = ParseMetadataContent(metadata);

            var merged = new LaconicConfiguration();
            merged.CreateFromMerge(conf1, conf2);
            MetadataContent = merged.SaveToString();
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


    private StoreFlag m_StoreFlag;
    /// <summary>
    /// Determines whether field should be loaded/stored from/to storage
    /// </summary>
    public StoreFlag StoreFlag { get => m_StoreFlag; set => m_StoreFlag = CheckNotSealed(value); }


    private string m_BackendName;
    /// <summary>
    /// Provides an overridden name for this field
    /// </summary>
    public string BackendName { get => m_BackendName; set => m_BackendName = CheckNotSealed(value); }


    private string m_BackendType;
    /// <summary>
    /// Provides an overridden type for this field in backend,
    /// i.e. CLR string may be stored as ErlPid in Erlang
    /// </summary>
    public string BackendType { get => m_BackendType; set => m_BackendType = CheckNotSealed(value); }


    private bool m_Key;
    /// <summary>
    /// Determines whether this field is a part of the primary key
    /// </summary>
    public bool Key { get => m_Key; set => m_Key = CheckNotSealed(value); }


    private DataKind m_Kind;
    /// <summary>
    /// Provides hint/classification for textual field data
    /// </summary>
    public DataKind Kind { get => m_Kind; set => m_Kind = CheckNotSealed(value); }


    private bool m_Required;
    /// <summary>
    /// Determines whether the field must have data
    /// </summary>
    public bool Required { get => m_Required; set => m_Required = CheckNotSealed(value); }


    private bool m_Visible;
    /// <summary>
    /// Determines whether the field is shown to user (e.g. as a grid column)
    /// </summary>
    public bool Visible { get => m_Visible; set => m_Visible = CheckNotSealed(value); }


    private string m_ValueList;
    /// <summary>
    /// Returns a ";/,/|"-delimited list of permitted field values - used for lookup validation
    /// </summary>
    public string ValueList { get => m_ValueList; set => m_ValueList = CheckNotSealed(value); }

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

    /// <summary>
    /// Returns a string parsed into key values as:  val1: descr1,val2: desc2...
    /// </summary>
    public static JsonDataMap ParseValueListString(string valueList, bool caseSensitiveKeys = false)
    {
      var result = new JsonDataMap(caseSensitiveKeys);
      if (valueList.IsNullOrWhiteSpace()) return result;

      var segs = valueList.Split(',','|',';');
      foreach(var seg in segs)
      {
        var i = seg.LastIndexOf(':');
        if (i>0&&i<seg.Length-1) result[seg.Substring(0,i).Trim()] = seg.Substring(i+1).Trim();
        else
          result[seg] = seg;
      }

      return result;
    }


    private object m_Min;
    /// <summary>
    /// Provides low-bound validation check
    /// </summary>
    public object Min { get => m_Min; set => m_Min = CheckNotSealed(value); }


    private object m_Max;
    /// <summary>
    /// Provides high-bound validation check
    /// </summary>
    public object Max { get => m_Max; set => m_Max = CheckNotSealed(value); }


    private object m_Default;
    /// <summary>
    /// Provides default value
    /// </summary>
    public object Default { get => m_Default; set => m_Default = CheckNotSealed(value); }


    private int m_MinLength;
    /// <summary>
    /// Imposes a limit on minimum amount of characters in a textual field
    /// </summary>
    public int MinLength { get => m_MinLength; set => m_MinLength = CheckNotSealed(value); }


    private int m_MaxLength;
    /// <summary>
    /// Imposes a limit on maximum amount of characters in a textual field
    /// </summary>
    public int MaxLength { get => m_MaxLength; set => m_MaxLength = CheckNotSealed(value); }


    private CharCase m_CharCase;
    /// <summary>
    /// Controls character casing of textual fields
    /// </summary>
    public CharCase CharCase { get => m_CharCase; set => m_CharCase = CheckNotSealed(value); }


    private string m_FormatRegExp;
    /// <summary>
    /// Regular expression used for field format validation if set
    /// </summary>
    public string FormatRegExp { get => m_FormatRegExp; set => m_FormatRegExp = CheckNotSealed(value); }


    private string m_FormatDescription;
    /// <summary>
    /// Description for regular expression used for field format validation if set
    /// </summary>
    public string FormatDescription { get => m_FormatDescription; set => m_FormatDescription = CheckNotSealed(value); }


    private string m_DisplayFormat;
    /// <summary>
    /// Display format string or null
    /// </summary>
    public string DisplayFormat { get => m_DisplayFormat; set => m_DisplayFormat = CheckNotSealed(value); }


    private bool m_NonUI;
    /// <summary>
    /// If true indicates that this field is ignored when generating UI and ignored when UI supplies the value to the server.
    /// Pass true to protect server-only structures from being modified by client
    /// </summary>
    public bool NonUI { get => m_NonUI; set => m_NonUI = CheckNotSealed(value); }


    private bool m_IsArow;
    /// <summary>
    /// True if this field definition is used by Arow serializer. This used here for convenience not to repeat voluminous field attributes for
    /// Arow serialization as field def already contains all data see Azos.Serialization.Arow
    /// </summary>
    public bool IsArow { get => m_IsArow; set => m_IsArow = CheckNotSealed(value); }


    public override int GetHashCode() => base.GetHashCode() ^ (int)Kind ^ (int)StoreFlag ^ (Required ? 0b10101010 : 0b01010101);

    public override bool Equals(object obj)
    {
      var other = obj as FieldAttribute;
      if (other==null) return false;

      if (!base.Equals(other)) return false;

      var equ =
          this.StoreFlag   == other.StoreFlag &&
          this.BackendName.EqualsOrdSenseCase(other.BackendName) &&
          this.BackendType.EqualsOrdSenseCase(other.BackendType) &&
          this.Key         == other.Key &&
          this.Kind        == other.Kind &&
          this.Required    == other.Required &&
          this.Visible     == other.Visible &&

          (
            (this.Min==null && other.Min==null) ||
            (this.Min!=null && other.Min!=null && this.Min.Equals(other.Min))
          ) &&

          (
            (this.Max==null && other.Max==null) ||
            (this.Max!=null && other.Max!=null && this.Max.Equals(other.Max))
          ) &&

          (
            (this.Default==null && other.Default==null) ||
            (this.Default!=null && other.Default!=null && this.Default.Equals(other.Default))
          ) &&

          this.MinLength   == other.MinLength &&
          this.MaxLength   == other.MaxLength &&
          this.CharCase    == other.CharCase &&
          this.ValueList.EqualsOrdSenseCase(other.ValueList) &&
          this.MetadataContent.EqualsOrdSenseCase(other.MetadataContent) &&
          this.NonUI == other.NonUI &&
          this.FormatRegExp.EqualsOrdSenseCase(other.FormatRegExp) &&
          this.FormatDescription.EqualsOrdSenseCase(other.FormatDescription)&&
          this.DisplayFormat.EqualsOrdSenseCase(other.DisplayFormat) &&
          this.IsArow == other.IsArow &&
          (
              (!m_CacheValueListPresetInCtor)||
              (this.m_CacheValueList_Sensitive==null && other.m_CacheValueList_Sensitive==null) ||
              (
                this.m_CacheValueList_Sensitive!=null && other.m_CacheValueList_Sensitive!=null &&
                object.ReferenceEquals(this.m_CacheValueList_Sensitive, other.m_CacheValueList_Sensitive)
              )
          );

      return equ;
    }

    protected internal override void ExpandResourceReferencesRelativeTo(Type tDoc, string entity)
    {
      if (tDoc==null) return;

      base.ExpandResourceReferencesRelativeTo(tDoc, entity);

      this.FormatDescription = ExpandOneResourceReferences(tDoc, entity, "format-description", this.FormatDescription);

      if (!m_CacheValueListPresetInCtor)
      {
        this.ValueList = ExpandOneResourceReferences(tDoc, entity, "value-list", this.ValueList);
        this.m_CacheValueList_Insensitive = null;
        this.m_CacheValueList_Sensitive = null;
      }
    }

  }
}
