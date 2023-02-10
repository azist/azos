/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Azos.Serialization.JSON;

namespace Azos.Data
{
  /// <summary>
  /// Represents a Global Distributed ID key (key field, pronounced like "gee-did") used in distributed databases that identify entities with a
  /// combination of unsigned 32 bit integer 'Era' and unsigned 64 bit integer 'ID'.
  /// The first 32 bit integer is an 'era' in which the 'ID' (64 bit) was created, consequently
  /// a GDID is a 12 byte = 96 bit integer that can hold 2^96 = 79,228,162,514,264,337,593,543,950,336 combinations.
  /// The ID consists of two segments: 4 bit authority + 60 bits counter. Authority segment occupies
  /// the most significant 4 bits of uint64, so the system may efficiently query the data store to identify the highest
  /// stored ID value in a range. Authorities identify one of 16 possible ID generation sources in the global distributed system,
  /// therefore ID duplications are not possible between authorities.
  /// Within a single era, GDID structure may identify
  ///   2^60 = 1,152,921,504,606,846,976(per authority) * 16(authorities) = 2^64 = 18,446,744,073,709,551,616 total combinations.
  /// Because of such a large number of combinations supported by GDID.ID alone (having the same Era), some systems may
  /// always use Era=0 and only store the ID part (i.e. as UNSIGNED BIGINT in SQL data stores).
  /// Note GDID.Zero is never returned by generators as it represents the absence of a value (special value)
  /// </summary>
  [Serializable]
  public struct GDID : Access.IDataStoreKey,
                       Idgen.IDistributedStableHashProvider,
                       IComparable<GDID>,
                       IEquatable<GDID>,
                       IComparable,
                       IJsonWritable,
                       IJsonReadable,
                       IRequiredCheck
  {
    internal static readonly IFormatProvider INVARIANT = CultureInfo.InvariantCulture;
    internal const NumberStyles ISTYLES = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;

    private static readonly string[] SAUTHORITIES = new []{"0", "1" , "2" , "3" , "4" , "5" , "6" , "7" , "8" , "9" , "10" , "11" , "12" , "13" , "14" , "15"};

    public const int BYTE_SIZE = sizeof(UInt32) + sizeof(UInt64);

    public const UInt64 AUTHORITY_MASK = 0xf000000000000000;
    public const UInt64 COUNTER_MASK   = 0x0fffffffffffffff;//0x 0f  ff  ff  ff  ff  ff  ff  ff
                                                            //    1   2   3   4   5   6   7   8
    /// <summary>
    /// Provides maximum value for counter segment
    /// </summary>
    public const UInt64 COUNTER_MAX    = COUNTER_MASK;

    /// <summary>
    /// Provides maximum value for authority segment
    /// </summary>
    public const int AUTHORITY_MAX    = 0x0f;

    /// <summary>
    /// Zero GDID constant which is equivalent to an uninitialized structure with zero era and zero authority and zero counter
    /// </summary>
    public static readonly GDID ZERO = new GDID(0, 0ul);

    public GDID(UInt32 era, UInt64 id)
    {
      Era = era;
      ID = id;
    }

    public GDID(uint era, int authority, UInt64 counter)
    {
      if (authority>AUTHORITY_MAX || counter>COUNTER_MAX)
        throw new DataException(StringConsts.DISTRIBUTED_DATA_GDID_CTOR_ERROR.Args(authority, AUTHORITY_MAX, counter, COUNTER_MAX));

      Era = era;
      ID = (((UInt64)authority)<<60) | (counter & COUNTER_MASK);
    }

    public GDID(byte[] bytes, int startIdx = 0)
    {
      if (bytes==null || startIdx <0 || (bytes.Length-startIdx) < BYTE_SIZE)
        throw new DataException(StringConsts.ARGUMENT_ERROR+"GDID.ctor(bytes==null<minsz)");

      Era = bytes.ReadBEUInt32(ref startIdx);
      ID =  bytes.ReadBEUInt64(ref startIdx);
    }

    public unsafe GDID(byte* ptr)
    {
      if (ptr == null)
        throw new DataException(StringConsts.ARGUMENT_ERROR + "GDID.ctor(ptr==null)");

      var idx = 0;
      Era = IOUtils.ReadBEUInt32(ptr, ref idx);
      ID = IOUtils.ReadBEUInt64(ptr, ref idx);
    }

    public readonly UInt32 Era;
    public readonly UInt64 ID;

    /// <summary>
    /// Returns the 0..15 index of the authority that issued this ID
    /// </summary>
    public int Authority => (int)( (ID & AUTHORITY_MASK) >> 60 );

    /// <summary>
    /// Returns the 60 bits of counter segment of this id (without authority segment upper 4 bits)
    /// </summary>
    public UInt64 Counter => ID & COUNTER_MASK;

    /// <summary>
    /// Returns the GDID buffer as BigEndian Era:ID tuple
    /// </summary>
    public byte[] Bytes
    {
      get
      { //WARNING!!! NEVER EVER CHANGE this method without considering the effect:
        // Database keys RELY on the specific byte ordering for proper tree balancing
        // MUST use BIG ENDIAN encoding  ERA, COUNTER not vice-versa
        var result = new byte[BYTE_SIZE];
        result.WriteBEUInt32(0, Era);
        result.WriteBEUInt64(sizeof(UInt32), ID);
        return result;
      }
    }

    /// <summary>
    /// Writes value into an existing buffer (thus avoiding allocation) starting at the specified index,
    /// the buffer /index must be big enough to take 12 bytes
    /// </summary>
    public void WriteIntoBuffer(byte[] buff, int startIndex = 0)
    {
      if (buff==null || startIndex < 0 || startIndex + BYTE_SIZE >= buff.Length)
        throw new DataException(StringConsts.ARGUMENT_ERROR + "GDID.WriteIntoBuffer(buff==null<minsz)");

      WriteIntoBufferUnsafe(buff, startIndex);
    }

    internal void WriteIntoBufferUnsafe(byte[] buff, int startIndex)
    {
      buff.WriteBEUInt32(startIndex, Era);
      buff.WriteBEUInt64(startIndex + sizeof(UInt32), ID);
    }

    /// <summary>
    /// True is this instance is invalid/Zero - represents 0:0:0
    /// </summary>
    public bool IsZero => Era==0 && ID==0;

    public bool CheckRequired(string targetName) => !IsZero;

    /// <summary>
    /// Returns the guaranteed parsable stable string representation of GDID in the form 'Era:Authority:Counter'
    /// </summary>
    public override string ToString()
    {
      // WARNING!!! returned string representation must be parsable in the original form 'Era:Authority:Counter' and must not change
      return Era.ToString(INVARIANT) + ":" + SAUTHORITIES[Authority] + ":" + Counter.ToString(INVARIANT);
    }


    [ThreadStatic] private static StringBuilder ts_HexCache;

    //20210803 #524 speed up with string builder and explicit conversion
    /// <summary>
    /// Returns a hexadecimal representation of GDID which is guaranteed to be parsable by Parse()/TryParse()
    /// </summary>
    public string ToHexString() // #524 => Era.ToString("X8") + ID.ToString("X16");
    {
      const int NIBBLE = 4; //size of 1 hex digit = 1/2 byte
      const int HEX_SZ = 24;//Era(4)+ID(8)=12 bytes * 2 symbols per byte

      var sb = ts_HexCache;
      if (sb == null)
      {
        sb = new StringBuilder(HEX_SZ, HEX_SZ);
        ts_HexCache = sb;
      }
      else
      {
        sb.Clear();
      }

      for(var i = 32 - NIBBLE; i >= 0; i -= NIBBLE)
        sb.Append(hexDigit(Era >> i));

      for (var i = 64 - NIBBLE; i >= 0; i -= NIBBLE)
        sb.Append(hexDigit(ID >> i));

      return sb.ToString();
    }

    public override int GetHashCode() => (int)Era ^ (int)ID ^ (int)(ID >> 32);

    public ulong GetDistributedStableHash() => ((ulong)Era << 32) ^ ID;

    public override bool Equals(object obj) => obj is GDID gdid ? this.Equals(gdid) : false;
    public bool          Equals(GDID other) => (this.Era == other.Era) && (this.ID == other.ID);

    public int CompareTo(object obj)
    {
      if (obj==null) return -1;
      return this.CompareTo((GDID)obj);
    }

    public int CompareTo(GDID other)
    {
      var result = this.Era.CompareTo(other.Era);
      if (result!=0) return result;
      return this.ID.CompareTo(other.ID);
    }

    public static bool operator == (GDID x, GDID y) => x.Equals(y);
    public static bool operator != (GDID x, GDID y) => !x.Equals(y);
    public static bool operator <  (GDID x, GDID y) => x.CompareTo(y) < 0;
    public static bool operator >  (GDID x, GDID y) => x.CompareTo(y) > 0;
    public static bool operator <= (GDID x, GDID y) => x.CompareTo(y) <= 0;
    public static bool operator >= (GDID x, GDID y) => x.CompareTo(y) >= 0;

    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
    {
      wri.Write('"');
      itoa(wri, Era);
      wri.Write(':');
      itoa(wri, (ulong)Authority);
      wri.Write(':');
      itoa(wri, Counter);
      wri.Write('"');
    }

    private static unsafe void itoa(TextWriter wri, ulong v)
    {
      var buf = stackalloc char[24];//max char length of ulong number is 21
      var i = 0;
      while(true)
      {
        buf[i++] = (char)('0' + (v % 10));
        v /= 10;
        if (v == 0) break;
      }

      while(--i>=0) wri.Write(buf[i]);
    }

    (bool match, IJsonReadable self) IJsonReadable.ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      try
      {
        return (true, data.AsGDID());
      }
      catch
      {
        return (false, null);
      }
    }

