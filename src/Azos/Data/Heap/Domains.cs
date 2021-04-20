/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Data.Heap
{
  /// <summary>
  /// Defines system data domains
  /// </summary>
  public static class Domains
  {
    /// <summary>
    /// Minimum ID length is 3 characters
    /// </summary>
    public const int MIN_ID_LEN = 3;

    /// <summary>
    /// Maximum ID length is 32 characters
    /// </summary>
    public const int MAX_ID_LEN = 32;

    /// <summary>
    /// Checks heap id for validity and throws CallGuardException
    /// </summary>
    public static string CheckId(this string id, string name)
      => id.IsTrue( v => IsIdentifierValid(v), $"Valid id: <<{name.NonBlank(nameof(name))}>>");

    /// <summary>
    /// Valid Data Heap Identifiers are between 3 to 32 characters in length, having only these characters: [A..Z|a..z|0..9|_|-].
    /// The '-' and '_' are not allowed as the very first or the very last characters.
    /// </summary>
    /// <remarks>
    /// The enforcement of Latin-only characters is needed to ensure compatibility with various storage engines
    /// which may be incapable of storing file/entity names with various non-ASCII encodings.
    /// The heap may be hosted on a co-location hosting with the underlying OS set for certain culture.
    /// The ASCII-only constraint is a good guarantee, also it can be used for some protocol optimizations (e.g. ASCII byte/char encoding)
    /// </remarks>
    public static bool IsIdentifierValid(string id)
    {
      if (id.IsNullOrWhiteSpace()) return false;
      var len = id.Length;
      if (len < MIN_ID_LEN || len > MAX_ID_LEN) return false;
      for (var i = 0; i < len; i++)
      {
        var c = id[i];
        if (c >= '0' && c <= '9') continue;
        if (c >= 'a' && c <= 'z') continue;
        if (c >= 'A' && c <= 'Z') continue;
        if ((c == '_' || c == '-') && (i > 0) && (i < len - 1)) continue;
        return false;
      }

      return true;
    }
  }
}
