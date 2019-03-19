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

namespace Azos.CodeAnalysis.JSON
{
      /// <summary>
      /// Represents a token of JSON language
      /// </summary>
      public class JsonToken : Token
      {
        public readonly JsonTokenType Type;


        public JsonToken(JsonLexer lexer, JsonTokenType type, SourcePosition startPos, SourcePosition endPos, string text, object value = null) :
                           base(lexer, startPos, endPos, text, value)
        {
          Type = type;
        }

        public override Language Language
        {
            get { return JsonLanguage.Instance;}
        }

        public override string ToString()
        {
          return "{0} {1}".Args(Type, base.ToString());
        }

        public override TokenKind Kind
        {
            get
            {
                if (Type > JsonTokenType.LITERALS_START && Type < JsonTokenType.LITERALS_END)
                     return TokenKind.Literal;

                if (Type > JsonTokenType.SYMBOLS_START && Type < JsonTokenType.SYMBOLS_END)
                     return TokenKind.Symbol;

                switch(Type)
                {

                    case JsonTokenType.tBOF:          return TokenKind.BOF;
                    case JsonTokenType.tEOF:          return TokenKind.EOF;
                    case JsonTokenType.tDirective:    return TokenKind.Directive;
                    case JsonTokenType.tComment:      return TokenKind.Comment;
                    case JsonTokenType.tIdentifier:   return TokenKind.Identifier;

                    case JsonTokenType.tPlus:
                    case JsonTokenType.tMinus:
                    case JsonTokenType.tColon:        return TokenKind.Operator;

                    default: return TokenKind.Other;
                }
            }
        }

        public override bool IsPrimary
        {
            get { return !IsNonLanguage && Type!=JsonTokenType.tComment; }
        }

        public override bool IsNonLanguage
        {
            get { return Type > JsonTokenType.NONLANG_START && Type < JsonTokenType.NONLANG_END; }
        }

        public override int OrdinalType
        {
            get { return (int)Type; }
        }

        public override bool IsTextualLiteral
        {
            get { return Type == JsonTokenType.tStringLiteral; }
        }

        public override bool IsNumericLiteral
        {
            get { return Type > JsonTokenType.NUMLITERALS_START && Type < JsonTokenType.NUMLITERALS_END; }
        }
      }




}
