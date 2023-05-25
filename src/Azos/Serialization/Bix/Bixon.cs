/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Azos.Data;
using Azos.Serialization;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

using TypeCode = Azos.Serialization.Bix.TypeCode;

namespace Azos.Log
{
  /// <summary>
  /// Bixon serialization technology is very similar to JSON (hence the name) and is based
  /// on Bix wire format and efficiently serializes primitives and IJsonDataObject (JsonDataMap and JsonDataArray) types along with
  /// IList and IDictionary (as maps and arrays), and anonymous types (as json maps).
  /// Bixon is designed for archiving/storage of json-like data in a more efficient binary way.
  /// It is similar to Json buts supports additional types for efficiency.
  /// Bixon supports serialization and deserialization of CLR primitive types and Azos-foundational types
  /// such as: JsonDataMap, JsonDataArray, Atom, EntityId, GDID, RGDID, Guid, byte[].
  /// Unsupported/complex types are serialized as json segments.
  /// Bixon does NOT support polymorphism, reference tracking, and complex custom types.
  /// Bixon has a built-in limits on sizes of objects/arrays (see constants). Recursive graphs are disallowed.
  /// You should use <see cref="Bixer"/> serializer for serializing data documents (complex types) if you have
  /// bix satellite assembly with ser/deser cores as Bixer is more efficient than Bixon, however
  /// it requires the presence of assemblies on target machine.
  /// </summary>
  public static class Bixon
  {
    /// <summary>
    /// Structured data bix binary version tag. Serializer writes this as the first field of structured data stream.
    /// Deserializer reads it and can adjust the reading format for backward compatibility
    /// </summary>
    public const byte SUBVERSION = 1;

    public const byte HEADER1 = 0xB1;
    public const byte HEADER2 = 0xD1;

    /// <summary>
    /// Absolute limit on maximum number of map properties per level
    /// </summary>
    public const int MAX_MAP_PROPS = 8 * 1024;

    /// <summary>
    /// Absolute limit on maximum number of array elements
    /// </summary>
    public const int MAX_ARRAY_ELM = 96 * 1024;

    // Null/NonNull map flags, also used as a stream alignment crosscheck code
    private const byte FLAG_FMT_NULL = 0x55;
    private const byte FLAG_FMT_NOTNULL = 0xAA;


    private static readonly Dictionary<Type, Action<BixWriter, object>> WRITERS = new()
    {
      {typeof(string),   (w, v) => { w.Write(TypeCode.String);   w.Write((string)v);  } },
      {typeof(bool),     (w, v) => { w.Write(TypeCode.Bool);     w.Write((bool)v);    } },
      {typeof(byte),     (w, v) => { w.Write(TypeCode.Byte);     w.Write((byte)v);    } },
      {typeof(sbyte),    (w, v) => { w.Write(TypeCode.Sbyte);    w.Write((sbyte)v);   } },
      {typeof(short),    (w, v) => { w.Write(TypeCode.Int16);    w.Write((short)v);   } },
      {typeof(ushort),   (w, v) => { w.Write(TypeCode.Uint16);   w.Write((ushort)v);  } },
      {typeof(int),      (w, v) => { w.Write(TypeCode.Int32);    w.Write((int)v);     } },
      {typeof(uint),     (w, v) => { w.Write(TypeCode.Uint32);   w.Write((uint)v);    } },
      {typeof(long),     (w, v) => { w.Write(TypeCode.Int64);    w.Write((long)v);    } },
      {typeof(ulong),    (w, v) => { w.Write(TypeCode.Uint64);   w.Write((ulong)v);   } },
      {typeof(float),    (w, v) => { w.Write(TypeCode.Float);    w.Write((float)v);   } },
      {typeof(double),   (w, v) => { w.Write(TypeCode.Double);   w.Write((double)v);  } },
      {typeof(decimal),  (w, v) => { w.Write(TypeCode.Decimal);  w.Write((decimal)v); } },
      {typeof(DateTime), (w, v) => { w.Write(TypeCode.DateTime); w.Write((DateTime)v);} },
      {typeof(TimeSpan), (w, v) => { w.Write(TypeCode.TimeSpan); w.Write((TimeSpan)v);} },
      {typeof(Atom),     (w, v) => { w.Write(TypeCode.Atom);     w.Write((Atom)v);    } },
      {typeof(EntityId), (w, v) => { w.Write(TypeCode.EntityId); w.Write((EntityId)v);} },
      {typeof(Guid),     (w, v) => { w.Write(TypeCode.Guid);     w.Write((Guid)v);    } },
      {typeof(GDID),     (w, v) => { w.Write(TypeCode.GDID);     w.Write((GDID)v);    } },
      {typeof(RGDID),    (w, v) => { w.Write(TypeCode.RGDID);    w.Write((RGDID)v);   } },
      {typeof(byte[]),   (w, v) => { w.Write(TypeCode.Buffer);   w.WriteBuffer((byte[])v);} },
    };

