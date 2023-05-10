/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Data;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

using TypeCode = Azos.Serialization.Bix.TypeCode;

namespace Azos.Log
{
  /// <summary>
  /// Provides helper methods for encoding and decoding ArchiveDimensions string in a terse JSON format -
  /// a form of a convention of ArchiveDimensions use with IArchiveLoggable
  /// </summary>
  public static class ArchiveConventions
  {
    /// <summary>
    /// Structured data bix binary version tag. Serializer writes this as the first field of structured data stream.
    /// Deserializer reads it and can adjust the reading format for backward compatibility
    /// </summary>
    public const byte SD_BIX_VERSION = 1;

    /// <summary>
    /// Absolute limit on maximum number of map properties per level
    /// </summary>
    public const int SD_MAX_MAP_PROPS = 512;

    /// <summary>
    /// Absolute limit on maximum number of array elements
    /// </summary>
    public const int SD_MAX_ARRAY_ELM = 4 * 1024;

    // Null/NonNull map flags, also used as a stream alignment crosscheck code
    private const byte SD_FMT_NULL = 0x55;
    private const byte SD_FMT_NOTNULL = 0xAA;

    private static readonly JsonWritingOptions AD_JSON_ENCODE_FORMAT = new (JsonWritingOptions.CompactRowsAsMap)
    {
      MapSkipNulls = true,
      MapSortKeys = true,
      ISODates = true,
      MaxNestingLevel = 4,//20230508 DKh
      EnableTypeHints = true,//#864 20230508 DKh
    };

    private static readonly JsonReadingOptions AD_JSON_DECODE_FORMAT = new (JsonReadingOptions.DefaultLimits)
    {
      EnableTypeHints = true,//#864 20230508 DKh
      MaxDepth = 4
    };

    private static readonly JsonWritingOptions SD_JSON_ENCODE_FORMAT = new(JsonWritingOptions.CompactRowsAsMap)
    {
      MapSkipNulls = false,//Keep nulls
      MapSortKeys = false,//Structured data does not sort on keys
      ISODates = true,
      MaxNestingLevel = 8,
      EnableTypeHints = true,//#864 20230508 DKh
    };

    private static readonly JsonReadingOptions SD_JSON_DECODE_FORMAT = new(JsonReadingOptions.DefaultLimits)
    {
      EnableTypeHints = true,//#864 20230508 DKh
      MaxDepth = 8
    };

    /// <summary> Maximum size of analytical fact archive dimensions </summary>
    public const int ANALYTICS_MAX_DIMENSIONS_CHAR_LEN = 512;

    /// <summary> Maximum size of analytical fact structured data/metrics </summary>
    public const int ANALYTICS_MAX_METRICS_CHAR_LEN = 2 * 1024;


    /// <summary>
    /// Used as a convention identifier.
    /// By this convention, all archive dimensions data must start with a hashbang: #!ad\n,
    /// otherwise the decode function will not parse it and return null
    /// </summary>
    public const string AD_HASHBANG = "#!ad\n";

    /// <summary>
    /// Used as a convention identifier.
    /// By this convention, all archived structured data must start with a hashbang: #!sd\n,
    /// otherwise the decode function will not parse it and return null
    /// </summary>
    public const string SD_HASHBANG = "#!sd\n";

    /// <summary>
    /// Returns a JsonDataMap parsed out of IArchiveLoggable.ArchiveDimensions content when it is set,
    /// or null. The function returns null for content which does not start with AD_HASHBANG
    /// which represents this convention. Throws on invalid JSON
    /// </summary>
    public static JsonDataMap DecodeArchiveDimensionsMap(this IArchiveLoggable entity)
    {
      if (entity==null) return null;
      var ad = entity.ArchiveDimensions;
      return DecodeArchiveDimensionsMap(ad);
    }

    /// <summary>
    /// Returns a JsonDataMap parsed out of ArchiveDimensions content when it is set,
    /// or null. The function returns null for content which does not start with AD_HASHBANG
    /// which represents this convention. Throws on invalid JSON
    /// </summary>
    public static JsonDataMap DecodeArchiveDimensionsMap(string archiveDimensions)
    {
      if (archiveDimensions.IsNullOrWhiteSpace()) return null;
      if (!archiveDimensions.StartsWith(AD_HASHBANG, StringComparison.Ordinal)) return null;
      var result = archiveDimensions.JsonToDataObject(AD_JSON_DECODE_FORMAT) as JsonDataMap;
      return result;
    }

    /// <summary>
    /// Returns a terse json object encoded into a string, or null if the supplied argument is null.
    /// The encoded string is prefixed with AD_HASHBANG to mark the content as the one
    /// encoded with this convention
    /// </summary>
    public static string EncodeArchiveDimensions(object archiveDimensions)
    {
      if (archiveDimensions == null) return null;
      return AD_HASHBANG + archiveDimensions.ToJson(AD_JSON_ENCODE_FORMAT);
    }


    /// <summary>
    /// Returns a JsonDataMap parsed out of structured data content when it is set,
    /// or null. The function returns null for content which does not start with SD_HASHBANG
    /// which represents this convention. Throws on invalid JSON
    /// </summary>
    public static JsonDataMap DecodeStructuredDataMap(string data)
    {
      if (data.IsNullOrWhiteSpace()) return null;
      if (!data.StartsWith(SD_HASHBANG, StringComparison.Ordinal)) return null;
      var result = data.JsonToDataObject(SD_JSON_DECODE_FORMAT) as JsonDataMap;
      return result;
    }

