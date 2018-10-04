
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        LITERALS_END
  }
}