    private static readonly Dictionary<TypeCode, Func<BixReader, byte, object>> READERS = new()
    {
      {TypeCode.String,   (r, ver) =>  r.ReadString()   },
      {TypeCode.Bool,     (r, ver) =>  r.ReadBool()     },
      {TypeCode.Byte,     (r, ver) =>  r.ReadByte()     },
      {TypeCode.Sbyte,    (r, ver) =>  r.ReadSbyte()    },
      {TypeCode.Int16,    (r, ver) =>  r.ReadShort()    },
      {TypeCode.Uint16,   (r, ver) =>  r.ReadUshort()   },
      {TypeCode.Int32,    (r, ver) =>  r.ReadInt()      },
      {TypeCode.Uint32,   (r, ver) =>  r.ReadUint()     },
      {TypeCode.Int64,    (r, ver) =>  r.ReadLong()     },
      {TypeCode.Uint64,   (r, ver) =>  r.ReadUlong()    },
      {TypeCode.Float,    (r, ver) =>  r.ReadFloat()    },
      {TypeCode.Double,   (r, ver) =>  r.ReadDouble()   },
      {TypeCode.Decimal,  (r, ver) =>  r.ReadDecimal()  },
      {TypeCode.DateTime, (r, ver) =>  r.ReadDateTime() },
      {TypeCode.TimeSpan, (r, ver) =>  r.ReadTimeSpan() },
      {TypeCode.Atom,     (r, ver) =>  r.ReadAtom()     },
      {TypeCode.EntityId, (r, ver) =>  r.ReadEntityId() },
      {TypeCode.Guid,     (r, ver) =>  r.ReadGuid()     },
      {TypeCode.GDID,     (r, ver) =>  r.ReadGDID()     },
      {TypeCode.RGDID,    (r, ver) =>  r.ReadRGDID()    },
      {TypeCode.Buffer,   (r, ver) =>  r.ReadBuffer()   },
    };

    private static readonly JsonWritingOptions JSON_ENCODE_FORMAT = new(JsonWritingOptions.CompactRowsAsMap)
    {
      MapSkipNulls = false,//Keep nulls
      MapSortKeys = false,//Structured data does not sort on keys
      ISODates = true,
      MaxNestingLevel = 12,
      EnableTypeHints = true,
    };

    private static readonly JsonReadingOptions JSON_DECODE_FORMAT = new(JsonReadingOptions.DefaultLimits)
    {
      EnableTypeHints = true,
      MaxDepth = 12
    };

    /// <summary>
    /// Reads an object (map, array, primitive).
    /// Null value may be returned if it was written that way
    /// </summary>
    public static object ReadObject(BixReader reader, JsonReader.DocReadOptions? docReadOptions = null)
    {
      if ( HEADER1 != reader.ReadByte() ||
           HEADER2 != reader.ReadByte() )
      {
        throw new BixException(StringConsts.BIX_BIXON_CORRUPT_HEADER_ERROR);
      }

      var readVersion = reader.ReadInt();
      if (Format.VERSION < readVersion)
      {
        throw new BixException(StringConsts.BIX_BIXON_UNSUPPORTED_VERSION_ERROR.Args(readVersion, Format.VERSION));
      }

      var ver = reader.ReadByte();//sub version of this
      return readValue(reader, ver, docReadOptions);
    }

