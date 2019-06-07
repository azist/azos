/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using Azos.Collections;
using Azos.Serialization.JSON;

namespace Azos.Data
{
    /// <summary>
    /// Describes a schema for data documents: TypedDocs and DynamicDocs.
    /// DynamicDocs are "shaped" in memory from schema, whereas, TypedDocs define schema.
    /// Schema for TypedDocs is cached in static dictionary for speed
    /// </summary>
    [Serializable, CustomMetadata(typeof(SchemaCustomMetadataProvider))]
    public sealed class Schema : INamed, IEnumerable<Schema.FieldDef>, IJsonWritable
    {
        public const string EXTRA_SUPPORTS_INSERT_ATTR = "supports-insert";
        public const string EXTRA_SUPPORTS_UPDATE_ATTR = "supports-update";
        public const string EXTRA_SUPPORTS_DELETE_ATTR = "supports-delete";


        #region Inner Classes

            /// <summary>
            /// Provides a definition for a single field of a row
            /// </summary>
            [Serializable]
            public sealed class FieldDef : INamed, IOrdered, ISerializable, IJsonWritable
            {

                public FieldDef(string name, Type type, FieldAttribute attr)
                {
                    ctor(name, 0, type, new[]{attr}, null);
                }

                public FieldDef(string name, Type type, IEnumerable<FieldAttribute> attrs)
                {
                    ctor(name, 0, type, attrs, null);
                }

                public FieldDef(string name, Type type, Access.QuerySource.ColumnDef columnDef)
                {
                    FieldAttribute attr;
                    if (columnDef!=null)
                        attr =  new FieldAttribute(targetName: TargetedAttribute.ANY_TARGET,
                                                  storeFlag: columnDef.StoreFlag,
                                                  required: columnDef.Required,
                                                  visible: columnDef.Visible,
                                                  key: columnDef.Key,
                                                  backendName: columnDef.BackendName,
                                                  description: columnDef.Description);
                    else
                       attr =  new FieldAttribute(targetName: TargetedAttribute.ANY_TARGET);

                    var attrs = new FieldAttribute[1] { attr };
                    ctor(name, 0, type, attrs, null);
                }

                internal FieldDef(string name, int order, Type type, IEnumerable<FieldAttribute> attrs, PropertyInfo memberInfo = null)
                {
                    ctor(name, order, type, attrs, memberInfo);
                }

                private void ctor(string name, int order, Type type, IEnumerable<FieldAttribute> attrs, PropertyInfo memberInfo = null)
                {
                    if (name.IsNullOrWhiteSpace() || type==null || attrs==null)
                        throw new DataException(StringConsts.ARGUMENT_ERROR + "FieldDef.ctor(..null..)");

                    m_Name = name;
                    m_Order = order;
                    m_Type = type;
                    m_Attrs = new List<FieldAttribute>(attrs);

                    if (m_Attrs.Count<1)
                     throw new DataException(StringConsts.CRUD_FIELDDEF_ATTR_MISSING_ERROR.Args(name));

                    //add ANY_TARGET attribute
                    if (!m_Attrs.Any(a => a.TargetName == TargetedAttribute.ANY_TARGET))
                    {
                      var isAnyKey = m_Attrs.Any( a => a.Key );
                      var ata = new FieldAttribute(FieldAttribute.ANY_TARGET, key: isAnyKey);
                      m_Attrs.Add( ata );
                    }

                    m_MemberInfo = memberInfo;

                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        m_NonNullableType = type.GetGenericArguments()[0];
                    else
                        m_NonNullableType = type;

                    m_AnyTargetKey = this[null].Key;
                }


