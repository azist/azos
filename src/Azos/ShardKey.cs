/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.CompilerServices;

using Azos.Data;
using Azos.Data.Idgen;

namespace Azos
{
  /// <summary>
  /// Provides an efficient representation of a shard key - a variant value used for shard routing.
  /// This type establishes a formal domain for types of values that can be used for sharding.
  /// Stores value types in an efficient way using bit union.
  /// The following data types are supported:
  ///  Gdid, Atom, EntityId, Ulong, Uint, DateTime(UTC), Guid, String, ByteArray, and custom entities implementing IDistributedStableHashProvider.
  ///  Warning: The returned hash is NOT intended to be used by cryptography, as the computed result
  ///  is not guaranteed to produce cryptographically-safe hash value
  /// </summary>
  /// <remarks>
  /// WARNING! Changing anything in this type may render all existing sharding partitioning invalid. Use extreme care!
  /// </remarks>
  public struct ShardKey : IDistributedStableHashProvider, IEquatable<ShardKey>
  {
    public enum Type : byte
    {
      Uninitialized = 0,
      Gdid,
      Atom,
      EntityId,
      Ulong,
      Uint,
      DateTime,
      IDistributedStableHashProvider,//reference types
      Guid,
      String,
      ByteArray
    }

    public ShardKey(GDID key)
    { DataType = Type.Gdid;  Buffer = key;  ObjectValue = null; }

    public ShardKey(Atom key)
    { DataType = Type.Atom; Buffer = new GDID(0, key.ID); ObjectValue = null; }

    public ShardKey(EntityId key)
    { DataType = Type.EntityId; Buffer = new GDID(); ObjectValue = key.AsString; }

    public ShardKey(ulong key)
    { DataType = Type.Ulong; Buffer = new GDID(0, key); ObjectValue = null; }

    public ShardKey(uint  key)
    { DataType = Type.Uint; Buffer = new GDID(0, key); ObjectValue = null; }

    public ShardKey(DateTime key)
    { DataType = Type.DateTime; Buffer = new GDID(0, (ulong)key.IsTrue( v => v.Kind == DateTimeKind.Utc).ToMillisecondsSinceUnixEpochStart()); ObjectValue = null; }

    public ShardKey(object key)
    { DataType = Type.IDistributedStableHashProvider; Buffer = new GDID(); ObjectValue = (key==null) ? null : key.CastTo<IDistributedStableHashProvider>(); }

    public ShardKey(Guid guid)
    { DataType = Type.Guid; Buffer = new GDID(); ObjectValue = guid.ToNetworkByteOrder(); }

    public ShardKey(string key)
    { DataType = Type.String; Buffer = new GDID(); ObjectValue = key; }

    public ShardKey(byte[] key)
    { DataType = Type.ByteArray; Buffer = new GDID(); ObjectValue = key; }


    public readonly Type DataType;
    public readonly GDID Buffer;
    public readonly object ObjectValue;

    /// <summary>
    /// Alias to .GetDistributedStableHash()
    /// </summary>
    public ulong Hash => GetDistributedStableHash();

    /// <summary>
    /// Returns a stable hash value derived from the actual value encoded in this instance.
    /// The system adds "avalanche" effect for better bit distribution
    /// </summary>
    public ulong GetDistributedStableHash()
    {
      switch(DataType)
      {
        case Type.Uninitialized: return 0;
        case Type.Gdid:  return ForUlong(Buffer.GetDistributedStableHash());
        case Type.Atom:  return ForUlong( new Atom(Buffer.ID).GetDistributedStableHash());
        case Type.EntityId: return EntityId.Parse((string)ObjectValue).GetDistributedStableHash();//EntityId is already distributed
        case Type.Ulong: return ForUlong(Buffer.ID);
        case Type.Uint:  return ForUlong(Buffer.ID);
        case Type.DateTime: return ForUlong(Buffer.ID);
        case Type.IDistributedStableHashProvider:
        {
          if (ObjectValue == null) return 0ul;
          return ForUlong(((IDistributedStableHashProvider)ObjectValue).GetDistributedStableHash());
        }
        case Type.Guid:
        {
          if (ObjectValue == null) return 0ul;
          return ForBytes((byte[])ObjectValue);
        }
        case Type.String:
        {
          if (ObjectValue == null) return 0ul;
          return ForString((string)ObjectValue);
        }
        case Type.ByteArray:
        {
          if (ObjectValue == null) return 0ul;
          return ForBytes((byte[])ObjectValue);
        }
      }

      throw new DataException(StringConsts.SHARDING_OBJECT_ID_ERROR.Args(DataType));
    }

