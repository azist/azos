/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;

using Azos.Serialization.JSON;

namespace Azos.Data
{
  /// <summary>
  /// Represents a <see cref="GDID"/> with a 32bit routing specifier.
  /// Pronounced like "r-gee-did"
  /// </summary>
  [Serializable]
  public struct RGDID : Access.IDataStoreKey,
                       IEquatable<RGDID>,
                       IJsonWritable,
                       IJsonReadable,
                       IRequiredCheck
  {
    public const int BYTE_SIZE = sizeof(UInt32) + GDID.BYTE_SIZE;

    /// <summary>
    /// Zero GDID constant which is equivalent to an uninitialized structure with zero era and zero authority and zero counter
    /// </summary>
    public static readonly RGDID ZERO = new RGDID(0, GDID.ZERO);

    public RGDID(UInt32 route, GDID gdid)
    {
      Route = route;
      Gdid = gdid;
    }

    public RGDID(byte[] bytes, int startIdx = 0)
    {
      if (bytes==null || startIdx <0 || (bytes.Length-startIdx) < BYTE_SIZE)
        throw new DataException(StringConsts.ARGUMENT_ERROR+"RGDID.ctor(bytes==null<minsz)");

      Route = bytes.ReadBEUInt32(ref startIdx);
      Gdid = new GDID(bytes, startIdx);
    }

    public unsafe RGDID(byte* ptr)
    {
      if (ptr == null)
        throw new DataException(StringConsts.ARGUMENT_ERROR + "RGDID.ctor(ptr==null)");

      var idx = 0;
      Route = IOUtils.ReadBEUInt32(ptr, ref idx);
      Gdid = new GDID(ptr + idx);
    }

    public readonly UInt32 Route;
    public readonly GDID Gdid;


    /// <summary>
    /// Returns the RGDID buffer as BigEndian Route:GDID tuple
    /// </summary>
    public byte[] Bytes
    {
      get
      { //WARNING!!! NEVER EVER CHANGE this method without considering the effect:
        // Database keys RELY on the specific byte ordering for proper tree balancing
        // MUST use BIG ENDIAN encoding  ERA, COUNTER not vice-versa
        var result = new byte[BYTE_SIZE];
        result.WriteBEUInt32(0, Route);
        Gdid.WriteIntoBufferUnsafe(result, sizeof(UInt32));
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
        throw new DataException(StringConsts.ARGUMENT_ERROR + "RGDID.WriteIntoBuffer(buff==null<minsz)");

      WriteIntoBufferUnsafe(buff, startIndex);
    }

    internal void WriteIntoBufferUnsafe(byte[] buff, int startIndex)
    {
      buff.WriteBEUInt32(startIndex, Route);
      Gdid.WriteIntoBufferUnsafe(buff, startIndex + sizeof(UInt32));
    }

    /// <summary>
    /// True is this instance has an invalid/Zero GDID - represents 0:0:0, Route is not even considered
    /// because GDID.ZERO is not a valid GDID, so is RGDID
    /// </summary>
    public bool IsZero => Gdid.IsZero;

    public bool CheckRequired(string targetName) => !IsZero;

    /// <summary>
    /// Returns the guaranteed parsable stable string representation of RGDID in the form 'Route:Era:Authority:Counter'
    /// </summary>
    public override string ToString()
    {
      // WARNING!!! returned string representation must be parsable in the original form 'Route:Era:Authority:Counter' and must not change
      return Route + ":" + Gdid.ToString();
    }

    /// <summary>
    /// Returns a hexadecimal representation of RGDID which is guaranteed to be parsable by Parse()/TryParse()
    /// </summary>
    public string ToHexString()  => Route.ToString("X8") + Gdid.ToHexString();

    public override int GetHashCode() => (int)Route ^ Gdid.GetHashCode();

    public override bool Equals(object obj) => obj is RGDID rgdid ? this.Equals(rgdid) : false;
    public bool          Equals(RGDID other) => (this.Route == other.Route) && (this.Gdid == other.Gdid);


    public static bool operator == (RGDID x, RGDID y) => x.Equals(y);
    public static bool operator != (RGDID x, RGDID y) => !x.Equals(y);

    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
      => JsonWriter.EncodeString(wri, ToString(), options);

    (bool match, IJsonReadable self) IJsonReadable.ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      try
      {
        return (true, data.AsRGDID());
      }
      catch
      {
        return (false, null);
      }
    }

    public static RGDID Parse(string str)
    {
      RGDID result;
      if (!TryParse(str, out result))
        throw new DataException(StringConsts.DISTRIBUTED_DATA_RGDID_PARSE_ERROR.Args(str.TakeFirstChars(10)));

      return result;
    }

    public static bool TryParse(string str, out RGDID? rgdid)
    {
      RGDID parsed;
      if (TryParse(str, out parsed))
      {
        rgdid = parsed;
        return true;
      }

      rgdid = null;
      return false;
    }

    //todo: Candidate for Span<char> rewrite - no need for substrings
    public static unsafe bool TryParse(string str, out RGDID rgdid)
    {
      rgdid = RGDID.ZERO;
      if (str==null) return false;

      var ic = str.IndexOf(':');
      if (ic > -1) //regular Route:Era:Auth:Counter format
      {
        const int MIN_LEN = 7;// "0:0:0:0"
        if (str.Length < MIN_LEN) return false;

        string sroute, sgdid;
        if (ic<=0 || ic==str.Length-1) return false;

        sroute = str.Substring(0, ic);
        sgdid = str.Substring(ic + 1);
        uint route=0;
        if (!uint.TryParse(sroute, GDID.ISTYLES, GDID.INVARIANT, out route)) return false;

        GDID gdid;
        if (!GDID.TryParse(sgdid, out gdid)) return false;

        rgdid = new RGDID(route, gdid);
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
        var dh = GDID.hexDigit(str[i]); i++;
        if (dh<0 || i==str.Length) return false;
        var dl = GDID.hexDigit(str[i]); i++;
        if (dl<0) return false;

        if (j == BYTE_SIZE) return false;
        buf[j] = (byte)((dh << 4) + dl);
        j++;
      }
      if (j < BYTE_SIZE) return false;

      rgdid = new RGDID(buf);
      return true;
    }
  }


}
