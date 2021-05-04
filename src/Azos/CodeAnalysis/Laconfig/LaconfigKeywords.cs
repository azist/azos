/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.CodeAnalysis.Laconfig
{
  /// <summary>
  /// Provides Laconfig keyword resolution services, this class is thread safe
  /// </summary>
  public static class LaconfigKeywords
  {
    /// <summary>
    /// Resolves a Laconfig keyword
    /// </summary>
    public static LaconfigTokenType Resolve(string str)
    {
      if (str.Length!=1)
      {
        return str.EqualsOrdSenseCase("null") ? LaconfigTokenType.tNull
                                              : LaconfigTokenType.tIdentifier;
      }

      var c = str[0];

      switch(c)
      {
        case '{': return LaconfigTokenType.tBraceOpen;
        case '}': return LaconfigTokenType.tBraceClose;
        case '=': return LaconfigTokenType.tEQ;
      }

      return LaconfigTokenType.tIdentifier;
    }

  }
}