    /// <summary>
    /// Writes object: a primitive, anonymous object, JsonDataMap or JsonDataArray
    /// </summary>
    public static void WriteObject(BixWriter writer, object obj, JsonWritingOptions jopt = null)
    {
      writer.Write(HEADER1);
      writer.Write(HEADER2);
      writer.Write(Format.VERSION);
      writer.Write(Bixon.SUBVERSION);
      writeValue(writer, obj, jopt, null);
    }


    private static JsonDataMap readMap(BixReader reader, byte ver, JsonReader.DocReadOptions? docReadOptions)
    {
      var flagNullNotNull = reader.ReadByte();
      if (flagNullNotNull == FLAG_FMT_NULL) return null;
      if (FLAG_FMT_NOTNULL != flagNullNotNull) throw new BixException(StringConsts.BIX_BIXON_CORRUPT_STREAM_FLAG_ERROR);

      var caseSensitive = reader.ReadBool();
      var result = new JsonDataMap(caseSensitive);

      while(true)
      {
        var key = reader.ReadString();
        if (key == null) break;//eof
        if (result.Count == MAX_MAP_PROPS) throw new BixException(StringConsts.BIX_BIXON_LIMIT_EXCEEDED_ERROR + ("max props {0}".Args(MAX_MAP_PROPS)));
        var val = readValue(reader, ver, docReadOptions);
        result[key] = val;
      }

      return result;
    }

    private static JsonDataArray readArray(BixReader reader, byte ver, JsonReader.DocReadOptions? docReadOptions)
    {
      var flagNullNotNull = reader.ReadByte();
      if (flagNullNotNull == FLAG_FMT_NULL) return null;
      if (FLAG_FMT_NOTNULL != flagNullNotNull) throw new BixException(StringConsts.BIX_BIXON_CORRUPT_STREAM_FLAG_ERROR);

      var count = reader.ReadInt();
      if (count > MAX_ARRAY_ELM) throw new BixException(StringConsts.BIX_BIXON_LIMIT_EXCEEDED_ERROR + ("max array elm {0}".Args(MAX_ARRAY_ELM)));
      var result = new JsonDataArray();

      for (var i=0; i < count; i++)
      {
        var val = readValue(reader, ver, docReadOptions);
        result.Add(val);
      }

      return result;
    }

    private static void writeMap(BixWriter writer, IDictionary map, JsonWritingOptions jopt, HashSet<object> set)
    {
      if (map == null)
      {
        writer.Write(FLAG_FMT_NULL);//map is null, byte code is used as a bool and stream consistency flag as well
        return;
      }

      if (map.Count > MAX_MAP_PROPS) throw new BixException(StringConsts.BIX_BIXON_LIMIT_EXCEEDED_ERROR + ("max props {0}".Args(MAX_MAP_PROPS)));

      writer.Write(FLAG_FMT_NOTNULL);//non null, byte code is used as a bool and stream consistency flag as well
      writer.Write(map is JsonDataMap jdm ? jdm.CaseSensitive : true);//case sensitivity

      foreach(var entry in new JsonWriter.dictEnumberable(map))
      {
        writer.Write(entry.Key?.ToString());//string property name
        writeValue(writer, entry.Value, jopt, set);
      }
      writer.Write((string)null);//eof
    }

    private static void writeArray(BixWriter writer, IList array, JsonWritingOptions jopt, HashSet<object> set)
    {
      if (array == null)
      {
        writer.Write(FLAG_FMT_NULL);//array is null, byte code is used as a bool and stream consistency flag as well
        return;
      }

      if (array.Count > MAX_ARRAY_ELM) throw new BixException(StringConsts.BIX_BIXON_LIMIT_EXCEEDED_ERROR + ("max array elms {0}".Args(MAX_ARRAY_ELM)));

      writer.Write(FLAG_FMT_NOTNULL);//non null, byte code is used as a bool and stream consistency flag as well
      writer.Write(array.Count);
      for(var i=0; i< array.Count; i++)
      {
        writeValue(writer, array[i], jopt, set);
      }
    }

