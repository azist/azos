/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using Azos.CodeAnalysis.JSON;
using Azos.CodeAnalysis.Source;

namespace Azos.Serialization.JSON.Backends
{
  public static class JazonParser
  {
    public static object Parse(ISourceText src, bool senseCase, int maxDepth = 64)
    {
      if (maxDepth<0) maxDepth = 0;// 0 = root literal value
      var lexer = new JazonLexer(src);
      fetchPrimary(lexer);
      var data = doAny(lexer, senseCase, maxDepth);

      return data;
    }


    private static void fetch(JazonLexer tokens)
    {
      if (!tokens.MoveNext())
        throw new JazonDeserializationException(JsonMsgCode.ePrematureEOF, "Premature end of Json content");
    }

    private static JazonToken fetchPrimary(JazonLexer tokens)
    {
      do fetch(tokens);
      while (!tokens.Current.IsPrimary);
      return tokens.Current;
    }

    private static readonly object TRUE;
    private static readonly object FALSE;
    static JazonParser()
    {
      TRUE = true;
      FALSE = false;
    }


    private static object doAny(JazonLexer lexer, bool senseCase, int maxDepth)
    {
      var token = lexer.Current;

      switch(token.Type)
      {
        case JsonTokenType.tBraceOpen:      return doObject(lexer, senseCase, maxDepth - 1);
        case JsonTokenType.tSqBracketOpen:  return doArray(lexer, senseCase, maxDepth - 1);
        case JsonTokenType.tNull:           return null;
        case JsonTokenType.tTrue:           return TRUE;
        case JsonTokenType.tFalse:          return FALSE;
        case JsonTokenType.tStringLiteral:  return token.Text;
        case JsonTokenType.tIntLiteral:     return (int)token.ULValue;
        case JsonTokenType.tLongIntLiteral: return token.ULValue > long.MaxValue ? token.ULValue : (object)(long)token.ULValue;
        case JsonTokenType.tDoubleLiteral:  return token.DValue;

        case JsonTokenType.tPlus: {
          token = fetchPrimary(lexer);//skip "+"

          if (token.Type == JsonTokenType.tIntLiteral) return (int)token.ULValue;
          if (token.Type == JsonTokenType.tLongIntLiteral) return (long)token.ULValue;
          if (token.Type == JsonTokenType.tDoubleLiteral) return token.DValue;
          throw new JazonDeserializationException(JsonMsgCode.eNumericLiteralExpectedAfterSignOperator, "Numeric literal expected", lexer.Position);
        }

        case JsonTokenType.tMinus: {
          token = fetchPrimary(lexer);//skip "-"

          if (token.Type == JsonTokenType.tIntLiteral) return -(int)token.ULValue;
          if (token.Type == JsonTokenType.tLongIntLiteral) return -(long)token.ULValue;
          if (token.Type == JsonTokenType.tDoubleLiteral) return -token.DValue;
          throw new JazonDeserializationException(JsonMsgCode.eNumericLiteralExpectedAfterSignOperator, "Numeric literal expected", lexer.Position);
        }
      }

      throw new JazonDeserializationException(token.IsError ? token.MsgCode : JsonMsgCode.eSyntaxError, "Bad syntax", lexer.Position);
    }

    private static JsonDataArray doArray(JazonLexer lexer, bool senseCase, int maxDepth)
    {
      if (maxDepth < 0)
        throw new JazonDeserializationException(JsonMsgCode.eGraphDepthLimit, "The graph is too deep", lexer.Position);

      var token = fetchPrimary(lexer); // skip [

      var arr = new JsonDataArray();

      if (token.Type != JsonTokenType.tSqBracketClose)//empty array  []
      {
        while (true)
        {
          var item = doAny(lexer, senseCase, maxDepth);
          arr.Add( item );  // [any, any, any]

          token = fetchPrimary(lexer);
          if (token.Type != JsonTokenType.tComma) break;
          token = fetchPrimary(lexer);//eat coma
          if (token.Type == JsonTokenType.tSqBracketClose) break;//allow for [el,] trailing coma at the end
        }

        if (token.Type != JsonTokenType.tSqBracketClose)
          throw new JazonDeserializationException(JsonMsgCode.eUnterminatedArray, "Unterminated array", lexer.Position);
      }

      return arr;
    }

    private static JsonDataMap doObject(JazonLexer lexer, bool senseCase, int maxDepth)
    {
      if (maxDepth < 0)
        throw new JazonDeserializationException(JsonMsgCode.eGraphDepthLimit, "The graph is too deep", lexer.Position);

      var token = fetchPrimary(lexer); // skip {

      var obj = new JsonDataMap(senseCase);

      if (token.Type != JsonTokenType.tBraceClose)//empty object  {}
      {
        while (true)
        {
          if (token.Type != JsonTokenType.tIdentifier && token.Type != JsonTokenType.tStringLiteral)
            throw new JazonDeserializationException(JsonMsgCode.eObjectKeyExpected, "Expecting object key", lexer.Position);

          var key = token.Text;

          //Duplicate keys are NOT forbidden by standard

          token = fetchPrimary(lexer);
          if (token.Type != JsonTokenType.tColon)
            throw new JazonDeserializationException(JsonMsgCode.eColonOperatorExpected, "Missing colon", lexer.Position);

          token = fetchPrimary(lexer);

          var value = doAny(lexer, senseCase, maxDepth);

          obj[key] = value;

          token = fetchPrimary(lexer);
          if (token.Type != JsonTokenType.tComma) break;
          token = fetchPrimary(lexer);//eat comma
          if (token.Type == JsonTokenType.tBraceClose) break;//allow for {el,} trailing coma at the end
        }

        if (token.Type != JsonTokenType.tBraceClose)
          throw new JazonDeserializationException(JsonMsgCode.eUnterminatedObject, "Unterminated object", lexer.Position);
      }
      return obj;
    }

  }
}
