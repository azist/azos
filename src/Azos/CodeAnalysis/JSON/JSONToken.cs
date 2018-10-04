
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
      public class JSONToken : Token
      {
        public readonly JSONTokenType Type;


        public JSONToken(JSONLexer lexer, JSONTokenType type, SourcePosition startPos, SourcePosition endPos, string text, object value = null) :
                           base(lexer, startPos, endPos, text, value)
        {
          Type = type;
        }

        public override Language Language
        {
            get { return JSONLanguage.Instance;}
        }

        public override string ToString()
        {
          return "{0} {1}".Args(Type, base.ToString());
        }

        public override TokenKind Kind
        {
            get
            {
                if (Type > JSONTokenType.LITERALS_START && Type < JSONTokenType.LITERALS_END)
                     return TokenKind.Literal;

                if (Type > JSONTokenType.SYMBOLS_START && Type < JSONTokenType.SYMBOLS_END)
                     return TokenKind.Symbol;

                switch(Type)
                {

                    case JSONTokenType.tBOF:          return TokenKind.BOF;
                    case JSONTokenType.tEOF:          return TokenKind.EOF;
                    case JSONTokenType.tDirective:    return TokenKind.Directive;
                    case JSONTokenType.tComment:      return TokenKind.Comment;
                    case JSONTokenType.tIdentifier:   return TokenKind.Identifier;

                    case JSONTokenType.tPlus:
                    case JSONTokenType.tMinus:
                    case JSONTokenType.tColon:        return TokenKind.Operator;

                    default: return TokenKind.Other;
                }
            }
        }

        public override bool IsPrimary
        {
            get { return !IsNonLanguage && Type!=JSONTokenType.tComment; }
        }

        public override bool IsNonLanguage
        {
            get { return Type > JSONTokenType.NONLANG_START && Type < JSONTokenType.NONLANG_END; }
        }

        public override int OrdinalType
        {
            get { return (int)Type; }
        }

        public override bool IsTextualLiteral
        {
            get { return Type == JSONTokenType.tStringLiteral; }
        }

        public override bool IsNumericLiteral
        {
            get { return Type > JSONTokenType.NUMLITERALS_START && Type < JSONTokenType.NUMLITERALS_END; }
        }
      }




}
