/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.CodeAnalysis.Laconfig
{
  /// <summary>
  /// Denotes Laconfig token types
  /// </summary>
  public enum LaconfigTokenType
  {
    tUnknown = 0,
      NONLANG_START,
          tBOF,
          tEOF,
          tDirective,
      NONLANG_END,

          tComment,

      SYMBOLS_START,
          tBraceOpen,
          tBraceClose,
      SYMBOLS_END,

          tEQ,
          tIdentifier,

      LITERALS_START,
          tStringLiteral,
          tNull,
      LITERALS_END
  }
}
