/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;
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
    public static readonly Atom THINT_BIN      = Atom.Encode("bin"); //byte[] encoded base64
    public static readonly Atom THINT_STR      = Atom.Encode("str"); //escaped string which starts with `$...:` pattern
    public static readonly Atom THINT_ATOM     = Atom.Encode("atm");
    public static readonly Atom THINT_DATE     = Atom.Encode("dtm");
    public static readonly Atom THINT_TIMESPAN = Atom.Encode("tsp");
    public static readonly Atom THINT_GUID     = Atom.Encode("uid");
    public static readonly Atom THINT_GDID     = Atom.Encode("gdi");
    public static readonly Atom THINT_RGDID    = Atom.Encode("rgd");
    public static readonly Atom THINT_ENTITYID = Atom.Encode("eid");


    private static readonly Dictionary<Atom, Func<string, object>> STR_CONVERTERS = new()
    {
      {THINT_BIN,  v => v.TryFromWebSafeBase64()},//Confirmed that writer uses  ToWebSafeBase64
      {THINT_STR,  v => v}, //as-is
      {THINT_ATOM, v => v.AsAtom(Atom.ZERO)},
      {THINT_DATE, v => v.AsDateTime(default(DateTime), CoreConsts.UTC_TIMESTAMP_STYLES)},
      {THINT_TIMESPAN, v => v.AsTimeSpan(TimeSpan.Zero)},
      {THINT_GUID,     v => v.AsGUID(Guid.Empty)},
      {THINT_GDID,     v => v.AsGDID(GDID.ZERO)},
      {THINT_RGDID,    v => v.AsRGDID(RGDID.ZERO)},
      {THINT_ENTITYID, v => v.AsEntityId(EntityId.EMPTY)}
    };

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
