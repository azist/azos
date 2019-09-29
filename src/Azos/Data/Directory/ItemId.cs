using System;
using System.IO;
using System.Runtime.CompilerServices;
using Azos.Data.Access;
using Azos.Serialization.JSON;

namespace Azos.Data.Directory
{
  /// <summary>
  /// Identifies a directory item using unique Id within a named collection
  /// </summary>
  public struct ItemId : IEquatable<ItemId>, IDistributedStableHashProvider, IJsonReadable, IJsonWritable
  {
    public const int MAX_TYPE_NAME_LEN = 32;

    /// <summary>
    /// Ensures that a collection name is a non-blank string of at most 32 ASCII Latin/or digit 'A..Z'/'0..9' characters only. Underscore is allowed
    /// </summary>
    public static void CheckCollectionName(string collection, [CallerMemberName]string opName = null)
    {
      if (collection.IsNullOrWhiteSpace()) throw new DataException(StringConsts.DIRECTORY_COLLECTION_IS_NULL_OR_EMPTY_ERROR.Args(opName));

      var len = collection.Length;
      if (len > MAX_TYPE_NAME_LEN) throw new DataException(StringConsts.DIRECTORY_COLLECTION_MAX_LEN_ERROR.Args(opName, len, MAX_TYPE_NAME_LEN));
      for (var i = 0; i < len; i++)
      {
        var c = collection[i];
        if ((c >= 'a' && c <= 'z') ||
            (c >= 'A' && c <= 'Z') ||
            (i > 0 && c >= '0' && c <= '9') ||
            c == '_') continue;

        throw new DataException(StringConsts.DIRECTORY_COLLECTION_CHARACTER_ERROR.Args(opName, collection));
      }
    }

    /// <summary>
    /// Creates new ItemID. You must generate a new unique GDID
    /// </summary>
    public ItemId(string collection, GDID id)
    {
      CheckCollectionName(collection);
      if (id.IsZero) throw new DataException(StringConsts.ARGUMENT_ERROR + "ItemId.ctor(id.isZero)");
      Collection = collection.ToLowerInvariant();
      Id = id;
    }

    /// <summary>
    /// Item collection name. Type names are case-insensitive lower case
    /// </summary>
    public readonly string Collection;

    /// <summary>
    /// A unique Id of an entity
    /// </summary>
    public readonly GDID Id;

    /// <summary>
    /// Returns true to signify that structure has been initialized/assigned
    /// </summary>
    public bool IsAssigned => !Id.IsZero && Collection.IsNotNullOrWhiteSpace();

    public override string ToString() => $"Item({Id}@`{Collection}`)";

    public override int GetHashCode() => Id.GetHashCode();

    public override bool Equals(object obj)
    => obj is ItemId eid ? this.Equals(eid) : false;

    public bool Equals(ItemId other)
    => this.Collection == other.Collection &&
       this.Id == other.Id;

    public ulong GetDistributedStableHash() => Id.GetDistributedStableHash();

    public static bool operator ==(ItemId a, ItemId b) => a.Equals(b);
    public static bool operator !=(ItemId a, ItemId b) => !a.Equals(b);

    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
    {
      wri.Write("{\"c\": ");
      JsonWriter.EncodeString(wri, Collection, options);
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
        var t = map["c"].AsString();
        var id = map["id"].AsGDID();

        return (true, new ItemId(t, id));
      }

      return (false, null);
    }

  }
}