                private FieldDef(SerializationInfo info, StreamingContext context)
                {
                    m_Name = info.GetString("nm");
                    m_Order = info.GetInt32("o");
                    m_Type = Type.GetType( info.GetString("t"), true);
                    m_NonNullableType = Type.GetType( info.GetString("nnt"), true);
                    m_Attrs = info.GetValue("attrs", typeof(List<FieldAttribute>)) as List<FieldAttribute>;
                    m_AnyTargetKey = info.GetBoolean("atk");

                    var mtp = info.GetString("mtp");
                    if (mtp!=null)
                    {
                        var tp = Type.GetType( mtp, true);

                        var mn = info.GetString("mn");
                        if (mn!=null)
                        {
                            m_MemberInfo = tp.GetProperty(mn);
                        }
                    }

                }

                public void GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    info.AddValue("nm", m_Name);
                    info.AddValue("o", m_Order);
                    info.AddValue("t", m_Type.AssemblyQualifiedName);
                    info.AddValue("nnt", m_NonNullableType.AssemblyQualifiedName);
                    info.AddValue("attrs", m_Attrs);
                    info.AddValue("atk", m_AnyTargetKey);

                    if (m_MemberInfo==null)
                    {
                       info.AddValue("mtp", null);
                       info.AddValue("mn", null);
                    }
                    else
                    {
                       info.AddValue("mtp", m_MemberInfo.DeclaringType.AssemblyQualifiedName);
                       info.AddValue("mn", m_MemberInfo.Name);
                    }


                }


                private string m_Name;
                internal int m_Order;
                private Type m_Type;
                private Type m_NonNullableType;
                private List<FieldAttribute> m_Attrs;
                private PropertyInfo m_MemberInfo;
                private bool m_AnyTargetKey;

                /// <summary>
                /// Returns the name of the field
                /// </summary>
                public string                        Name  { get { return m_Name;}}

                /// <summary>
                /// Returns the field type
                /// </summary>
                public Type                          Type  { get { return m_Type;}}

                /// <summary>
                /// For nullable value types returns the field type regardless of nullability, it is the type argument of Nullable struct;
                /// For reference types returns the same type as Type property
                /// </summary>
                public Type                          NonNullableType  { get { return m_NonNullableType;}}

                /// <summary>
                /// Returns field attributes
                /// </summary>
                public IEnumerable<FieldAttribute>   Attrs { get { return m_Attrs;}}

                /// <summary>
                /// Gets absolute field order index in a row
                /// </summary>
                public int                           Order { get {return m_Order;} }

                /// <summary>
                /// For TypedRow-descendants returns a PropertyInfo object for the underlying property
                /// </summary>
                public PropertyInfo                  MemberInfo { get {return m_MemberInfo;} }

                /// <summary>
                /// Returns true when this field is attributed as being a key field in an attribute that targets ANY_TARGET
                /// </summary>
                public bool                          AnyTargetKey { get {return m_AnyTargetKey;} }


                /// <summary>
                /// Returns true when this field is attributed as being a visible in any of the targeted attribute
                /// </summary>
                public bool AnyVisible { get { return m_Attrs.Any(a=>a.Visible);} }


                /// <summary>
                /// Returns description from field attribute or parses it from field name
                /// </summary>
                public string Description
                {
                  get
                  {
                    var attr = this[null];
                    var result = attr!=null ? attr.Description : "";

                    if (result.IsNullOrWhiteSpace())
                     result = Azos.Text.Utils.ParseFieldNameToDescription(Name, true);

                    return result;
                  }
                }

                /// <summary>
                /// For fields with ValueList returns value's description per specified schema
                /// </summary>
                public string ValueDescription(object fieldValue, string target = null, bool caseSensitiveKeys = false)
                {
                  var sv = fieldValue.AsString();
                  if (sv.IsNullOrWhiteSpace()) return string.Empty;
                  var atr = this[target];
                  if (atr==null) return fieldValue.AsString(string.Empty);
                  var vl = atr.ParseValueList(caseSensitiveKeys);

                  return vl[sv].AsString(string.Empty);
                }


                /// <summary>
                /// Returns true when at least one attribute was marked as NonUI - meaning that this field must not be serialized-to/deserialized-from client UI
                /// </summary>
                public bool  NonUI
                {
                  get { return m_Attrs.Any(a=>a.NonUI);}
                }


