/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.CodeAnalysis.Source;

namespace Azos.CodeAnalysis.Laconfig
{
      /// <summary>
      /// Represents a token of Laconfig language
      /// </summary>
      public class LaconfigToken : Token
      {
        public readonly LaconfigTokenType Type;


        public LaconfigToken(LaconfigLexer lexer, LaconfigTokenType type, SourcePosition startPos, SourcePosition endPos, string text, object value = null) :
                           base(lexer, startPos, endPos, text, value)
        {
          Type = type;
        }

        public override Language Language
        {
            get { return LaconfigLanguage.Instance;}
        }

        public override string ToString()
        {
          return "{0} {1}".Args(Type, base.ToString());
        }

        public override TokenKind Kind
        {
            get
            {
                if (Type > LaconfigTokenType.LITERALS_START && Type < LaconfigTokenType.LITERALS_END)
                     return TokenKind.Literal;

                if (Type > LaconfigTokenType.SYMBOLS_START && Type < LaconfigTokenType.SYMBOLS_END)
                     return TokenKind.Symbol;

                switch(Type)
                {

                    case LaconfigTokenType.tBOF:          return TokenKind.BOF;
                    case LaconfigTokenType.tEOF:          return TokenKind.EOF;
                    case LaconfigTokenType.tDirective:    return TokenKind.Directive;
                    case LaconfigTokenType.tComment:      return TokenKind.Comment;
                    case LaconfigTokenType.tIdentifier:   return TokenKind.Identifier;
                    case LaconfigTokenType.tEQ:           return TokenKind.Operator;

                    default: return TokenKind.Other;
                }
            }
        }

        public override bool IsPrimary
        {
            get { return !IsNonLanguage && Type!=LaconfigTokenType.tComment; }
        }

        public override bool IsNonLanguage
        {
            get { return Type > LaconfigTokenType.NONLANG_START && Type < LaconfigTokenType.NONLANG_END; }
        }

        public override int OrdinalType
        {
            get { return (int)Type; }
        }

        public override bool IsTextualLiteral
        {
            get { return Type == LaconfigTokenType.tStringLiteral; }
        }

        public override bool IsNumericLiteral
        {
            get { return false; } //not supported
        }
      }




}
