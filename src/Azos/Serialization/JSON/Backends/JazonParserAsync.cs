/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azos.CodeAnalysis.JSON;
using Azos.CodeAnalysis.Source;

namespace Azos.Serialization.JSON.Backends
{
  //#731 Implements asynchronous version of JSON datagram  parser
  public static class JazonParserAsync
  {
    public static async Task<object> ParseAsync(ISourceText src, bool senseCase, int maxDepth = 64)
    {
      if (maxDepth<0) maxDepth = 0;// 0 = root literal value
      var lexer = new JazonLexer(src);

      await fetchPrimary(lexer).ConfigureAwait(false);

      var data = await doAny(lexer, senseCase, maxDepth).ConfigureAwait(false);

      lexer.ReuseResources();

      return data;
    }


    //synchronous call against the data segment which was pre-fetched asynchronously
    private static void fetch(JazonLexer tokens)
    {
      if (!tokens.MoveNext())
        throw new JazonDeserializationException(JsonMsgCode.ePrematureEOF, "Premature end of Json content");
    }

    //called by async version after segment was pre-filled with data asynchronously
    private static JazonToken fetchPrimarySync(JazonLexer tokens)
    {
      do{ fetch(tokens); }
      while (!tokens.Current.IsPrimary && !tokens.Current.IsError);

      return tokens.Current;
    }

    //do NOT make this function async for performance.
    //because this is a low-level function called for every JSON token,
    //we do NOT want to allocate async state machine for every call, instead we use
    //ValueTask which completes in alloc-free way synchronously 80+% of the time.
    //as the buffer is (already) pre-filled with future data to read
    private static ValueTask<JazonToken> fetchPrimary(JazonLexer tokens)
    {
      do
      {
        if (tokens.source.NearEndOfSegment)
        {
          //pre-fetch the next character segment asynchronously, eventually continuing with JSON processing
          var future = tokens.source
                             .FetchSegmentAsync()
                             .ContinueWith((_, t) => fetchPrimarySync((JazonLexer)t), tokens);
          //notice no await: explicit use of ValueTask.ctor
          return new ValueTask<JazonToken>(future);
        }

        fetch(tokens);
      }
      while (!tokens.Current.IsPrimary && !tokens.Current.IsError);

      //notice no await: explicit use of ValueTask.ctor
      //avoids allocation of task/async state machine
      return new ValueTask<JazonToken>(tokens.Current);
    }

    private static readonly object TRUE;
    private static readonly object FALSE;
    static JazonParserAsync()
    {
      TRUE = true;
      FALSE = false;
    }


    private static async ValueTask<object> doAny(JazonLexer lexer, bool senseCase, int maxDepth)
    {
      var token = lexer.Current;

      switch(token.Type)
      {
        case JsonTokenType.tBraceOpen:
        {
          lexer.fsmResources.StackPushObject();//#833
          var obj = await doObject(lexer, senseCase, maxDepth - 1).ConfigureAwait(false);
          lexer.fsmResources.StackPop();//#833
          return obj;
        }

        case JsonTokenType.tSqBracketOpen:
        {
          lexer.fsmResources.StackPushArray();//#833
          var arr = await doArray(lexer, senseCase, maxDepth - 1).ConfigureAwait(false);
          lexer.fsmResources.StackPop();//#833
          return arr;
        }

        case JsonTokenType.tNull:           return null;
        case JsonTokenType.tTrue:           return TRUE;
        case JsonTokenType.tFalse:          return FALSE;
        case JsonTokenType.tStringLiteral:  return token.Text;
        case JsonTokenType.tIntLiteral:     return (int)token.ULValue;
        case JsonTokenType.tLongIntLiteral: return token.ULValue > long.MaxValue ? token.ULValue : (object)(long)token.ULValue;
        case JsonTokenType.tDoubleLiteral:  return token.DValue;

        case JsonTokenType.tPlus: {
          token = await fetchPrimary(lexer).ConfigureAwait(false);//skip "+"

          if (token.Type == JsonTokenType.tIntLiteral) return (int)token.ULValue;
          if (token.Type == JsonTokenType.tLongIntLiteral) return (long)token.ULValue;
          if (token.Type == JsonTokenType.tDoubleLiteral) return token.DValue;
          throw JazonDeserializationException.From(JsonMsgCode.eNumericLiteralExpectedAfterSignOperator, "Numeric literal expected", lexer);
        }

        case JsonTokenType.tMinus: {
          token = await fetchPrimary(lexer).ConfigureAwait(false);//skip "-"

          if (token.Type == JsonTokenType.tIntLiteral) return -(int)token.ULValue;
          if (token.Type == JsonTokenType.tLongIntLiteral) return -(long)token.ULValue;
          if (token.Type == JsonTokenType.tDoubleLiteral) return -token.DValue;
          throw JazonDeserializationException.From(JsonMsgCode.eNumericLiteralExpectedAfterSignOperator, "Numeric literal expected", lexer);
        }
      }

      throw JazonDeserializationException.From(token.IsError ? token.MsgCode : JsonMsgCode.eSyntaxError, "Bad syntax", lexer);
    }