                private volatile Dictionary<string, FieldAttribute> m_TargetAttrsCache = new Dictionary<string, FieldAttribute>(StringComparer.InvariantCultureIgnoreCase);

                /// <summary>
                /// Returns a FieldAttribute that matches the supplied targetName, or if one was not defined then
                ///  returns FieldAttribute which matches any target or null
                /// </summary>
                public FieldAttribute this[string targetName]
                {
                    get
                    {
                      if (targetName.IsNullOrWhiteSpace()) targetName = FieldAttribute.ANY_TARGET;

                      FieldAttribute result = null;
                      if (!m_TargetAttrsCache.TryGetValue(targetName, out result))
                      {
                        if (targetName!=FieldAttribute.ANY_TARGET)
                        {
                            result = m_Attrs.FirstOrDefault(a => targetName.EqualsIgnoreCase(a.TargetName));
                        }

                        if (result==null)
                          result = m_Attrs.FirstOrDefault(a => TargetedAttribute.ANY_TARGET.EqualsIgnoreCase(a.TargetName) );

                        var dict = new Dictionary<string, FieldAttribute>(m_TargetAttrsCache, StringComparer.InvariantCultureIgnoreCase);
                        dict[targetName] = result;
                        m_TargetAttrsCache = dict;//atomic
                      }

                      return result;
                    }
                }

                /// <summary>
                /// Returns the name of the field in backend that was possibly overriden for a particular target
                /// </summary>
                public string GetBackendNameForTarget(string targetName)
                {
                    FieldAttribute attr;
                    return GetBackendNameForTarget(targetName, out attr);
                }

                /// <summary>
                /// Returns the name of the field in backend that was possibly overriden for a particular target
                /// along with store flag
                /// </summary>
                public string GetBackendNameForTarget(string targetName, out FieldAttribute attr)
                {
                    var result = m_Name;
                    var fattr = this[targetName];
                    attr = fattr;
                    if (fattr!=null)
                    {
                      if (fattr.BackendName.IsNotNullOrWhiteSpace()) result = fattr.BackendName;
                    }
                    return result;
                }

                public override string ToString()
                {
                    return "FieldDef(Name: '{0}', Type: '{1}', Order: {2})".Args(m_Name, m_Type.FullName, m_Order);
                }

                public override int GetHashCode()
                {
                    return m_Name.GetHashCodeOrdIgnoreCase() ^ m_Order;
                }

                public override bool Equals(object obj)
                {
                    var other = obj as FieldDef;
                    if (other==null) return false;
                    return
                       this.m_Name.EqualsOrdIgnoreCase(other.m_Name) &&
                       this.m_Order==other.m_Order &&
                       this.m_Type == other.m_Type &&
                       this.m_Attrs.SequenceEqual(other.m_Attrs);
                }


