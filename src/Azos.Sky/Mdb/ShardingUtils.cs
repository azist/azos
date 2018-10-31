using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Data;
using Azos.Data.Access;
using Azos.IO.ErrorHandling;


namespace Azos.Sky.Mdb
{
  /// <summary>
  /// WARNING!!! Any change to this class may lead to change in sharding distribution
  /// and render all existing data partitioning invalid. Use EXTREME care!
  /// </summary>
  public static class ShardingUtils
  {

    /// <summary>
    /// Takes first X chars from a trimmed string.
    /// If a string is null returns null. If a string does not have enough chars the function returns what the string has.
    /// WARNING! Changing this function will render all existing sharding partitioning invalid. Use extreme care!
    /// </summary>
    public static string TakeFirstChars(string str, int count)
    {
      if (str == null) return null;
      str = str.Trim();
      if (str.Length <= count) return str;
      return str.Substring(0, count);
    }


    /// <summary>
    /// Gets sharding ID for object that can be IShardingIDProvider, string, parcel, GDID, long/ulong and int/uint.
    /// WARNING! Changing this function will render all existing sharding partitioning invalid. Use extreme care!
    /// </summary>
    public static ulong ObjectToShardingID(object key)
    {
      //WARNING! Never use GetHashCode here as it is platform-dependent, but this function must be 100% deterministic

      if (key == null) return 0;

      var cnt = 0;
      while (key != null && key is IShardingPointerProvider)
      {
        key = ((IShardingPointerProvider)key).ShardingPointer.ID;
        cnt++;
        if (cnt > 10)
          throw new MdbException(StringConsts.MDB_POSSIBLE_SHARDING_ID_CYCLE_ERROR);
      }

      if (key == null) return 0;

      if (key is IDistributedStableHashProvider) return ((IDistributedStableHashProvider)key).GetDistributedStableHash();//covers GDID
      if (key is string) return StringToShardingID((string)key);
      if (key is byte[]) return ByteArrayToShardingID((byte[])key);
      if (key is Guid)
      {
        var garr = ((Guid)key).ToByteArray();
        var seg1 = garr.ReadBEUInt64();
        var seg2 = garr.ReadBEUInt64(8);

        return seg1 ^ seg2;
      }
      if (key is DateTime) return (ulong)((DateTime)key).ToMillisecondsSinceUnixEpochStart();

      if (key is ulong) return ((ulong)key);
      if (key is long) return ((ulong)(long)key);

      if (key is uint) return ((ulong)((uint)key) * 1566083941ul);
      if (key is int) return ((ulong)((int)key) * 1566083941ul);
      if (key is bool) return ((bool)key) ? 999331ul : 3ul;

      throw new MdbException(StringConsts.MDB_OBJECT_SHARDING_ID_ERROR.Args(key.GetType().FullName));
    }

    /// <summary>
    /// Gets sharding ID for string, that is - computes string hash as UInt64 .
    /// WARNING! Changing this function will render all existing sharding partitioning invalid. Use extreme care!
    /// </summary>
    public static ulong StringToShardingID(string key)
    {
      //WARNING! Never use GetHashCode here as it is platform-dependent, but this function must be 100% deterministic

      /*
From Microsoft on MSDN:

    Best Practices for Using Strings in the .NET Framework

    Recommendations for String Usage

        Use the String.ToUpperInvariant method instead of the String.ToLowerInvariant method when you normalize strings for comparison.

Why? From Microsoft:

    Normalize strings to uppercase

    There is a small group of characters that when converted to lowercase cannot make a round trip.

What is example of such a character that cannot make a round trip?

    Start: Greek Rho Symbol (U+03f1) ϱ
    Uppercase: Capital Greek Rho (U+03a1) Ρ
    Lowercase: Small Greek Rho (U+03c1) ρ

    ϱ , Ρ , ρ

That is why, if your want to do case insensitive comparisons you convert the strings to uppercase, and not lowercase.
       */


      if (key == null) return 0;
#warning DANGER!!!!!!!!! This needs carefull review to not depend on ToUpperInvariant(): todo Dima review!!!
      key = key.ToUpperInvariant();
      var sl = key.Length;
      if (sl == 0) return 0;

      ulong hash1 = 0;
      for (int i = sl - 1; i > sl - 1 - sizeof(ulong) && i >= 0; i--)//take 8 chars from end (string suffix), for most string the
      {                                                 //string tail is the most changing part (i.e. 'Alex Kozloff'/'Alex Richardson'/'System.A'/'System.B'
        if (i < sl - 1) hash1 <<= 8;
        var c = key[i];
        var b1 = (c & 0xff00) >> 8;
        var b2 = c & 0xff;
        hash1 |= (byte)(b1 ^ b2);
      }

      ulong hash2 = 1566083941ul * (ulong)Adler32.ForString(key);

      return hash1 ^ hash2;
    }