    public override string ToString()
      => $"ShardKey({DataType}, `{(ObjectValue != null ? ObjectValue.ToString() : DataType == Type.Gdid ? Buffer.ToString() : "{0:X8}".Args(Buffer.ID) )}`)";

    public override int GetHashCode()
      => (int)this.DataType ^ this.Buffer.GetHashCode() ^ (ObjectValue==null ? 0 : RuntimeHelpers.GetHashCode(ObjectValue));

    public bool Equals(ShardKey other)
      => this.DataType == other.DataType &&
         this.Buffer == other.Buffer &&
         object.ReferenceEquals(this.ObjectValue, other.ObjectValue);

    public override bool Equals(object obj) => obj is ShardKey sk ? this.Equals(sk) : false;


    public static bool operator ==(ShardKey a, ShardKey b) => a.Equals(b);
    public static bool operator !=(ShardKey a, ShardKey b) => !a.Equals(b);

    //Fowler–Noll–Vo hash function
    //https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
    //http://www.isthe.com/chongo/tech/comp/fnv/index.html#public_domain
    //https://softwareengineering.stackexchange.com/questions/49550/which-hashing-algorithm-is-best-for-uniqueness-and-speed
    private const ulong FNV1A_64_PRIME = 1099511628211; // 2^40 + 2^8 + 0xb3
    private const ulong FNV1A_64_OFFSET = 14695981039346656037;

    /// <summary>
    /// Computes a FNV1A64 hash of string content using UTC16 binary encoding. The function IS case-sensitive.
    /// Zero is returned for null reference for homogeneity with byte[] and ulong
    /// </summary>
    public static ulong ForString(string key)
    {
      //WARNING! Never use GetHashCode here as it is platform-dependent, but this function must be 100% deterministic
      if (key == null) return 0;
      var len = key.Length;
      if (len == 0) return 0;

      unchecked
      {
        ulong hash = FNV1A_64_OFFSET;

        for(var i=0; i<=len-1; i++)
        {
          var c = key[i];
          var b = (c >> 8) ^ (c & 0xff);
          hash = hash ^ (ulong)b;
          hash = hash * FNV1A_64_PRIME;
        }
        return hash;
      }
    }

    /// <summary>
    /// Computes a FNV1A64 hash of byte[] content. Zero is returned for null reference for homogeneity with strings and ulong
    /// </summary>
    public static ulong ForBytes(byte[] key)
    {
      if (key == null) return 0;
      var len = key.Length;
      if (len == 0) return 0;

      unchecked
      {
        ulong hash = FNV1A_64_OFFSET;

        for (var i = 0; i <= len - 1; i++)
        {
          var b = key[i];
          hash = hash ^ b;
          hash = hash * FNV1A_64_PRIME;
        }
        return hash;
      }
    }

    /// <summary>
    /// Computes a FNV1A64 hash of ulong. Zero is returned for 0 for homogeneity with strings and byte[]
    /// </summary>
    public static ulong ForUlong(ulong key)
    {
      if (key == 0) return 0;

      unchecked
      {
        ulong hash = FNV1A_64_OFFSET;


        var b = key & 0xff;
        hash = hash ^ b;
        hash = hash * FNV1A_64_PRIME;

        b = (key >> 8) & 0xff;
        hash = hash ^ b;
        hash = hash * FNV1A_64_PRIME;

        b = (key >> 16) & 0xff;
        hash = hash ^ b;
        hash = hash * FNV1A_64_PRIME;

        b = (key >> 24) & 0xff;
        hash = hash ^ b;
        hash = hash * FNV1A_64_PRIME;

        b = (key >> 32) & 0xff;
        hash = hash ^ b;
        hash = hash * FNV1A_64_PRIME;

        b = (key >> 40) & 0xff;
        hash = hash ^ b;
        hash = hash * FNV1A_64_PRIME;

        b = (key >> 48) & 0xff;
        hash = hash ^ b;
        hash = hash * FNV1A_64_PRIME;

        b = (key >> 56) & 0xff;
        hash = hash ^ b;
        hash = hash * FNV1A_64_PRIME;

        return hash;
      }
    }

  }
}