    private static void writeValue(BixWriter writer, object value, JsonWritingOptions jopt, HashSet<object> set)
    {
      if (value == null)
      {
        writer.Write(TypeCode.Null);
        return;
      }

      var tvalue = value.GetType();

      //Reinterpret new{a=1} as JsonDataMap
      if (tvalue.IsAnonymousType()) value = anonymousToMap(value);
      //Reinterpret Doc as map
      else if (value is Doc doc)
      {
        var includeType = jopt != null && jopt.Purpose == JsonSerializationPurpose.Marshalling;

        if (includeType)//include type code
        {
          writer.Write(TypeCode.DocWithType);
          var atr = BixAttribute.TryGetGuidTypeAttribute<TypedDoc, BixAttribute>(tvalue);
          writer.Write(atr != null ? atr.TypeGuid : null);
        }
        value = doc.ToJsonDataMap(jopt);
      }//doc


      if (value is IDictionary map)
      {
        writer.Write(TypeCode.Map);

        if (set == null)//the trick is to allocate set only here
        {               //so most cases with top-level map do NOT allocate set as it allocates only on a first field of type map
          set = new HashSet<object>();
        }

        if (!set.Add(map)) throw new BixException(StringConsts.BIX_BIXON_DATA_CIRCULAR_REFERENCE_ERROR);
        try
        {
          writeMap(writer, map, jopt, set);
        }
        finally
        {
          set.Remove(map);
        }
        return;
      }

      //Array/List
      if (value is IList array && value is not byte[])
      {
        writer.Write(TypeCode.Array);

        if (set == null)//the trick is to allocate set only here
        {               //so most cases with top-level map do NOT allocate set as it allocates only on a first field of type map
          set = new HashSet<object>();
        }

        if (!set.Add(array)) throw new BixException(StringConsts.BIX_BIXON_DATA_CIRCULAR_REFERENCE_ERROR);
        try
        {
          writeArray(writer, array, jopt, set);
        }
        finally
        {
          set.Remove(array);
        }
        return;
      }

      var tv = value.GetType();
      if (WRITERS.TryGetValue(tv, out var vw))
      {
        vw(writer, value);
      }
      else
      {
        writer.Write(TypeCode.JsonObject);
        var json = JsonWriter.Write(value, JSON_ENCODE_FORMAT);
        writer.Write(json);
      }
    }

    private static object readValue(BixReader reader, byte ver, JsonReader.DocReadOptions? docReadOptions)
    {
      var tc = reader.ReadTypeCode();
      if (tc == TypeCode.Null) return null;

      if (tc == TypeCode.DocWithType)
      {
        var tguid = reader.ReadNullableGuid();
        if (tguid.HasValue)
        {
          tc = reader.ReadTypeCode();
          if (tc != TypeCode.Map) throw new BixException(StringConsts.BIX_BIXON_CORRUPT_STREAM_DOCMAP_ERROR);

          var map = readMap(reader, ver, docReadOptions);
          if (docReadOptions?.BindBy == JsonReader.DocReadOptions.By.BixonDoNotMaterializeDocuments)
          {
            return map;//do not materialize
          }
          else
          {
            var tdoc = Bixer.GuidTypeResolver.Resolve(tguid.Value);//throws error on not found Bix
            var doc = (Doc)SerializationUtils.MakeNewObjectInstance(tdoc);
            JsonReader.ToDoc(doc, map, false, docReadOptions);
            return doc;
          }
        }
      }

      if (tc == TypeCode.Map) return readMap(reader, ver, docReadOptions);

      if (tc == TypeCode.Array) return readArray(reader, ver, docReadOptions);
      if (READERS.TryGetValue(tc, out var vr))
      {
        return vr(reader, ver);
      }
      else
      {
        var json = reader.ReadString();
        return JsonReader.Deserialize(json, JSON_DECODE_FORMAT);
      }
    }

    private static JsonDataMap anonymousToMap(object v)
    {
      if (v == null) return null;
      var fields = SerializationUtils.GetSerializableFields(v.GetType());

      var data = fields.Select(
      f =>
      {
        var name = f.Name;
        var iop = name.IndexOf('<');
        if (iop >= 0)//handle anonymous type field name
        {
          var icl = name.IndexOf('>');
          if (icl > iop + 1)
            name = name.Substring(iop + 1, icl - iop - 1);
        }

        return new KeyValuePair<string, object>(name, f.GetValue(v));
      });//select


      return new JsonDataMap(true, data);
    }
  }
}