    /// <summary>
    /// Returns a terse json object encoded into a string, or null if the supplied argument is null.
    /// The encoded string is prefixed with SD_HASHBANG to mark the content as the one
    /// encoded with this convention
    /// </summary>
    public static string EncodeStructuredData(object data)
    {
      if (data == null) return null;
      return SD_HASHBANG + data.ToJson(SD_JSON_ENCODE_FORMAT);
    }

    /// <summary>
    /// Converts analytics fact data into <see cref="Message"/> suitable for processing in log archives/pipes, such as chronicle.
    /// Returns a message, you must to call `msg.InitDefaultFields(m_App);` if you need default values
    /// </summary>
    public static Message AnalyticsFactDataToLogMessage(Atom factType,
                                                        object dims,
                                                        object metrics,
                                                        int source = 0,
                                                        Guid rel = default(Guid),
                                                        string host = null,
                                                        MessageType messageType = MessageType.Info,
                                                        DateTime utcTimeStamp = default,
                                                        string topic = null,
                                                        Atom channel = default)
    {
      factType.HasRequiredValue(nameof(factType));

      var msg = new Message
      {
        Channel = channel.Default(CoreConsts.LOG_CHANNEL_ANALYTICS),
        Host = host,
        Topic = topic.Default(CoreConsts.LOG_TOPIC),
        Type = messageType,
        Source = source,
        From = factType.Value,
        RelatedTo = rel,
        UTCTimeStamp = utcTimeStamp
      };

      if (dims != null)
      {
        msg.ArchiveDimensions = ArchiveConventions.EncodeArchiveDimensions(dims)
                                                  .NonBlankMax(ANALYTICS_MAX_DIMENSIONS_CHAR_LEN, "dims.len < 512");
      }

      if (metrics != null)
      {
        msg.Parameters =  ArchiveConventions.EncodeStructuredData(metrics)
                                            .NonBlankMax(ANALYTICS_MAX_METRICS_CHAR_LEN, "metrics.len < 2k");
      }

      return msg;
    }

    /// <summary>
    /// Extracts Fact representation from log message
    /// </summary>
    public static Fact LogMessageToFact(Message msg, string targetName = null, Func<Message, string, Fact> factory = null)
    {
      if (msg == null) return null;
      var fact = factory != null ? factory(msg, targetName) : new Fact();


      if (Atom.TryEncodeValueOrId(msg.From, out var aFrom))
      {
        fact.FactType = aFrom;
      }
      fact.Id = msg.Guid;
      fact.Gdid = msg.Gdid;
      fact.RelatedId = msg.RelatedTo;
      fact.Channel = msg.Channel;
      fact.Topic = msg.Topic;
      fact.Host = msg.Host;
      fact.App = msg.App;
      fact.RecordType = msg.Type;
      fact.Source = msg.Source;
      fact.UtcTimestamp = msg.UTCTimeStamp;

      if (msg.ArchiveDimensions.IsNotNullOrWhiteSpace())
      {
        fact.Dimensions = ArchiveConventions.DecodeArchiveDimensionsMap(msg.ArchiveDimensions);
      }

      if (msg.Parameters.IsNotNullOrWhiteSpace())
      {
        fact.Metrics = ArchiveConventions.DecodeStructuredDataMap(msg.Parameters);
      }

      ((IAmorphousData)fact).AfterLoad(targetName);

      return fact;
    }

    /// <summary>
    /// Reads a map written in a canonical format which is based on Bix wire primitives.
    /// Null value may be returned if it was written that way
    /// </summary>
    public static JsonDataMap ReadArchivedDataMap(BixReader reader)
    {
      var ver = reader.ReadByte();
      return readArchivedDataMap(reader, ver);
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
        Aver.IsTrue(result.Count < SD_MAX_MAP_PROPS, "max props");
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
      Aver.IsTrue(count < SD_MAX_ARRAY_ELM, "max elm");
      var result = new JsonDataArray();

      for (var i=0; i<count; i++)
      {
        var val = readValue(reader, ver);
        result.Add(val);
      }

      return result;
    }

    /// <summary>
    /// Writes map into a stream using a canonical archive format which is based on Bix wire primitives.
    /// Null is permitted. The sub-objects should be wither JsonDataMap or JsonDataArray
    /// </summary>
    public static void WriteArchivedDataMap(BixWriter writer, JsonDataMap map)
    {
      writer.Write(SD_BIX_VERSION);
      writeArchivedDataMap(writer, map, null);
    }

    private static void writeArchivedDataMap(BixWriter writer, JsonDataMap map, HashSet<IJsonDataObject> set)
    {
      if (map == null)
      {
        writer.Write(SD_FMT_NULL);//map is null, byte code is used as a bool and stream consistency flag as well
        return;
      }

      Aver.IsTrue(map.Count < SD_MAX_MAP_PROPS, "max props");

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

      Aver.IsTrue(array.Count < SD_MAX_ARRAY_ELM, "max elms");

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
        var json = JsonWriter.Write(value, SD_JSON_ENCODE_FORMAT);
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
        return JsonReader.Deserialize(json, SD_JSON_DECODE_FORMAT);
      }
    }

  }
}
