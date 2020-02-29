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
    public static object Parse(ISourceText src, bool senseCase)
    {
#warning add a limit to parsing depth (e.g. 256)
      var lexer = new JazonLexer(src);//pool the instance in thread-static var
      var tokensEnumerable = lexer.Scan();
      var tokens = tokensEnumerable.GetEnumerator();
      fetchPrimary(tokens);
      var data = doAny(tokens, senseCase);
      return data;
    }


    private static void fetch(IEnumerator<JazonToken> tokens)
    {
      if (!tokens.MoveNext())
        throw new JazonDeserializationException(JsonMsgCode.ePrematureEOF, "Premature end of Json content");
    }

    private static JazonToken fetchPrimary(IEnumerator<JazonToken> tokens)
    {
      do fetch(tokens);
      while (!tokens.Current.IsPrimary);
      return tokens.Current;
    }

    private static object doAny(IEnumerator<JazonToken> tokens, bool senseCase)
    {
      var token = tokens.Current;

      switch(token.Type)
      {
        case JsonTokenType.tBraceOpen:      return doObject(tokens, senseCase);
        case JsonTokenType.tSqBracketOpen:  return doArray(tokens, senseCase);
        case JsonTokenType.tNull:           return null;
        case JsonTokenType.tTrue:           return true;
        case JsonTokenType.tFalse:          return false;
        case JsonTokenType.tStringLiteral:  return token.Text;
        case JsonTokenType.tPlus: {
          token = fetchPrimary(tokens);//skip "+"

          if (token.Type == JsonTokenType.tIntLiteral) return (int)token.ULValue;
          if (token.Type == JsonTokenType.tLongIntLiteral) return (long)token.ULValue;
          if (token.Type == JsonTokenType.tDoubleLiteral) return token.DValue;
          throw new JazonDeserializationException(JsonMsgCode.eNumericLiteralExpectedAfterSignOperator, "Numeric literal expected", token.Position);
        }

        case JsonTokenType.tMinus: {
          token = fetchPrimary(tokens);//skip "-"
          if (!token.IsNumericLiteral)
            throw new JazonDeserializationException(JsonMsgCode.eNumericLiteralExpectedAfterSignOperator, "Numeric literal expected", token.Position);

          if (token.Type == JsonTokenType.tIntLiteral) return -(int)token.ULValue;
          if (token.Type == JsonTokenType.tLongIntLiteral) return -(long)token.ULValue;
          if (token.Type == JsonTokenType.tDoubleLiteral) return -token.DValue;
          throw new JazonDeserializationException(JsonMsgCode.eNumericLiteralExpectedAfterSignOperator, "Numeric literal expected", token.Position);
        }
      }

      throw new JazonDeserializationException(JsonMsgCode.eSyntaxError, "Bad syntax", token.Position);
    }

    private static JsonDataArray doArray(IEnumerator<JazonToken> tokens, bool senseCase)
    {
      var token = fetchPrimary(tokens); // skip [

      var arr = new JsonDataArray();

      if (token.Type != JsonTokenType.tSqBracketClose)//empty array  []
      {
        while (true)
        {
          var item = doAny(tokens, senseCase);
          arr.Add( item );  // [any, any, any]

          token = fetchPrimary(tokens);
          if (token.Type != JsonTokenType.tComma) break;
          token = fetchPrimary(tokens);//eat coma
          if (token.Type == JsonTokenType.tSqBracketClose) break;//allow for [el,] trailing coma at the end
        }

        if (token.Type != JsonTokenType.tSqBracketClose)
          throw new JazonDeserializationException(JsonMsgCode.eUnterminatedArray, "Unterminated array", token.Position);
      }

      return arr;
    }

    private static JsonDataMap doObject(IEnumerator<JazonToken> tokens, bool senseCase)
    {
      var token = fetchPrimary(tokens); // skip {

      var obj = new JsonDataMap(senseCase);

      if (token.Type != JsonTokenType.tBraceClose)//empty object  {}
      {
        while (true)
        {
          if (token.Type != JsonTokenType.tIdentifier && token.Type != JsonTokenType.tStringLiteral)
            throw new JazonDeserializationException(JsonMsgCode.eObjectKeyExpected, "Expecting object key", token.Position);

          var key = token.Text;

          //Duplicate keys are NOT forbidden by standard

          token = fetchPrimary(tokens);
          if (token.Type != JsonTokenType.tColon)
            throw new JazonDeserializationException(JsonMsgCode.eColonOperatorExpected, "Missing colon", token.Position);

          token = fetchPrimary(tokens);

          var value = doAny(tokens, senseCase);

          obj[key] = value;

          token = fetchPrimary(tokens);
          if (token.Type != JsonTokenType.tComma) break;
          token = fetchPrimary(tokens);
        }

        if (token.Type != JsonTokenType.tBraceClose)
          throw new JazonDeserializationException(JsonMsgCode.eUnterminatedObject, "Unterminated object", token.Position);
      }
      return obj;
    }

  }
}
