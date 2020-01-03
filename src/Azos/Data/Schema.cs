/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
  public sealed partial class Schema : INamed, IEnumerable<Schema.FieldDef>, IJsonWritable
  {
      public const string EXTRA_SUPPORTS_INSERT_ATTR = "supports-insert";
      public const string EXTRA_SUPPORTS_UPDATE_ATTR = "supports-update";
      public const string EXTRA_SUPPORTS_DELETE_ATTR = "supports-delete";

      #region .ctor / static

      /// <summary>
      /// Gets all property members of TypedDoc that are tagged as [Field]
      /// </summary>
      public static IEnumerable<PropertyInfo> GetFieldMembers(Type type)
      {
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
        => GetForTypedDoc(doc.NonNull(nameof(doc)).GetType());

      /// <summary>
      /// Returns schema instance for the TypedRow instance by fetching schema object from cache or
      ///  creating it if it has not been cached yet
      /// </summary>
      public static Schema GetForTypedDoc<TDoc>() where TDoc : TypedDoc
        => GetForTypedDoc(typeof(TDoc));


      /// <summary>
      /// Returns schema instance for the TypedRow instance by fetching schema object from cache or
      ///  creating it if it has not been cached yet
      /// </summary>
      public static Schema GetForTypedDoc(Type tdoc)
      {
          if (!typeof(TypedDoc).IsAssignableFrom(tdoc.NonNull(nameof(tdoc))))
              throw new DataException(StringConsts.CRUD_TYPE_IS_NOT_DERIVED_FROM_TYPED_DOC_ERROR.Args(tdoc.FullName));

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


              var tattrs = tdoc.GetCustomAttributes(typeof(SchemaAttribute), false).Cast<SchemaAttribute>();
              tattrs.ForEach(a => a.StopPropAssignmentTracking());
              m_SchemaAttrs = new List<SchemaAttribute>( tattrs );

              //20191026 DKh. Expand resource references in Descriptions
              m_SchemaAttrs.ForEach(a => { a.ExpandResourceReferencesRelativeTo(tdoc, null); a.Seal(); });


              m_FieldDefs = new OrderedRegistry<FieldDef>();
              var props = GetFieldMembers(tdoc);
              var order = 0;
              foreach(var prop in props)
              {
                var fattrs = prop.GetCustomAttributes(typeof(FieldAttribute), false)
                                  .Cast<FieldAttribute>()
                                  .ToArray();

                fattrs.ForEach(a => a.StopPropAssignmentTracking());

                //Interpret [Field(CloneFromType)]
                for(var i=0; i<fattrs.Length; i++)
                {
                  var attr = fattrs[i];

                  if (attr.CloneFromDocType==null)
                  {
                   //20190831 DKh. Expand resource references in Descriptions
                    attr.ExpandResourceReferencesRelativeTo(tdoc, prop.Name);
                    continue;
                  }

                  if (fattrs.Length>1)
                    throw new DataException(StringConsts.CRUD_TYPED_DOC_SINGLE_CLONED_FIELD_ERROR.Args(tdoc.FullName, prop.Name));

                  var clonedSchema = Schema.GetForTypedDoc(attr.CloneFromDocType);
                  var clonedDef = clonedSchema[prop.Name];
                  if (clonedDef==null)
                    throw new DataException(StringConsts.CRUD_TYPED_DOC_CLONED_FIELD_NOTEXISTS_ERROR.Args(tdoc.FullName, prop.Name));

                  fattrs = clonedDef.Attrs.ToArray();//replace these attrs from the cloned target
                  break;
                }

                FieldAttribute.FixupInheritedTargets($"{tdoc.Name}.{prop.Name}", fattrs);

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

      public Schema(string name, bool readOnly, IEnumerable<SchemaAttribute> tableAttributes, params FieldDef[] fieldDefs) : this(name, readOnly, fieldDefs, tableAttributes)
      {

      }

      public Schema(string name, bool readOnly, IEnumerable<FieldDef> fieldDefs, IEnumerable<SchemaAttribute> schemaAttributes = null)
      {
          if (name.IsNullOrWhiteSpace())
              throw new DataException(StringConsts.ARGUMENT_ERROR + "CRUD.Schema.ctor(name==null|empty)");

          if (fieldDefs==null || !fieldDefs.Any())
              throw new DataException(StringConsts.ARGUMENT_ERROR + "CRUD.Schema.ctor(fieldDefs==null|empty)");

          m_Name = name;
          m_ReadOnly = readOnly;
          if (schemaAttributes==null)
              m_SchemaAttrs = new List<SchemaAttribute>();
          else
              m_SchemaAttrs = new List<SchemaAttribute>( schemaAttributes );


          m_SchemaAttrs.ForEach(a => a.Seal());

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

      private List<SchemaAttribute> m_SchemaAttrs;
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
          get { return m_TypedDocType==null ? Name : (m_TypedDocType.Namespace.TakeLastSegment('.') + "::"+ m_TypedDocType.Name); }
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
      public IEnumerable<SchemaAttribute> SchemaAttrs => m_SchemaAttrs;

      /// <summary>
      /// Returns FieldDefs in their order within rows that this schema describes
      /// </summary>
      public IEnumerable<FieldDef> FieldDefs => m_FieldDefs.OrderedValues;


      /// <summary>
      /// Returns FieldDefs in their order within rows that are declared as key fields in ANY_TARGET
      /// </summary>
      public IEnumerable<FieldDef> AnyTargetKeyFieldDefs => m_FieldDefs.OrderedValues.Where(fd => fd.AnyTargetKey);



      /// <summary>
      /// Returns FieldDefs in their order within rows as
      /// </summary>
      public IEnumerable<FieldDef> AnyVisibleFieldDefs => m_FieldDefs.OrderedValues.Where(fd => fd.AnyVisible);


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
      public FieldDef this[int index] => m_FieldDefs[index];

      /// <summary>
      /// Returns field count
      /// </summary>
      public int FieldCount => m_FieldDefs.Count;


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
      /// Returns a SchemaAttribute that matches the supplied targetName, or if one was not defined then
      ///  returns SchemaAttribute which matches any target or null
      /// </summary>
      public SchemaAttribute GetSchemaAttrForTarget(string targetName)
      {
          if (targetName.IsNotNullOrWhiteSpace())
          {
              var atr = m_SchemaAttrs.FirstOrDefault(a => targetName.EqualsIgnoreCase(a.TargetName));
              if (atr!=null) return atr;
          }
          return m_SchemaAttrs.FirstOrDefault(a => TargetedAttribute.ANY_TARGET.EqualsIgnoreCase(a.TargetName));
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

          if (this.m_SchemaAttrs.Count != other.m_SchemaAttrs.Count ||
              this.m_FieldDefs.Count != other.m_FieldDefs.Count) return false;

          var cnt = this.m_SchemaAttrs.Count;
          for(var i=0; i<cnt; i++)
            if (!this.m_SchemaAttrs[i].Equals(other.m_SchemaAttrs[i])) return false;

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
      void IJsonWritable.WriteAsJson(System.IO.TextWriter wri, int nestingLevel, JsonWritingOptions options)
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