                /// <summary>
                /// Writes fielddef as JSON. Do not call this method directly, instead call rowset.ToJSON() or use JSONWriter class
                /// </summary>
                void IJsonWritable.WriteAsJson(System.IO.TextWriter wri, int nestingLevel, JsonWritingOptions options)
                {
                    var attr = this[null];

                    if (attr != null && attr.NonUI)
                    {
                      wri.Write("{}");
                      return;//nothing to write for NONUI
                    }

                    bool typeIsNullable;
                    string tp = JSONMappings.MapCLRTypeToJSON(m_Type, out typeIsNullable);

                    var map = new Dictionary<string, object>
                    {
                      {"Name",  m_Name},
                      {"Order", m_Order},
                      {"Type",  tp},
                      {"Nullable", typeIsNullable}
                    };

                    //20190322 DKh inner schema
                    if (typeof(Doc).IsAssignableFrom(this.NonNullableType))
                    {
                      map["IsDataDoc"] = true;
                      map["IsAmorphous"] = typeof(IAmorphousData).IsAssignableFrom(this.NonNullableType);
                      map["IsForm"] = typeof(Form).IsAssignableFrom(this.NonNullableType);

                      if (typeof(TypedDoc).IsAssignableFrom(this.NonNullableType))
                      {
                        var innerSchema = Schema.GetForTypedDoc(this.NonNullableType);
                        if (innerSchema.Any(fd => typeof(TypedDoc).IsAssignableFrom(fd.Type)))
                          map["DataDocSchema"] = "@complex";
                        else
                          map["DataDocSchema"] = innerSchema;
                      }
                    }

                    if (attr!=null)
                    {
                        map.Add("IsKey", attr.Key);
                        map.Add("IsRequired", attr.Required);
                        map.Add("Visible", attr.Visible);
                        if (attr.Default!=null)map.Add("Default", attr.Default);
                        if (attr.CharCase!=CharCase.AsIs) map.Add("CharCase", attr.CharCase);
                        if (attr.Kind!=DataKind.Text) map.Add("Kind", attr.Kind);
                        if (attr.MinLength!=0) map.Add("MinLen", attr.MinLength);
                        if (attr.MaxLength!=0) map.Add("MaxLen", attr.MaxLength);
                        if (attr.Min!=null) map.Add("Min", attr.Min);
                        if (attr.Max!=null) map.Add("Max", attr.Max);
                        if (attr.ValueList!=null) map.Add("ValueList", attr.ValueList);
                        if (attr.Description!=null)map.Add("Description", attr.Description);
                        //metadata content is in the internal format and not dumped
                    }

                    JsonWriter.WriteMap(wri, map, nestingLevel, options);
                }


            }


        #endregion

        #region CONSTS


        #endregion

        #region .ctor / static

            /// <summary>
            /// Gets all property members of TypedDoc that are tagged as [Field]
            /// </summary>
            public static IEnumerable<PropertyInfo> GetFieldMembers(Type type)
            {
                //20140926 DKh +DeclaredOnly
                var local = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                                .Where(pi => Attribute.IsDefined(pi, typeof(FieldAttribute)));
                if (type.BaseType==typeof(object)) return local;

                var bt = type.BaseType;//null for Object

                if (bt != null)
                    return GetFieldMembers(type.BaseType).Concat(local);
                else
                    return local;
            }


            public static Schema FromJSON(string json, bool readOnly = false)
            {
              return FromJSON(JsonReader.DeserializeDataObject( json ) as JsonDataMap, readOnly);
            }

            public static Schema FromJSON(JsonDataMap map, bool readOnly = false)
            {
              if (map==null || map.Count==0)
                 throw new DataException(StringConsts.ARGUMENT_ERROR+"Schema.FromJSON(map==null|empty)");

              var name = map["Name"].AsString();
              if (name.IsNullOrWhiteSpace())
                throw new DataException(StringConsts.ARGUMENT_ERROR+"Schema.FromJSON(map.Name=null|empty)");

              var adefs = map["FieldDefs"] as JsonDataArray;
              if (adefs==null || adefs.Count==0)
                throw new DataException(StringConsts.ARGUMENT_ERROR+"Schema.FromJSON(map.FieldDefs=null|empty)");

              var defs = new List<Schema.FieldDef>();
              foreach(var mdef in adefs.Cast<JsonDataMap>())
              {
                var fname = mdef["Name"].AsString();
                if (fname.IsNullOrWhiteSpace())
                  throw new DataException(StringConsts.ARGUMENT_ERROR+"Schema.FromJSON(map.FierldDefs[name=null|empty])");

                var req = mdef["IsRequired"].AsBool();
                var key = mdef["IsKey"].AsBool();
                var vis = mdef["Visible"].AsBool();
                var desc = mdef["Description"].AsString();
                var minLen = mdef["MinLen"].AsInt();
                var maxLen = mdef["MaxLen"].AsInt();
                var valList = mdef["ValueList"].AsString();
                var strKind = mdef["Kind"].AsString();
                var dataKind = strKind==null ? DataKind.Text : (DataKind)Enum.Parse(typeof(DataKind), strKind);
                var strCase = mdef["CharCase"].AsString();
                var chrCase = strCase==null ? CharCase.AsIs : (CharCase)Enum.Parse(typeof(CharCase), strCase);

                var tp = mdef["Type"].AsString(string.Empty).ToLowerInvariant().Trim();
                var tpn = mdef["Nullable"].AsBool();

                var type = JSONMappings.MapJSONTypeToCLR(tp, tpn);

                var atr = new FieldAttribute(required: req, key: key, visible: vis, description: desc,
                                             minLength: minLen, maxLength: maxLen,
                                             valueList: valList, kind: dataKind, charCase: chrCase);
                var def = new Schema.FieldDef(fname, type, atr);
                defs.Add(def);
              }

              return new Schema(name, readOnly, defs);
            }





