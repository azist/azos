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
                                        Func<Type, JsonDataMap, object> fTypeMapper = null,
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
                                                                 Func<Type, JsonDataMap, object> fTypeMapper,
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
                                                            Func<Type, JsonDataMap, object> fTypeMapper,
                                                            MetadataConverterFunc fMetaConverter)
    {
      foreach (var def in defs)
      {
        var result = new JsonDataMap();
        result["name"] = def.Name;
        result["order"] = def.Order;
        result["getOnly"] = def.GetOnly;
        result["type"] = fTypeMapper(def.Type, tpMap);

        var attrs = new List<JsonDataMap>();
        result["attributes"] = attrs;

        foreach(var attr in def.Attrs.Where(one => fTargetFilter(one)))
        {
          var atrMap = new JsonDataMap();
          atrMap["target"] = attr.TargetName;
          atrMap["description"] = attr.Description;
          atrMap["meta"] = fMetaConverter(attr);
          atrMap["backName"] = attr.BackendName;
          atrMap["backType"] = attr.BackendType;
          atrMap["charCase"] = attr.CharCase;
          atrMap["storeFlag"] = attr.StoreFlag;
          atrMap["key"] = attr.Key;
          atrMap["required"] = attr.Required;
          atrMap["kind"] = attr.Kind;
          atrMap["visible"] = attr.Visible;
          atrMap["min"] = attr.Min;
          atrMap["max"] = attr.Max;
          atrMap["default"] = attr.Default;

          if (attr.HasValueList)
          {
            atrMap["valueListContent"] = attr.ValueList;
            atrMap["valueList"] = attr.ParseValueList();
          }


          attrs.Add(atrMap);
        }

        yield return result;
      }
    }


    public static bool DefaultTargetFilter(TargetedAttribute targetedAttr)
    {
      return targetedAttr.TargetName == TargetedAttribute.ANY_TARGET;
    }

    public static object DefaultTypeMapper(Type t, JsonDataMap tpMap)
    {
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
