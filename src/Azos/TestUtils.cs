/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Data;
using Azos.Text;
using Azos.Serialization.JSON;

namespace Azos
{
  /// <summary>
  /// Provides utilities for testing
  /// </summary>
  public static class TestUtils
  {
    /// <summary>
    /// Returns true if JsonDataMap map is not null and it has a named key and its value contains the specified substring doing case insensitive
    /// comparison by default. Used in unit tests doing partial data matches
    /// </summary>
    public static bool MapValueContains(this JsonDataMap map, string key, string substring, bool senseCase = false)
    {
      if (map==null) return false;
      if (key.IsNullOrWhiteSpace()) return false;

      if (!map.ContainsKey(key)) return false;

      var got = map[key].AsString(null);
      if (got == null)
      {
       if (substring == null) return true;
       return false;
      }

      return got.IndexOf(substring, senseCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) >=0;
    }

    /// <summary>
    /// Returns true if JsonDataMap map is not null and it has a named key and its value matches the specified pattern using * and ? wild cards a la DOS
    /// </summary>
    public static bool MapValueMatches(this JsonDataMap map, string key, string pattern, bool senseCase = false)
    {
      if (map == null) return false;
      if (key.IsNullOrWhiteSpace()) return false;

      if (!map.ContainsKey(key)) return false;

      var got = map[key].AsString(null);
      if (got == null && pattern == null) return true;

      return got.MatchPattern(pattern, senseCase: senseCase);
    }



  }
}
