/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


namespace Azos.Serialization.JSON
{
  /// <summary>
  /// Facilitates TypeHint data interpretation.
  /// TypeHints only support primitive framework built-in types by design,
  /// they are NOT built for extension
  /// </summary>
  internal static class TypeHint
  {
    //Pattern:  $abc:
    public const char CHR_0 = '$';
    public const char CHR_4 = ':';

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

    /// <summary>
    /// Tries to extract type hint from string, if it contains one, returning value without hint and the hint as an atom of 3 chars
    /// </summary>
    public static (string str, Atom hint) TryGetStringHint(string v)
    {
      if (v == null) return (null, Atom.ZERO);
      var len = v.Length;
      if (len < 5) return (v, Atom.ZERO);

      if (v[0] != CHR_0 || v[4] != CHR_4) return (v, Atom.ZERO);// $abc:xxxxxxxxx

      var str = v.Substring(5);
      var hint = new Atom((ulong)(v[3] << 24 | v[2] << 16 | v[1]));
      return (str, hint);
    }

  }
}
