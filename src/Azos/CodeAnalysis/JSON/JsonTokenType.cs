/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.CodeAnalysis.JSON
{
  /// <summary>
  /// Denotes JSON token types.
  /// NOTE: Although called JSON, this is really a JSON superset implementation that includes extra features:
  ///  comments, directives, verbatim strings(start with $), ' or " string escapes, unquoted object key names
  /// </summary>
  public enum JsonTokenType
  {
    tUnknown = 0,
        NONLANG_START,
            tBOF,
            tEOF,
            tDirective,
        NONLANG_END,

            tComment,

        SYMBOLS_START,
            tComma,

            tBraceOpen,
            tBraceClose,

            tSqBracketOpen,
            tSqBracketClose,
            tDot,
        SYMBOLS_END,

            tIdentifier,

            tPlus,
            tMinus,
            tColon,


        LITERALS_START,
            NUMLITERALS_START,
                tIntLiteral,
                tLongIntLiteral,
                tDoubleLiteral,
            NUMLITERALS_END,

                tStringLiteral,

                tTrue,
                tFalse,

                tNull,
        LITERALS_END
  }
}
