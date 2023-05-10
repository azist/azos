/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
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
  /// on Bix wire format and efficiently serializes primitives and JsonDataMap and JsonDataArray types along with
  /// object[], and anonymous types (as json maps).
  /// Bixon is designed for archiving/storage of json-like data in a more efficient binary way.
  /// It is similar to Json buts supports additional types for efficiency.
  /// Bixon supports serialization and deserialization of CLR primitive types and Azos-foundational types
  /// such as: JsonDataMap, JsonDataArray, Atom, EntityId, GDID, RGDID, Guid, byte[].
  /// Unsupported/complex types are serialized as json segments.
  /// Bixon does NOT support polymorphism, references, and complex custom types.
  /// Bixon has a built-in limits on sizes of objects/arrays (see constants).
  /// You should use <see cref="Bixer"/> serializer for serializing data documents (complex types).
  /// </summary>
  public static class Bixon
  {
    /// <summary>
    /// Structured data bix binary version tag. Serializer writes this as the first field of structured data stream.
    /// Deserializer reads it and can adjust the reading format for backward compatibility
    /// </summary>
    public const byte VERSION = 1;

    public const byte HEADER1 = 0xB1;
    public const byte HEADER2 = 0xD1;

    /// <summary>
    /// Absolute limit on maximum number of map properties per level
    /// </summary>
    public const int MAX_MAP_PROPS = 512;

    /// <summary>
    /// Absolute limit on maximum number of array elements
    /// </summary>
    public const int MAX_ARRAY_ELM = 4 * 1024;

    // Null/NonNull map flags, also used as a stream alignment crosscheck code
    private const byte SD_FMT_NULL = 0x55;
    private const byte SD_FMT_NOTNULL = 0xAA;

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
    public static object ReadObject(BixReader reader)
    {
      Aver.AreEqual(HEADER1, reader.ReadByte(), "hdr1");
      Aver.AreEqual(HEADER2, reader.ReadByte(), "hdr2");
      var ver = reader.ReadByte();
      return readValue(reader, ver);
    }

    private static JsonDataMap readArchivedDataMap(BixReader reader, byte ver)
    {
      var flagNullNotNull = reader.ReadByte();
      if (flagNullNotNull == SD_FMT_NULL) return null;
      Aver.AreEqual(SD_FMT_NOTNULL, flagNullNotNull, "corrupted data");

      var caseSensitive = reader.ReadBool();
      var result = new JsonDataMap(caseSensitive);

      while(true)
      {
        var key = reader.ReadString();
        if (key == null) break;//eof
        var val = readValue(reader, ver);
        Aver.IsTrue(result.Count < MAX_MAP_PROPS, "max props");
        result[key] = val;
      }

      return result;
    }

    private static JsonDataArray readArchivedDataArray(BixReader reader, byte ver)
    {
      var flagNullNotNull = reader.ReadByte();
      if (flagNullNotNull == SD_FMT_NULL) return null;
      Aver.AreEqual(SD_FMT_NOTNULL, flagNullNotNull, "corrupted data");

      var count = reader.ReadInt();
      Aver.IsTrue(count < MAX_ARRAY_ELM, "max elm");
      var result = new JsonDataArray();

      for (var i=0; i<count; i++)
      {
        var val = readValue(reader, ver);
        result.Add(val);
      }

      return result;
    }

    /// <summary>
    /// Writes object: a primitive, anonymous object, JsonDataMap or JsonDataArray
    /// </summary>
    public static void WriteObject(BixWriter writer, object obj)
    {
      writer.Write(HEADER1);
      writer.Write(HEADER2);
      writer.Write(VERSION);
      writeValue(writer, obj, null);
    }

    private static void writeArchivedDataMap(BixWriter writer, JsonDataMap map, HashSet<IJsonDataObject> set)
    {
      if (map == null)
      {
        writer.Write(SD_FMT_NULL);//map is null, byte code is used as a bool and stream consistency flag as well
        return;
      }

      Aver.IsTrue(map.Count < MAX_MAP_PROPS, "max props");

      writer.Write(SD_FMT_NOTNULL);//non null, byte code is used as a bool and stream consistency flag as well
      writer.Write(map.CaseSensitive);//case sensitivity
      foreach(var kvp in map)
      {
        writer.Write(kvp.Key);//string property name
        writeValue(writer, kvp.Value, set);
      }
      writer.Write((string)null);//eof
    }

    private static void writeArchivedDataArray(BixWriter writer, JsonDataArray array, HashSet<IJsonDataObject> set)
    {
      if (array == null)
      {
        writer.Write(SD_FMT_NULL);//array is null, byte code is used as a bool and stream consistency flag as well
        return;
      }

      Aver.IsTrue(array.Count < MAX_ARRAY_ELM, "max elms");

      writer.Write(SD_FMT_NOTNULL);//non null, byte code is used as a bool and stream consistency flag as well
      writer.Write(array.Count);
      for(var i=0; i< array.Count; i++)
      {
        writeValue(writer, array[i], set);
      }
    }

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

    private static void writeValue(BixWriter writer, object value, HashSet<IJsonDataObject> set)
    {
      if (value == null)
      {
        writer.Write(TypeCode.Null);
        return;
      }

      //Reinterpret new{a=1} as JsonDataMap
      if (value.GetType().IsAnonymousType()) value = anonymousToMap(value);
      if (value is JsonDataMap map)
      {
        writer.Write(TypeCode.Map);

        if (set == null)//the trick is to allocate set only here
        {               //so most cases with top-level map do NOT allocate set as it allocates only on a first field of type map
          set = new HashSet<IJsonDataObject>();
        }

        Aver.IsTrue(set.Add(map), "circular reference check");
        try
        {
          writeArchivedDataMap(writer, map, set);
        }
        finally
        {
          set.Remove(map);
        }
        return;
      }

      //Reinterpret cast object[] -> JsonDataArray(object[])
      if (value is object[] objarray) value = new JsonDataArray(objarray);
      if (value is JsonDataArray array)
      {
        writer.Write(TypeCode.Array);

        if (set == null)//the trick is to allocate set only here
        {               //so most cases with top-level map do NOT allocate set as it allocates only on a first field of type map
          set = new HashSet<IJsonDataObject>();
        }

        Aver.IsTrue(set.Add(array), "circular reference check");
        try
        {
          writeArchivedDataArray(writer, array, set);
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

    private static object readValue(BixReader reader, byte ver)
    {
      var tc = reader.ReadTypeCode();
      if (tc == TypeCode.Null) return null;

      if (tc == TypeCode.Map) return readArchivedDataMap(reader, ver);

      if (tc == TypeCode.Array) return readArchivedDataArray(reader, ver);
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
