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
      Description = description;
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
      Description = description;
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
            m_MetadataContent = protoAttr.m_MetadataContent;
          else
          if (protoAttr.m_MetadataContent.IsNullOrWhiteSpace()) m_MetadataContent = metadata;
          else
          {
            var conf1 = ParseMetadataContent(protoAttr.m_MetadataContent);
            var conf2 = ParseMetadataContent(metadata);

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
    /// Determines whether field should be loaded/stored from/to storage
    /// </summary>
    public StoreFlag StoreFlag { get; private set; }

    /// <summary>
    /// Provides an overridden name for this field
    /// </summary>
    public string BackendName { get; private set; }

    /// <summary>
    /// Provides an overridden type for this field in backend,
    /// i.e. CLR string may be stored as ErlPid in Erlang
    /// </summary>
    public string BackendType { get; private set; }

    /// <summary>
    /// Determines whether this field is a part of the primary key
    /// </summary>
    public bool Key { get; private set; }

    /// <summary>
    /// Provides hint/classification for textual field data
    /// </summary>
    public DataKind Kind { get; private set; }

    /// <summary>
    /// Determines whether the field must have data
    /// </summary>
    public bool Required { get; private set; }

    /// <summary>
    /// Determines whether the field is shown to user (i.e. as a grid column)
    /// </summary>
    public bool Visible { get; private set; }

    /// <summary>
    /// Returns a ";/,/|"-delimited list of permitted field values - used for lookup validation
    /// </summary>
    public string ValueList { get; private set; }

    /// <summary>
    /// Returns true if the value list is set or internal JSONDataMap is set
    /// </summary>
    public bool HasValueList{ get{ return ValueList.IsNotNullOrWhiteSpace() || m_CacheValueList_Sensitive!=null;} }

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

    /// <summary>
    /// Provides low-bound validation check
    /// </summary>
    public object Min { get; private set; }

    /// <summary>
    /// Provides high-bound validation check
    /// </summary>
    public object Max { get; private set; }

    /// <summary>
    /// Provides default value
    /// </summary>
    public object Default { get; private set; }


    /// <summary>
    /// Imposes a limit on minimum amount of characters in a textual field
    /// </summary>
    public int MinLength { get; private set; }

    /// <summary>
    /// Imposes a limit on maximum amount of characters in a textual field
    /// </summary>
    public int MaxLength { get; private set; }

    /// <summary>
    /// Controls character casing of textual fields
    /// </summary>
    public CharCase CharCase { get; private set; }

    /// <summary>
    /// Regular expression used for field format validation if set
    /// </summary>
    public string FormatRegExp { get; private set; }

    /// <summary>
    /// Description for regular expression used for field format validation if set
    /// </summary>
    public string FormatDescription { get; private set; }

    /// <summary>
    /// Display format string or null
    /// </summary>
    public string DisplayFormat { get; private set; }

    /// <summary>
    /// Provides description
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// If true indicates that this field is ignored when generating UI and ignored when UI supplies the value to the server.
    /// Pass true to protect server-only structures from being modified by client
    /// </summary>
    public bool NonUI { get; private set; }

    // This is moved here for convenience not to repeat voluminous field attributes for Arow serialization as field def already contains all data,
    // see Azos.Serialization.Arow
    /// <summary>
    /// True if this field definition is used by Arow serializer
    /// </summary>
    public bool IsArow { get; private set; }


    public override int GetHashCode()
    {
      return TargetName.GetHashCodeOrdSenseCase();
    }

    public override bool Equals(object obj)
    {
      var other = obj as FieldAttribute;
      if (other==null) return false;
      var equ =
          this.TargetName.EqualsOrdSenseCase(other.TargetName) &&
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
          this.Description.EqualsOrdSenseCase(other.Description) &&
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

    internal void __ExpandResourceReferencesRelativeTo(Type tDoc, string prop)
    {
      if (tDoc==null || prop.IsNullOrWhiteSpace()) return;

      this.Description = expand(tDoc, prop, "description", this.Description);
      this.FormatDescription = expand(tDoc, prop, "format-description", this.FormatDescription);

      if (!m_CacheValueListPresetInCtor)
      {
        this.ValueList = expand(tDoc, prop, "value-list", this.ValueList);
        this.m_CacheValueList_Insensitive = null;
        this.m_CacheValueList_Sensitive = null;
      }
    }

    private string expand(Type tDoc, string prop, string name, string value)
    {
      const string PFX = "./";
      if (value.IsNullOrWhiteSpace()) return value;
      if (!value.StartsWith(PFX)) return value;
      value = value.Substring(PFX.Length);

      var res = value.IsNullOrWhiteSpace() ? "schemas.laconf" : "{0}.laconf".Args(value);

      var resContent = tDoc.NonNull(nameof(tDoc))
                            .GetText(res)
                            .NonBlank("Resource `{0}` referenced by {1}.{2}.{3}".Args(res, tDoc.Name, prop, name));

      try
      {
        var cfg = resContent.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
        return cfg.Navigate("!/{0}/{1}/${2}".Args(tDoc.Name, prop, name)).Value;
      }
      catch(Exception error)
      {
        throw new DataException("Error expanding resource reference {0}.{1}.{2} resource: {3}".Args(tDoc.Name, prop, name, error.ToMessageWithType()), error);
      }
    }

  }
}