    public static GDID Parse(string str)
    {
      GDID result;
      if (!TryParse(str, out result))
        throw new DataException(StringConsts.DISTRIBUTED_DATA_GDID_PARSE_ERROR.Args(str.TakeFirstChars(10)));

      return result;
    }

    internal static int hexDigit(char c)
    {
      var d = c - '0';
      if (d>=0 && d<=9) return d;

      d = c - 'A';
      if (d>=0 && d<=5) return 10 + d;

      d = c - 'a';
      if (d>=0 && d<=5) return 10 + d;

      return -1;
    }

    private static char hexDigit(ulong i)
    {
      i &= 0xf;
      if (i > 9) return (char)('a' + (i - 10));
      return (char)('0' + i);
    }

    public static bool TryParse(string str, out GDID? gdid)
    {
      GDID parsed;
      if (TryParse(str, out parsed))
      {
        gdid = parsed;
        return true;
      }

      gdid = null;
      return false;
    }

    //todo: Candidate for Span<char> rewrite
    public static unsafe bool TryParse(string str, out GDID gdid)
    {
      gdid = GDID.ZERO;
      if (str==null) return false;

      var ic = str.IndexOf(':');
      if (ic>-1) //regular Era:Auth:Counter format
      {
        const int MIN_LEN = 5;// "0:0:0"
        if (str.Length<MIN_LEN) return false;

        string sera, sau, sctr;
        var i1 = ic;
        if (i1<=0 || i1==str.Length-1) return false;

        sera = str.Substring(0, i1);

        var i2 = str.IndexOf(':', i1+1);
        if (i2<0 || i2==str.Length-1 || i2==i1+1) return false;

        sau = str.Substring(i1+1, i2-i1-1);

        sctr = str.Substring(i2+1);



        uint era=0;
        if (!uint.TryParse(sera, ISTYLES, INVARIANT, out era)) return false;

        byte au=0;
        if (!byte.TryParse(sau, ISTYLES, INVARIANT, out au)) return false;

        ulong ctr;
        if (!ulong.TryParse(sctr, ISTYLES, INVARIANT, out ctr)) return false;

        if (au>AUTHORITY_MAX || ctr>COUNTER_MAX) return false;

        gdid = new GDID(era, au, ctr);
        return true;
      }

      //HEX format
      str = str.Trim(); //trim returns as-is if there is nothing to trim

      var ix = str.IndexOf("0x", StringComparison.OrdinalIgnoreCase);
      if (ix == 0) ix += 2;//skip 0x
      else ix = 0;

      var buf = stackalloc byte[BYTE_SIZE];
      var j = 0;
      for(var i=ix; i<str.Length;)
      {
        var dh = hexDigit(str[i]); i++;
        if (dh<0 || i==str.Length) return false;
        var dl = hexDigit(str[i]); i++;
        if (dl<0) return false;

        if (j == BYTE_SIZE) return false;
        buf[j] = (byte)((dh << 4) + dl);
        j++;
      }
      if (j < BYTE_SIZE) return false;

      gdid = new GDID(buf);
      return true;
    }
  }


  /// <summary>
  /// Represents a tuple of GDID and its symbolic representation (framework usually uses an ELink as symbolic representation).
  /// This struct is needed to pass GDID along with its ELink representation together.
  /// Keep in mind that string poses a GC load, so this struct is not suitable for being used as a pile cache key
  /// </summary>
  [Serializable]
  public struct GDIDSymbol : IEquatable<GDIDSymbol>, IJsonWritable, IJsonReadable
  {
    public GDIDSymbol(GDID gdid, string symbol)
    {
      GDID = gdid;
      Symbol = symbol;
    }

    public readonly GDID GDID;
    public readonly string Symbol;

    public bool IsZero{ get{ return GDID.IsZero;}}

    public override string ToString()
    {
      return "{0}::'{1}'".Args(GDID, Symbol);
    }

    public override int GetHashCode()
    {
      return GDID.GetHashCode() ^ (Symbol!=null ? Symbol.GetHashCode() : 0);
    }

    public override bool Equals(object obj)
    {
      if (!(obj is GDIDSymbol)) return false;
      return this.Equals((GDIDSymbol)obj);
    }

    public bool Equals(GDIDSymbol other)
    {
      return this.GDID.Equals(other.GDID) && this.Symbol.EqualsOrdSenseCase(other.Symbol);
    }

    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
    {
      JsonWriter.WriteMap(wri, nestingLevel, options, new DictionaryEntry("gdid", GDID), new DictionaryEntry("sym", Symbol));
    }

    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is JsonDataMap map)
      {
        return (true, new GDIDSymbol(map["gdid"].AsGDID(GDID.ZERO), map["sym"].AsString()));
      }
      return (false, null);
    }

    public static bool operator ==(GDIDSymbol lhs, GDIDSymbol rhs) => lhs.Equals(rhs);
    public static bool operator !=(GDIDSymbol lhs, GDIDSymbol rhs) => !lhs.Equals(rhs);
  }

  /// <summary>
  /// Compares GDID regardless of authority. This is useful for range checking, when authorities generating GDIDs in the same
  ///  range should be disregarded. Use GDIDRangeComparer.Instance.
  ///  Only relative range comparison can be made.
  ///  The Equality returned by this comparer can not be relied upon for GDID comparison as it disregards authority.
  ///  Equality can only be tested for range comparison.
  /// </summary>
  public class GDIDRangeComparer : IComparer<GDID>
  {
    public static readonly GDIDRangeComparer Instance = new GDIDRangeComparer();

    private GDIDRangeComparer() {}

    public int Compare(GDID x, GDID y)
    {
      var result = x.Era.CompareTo(y.Era);
      if (result!=0) return result;
      return x.Counter.CompareTo(y.Counter);//Authority is disregarded
    }
  }

}