            private static Registry<Schema> s_TypedRegistry = new Registry<Schema>();


            /// <summary>
            /// Returns schema instance for the TypedDoc instance by fetching schema object from cache or
            ///  creating it if it has not been cached yet
            /// </summary>
            public static Schema GetForTypedDoc(TypedDoc doc)
            {
                return GetForTypedDoc(doc.GetType());
            }

            /// <summary>
            /// Returns schema instance for the TypedRow instance by fetching schema object from cache or
            ///  creating it if it has not been cached yet
            /// </summary>
            public static Schema GetForTypedDoc<TDoc>() where TDoc : TypedDoc
            {
                return GetForTypedDoc(typeof(TDoc));
            }


            /// <summary>
            /// Returns schema instance for the TypedRow instance by fetching schema object from cache or
            ///  creating it if it has not been cached yet
            /// </summary>
            public static Schema GetForTypedDoc(Type tdoc)
            {
                if (!typeof(TypedDoc).IsAssignableFrom(tdoc))
                    throw new DataException(StringConsts.CRUD_TYPE_IS_NOT_DERIVED_FROM_TYPED_ROW_ERROR.Args(tdoc.FullName));

                var name = tdoc.AssemblyQualifiedName;

                var schema = s_TypedRegistry[name];

                if (schema!=null) return schema;

                lock(s_TypedRegistry)
                {
                    schema = s_TypedRegistry[name];
                    if (schema!=null) return schema;
                    schema = new Schema(tdoc);
                    return schema;
                }
            }

            private static HashSet<Type> s_TypeLatch = new HashSet<Type>();

            private Schema(Type tdoc)
            {
                lock(s_TypeLatch)
                {
                  if (s_TypeLatch.Contains(tdoc))
                   throw new DataException(StringConsts.CRUD_TYPED_DOC_RECURSIVE_FIELD_DEFINITION_ERROR.Args(tdoc.FullName));

                  s_TypeLatch.Add(tdoc);
                  try
                  {
                      m_Name = tdoc.AssemblyQualifiedName;


                      var tattrs = tdoc.GetCustomAttributes(typeof(TableAttribute), false).Cast<TableAttribute>();
                      m_TableAttrs = new List<TableAttribute>( tattrs );


                      m_FieldDefs = new OrderedRegistry<FieldDef>();
                      var props = GetFieldMembers(tdoc);
                      var order = 0;
                      foreach(var prop in props)
                      {
                          var fattrs = prop.GetCustomAttributes(typeof(FieldAttribute), false)
                                           .Cast<FieldAttribute>()
                                           .ToArray();

                          //20160318 DKh. Interpret [Field(CloneFromType)]
                          for(var i=0; i<fattrs.Length; i++)
                          {
                            var attr = fattrs[i];
                            if (attr.CloneFromDocType==null) continue;

                            if (fattrs.Length>1)
                             throw new DataException(StringConsts.CRUD_TYPED_DOC_SINGLE_CLONED_FIELD_ERROR.Args(tdoc.FullName, prop.Name));

                            var clonedSchema = Schema.GetForTypedDoc(attr.CloneFromDocType);
                            var clonedDef = clonedSchema[prop.Name];
                            if (clonedDef==null)
                             throw new DataException(StringConsts.CRUD_TYPED_DOC_CLONED_FIELD_NOTEXISTS_ERROR.Args(tdoc.FullName, prop.Name));

                            fattrs = clonedDef.Attrs.ToArray();//replace these attrs from the cloned target
                            break;
                          }

                          var fdef = new FieldDef(prop.Name, order, prop.PropertyType, fattrs, prop);
                          m_FieldDefs.Register(fdef);

                          order++;
                      }
                      s_TypedRegistry.Register(this);
                      m_TypedDocType = tdoc;
                  }
                  finally
                  {
                    s_TypeLatch.Remove(tdoc);
                  }
                }//lock
            }

