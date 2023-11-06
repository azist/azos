/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azos.Conf;
using Azos.Serialization.JSON;

namespace Azos.Data
{
  /// <summary>
  /// Extracts metadata from <see cref="TargetedAttribute"/>.
  /// You can pattern match on <see cref="SchemaAttribute"/> vs <see cref="FieldAttribute"/>
  /// </summary>
  public delegate JsonDataMap MetadataConverterFunc(TargetedAttribute targetedAttr);


  /// <summary>
  /// Facilitates detailed schema serialization functionality.
  /// This serializer is capable of serializing/deserializing detailed schema definitions with multiple targets
  /// and references to other schemas (e.g. a collection).
  /// The type is used by clients/frameworks (such as AZOS.JS) etc to retrieve a detailed schema in a non-CLR environment
  /// </summary>
  public static class SchemaSerializer
  {
    public static JsonDataMap Serialize(Schema schema,
                                        string nameOverride = null,
                                        Func<TargetedAttribute, bool> fTargetFilter = null,
                                        Func<Type, JsonDataMap, Func<TargetedAttribute, bool>, MetadataConverterFunc, object> fTypeMapper = null,
                                        MetadataConverterFunc fMetaConverter = null)
    {
      if (schema==null) return null;
      if (fTargetFilter == null) fTargetFilter = DefaultTargetFilter;
      if (fTypeMapper == null) fTypeMapper = DefaultTypeMapper;
      if (fMetaConverter==null) fMetaConverter = DefaultMetadataConverter;

      var result = new JsonDataMap();
      var tpMap = new JsonDataMap();

      result["name"] = nameOverride.Default(schema.Name);
      result["readonly"] = schema.ReadOnly;

      result["attrs"] = serializeSchemaAttrs(schema.SchemaAttrs.Where(one => fTargetFilter(one)), fTypeMapper, fMetaConverter);
      result["fields"] = serializeFields(schema.FieldDefs, tpMap, fTargetFilter, fTypeMapper, fMetaConverter);

      if (tpMap.Count > 0) result["types"] = tpMap;

      return result;
    }

    private static IEnumerable<JsonDataMap> serializeSchemaAttrs(IEnumerable<SchemaAttribute> attrs,
                                                                 Func<Type, JsonDataMap, Func<TargetedAttribute, bool>, MetadataConverterFunc, object> fTypeMapper,
                                                                 MetadataConverterFunc fMetaConverter)
    {
      foreach(var attr in attrs)
      {
        var result = new JsonDataMap();
        result["target"] = attr.TargetName;
        result["name"] = attr.Name;
        result["description"] = attr.Description;
        result["immutable"] = attr.Immutable;
        result["meta"] = fMetaConverter(attr);
        yield return result;
      }
    }

    private static IEnumerable<JsonDataMap> serializeFields(IEnumerable<Schema.FieldDef> defs,
                                                            JsonDataMap tpMap,
                                                            Func<TargetedAttribute, bool> fTargetFilter,
                                                            Func<Type, JsonDataMap, Func<TargetedAttribute, bool>, MetadataConverterFunc, object> fTypeMapper,
                                                            MetadataConverterFunc fMetaConverter)
    {
      foreach (var def in defs)
      {
        var result = new JsonDataMap();
        result["name"] = def.Name;
        result["order"] = def.Order;
        result["getOnly"] = def.GetOnly;
        result["type"] = fTypeMapper(def.Type, tpMap, fTargetFilter, fMetaConverter);

        var attrs = new List<JsonDataMap>();
        result["attributes"] = attrs;

        foreach(var attr in def.Attrs.Where(one => fTargetFilter(one)))
        {
          var atrMap = new JsonDataMap();
          atrMap["target"] = attr.TargetName;
          atrMap["description"] = attr.Description;
          atrMap["meta"] = fMetaConverter(attr);
          if (attr.BackendName != null) atrMap["backName"] = attr.BackendName;
          if (attr.BackendType != null) atrMap["backType"] = attr.BackendType;
          if (attr.CharCase != CharCase.AsIs) atrMap["charCase"] = attr.CharCase;
          if (attr.StoreFlag != StoreFlag.LoadAndStore) atrMap["storeFlag"] = attr.StoreFlag;
          if (attr.Key) atrMap["key"] = attr.Key;
          if (attr.Required) atrMap["required"] = attr.Required;
          if (attr.Kind != DataKind.Text) atrMap["kind"] = attr.Kind;
          if (!attr.Visible) atrMap["visible"] = attr.Visible;
          if (attr.Min != null) atrMap["min"] = attr.Min;
          if (attr.Max != null) atrMap["max"] = attr.Max;
          if (attr.Default != null) atrMap["default"] = attr.Default;
          if (attr.MinLength != 0) atrMap["minLen"] = attr.MinLength;
          if (attr.MaxLength != 0) atrMap["maxLen"] = attr.MaxLength;
          if (attr.NonUI) atrMap["nonUi"] = attr.NonUI;
          if (attr.FormatRegExp != null) atrMap["fmtRegExp"] = attr.FormatRegExp;
          if (attr.FormatDescription != null) atrMap["fmtDescr"] = attr.FormatDescription;
          if (attr.DisplayFormat != null) atrMap["displayFormat"] = attr.DisplayFormat;

          if (attr.HasValueList)
          {
            atrMap["valueListContent"] = attr.ValueList;
            atrMap["valueList"] = attr.ParseValueList();
          }

          attrs.Add(atrMap);
        }
        yield return result;
      }//defs
    }


    public static bool DefaultTargetFilter(TargetedAttribute targetedAttr)
    {
      return targetedAttr.TargetName == TargetedAttribute.ANY_TARGET;
    }

    public static object DefaultTypeMapper(Type t, JsonDataMap tpMap, Func<TargetedAttribute, bool> fTargetFilter, MetadataConverterFunc fMetaConverter)
    {
     //build types
     //need to check for array[t] .GetElementType()
     var isArray = t.IsArray;
     var telm = isArray ? t.GetElementType() : t;

     //check for List<T>; Nullable<T>; JsonDataMap
     //else object

      if (typeof(TypedDoc).IsAssignableFrom(telm))
      {
        var schema = Schema.GetForTypedDoc(telm);
        var tkey = Guid.NewGuid().ToString();
        tpMap[tkey] = Serialize(schema, null, fTargetFilter, DefaultTypeMapper, fMetaConverter);
        return tkey;
      }
      return t.Name;
    }

    public static JsonDataMap DefaultMetadataConverter(TargetedAttribute targetedAttr)
    {
      return targetedAttr.Metadata?.ToJSONDataMap();
    }

    public static Schema Deserialize(JsonDataMap map)
    {
      return null;
    }
  }
}
