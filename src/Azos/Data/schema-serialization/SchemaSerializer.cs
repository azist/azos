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
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Data
{
  /// <summary>
  /// Facilitates detailed schema serialization functionality.
  /// This serializer is capable of serializing/deserializing detailed schema definitions with multiple targets
  /// and references to other schemas (e.g. a collection).
  /// The type is used by clients/frameworks (such as AZOS.JS) etc to retrieve a detailed schema in a non-CLR environment
  /// </summary>
  public static partial class SchemaSerializer
  {
    /// <summary>
    /// Serializes <see cref="Schema"/> object into <see cref="JsonDataMap"/> consumable by <see cref="Deserialize(JsonDataMap)"/>
    /// </summary>
    public static JsonDataMap Serialize(SerCtx ctx, string nameOverride = null)
    {
      ctx.IsAssigned.IsTrue("Assigned ctx");
      var result = serialize(new JsonDataMap(), ctx, ctx.RootSchema, nameOverride);

      if (ctx.HasTypes) result["types"] = ctx.GetAllTypes();
      return result;
    }

    private static JsonDataMap serialize(JsonDataMap map, SerCtx ctx, Schema schema, string nameOverride)
    {
      map["handle"] = $"#{ctx.TypeMap.Count}";
      map["name"] = nameOverride.Default(schema.TypedDocType?.DisplayNameWithExpandedGenericArgs()).Default(schema.Name);
      map["readonly"] = schema.ReadOnly;

      if (schema.TypedDocType != null)
      {
        var bix = BixAttribute.TryGetGuidTypeAttribute<TypedDoc, BixAttribute>(schema.TypedDocType);
        if (bix != null) map["bix"] = bix.TypeGuid.ToString();
      }

      map["attrs"] = serializeSchemaAttrs(ctx, schema.SchemaAttrs.Where(one => ctx.TargetFilter(ctx, one))).ToArray();
      map["fields"] = serializeFields(ctx, schema.FieldDefs).ToArray();
      return map;
    }

    private static IEnumerable<JsonDataMap> serializeSchemaAttrs(SerCtx ctx, IEnumerable<SchemaAttribute> attrs)
    {
      foreach(var attr in attrs)
      {
        var result = new JsonDataMap();
        result["target"] = attr.TargetName;
        result["name"] = attr.Name;
        result["description"] = attr.Description;
        result["immutable"] = attr.Immutable;
        result["meta"] = ctx.MetaConverter(ctx, attr);
        yield return result;
      }
    }

    private static IEnumerable<JsonDataMap> serializeFields(SerCtx ctx, IEnumerable<Schema.FieldDef> defs)
    {
      foreach (var def in defs)
      {
        var result = new JsonDataMap();
        result["name"] = def.Name;
        result["order"] = def.Order;
        result["getOnly"] = def.GetOnly;
        result["type"] = ctx.TypeMapper(ctx, def);

        var attrs = new List<JsonDataMap>();
        result["attributes"] = attrs;

        foreach(var attr in def.Attrs.Where(one => ctx.TargetFilter(ctx, one)))
        {
          var atrMap = new JsonDataMap();
          atrMap["target"] = attr.TargetName;
          atrMap["description"] = attr.Description;
          atrMap["meta"] = ctx.MetaConverter(ctx, attr);
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

    /// <summary>
    /// Default implementation which matches anything
    /// </summary>
    public static bool DefaultTargetFilter(SerCtx ctx, TargetedAttribute targetedAttr)
    {
      //targetedAttr.TargetName == TargetedAttribute.ANY_TARGET;
      return true;
    }

    public static object DefaultTypeMapper(SerCtx ctx, Schema.FieldDef def)
    {
      var t = def.NonNull(nameof(def)).Type;

      //-2 Array?
      var isArray = t.IsArray;
      if (isArray) t = t.GetElementType();

      //-1. List<T>?
      var isList = !isArray && t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>);
      if (isList) t = t.GetGenericArguments()[0];

      //collection specifier: adds optional [] at the end
      var cspec = isArray || isList ? "[]" : "";

      //0. Check Nullable<T>
      var isNullable = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
      if (isNullable) t = t.GetGenericArguments()[0];

      //1. Primitive/Nullable<Primitive>?
      if (PRIMITIVE_TYPES.TryGetValue(t, out var moniker)) return (isNullable ? $"{moniker}?" : moniker) + cspec;// amount?[]

      if (typeof(JsonDataMap).IsAssignableFrom(t))
      {
        return "map" + cspec;
      }

      if (typeof(JsonDataArray).IsAssignableFrom(t))
      {
        return "object[]";
      }

      if (typeof(TypedDoc).IsAssignableFrom(t))
      {
        var schema = Schema.GetForTypedDoc(t);
        return ctx.GetSchemaHandle(schema) + cspec;
      }

      if (typeof(DynamicDoc).IsAssignableFrom(t) && def.ComplexTypeSchema != null)
      {
        var schema = def.ComplexTypeSchema;
        return ctx.GetSchemaHandle(schema) + cspec;
      }

      return "object" + cspec;
    }

    /// <summary> Default implementation discloses everything from metadata property </summary>
    public static string DefaultMetadataConverter(SerCtx ctx, TargetedAttribute targetedAttr)
    {
      return targetedAttr.MetadataContent;
    }
  }
}
