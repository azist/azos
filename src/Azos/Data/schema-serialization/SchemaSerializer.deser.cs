/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Data
{
  partial class SchemaSerializer
  {
    /// <summary>
    /// Deserializes a schema from <see cref="DeserCtx.RootMap"/> into a <see cref="Schema"/>
    /// </summary>
    public static Schema Deserialize(DeserCtx ctx)
    {
      ctx.IsAssigned.IsTrue("Assigned ctx");

      //Phase I pre-allocate schema shells
      var rootSchema = new Schema();
      ctx.Schemas.Add("#0", rootSchema);

      var typeMap = ctx.RootMap["types"] as JsonDataMap;

      if (typeMap != null)
      {
        foreach(var kvp in typeMap)
        {
          ctx.Schemas.Add(kvp.Key, new Schema());
        }
      }

      //Phase II populate deserialize schema shells

      //Deserialize root Schema
      deserSchema(ctx, rootSchema, ctx.RootMap);

      if (typeMap != null)
      {
        foreach (var kvp in typeMap)
        {
          var schema = ctx.Schemas[kvp.Key];
          deserSchema(ctx, schema, kvp.Value.CastTo<JsonDataMap>($"JsonDataMap type entry `{kvp.Key}`") );
        }
      }

      return rootSchema;
    }

    /// <summary>
    /// Default implementation which matches anything
    /// </summary>
    public static bool DefaultTargetFilter(DeserCtx ctx, bool isField, JsonDataMap data)
    {
      return true;
    }

    private static void deserSchema(DeserCtx ctx, Schema schema, JsonDataMap map)
    {
      var name = map["name"].AsString();
      var readOnly = map["readonly"].AsBool(false);

      var bix = map["bix"].AsGUID(Guid.Empty);

      List<Schema.FieldDef> lstDefs = null;
      List<SchemaAttribute> lstAttrs = null;

      if (map["attrs"] is IEnumerable attrs)
      {
        lstAttrs = deserAttributes(ctx, schema, attrs.OfType<JsonDataMap>());
      }

      if (map["fields"] is IEnumerable fields)
      {
        lstDefs = deserFields(ctx, schema, fields.OfType<JsonDataMap>());
      }

      schema.__ctor(name, readOnly, lstDefs, lstAttrs);
    }

    private static List<SchemaAttribute> deserAttributes(DeserCtx ctx, Schema schema, IEnumerable<JsonDataMap> attrs)
    {
      var result = new List<SchemaAttribute>();
      foreach(var map in attrs)
      {
        if (!ctx.TargetFilter(ctx, false, map)) continue;

        var atr = new SchemaAttribute
        {
          TargetName = map["target"].AsString(),
          Name       = map["name"].AsString(),
          Immutable  = map["immutable"].AsBool(),
          MetadataContent = map["meta"].AsString(),
          Description     = map["description"].AsString()
        };

        result.Add(atr);
      }
      return result;
    }

    private static List<Schema.FieldDef> deserFields(DeserCtx ctx, Schema schema, IEnumerable<JsonDataMap> fields)
    {
      var result = new List<Schema.FieldDef>();
      foreach (var map in fields)
      {
        if (!ctx.TargetFilter(ctx, true, map)) continue;

        var name = map["name"].AsString();
        Type t = null;//<=================== TYPE
        var attrs = deserFieldAttributes(ctx, schema, map["attributes"].CastTo<IEnumerable<JsonDataMap>>("Attributes collection"));
        var def = new Schema.FieldDef(name, t, attrs);
        result.Add(def);
      }
      return result;
    }

    private static (Type t, Schema sch) DefaultTypeMapper(DeserCtx ctx, object tspec)
    {
      return (typeof(object), null);
    }


    private static List<FieldAttribute> deserFieldAttributes(DeserCtx ctx, Schema schema, IEnumerable<JsonDataMap> attributes)
    {
      var result = new List<FieldAttribute>();
      foreach (var map in attributes)
      {
        var attr = new FieldAttribute();
        attr.TargetName = map["target"].AsString();
        attr.Description = map["description"].AsString();
        attr.MetadataContent = map["meta"].AsString();
        if (map.ContainsKey("backName")) attr.BackendName = map["backName"].AsString();
        if (map.ContainsKey("backType")) attr.BackendType = map["backType"].AsString();
        if (map.ContainsKey("charCase")) attr.CharCase = map["charCase"].AsEnum(CharCase.AsIs);
        if (map.ContainsKey("storeFlag")) attr.StoreFlag = map["storeFlag"].AsEnum(StoreFlag.LoadAndStore);
        if (map.ContainsKey("key")) attr.Key = map["key"].AsBool();
        if (map.ContainsKey("required")) attr.Required = map["required"].AsBool();
        if (map.ContainsKey("kind")) attr.Kind = map["kind"].AsEnum(DataKind.Text);
        if (map.ContainsKey("visible")) attr.Visible = map["visible"].AsBool(true);
        if (map.ContainsKey("min")) attr.Min = map["min"];
        if (map.ContainsKey("max")) attr.Max = map["max"];
        if (map.ContainsKey("default")) attr.Default = map["default"];
        if (map.ContainsKey("minLen")) attr.MinLength = map["minLen"].AsInt();
        if (map.ContainsKey("maxLen")) attr.MaxLength = map["maxLen"].AsInt();
        if (map.ContainsKey("nonUi")) attr.NonUI = map["nonUi"].AsBool();
        if (map.ContainsKey("fmtRegExp")) attr.FormatRegExp = map["fmtRegExp"].AsString();
        if (map.ContainsKey("fmtDescr")) attr.FormatDescription = map["fmtDescr"].AsString();
        if (map.ContainsKey("displayFormat")) attr.DisplayFormat = map["displayFormat"].AsString();
        if (map.ContainsKey("valueListContent")) attr.ValueList = map["valueListContent"].AsString();

        result.Add(attr);
      }
      return result;
    }

  }
}
