/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;
using System.IO;
using Azos.Data;

namespace Azos.Serialization.JSON
{
  /// <summary>
  /// Facilitates TypeHint data interpretation. Type hints are used for low-level optimization
  /// for voluminous data streaming operations.
  /// TypeHints only support primitive framework built-in types by design,
  /// they are purposely NOT built for extension
  /// </summary>
  internal static class TypeHint
  {
    //Pattern:  $abc:
    public const char CHR_0 = '$';
    public const char CHR_4 = ':';
    public const int HINT_LEN = 5;// $234: = 5 chars

    //All type hints must contain 3 chars
    public const string H_BIN      = "bin";
    public const string H_STR      = "str";
    public const string H_ATOM     = "atm";
    public const string H_DATE     = "dtm";
    public const string H_TIMESPAN = "tsp";
    public const string H_GUID     = "uid";
    public const string H_GDID     = "gdi";
    public const string H_RGDID    = "rgd";
    public const string H_ENTITYID = "eid";

    public static readonly Atom HA_BIN      = Atom.Encode(H_BIN     ); //byte[] encoded base64
    public static readonly Atom HA_STR      = Atom.Encode(H_STR     ); //escaped string which starts with `$...:` pattern
    public static readonly Atom HA_ATOM     = Atom.Encode(H_ATOM    );
    public static readonly Atom HA_DATE     = Atom.Encode(H_DATE    );
    public static readonly Atom HA_TIMESPAN = Atom.Encode(H_TIMESPAN);
    public static readonly Atom HA_GUID     = Atom.Encode(H_GUID    );
    public static readonly Atom HA_GDID     = Atom.Encode(H_GDID    );
    public static readonly Atom HA_RGDID    = Atom.Encode(H_RGDID   );
    public static readonly Atom HA_ENTITYID = Atom.Encode(H_ENTITYID);


    private static readonly Dictionary<Atom, Func<string, object>> STR_CONVERTERS = new()
    {
      {HA_BIN,      v => v.TryFromWebSafeBase64()},//Confirmed that writer uses  ToWebSafeBase64
      {HA_STR,      v => v}, //as-is
      {HA_ATOM,     v => v.AsAtom(Atom.ZERO)},
      {HA_DATE,     v => v.AsDateTime(default(DateTime), CoreConsts.UTC_TIMESTAMP_STYLES)},
      {HA_TIMESPAN, v => v.AsTimeSpan(TimeSpan.Zero)},
      {HA_GUID,     v => v.AsGUID(Guid.Empty)},
      {HA_GDID,     v => v.AsGDID(GDID.ZERO)},
      {HA_RGDID,    v => v.AsRGDID(RGDID.ZERO)},
      {HA_ENTITYID, v => v.AsEntityId(EntityId.EMPTY)}
    };


    public static void EmitTypeHint(TextWriter wri, string th)
    {
      (th.Length == 3).IsTrue("thint.len==3");
      wri.Write(CHR_0);
      wri.Write(th);
      wri.Write(CHR_4);
    }

    /// <summary>
    /// Check string if it needs string escape, that is: string starts with a value
    /// which can be interpreted as a type hint
    /// </summary>
    public static bool StringNeedsEscape(string v)
    {
      if (v == null) return false;
      if (v.Length < HINT_LEN) return false;
      if (v[0] != CHR_0) return false;
      if (v[4] != CHR_4) return false;
      return true;
    }

    /// <summary>
    /// Assuming that type hints are enabled, post-processes a value which came back from Json deserialization
    /// returning a possibly different value cast/re-deserialized to the requested type
    /// </summary>
    public static object PostProcessRawDeserializedJsonValue(object value)
    {
      if (value == null) return null;

      if (value is string svalue)
      {
        var (str, hint) = TryGetHintFromString(svalue);
        if (hint.IsZero) return str;
        //reinterpret type
        if (!STR_CONVERTERS.TryGetValue(hint, out var fconv)) return str;//unknown type, return as-is
        return fconv(str);
      }

      return value;
    }

    /// <summary>
    /// Tries to extract type hint from a string value, if it contains one, returning value without hint and the hint as an atom of 3 chars
    /// </summary>
    public static (string str, Atom hint) TryGetHintFromString(string v)
    {
      if (v == null) return (null, Atom.ZERO);
      var len = v.Length;
      if (len < HINT_LEN) return (v, Atom.ZERO);

      if (v[0] != CHR_0 || v[4] != CHR_4) return (v, Atom.ZERO);// $abc:xxxxxxxxx

      var str = len == HINT_LEN ? string.Empty : v.Substring(5);
      var hint = new Atom((ulong)( (v[3] << 24) | (v[2] << 16) | v[1]) );
      return (str, hint);
    }

  }
}