            public Schema(string name, params FieldDef[] fieldDefs) : this(name, false, fieldDefs, null)
            {

            }

            public Schema(string name, bool readOnly, params FieldDef[] fieldDefs) : this(name, readOnly, fieldDefs, null)
            {

            }

            public Schema(string name, bool readOnly, IEnumerable<TableAttribute> tableAttributes, params FieldDef[] fieldDefs) : this(name, readOnly, fieldDefs, tableAttributes)
            {

            }

            public Schema(string name, bool readOnly, IEnumerable<FieldDef> fieldDefs, IEnumerable<TableAttribute> tableAttributes = null)
            {
                if (name.IsNullOrWhiteSpace())
                    throw new DataException(StringConsts.ARGUMENT_ERROR + "CRUD.Schema.ctor(name==null|empty)");

                if (fieldDefs==null || !fieldDefs.Any())
                    throw new DataException(StringConsts.ARGUMENT_ERROR + "CRUD.Schema.ctor(fieldDefs==null|empty)");

                m_Name = name;
                m_ReadOnly = readOnly;
                if (tableAttributes==null)
                    m_TableAttrs = new List<TableAttribute>();
                else
                    m_TableAttrs = new List<TableAttribute>( tableAttributes );

                m_FieldDefs = new OrderedRegistry<FieldDef>();
                int order = 0;
                foreach(var fd in fieldDefs)
                {
                    fd.m_Order = order;
                    m_FieldDefs.Register( fd );
                    order++;
                }

            }

        #endregion

        #region Fields

            private string m_Name;
            private bool m_ReadOnly;
            private Type m_TypedDocType;

            private List<TableAttribute> m_TableAttrs;
            private OrderedRegistry<FieldDef> m_FieldDefs;

            private JsonDataMap m_ExtraData;
        #endregion

        #region Properties

            /// <summary>
            /// For TypedDocs, returns a unique fully-qualified row type name, which is the global identifier of this schema instance
            /// </summary>
            public string Name
            {
                get { return m_Name; }
            }

            /// <summary>
            /// For typed docs returns a shortened name derived from type, otherwise uses name
            /// </summary>
            public string DisplayName
            {
               get { return m_TypedDocType==null ? Name : (m_TypedDocType.Namespace.TakeLastSegment('.') + m_TypedDocType.Name); }
            }

            /// <summary>
            /// Specifies that target that this schema represents (i.e. db table) is not updatable so DataStore will not be able to save row changes made in ram
            /// </summary>
            public bool ReadOnly
            {
                get { return m_ReadOnly;}
            }

            /// <summary>
            /// Returns a type of TypedDoc if schema was created for TypedRow, or null
            /// </summary>
            public Type TypedDocType
            {
                get { return m_TypedDocType;}
            }

            /// <summary>
            /// Returns table-level attributes
            /// </summary>
            public IEnumerable<TableAttribute> TableAttrs { get { return m_TableAttrs;}}