    /// <summary>
    /// Gets sharding ID for byte[], that is - computes byte[] hash as UInt64 .
    /// WARNING! Changing this function will render all existing sharding partitioning invalid. Use extreme care!
    /// </summary>
    public static ulong ByteArrayToShardingID(byte[] key)
    {
      if (key == null) return 0;
      var len = key.Length;
      if (len == 0) return 0;

      ulong result = 1566083941ul * (ulong)Adler32.ForBytes(key);
      return result;
    }

    /// <summary>
    /// Returns GDID range partition calculated as the counter bit shift from the original ID.
    /// This function is used to organize transactions into "batches" that otherwise would have required an unnecessary
    /// lookup entity (i.e. transaction partition). With this function the partition may be derived right from the original GDID
    /// </summary>
    /// <param name="id">Original ID</param>
    /// <param name="bitSize">Must be between 4..24, so the partitions are caped at 16M(2^24) entries</param>
    /// <param name="bitSubShard">Must be between 0 and less than bit size</param>
    /// <returns>Partition ID obtained id</returns>
    public static GDID MakeGDIDRangePartition(GDID id, int bitSize = 16, int bitSubShard = 4)
    {
      if (id.IsZero) return GDID.Zero;

      if (bitSize < 4 || bitSize > 24 || bitSubShard < 0 || bitSubShard >= bitSize)
        throw new MdbException(StringConsts.ARGUMENT_ERROR + "bitSize must be [4..24] and bitSubShard must be [0..<bitSize]");

      var era = id.Era;

      ulong lowMask = (1ul << bitSubShard) - 1ul;

      var newCtr = ((id.Counter >> bitSize) << bitSubShard) | (id.Counter & lowMask);

      var result = new GDID(era, 0, newCtr);
      if (result.IsZero) result = new GDID(0, 0, 1);
      return result;
    }
  }



  /// <summary>
  /// Used to return ID data from multiple elements, i.e. multiple parcel fields so sharding framework may obtain
  /// ULONG sharding key. You can not compare or equate instances (only reference comparison of data buffer)
  /// </summary>
  public struct CompositeShardingID : IDistributedStableHashProvider, IEnumerable<object>
  {

    public CompositeShardingID(params object[] data)
    {
      if (data == null || data.Length == 0)
        throw new DistributedDataAccessException(StringConsts.ARGUMENT_ERROR + "CompositeShardingID.ctor(data==null|empty)");

      m_Data = data;

      m_HashCode = 0ul;

      for (var i = 0; i < m_Data.Length; i++)
      {
        var elm = m_Data[i];

        ulong ehc;

        if (elm != null)
          ehc = ShardingUtils.ObjectToShardingID(elm);
        else
          ehc = 0xaa018055ul;

        m_HashCode <<= 1;
        m_HashCode ^= ehc;
      }
    }


    private ulong m_HashCode;
    private object[] m_Data;

    public int Count { get { return m_Data == null ? 0 : m_Data.Length; } }
    public object this[int i] { get { return m_Data == null ? null : (i >= 0 && i < m_Data.Length ? m_Data[i] : null); } }

    public ulong GetDistributedStableHash()
    {
      return m_HashCode;
    }

    public override string ToString()
    {
      if (m_Data == null) return "[]";

      var sb = new StringBuilder("CompositeShardingID[");
      for (var i = 0; i < m_Data.Length; i++)
      {
        if (i > 0) sb.Append(", ");

        var elm = m_Data[i];

        if (elm == null)
          sb.Append("<null>");
        else
          sb.Append(elm.ToString());
      }
      sb.Append(']');

      return sb.ToString();
    }

    public IEnumerator<object> GetEnumerator() { return m_Data != null ? ((IEnumerable<object>)m_Data).GetEnumerator() : Enumerable.Empty<object>().GetEnumerator(); }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return m_Data != null ? m_Data.GetEnumerator() : Enumerable.Empty<object>().GetEnumerator(); }
  }
}