    private static async ValueTask<JsonDataArray> doArray(JazonLexer lexer, bool senseCase, int maxDepth)
    {
      if (maxDepth < 0)
        throw JazonDeserializationException.From(JsonMsgCode.eGraphDepthLimit, "The graph is too deep", lexer);

      var token = await fetchPrimary(lexer).ConfigureAwait(false); // skip [

      var arr = new JsonDataArray();

      if (token.Type != JsonTokenType.tSqBracketClose)//empty array  []
      {
        while (true)
        {
          lexer.fsmResources.StackPushArrayElement(arr.Count);//#833
          var item = await doAny(lexer, senseCase, maxDepth).ConfigureAwait(false);
          lexer.fsmResources.StackPop();//#833
          arr.Add( item );  // [any, any, any]

          token = await fetchPrimary(lexer).ConfigureAwait(false);
          if (token.Type != JsonTokenType.tComma) break;
          token = await fetchPrimary(lexer).ConfigureAwait(false);//eat coma
          if (token.Type == JsonTokenType.tSqBracketClose) break;//allow for [el,] trailing coma at the end
        }

        if (token.Type != JsonTokenType.tSqBracketClose)
          throw JazonDeserializationException.From(JsonMsgCode.eUnterminatedArray, "Unterminated array", lexer);
      }

      return arr;
    }

    private static async ValueTask<JsonDataMap> doObject(JazonLexer lexer, bool senseCase, int maxDepth)
    {
      if (maxDepth < 0)
        throw JazonDeserializationException.From(JsonMsgCode.eGraphDepthLimit, "The graph is too deep", lexer);

      var token = await fetchPrimary(lexer).ConfigureAwait(false); // skip {

      var obj = new JsonDataMap(senseCase);

      if (token.Type != JsonTokenType.tBraceClose)//empty object  {}
      {
        while (true)
        {
          if (token.Type != JsonTokenType.tIdentifier && token.Type != JsonTokenType.tStringLiteral)
            throw JazonDeserializationException.From(JsonMsgCode.eObjectKeyExpected, "Expecting object key", lexer);

          var key = token.Text;
          //Duplicate keys are NOT forbidden by standard

          lexer.fsmResources.StackPushProp(key);//#833

          token = await fetchPrimary(lexer).ConfigureAwait(false);
          if (token.Type != JsonTokenType.tColon)
            throw JazonDeserializationException.From(JsonMsgCode.eColonOperatorExpected, "Missing colon", lexer);

          token = await fetchPrimary(lexer).ConfigureAwait(false);

          var value = await doAny(lexer, senseCase, maxDepth).ConfigureAwait(false);

          obj[key] = value;

          lexer.fsmResources.StackPop();//#833

          token = await fetchPrimary(lexer).ConfigureAwait(false);
          if (token.Type != JsonTokenType.tComma) break;
          token = await fetchPrimary(lexer).ConfigureAwait(false);//eat comma
          if (token.Type == JsonTokenType.tBraceClose) break;//allow for {el,} trailing coma at the end
        }

        if (token.Type != JsonTokenType.tBraceClose)
          throw JazonDeserializationException.From(JsonMsgCode.eUnterminatedObject, "Unterminated object", lexer);
      }
      return obj;
    }

  }
}