            /// <summary>
            /// Returns FieldDefs in their order within rows that this schema describes
            /// </summary>
            public IEnumerable<FieldDef> FieldDefs { get { return m_FieldDefs.OrderedValues;}}


            /// <summary>
            /// Returns FieldDefs in their order within rows that are declared as key fields in ANY_TARGET
            /// </summary>
            public IEnumerable<FieldDef> AnyTargetKeyFieldDefs { get { return m_FieldDefs.OrderedValues.Where(fd => fd.AnyTargetKey);}}



            /// <summary>
            /// Returns FieldDefs in their order within rows as
            /// </summary>
            public IEnumerable<FieldDef> AnyVisibleFieldDefs { get { return m_FieldDefs.OrderedValues.Where(fd => fd.AnyVisible);}}


            /// <summary>
            /// Returns a field definition by a unique case-insensitive field name within schema
            /// </summary>
            public FieldDef this[string name]
            {
                get{ return m_FieldDefs[name];}
            }

            /// <summary>
            /// Returns a field definition by a positional index within the row
            /// </summary>
            public FieldDef this[int index]
            {
                get{ return m_FieldDefs[index];}
            }

            /// <summary>
            /// Returns field count
            /// </summary>
            public int FieldCount {get { return m_FieldDefs.Count;}}


            /// <summary>
            /// Returns Extra data that may be associated with schema by various providers.
            /// The field is lazily allocated
            /// </summary>
            public JsonDataMap ExtraData
            {
              get
              {
                if (m_ExtraData==null)
                  m_ExtraData = new JsonDataMap(false);

                return m_ExtraData;
              }
            }

        #endregion


        #region Public

            /// <summary>
            /// Finds fielddef by name or throws if name is not found
            /// </summary>
            public FieldDef GetFieldDefByIndex(int index)
            {
                var result = this[index];
                if (result==null) throw new DataException(StringConsts.CRUD_FIELD_NOT_FOUND_ERROR.Args("["+index+"]", Name));
                return result;
            }

            /// <summary>
            /// Finds fielddef by name or throws if name is not found
            /// </summary>
            public FieldDef GetFieldDefByName(string fieldName)
            {
                var result = this[fieldName];
                if (result==null) throw new DataException(StringConsts.CRUD_FIELD_NOT_FOUND_ERROR.Args(fieldName, Name));
                return result;
            }

            /// <summary>
            /// Returns FieldDef by BackendName for TargetName
            /// </summary>
            public FieldDef GetFieldDefByBackendName(string targetName, string backendName, Func<FieldDef, FieldAttribute, bool> func = null)
            {
              return this.FirstOrDefault(fd =>
              {
                var match = fd.GetBackendNameForTarget(targetName).EqualsOrdIgnoreCase(backendName);
                if (!match) return false;
                var attr = fd[targetName];
                if (attr == null || func == null) return true;
                return func(fd, attr);
              });
            }


            /// <summary>
            /// Returns FieldDefs in their order within rows that are declared as key fields for particular target
            /// </summary>
            public IEnumerable<FieldDef> GetKeyFieldDefsForTarget(string targetName)
            {
                 foreach( var fd in m_FieldDefs)
                 {
                    var fattr = fd[targetName];
                    if (fattr!=null && fattr.Key) yield return fd;
                 }
            }

            //20170420 DKh+Ogee multitargeting for deserilization to ROW from JSON
            /// <summary>
            /// Returns a field def that matches the desired backed name for the specified target or null
            /// </summary>
            /// <param name="targetName">Target or null, if null any target assumed</param>
            /// <param name="backendName">The name of the backend</param>
            /// <param name="backendNameComparison">The string comparison to use against the backend name, OrdinalIgnoreCase is dflt</param>
            /// <returns>The desired field or null</returns>
            public FieldDef TryFindFieldByTargetedBackendName(
                                                      string targetName,
                                                      string backendName,
                                                      StringComparison backendNameComparison = StringComparison.OrdinalIgnoreCase)
            {
                 return m_FieldDefs.FirstOrDefault(  fd => fd.GetBackendNameForTarget(targetName).Equals(backendName, backendNameComparison)  );
            }



