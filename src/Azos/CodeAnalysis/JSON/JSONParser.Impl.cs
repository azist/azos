
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.CodeAnalysis.Source;
using Azos.Serialization.JSON;


namespace Azos.CodeAnalysis.JSON
{
    public sealed partial class JSONParser : Parser<JSONLexer>
    {
            private class abortException: Exception {}


            private IEnumerator<JSONToken> tokens;
            private JSONToken token;

            private void abort() { throw new abortException(); }

            private void errorAndAbort(JSONMsgCode code)
            {
               EmitMessage(MessageType.Error, (int)code, Lexer.SourceCodeReference,token: token);
               throw new abortException();
            }

            private void fetch()
            {
                if (!tokens.MoveNext())
                   errorAndAbort(JSONMsgCode.ePrematureEOF);

                token = tokens.Current;
            }

            private void fetchPrimary()
            {
                do fetch();
                while(!token.IsPrimary);
            }

            private object doAny()
            {
                if (token.Type==JSONTokenType.tBraceOpen) return doObject();
                else
                if (token.Type==JSONTokenType.tSqBracketOpen) return doArray();
                else
                if (token.Type==JSONTokenType.tNull) return null;
                else
                if (token.Type==JSONTokenType.tTrue) return true;
                else
                if (token.Type==JSONTokenType.tFalse) return false;
                else
                if (token.IsNumericLiteral || token.IsTextualLiteral) return token.Value;
                else
                if (token.Type==JSON.JSONTokenType.tPlus)
                {
                  fetchPrimary();//skip "+"
                  if (!token.IsNumericLiteral)  errorAndAbort(JSONMsgCode.eNumericLiteralExpectedAfterSignOperator);
                  return token.Value;
                }
                else
                if (token.Type==JSON.JSONTokenType.tMinus)
                {
                  fetchPrimary();//skip "-"
                  if (!token.IsNumericLiteral)  errorAndAbort(JSONMsgCode.eNumericLiteralExpectedAfterSignOperator);
                  if (token.Value is double)
                         return -(double)token.Value;

                  if (token.Value is ulong)
                         return -(long)(ulong)token.Value;

                  var v = (Int32)token.Value;
                  return -v;
                }


                errorAndAbort(JSONMsgCode.eSyntaxError);
                return null;
            }

            private JSONDataArray doArray()
            {
                fetchPrimary(); // skip [

                var arr = new JSONDataArray();

                if (token.Type!=JSONTokenType.tSqBracketClose)//empty array  []
                {
                    while(true)
                    {
                       arr.Add( doAny() );  // [any, any, any]
                       fetchPrimary();
                       if (token.Type!=JSONTokenType.tComma) break;
                       fetchPrimary();
                    }

                    if (token.Type!=JSONTokenType.tSqBracketClose)
                       errorAndAbort(JSONMsgCode.eUnterminatedArray);
                }
                return arr;
            }

            private JSONDataMap doObject()
            {
               fetchPrimary(); // skip {

               var obj = new JSONDataMap(this.m_CaseSensitiveMaps);

               if (token.Type!=JSONTokenType.tBraceClose)//empty object  {}
                {
                    while(true)
                    {
                       if (token.Type!=JSONTokenType.tIdentifier && token.Type!=JSONTokenType.tStringLiteral)
                             errorAndAbort(JSONMsgCode.eObjectKeyExpected);

                       var key = token.Text;

                       if (obj.ContainsKey(key)) errorAndAbort(JSONMsgCode.eDuplicateObjectKey);

                       fetchPrimary();
                       if (token.Type!=JSONTokenType.tColon)
                             errorAndAbort(JSONMsgCode.eColonOperatorExpected);

                       fetchPrimary();

                       var value = doAny();

                       obj[key] = value;

                       fetchPrimary();
                       if (token.Type!=JSONTokenType.tComma) break;
                       fetchPrimary();
                    }

                    if (token.Type!=JSONTokenType.tBraceClose)
                         errorAndAbort(JSONMsgCode.eUnterminatedObject);
                }
               return obj;
            }



    }
}
