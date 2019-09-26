using System;
using System.IO;
using Azos.Data.Access;
using Azos.Serialization.JSON;

namespace Azos.Data.Directory
{
  /// <summary>
  /// Identifies a directory entity by a unique id within a type
  /// </summary>
  public struct ItemId : IEquatable<ItemId>, IDistributedStableHashProvider, IJsonReadable, IJsonWritable
  {
    public const int MAX_TYPE_NAME_LEN = 32;

    public static void CheckItemTypeName(string table, string opName)
    {
      if (table.IsNullOrWhiteSpace()) throw new DataException(StringConsts.DIRECTORY_TYPE_IS_NULL_OR_EMPTY_ERROR.Args(opName));

      var len = table.Length;
      if (len > MAX_TYPE_NAME_LEN) throw new DataException(StringConsts.DIRECTORY_TYPE_MAX_LEN_ERROR.Args(opName, len, MAX_TYPE_NAME_LEN));
      for (var i = 0; i < len; i++)
      {
        var c = table[i];
        if ((c >= 'a' && c <= 'z') ||
            (c >= 'A' && c <= 'Z') ||
            (i > 0 && c >= '0' && c <= '9') ||
            c == '_') continue;

        throw new DataException(StringConsts.DIRECTORY_TYPE_CHARACTER_ERROR.Args(opName, table));
      }
    }


    public ItemId(string t, GDID id)
    {
      CheckItemTypeName(t, ".ctor");
      Type = t;
      Id = id;
    }

    /// <summary>
    /// Entity type. Type names are case-insensitive
    /// </summary>
    public readonly string Type;

    /// <summary>
    /// A unique Id of an entity
    /// </summary>
    public readonly GDID Id;

    public override int GetHashCode() => Id.GetHashCode();

    public override bool Equals(object obj)
    => obj is ItemId eid ? this.Equals(eid) : false;

    public bool Equals(ItemId other)
    => this.Type.EqualsIgnoreCase(other.Type) &&
       this.Id==other.Id;

    public ulong GetDistributedStableHash() => Id.GetDistributedStableHash();

    public static bool operator ==(ItemId a, ItemId b) => a.Equals(b);
    public static bool operator !=(ItemId a, ItemId b) => !a.Equals(b);

    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
    {
      wri.Write("{\"t\": ");
      JsonWriter.EncodeString(wri, Type, options);
      wri.Write(",");
      wri.Write("\"id\": ");
      ((IJsonWritable)Id).WriteAsJson(wri, nestingLevel, options);
      wri.Write("}");
    }

    (bool match, IJsonReadable self) IJsonReadable.ReadAsJson(object data, bool fromUI, JsonReader.NameBinding? nameBinding)
    {
      if (data == null) return (true, null);
      if (data is JsonDataMap map)
      {
        var t = map["t"].AsString();
        var id = map["id"].AsGDID();

        return (true, new ItemId(t, id));
      }

      return (false, null);
    }

  }
}