            public IEnumerator<Schema.FieldDef> GetEnumerator()
            {
                return FieldDefs.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return FieldDefs.GetEnumerator();
            }


            /// <summary>
            /// Returns a TableAttribute that matches the supplied targetName, or if one was not defined then
            ///  returns TableAttribute which matches any target or null
            /// </summary>
            public TableAttribute GetTableAttrForTarget(string targetName)
            {
                if (targetName.IsNotNullOrWhiteSpace())
                {
                    var atr = m_TableAttrs.FirstOrDefault(a => targetName.EqualsIgnoreCase(a.TargetName));
                    if (atr!=null) return atr;
                }
                return m_TableAttrs.FirstOrDefault(a => TargetedAttribute.ANY_TARGET.EqualsIgnoreCase(a.TargetName));
            }


            public override string ToString()
            {
                return "Schema(Name: '{0}', Count: {1})".Args(Name, m_FieldDefs.Count);
            }


            /// <summary>
            /// Performs logical equivalence testing of two schemas
            /// </summary>
            public bool IsEquivalentTo(Schema other, bool compareNames = true)
            {
                if (other==null) return false;
                if (object.ReferenceEquals(this, other)) return true;

                if (compareNames)
                 if (!Name.EqualsIgnoreCase(other.Name)) return false;

                if (this.m_TableAttrs.Count != other.m_TableAttrs.Count ||
                    this.m_FieldDefs.Count != other.m_FieldDefs.Count) return false;

                var cnt = this.m_TableAttrs.Count;
                for(var i=0; i<cnt; i++)
                  if (!this.m_TableAttrs[i].Equals(other.m_TableAttrs[i])) return false;

                cnt = this.m_FieldDefs.Count;
                for(var i=0; i<cnt; i++)
                  if (!this.m_FieldDefs[i].Equals(other.m_FieldDefs[i])) return false;

                return true;
            }


        #endregion

        #region IJSONWritable

            /// <summary>
            /// Writes schema as JSON. Do not call this method directly, instead call rowset.ToJSON() or use JSONWriter class
            /// </summary>
            public void WriteAsJson(System.IO.TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
            {
                IEnumerable<FieldDef> defs = m_FieldDefs;

                if (options!=null && options.RowMapTargetName.IsNotNullOrWhiteSpace())
                {
                  var newdefs = new List<FieldDef>();
                  foreach(var def in defs)
                  {
                     FieldAttribute attr;
                     var name = def.GetBackendNameForTarget(options.RowMapTargetName, out attr);
                     if (attr!=null)
                     {
                       if(attr.StoreFlag==StoreFlag.None || attr.StoreFlag==StoreFlag.OnlyLoad)
                       {
                        continue;
                       }
                     }
                     newdefs.Add(def);
                  }
                  defs = newdefs;
                }

                var map = new Dictionary<string, object>
                {
                  {"Name", "JSON"+m_Name.GetHashCode()},
                  {"FieldDefs", defs}
                };
                JsonWriter.WriteMap(wri, map, nestingLevel, options);
            }


        #endregion


        #region Comparer


          /// <summary>
          /// Returns an instance of IEqualityComparer(Schema) that performs logical equivalence testing
          /// </summary>
          public static IEqualityComparer<Schema> SchemaEquivalenceEqualityComparer{ get{ return new schemaEquivalenceEqualityComparer();} }


            private struct schemaEquivalenceEqualityComparer : IEqualityComparer<Schema>
            {

              public bool Equals(Schema x, Schema y)
              {
                if (x==null) return false;
                return x.IsEquivalentTo(y);
              }

              public int GetHashCode(Schema obj)
              {
                if (obj==null) return 0;
                return obj.Name.GetHashCodeIgnoreCase() ^ obj.FieldCount;
              }
            }
        #endregion

    }








}
