/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Serialization.JSON;

namespace Azos.CodeAnalysis.JSON
{
  public sealed partial class JsonParser : Parser<JsonLexer>
  {
    private class abortException : Exception { }


    private IEnumerator<JsonToken> tokens;
    private JsonToken token;

    private void abort() { throw new abortException(); }

    private void errorAndAbort(JsonMsgCode code)
    {
      EmitMessage(MessageType.Error, (int)code, Lexer.SourceCodeReference, token: token);
      throw new abortException();
    }

    private void fetch()
    {
      if (!tokens.MoveNext())
        errorAndAbort(JsonMsgCode.ePrematureEOF);

      token = tokens.Current;
    }

    private void fetchPrimary()
    {
      do fetch();
      while (!token.IsPrimary);
    }

    private object doAny()
    {
      if (token.Type == JsonTokenType.tBraceOpen) return doObject();
      else
      if (token.Type == JsonTokenType.tSqBracketOpen) return doArray();
      else
      if (token.Type == JsonTokenType.tNull) return null;
      else
      if (token.Type == JsonTokenType.tTrue) return true;
      else
      if (token.Type == JsonTokenType.tFalse) return false;
      else
      if (token.IsNumericLiteral || token.IsTextualLiteral) return token.Value;
      else
      if (token.Type == JSON.JsonTokenType.tPlus)
      {
        fetchPrimary();//skip "+"
        if (!token.IsNumericLiteral) errorAndAbort(JsonMsgCode.eNumericLiteralExpectedAfterSignOperator);
        return token.Value;
      }
      else
      if (token.Type == JSON.JsonTokenType.tMinus)
      {
        fetchPrimary();//skip "-"
        if (!token.IsNumericLiteral) errorAndAbort(JsonMsgCode.eNumericLiteralExpectedAfterSignOperator);
        if (token.Value is double)
          return -(double)token.Value;

        if (token.Value is ulong)
          return -(long)(ulong)token.Value;

        var v = (Int32)token.Value;
        return -v;
      }

      errorAndAbort(JsonMsgCode.eSyntaxError);
      return null;
    }

    private JsonDataArray doArray()
    {
      fetchPrimary(); // skip [

      var arr = new JsonDataArray();

      if (token.Type != JsonTokenType.tSqBracketClose)//empty array  []
      {
        while (true)
        {
          arr.Add(doAny());  // [any, any, any]
          fetchPrimary();
          if (token.Type != JsonTokenType.tComma) break;
          fetchPrimary();
        }

        if (token.Type != JsonTokenType.tSqBracketClose)
          errorAndAbort(JsonMsgCode.eUnterminatedArray);
      }
      return arr;
    }

    private JsonDataMap doObject()
    {
      fetchPrimary(); // skip {

      var obj = new JsonDataMap(this.m_CaseSensitiveMaps);

      if (token.Type != JsonTokenType.tBraceClose)//empty object  {}
      {
        while (true)
        {
          if (token.Type != JsonTokenType.tIdentifier && token.Type != JsonTokenType.tStringLiteral)
            errorAndAbort(JsonMsgCode.eObjectKeyExpected);

          var key = token.Text;

          if (obj.ContainsKey(key)) errorAndAbort(JsonMsgCode.eDuplicateObjectKey);

          fetchPrimary();
          if (token.Type != JsonTokenType.tColon)
            errorAndAbort(JsonMsgCode.eColonOperatorExpected);

          fetchPrimary();

          var value = doAny();

          obj[key] = value;

          fetchPrimary();
          if (token.Type != JsonTokenType.tComma) break;
          fetchPrimary();
        }

        if (token.Type != JsonTokenType.tBraceClose)
          errorAndAbort(JsonMsgCode.eUnterminatedObject);
      }
      return obj;
    }

  }
}
