/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data.Idgen;
using Azos.IO.ErrorHandling;

namespace Azos.Data
{
  /// <summary>
  /// Provides an efficient representation of a shard key - a variant value used for shard routing.
  /// This type establishes a formal domain for types of values that can be used for sharding.
  /// Stores value types in an efficient way using bit union.
  /// The following data types are supported:
  ///  Gdid, Atom, Ulong, Uint, DateTime(utc), Guid, String, ByteArray, and custom entities implementing IDistributedStableHashProvider
  /// </summary>
  public struct ShardKey : IDistributedStableHashProvider, IEquatable<ShardKey>
  {
    public enum Type : byte
    {
      Uninitialized = 0,
      Gdid,
      Atom,
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


    public ulong GetDistributedStableHash()
    {
      (DataType > Type.Uninitialized).IsTrue("Uninitialized {0}".Args(nameof(ShardKey)));

      switch(DataType)
      {
        case Type.Gdid:  return Buffer.GetDistributedStableHash();
        case Type.Atom:  return new Atom(Buffer.ID).GetDistributedStableHash();
        case Type.Ulong: return Buffer.ID;
        case Type.Uint:  return Buffer.ID;
        case Type.DateTime: return Buffer.ID;
        case Type.IDistributedStableHashProvider:
        {
          if (ObjectValue == null) return 0ul;
          return ((IDistributedStableHashProvider)ObjectValue).GetDistributedStableHash();
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
      => (int)this.DataType ^ this.Buffer.GetHashCode() ^ (ObjectValue==null ? 0 : ObjectValue.GetHashCode());

    public bool Equals(ShardKey other)
      => this.DataType == other.DataType &&
         this.Buffer == other.Buffer &&
         object.ReferenceEquals(this.ObjectValue, other.ObjectValue);

    public override bool Equals(object obj) => obj is ShardKey sk ? this.Equals(sk) : false;


    public static bool operator ==(ShardKey a, ShardKey b) => a.Equals(b);
    public static bool operator !=(ShardKey a, ShardKey b) => !a.Equals(b);

    /// <summary>
    /// Gets sharding key for string, that is - computes string hash as UInt64.
    /// The function IS case-sensitive.
    /// WARNING! Changing this function will render all existing sharding partitioning invalid. Use extreme care!
    /// </summary>
    public static ulong ForString(string key)
    {
      //WARNING! Never use GetHashCode here as it is platform-dependent, but this function must be 100% deterministic
      if (key == null) return 0;
      var len = key.Length;
      if (len == 0) return 0;

      var hash = ((ulong)(len & 0x0f) << 60)  ^  (1566083941ul * Adler32.ForString(key));
      return hash;
    }

    /// <summary>
    /// Gets sharding key for byte[], that is - computes byte[] hash as UInt64 .
    /// WARNING! Changing this function will render all existing sharding partitioning invalid. Use extreme care!
    /// </summary>
    public static ulong ForBytes(byte[] key)
    {
      if (key == null) return 0;
      var len = key.Length;
      if (len == 0) return 0;

      var hash = ((ulong)(len & 0x0f) << 60)  ^  (1566083941ul * Adler32.ForBytes(key));
      return hash;
    }

  }
}
