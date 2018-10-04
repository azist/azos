
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.CodeAnalysis.JSON
{
  /// <summary>
  /// Denotes JSON token types.
  /// NOTE: Although called JSON, this is really a JSON superset implementation that includes extra features:
  ///  comments, directives, verbatim strings(start with $), ' or " string escapes, unquoted object key names
  /// </summary>
  public enum JSONTokenType
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
