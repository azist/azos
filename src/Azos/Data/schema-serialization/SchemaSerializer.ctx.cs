/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Serialization.JSON;

namespace Azos.Data
{
  partial class SchemaSerializer
  {
    /// <summary> Serialization operation context passed between serialization calls </summary>
    public readonly struct SerCtx
    {
      public SerCtx(Schema rootSchema,
                    Func<SerCtx, TargetedAttribute, bool> targetFilter = null,
                    Func<SerCtx, Type, object> typeMapper = null,
                    Func<SerCtx, TargetedAttribute, string> metaConverter = null)
      {
        RootSchema = rootSchema.NonNull(nameof(rootSchema));
        TypeMap = new Dictionary<Schema, JsonDataMap>();
        TargetFilter  = targetFilter ?? DefaultTargetFilter;
        TypeMapper    = typeMapper ?? DefaultTypeMapper;
        MetaConverter = metaConverter ?? DefaultMetadataConverter;
      }

      public readonly Schema RootSchema;
      public readonly Dictionary<Schema, JsonDataMap> TypeMap;
      public readonly Func<SerCtx, TargetedAttribute, bool> TargetFilter;
      public readonly Func<SerCtx, Type, object> TypeMapper;
      public readonly Func<SerCtx, TargetedAttribute, string> MetaConverter;


      public bool IsAssigned => RootSchema != null;
      public bool HasTypes => TypeMap.Count > 0;

      public JsonDataMap GetAllTypes()
      {
        var result = new JsonDataMap();

        foreach (var kvp in TypeMap)
        {
          result[kvp.Value["handle"].AsString()] = kvp.Value;
        }
        return result;
      }

      public string GetSchemaHandle(Schema schema)
      {
        if (schema == RootSchema) return "#0";

        if (!TypeMap.TryGetValue(schema, out var map))
        {
          map = new JsonDataMap();
          TypeMap.Add(schema, map);
          serialize(map, this, schema, null);
        }
        return map["handle"].AsString();
      }
    }//SerCtx


    /// <summary> Deserialization operation context passed between serialization calls </summary>
    public readonly struct DeserCtx
    {
      public DeserCtx(JsonDataMap rootMap,
                    Func<DeserCtx, TargetedAttribute, bool> targetFilter = null,
                    Func<DeserCtx, Type, object> typeMapper = null,
                    Func<DeserCtx, TargetedAttribute, JsonDataMap> metaConverter = null)
      {
        RootMap = rootMap.NonNull(nameof(rootMap));
        Schemas = new Dictionary<string, Schema>();
        TargetFilter = targetFilter ?? DefaultTargetFilter;
        TypeMapper = typeMapper ?? DefaultTypeMapper;
        MetaConverter = metaConverter ?? DefaultMetadataConverter;
      }

      public readonly JsonDataMap RootMap;
      public readonly Dictionary<string, Schema> Schemas;
      public readonly Func<DeserCtx, TargetedAttribute, bool> TargetFilter;
      public readonly Func<DeserCtx, Type, object> TypeMapper;
      public readonly Func<DeserCtx, TargetedAttribute, JsonDataMap> MetaConverter;


      public bool IsAssigned => RootMap != null;
    }//DeserCtx

  }
}
