/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Azos.CodeAnalysis;
using Azos.CodeAnalysis.JSON;
using Azos.CodeAnalysis.Source;

namespace Azos.Serialization.JSON.Backends
{
  public struct JazonToken
  {
    // Error
    internal JazonToken(JsonMsgCode code, string text)
    {
      Type = (JsonTokenType)(-1);
      Text = text;
      ULValue = (ulong)code;
      DValue = 0d;
    }

    // Identifier
    internal JazonToken(JsonTokenType type, string text)
    {
      Type = type;
      Text = text;
      ULValue = 0ul;
      DValue = 0d;
    }

    //Int, or Long value
    internal JazonToken(JsonTokenType type, string text, ulong ulValue)
    {
      Type = type;
      Text = text;
      ULValue = ulValue;
      DValue = 0d;
    }

    //Double value
    internal JazonToken(JsonTokenType type,  string text, double dValue)
    {
      Type = type;
      Text = text;
      ULValue = 0ul;
      DValue = dValue;
    }

    public readonly JsonTokenType Type;

    public readonly string Text;

    //To avoid boxing the primitives are stored in-place, StructLayout Explicit with overlap of long over dbl does not yield any benefits
    public readonly ulong ULValue;//bool is stored as 1
    public readonly double DValue;

    public bool IsError => Type < 0;
    public JsonMsgCode MsgCode => IsError ? (JsonMsgCode)ULValue : JsonMsgCode.INFOS;
    public bool IsPrimary => Type > JsonTokenType.SYMBOLS_START; //  !IsNonLanguage && Type != JsonTokenType.tComment;
    public bool IsNonLanguage => Type==JsonTokenType.tUnknown || (Type > JsonTokenType.NONLANG_START && Type < JsonTokenType.NONLANG_END);
  }



  internal class JazonLexer : IEnumerator<JazonToken>, IEnumerable<JazonToken>
  {
    public JazonLexer(ISourceText src)
    {
      source = src;

      buffer = new StringBuilder(256);//caching this in TLS does not change much

      result = run().GetEnumerator();
    }

    public bool MoveNext() => result.MoveNext();
    void IEnumerator.Reset() => result.Reset();
    void IDisposable.Dispose() => result.Dispose();
    public JazonToken Current => result.Current;
    object IEnumerator.Current => result.Current;

    IEnumerator IEnumerable.GetEnumerator() => result;
    IEnumerator<JazonToken> IEnumerable<JazonToken>.GetEnumerator() => result;


    private readonly IEnumerator<JazonToken> result;
    private readonly ISourceText source;

    char chr, nchr;
    bool wasFlush;
    bool isError;
    bool isCommentBlock;
    char commentBlockEnding;
    bool isCommentLine;
    bool isDirective;
    bool freshLine = true;

    bool isString;
    char stringEnding;
    bool isVerbatim;

    int posLine = 1;
    int posCol = 0;
    int posChar = 0;

    SourcePosition tagStartPos;

    StringBuilder buffer;

    public SourcePosition Position => tagStartPos;

    private void moveNext()
    {
      posChar++;
      posCol++;
      chr = source.ReadChar();
      nchr = source.PeekChar();
    }

    //this is done on purpose do NOT use Char.isSymbol in .NET
    //we can control what WE consider symbols
    private bool isSymbol(char c)
    {
      return (
             ((c >= '!') && (c <= '/')) ||
             ((c >= ':') && (c <= '@')) ||
             ((c >= '[') && (c <= '`')) ||
             ((c >= '{') && (c <= '~'))
             ) && (c != '_') && (c != '.') && (c != '$') && (c != '#');
    }

    private void bufferAdd(char c)
    {
      if (buffer.Length == 0) tagStartPos = new SourcePosition(posLine, posCol, posChar);
      buffer.Append(c);
    }



    private IEnumerable<JazonToken> run()
    {

      yield return new JazonToken(JsonTokenType.tBOF, string.Empty);

      #region Main walk
      //=======================================================================================================================
      while (!source.EOF)
      {
        moveNext();

        #region CRLF
        if ((chr == '\n') || (chr == '\r'))
        {
          if ((isString) && (!isVerbatim))
          {
            yield return new JazonToken(JsonMsgCode.eUnterminatedString,  "Unterminated string");
            yield break;//no further parsing
          }


          if ((isString && isVerbatim) || (isCommentBlock))
            bufferAdd(chr);


          if (chr == '\n')
          {
            if ((!isString) && (!isCommentBlock))
            {
              if (buffer.Length>0)
              {
                yield return flush();
                if (isError) yield break;
              }
              if (isCommentLine)
              {
                isCommentLine = false;
                isDirective = false;
              }
              freshLine = true;
            }
            posLine++;
          }
          posCol = 0;


          continue;
        }

        #endregion


        if (isString)
        {
          #region Inside String

          if (isVerbatim || (chr != '\\') || (nchr != '\\'))//take care of 'c:\\dir\\';
          {
            //turn off strings
            if (
                 ((isVerbatim) && (chr == stringEnding) && (nchr == stringEnding)) ||
                 ((!isVerbatim) && (chr == '\\') && (nchr == stringEnding))
                )
            {
              //Verbatim: eat one extra:   $"string ""test"" syntax" == string "test" syntax
              //Regular: eat "\" escape:    "string \"test\" syntax" == string "test" syntax
              moveNext();

              if (source.EOF)
              {
                yield return new JazonToken(JsonMsgCode.eUnterminatedString, "Unterminated string");
                yield break;//stop further processing, as string did not terminate but EOF reached
              }
            }
            else if (chr == stringEnding)
            {
              yield return flush(); //even empty
              if (isError) yield break;
              isString = false;
              continue; // eat terminating string char
            }

          }
          else//take care of 'c:\\dir\\'
          {
            bufferAdd(chr); //preserve  \
            moveNext();
          }
          #endregion
        }//in string
        else
        {
          #region Not Inside String

          if (!isCommentLine)
          {
            if (!isCommentBlock)
            {
              #region Not inside comments

              #region Turn On Comments
              //turn on comment block
              if (((chr == '/') || (chr == '|')) && (nchr == '*'))
              {
                if (buffer.Length>0)
                {
                  yield return flush();
                  if (isError) yield break;
                }
                isCommentBlock = true;
                commentBlockEnding = chr;
                moveNext();
                continue;
              }

              //turn on comment line
              if ((chr == '/') && (nchr == '/'))
              {
                if (buffer.Length>0)
                {
                  yield return flush();
                  if (isError) yield break;
                }
                isCommentLine = true;
                moveNext();
                continue;
              }

              //turn on comment line mode for directive
              //directives MUST be the first non-white char on the line
              if (freshLine && chr == '#')
              {
                if (buffer.Length>0)
                {
                  yield return flush();
                  if (isError) yield break;
                }
                isCommentLine = true;
                isDirective = true;
                continue;
              }


              #endregion

              #region Turn On Strings
              if ((chr == '$') && ((nchr == '"') || (nchr == '\'')))
              {
                if (buffer.Length>0)
                {
                  yield return flush();
                  if (isError) yield break;
                }
                isString = true;
                isVerbatim = true;
                stringEnding = nchr;
                moveNext();
                continue;
              }
              if ((chr == '"') || (chr == '\''))
              {
                if (buffer.Length>0)
                {
                  yield return flush();
                  if (isError) yield break;
                }
                isString = true;
                isVerbatim = false;
                stringEnding = chr;
                continue;
              }

              #endregion

              #region Syntactic Separators - Space, colons and Symbols

              if ((chr == ' ') || (chr == '\t')) //space or TAB
              {
                if (buffer.Length>0)
                {
                  yield return flush();
                  if (isError) yield break;
                }
                continue; //eat it
              }

              if (
                 // (chr == ';') ||
                  (chr == '{') ||
                  (chr == '}') ||
                //  (chr == '(') ||
                //  (chr == ')') ||
                  (chr == '[') ||
                  (chr == ']') ||
                  (chr == ',') ||
                  (chr == ':') ||
                  ((chr == '.') && (!JsonIdentifiers.ValidateDigit(nchr)))
                 )
              {
                if (buffer.Length > 0)
                {
                  yield return flush();
                  if (isError) yield break;
                }

                bufferAdd(chr);

                yield return flush();
                if (isError) yield break;
                continue;
              }

              //Scientific numbers like:   2e+30, 45E-10
              if (buffer.Length > 0 &&
                   JsonIdentifiers.ValidateDigit(buffer[0]) &&
                   (chr == 'e' || chr == 'E') &&
                   (nchr == '+' || nchr == '-')
                  )
              {
                bufferAdd(chr); //e
                moveNext();
                bufferAdd(chr); //+ or -
                moveNext();
                bufferAdd(chr); // add digit after + or -
                continue;
              }



              //for operators like -- /= += etc...
              if ((buffer.Length > 0) && (isSymbol(chr) != isSymbol(buffer[0])))
              {
                yield return flush();
                if (isError) yield break;
              }

              #endregion

              #endregion
            }
            else
            {
              #region Turn Off Comment Block
              if ((chr == '*') && (nchr == commentBlockEnding))
              {
                yield return flush();
                if (isError) yield break;
                isCommentBlock = false;
                moveNext();
                continue;
              }
              #endregion
            }//block comments off

          }//NOT CommentLine

          #endregion
        }//not in string

        bufferAdd(chr);
        freshLine = false;

      }//while
       //=======================================================================================================================
      #endregion

      if (buffer.Length>0)
      {
        yield return flush(); //flush any remains
        if (isError) yield break;
      }

      #region Post-walk check
      if (!wasFlush)
        yield return new JazonToken(JsonMsgCode.ePrematureEOF, "Premature EOF");


      if (isCommentBlock)
        yield return new JazonToken(JsonMsgCode.eUnterminatedComment,  "Unterminated comment");


      if (isString)
        yield return new JazonToken(JsonMsgCode.eUnterminatedString, "Unterminated string");


      #endregion

      yield return new JazonToken(JsonTokenType.tEOF, string.Empty);
    }//Scan



    private JazonToken flush()
    {
      if (
          (buffer.Length == 0) &&
          (!isString) &&
          (!isCommentBlock) &&
          (!isCommentLine)
         ) return new JazonToken(JsonTokenType.tUnknown, null);

      wasFlush = true;

      var type = JsonTokenType.tUnknown;

      if (isString)
      {
        var text = buffer.ToString();
        buffer.Clear();

        type = JsonTokenType.tStringLiteral;

        if (!isVerbatim)
        {
          try //expand escapes
          {
            text = JsonStrings.UnescapeString(text);
          }
          catch (StringEscapeErrorException err)
          {
            isError = true;
            return new JazonToken(JsonMsgCode.eInvalidStringEscape, err.ErroredEscape);
          }
        }

        return new JazonToken(JsonTokenType.tStringLiteral,  text);
      }
      else if (isCommentLine && isDirective)//directives treated similar to line comments
      {
        var text = buffer.ToString();
        buffer.Clear();
        return new JazonToken(JsonTokenType.tDirective, text);
      }
      else if (isCommentBlock || isCommentLine)
      {
        var text = buffer.ToString();
        buffer.Clear();
        return new JazonToken(JsonTokenType.tComment,  text);
      }
      else
      {
        string text;
        var blen = buffer.Length;
        if (blen==1)
        {
          var fc = buffer[0];

          switch(fc)
          {
            case '+': { buffer.Clear(); return new JazonToken(JsonTokenType.tPlus, "+");           }
            case ',': { buffer.Clear(); return new JazonToken(JsonTokenType.tComma, ",");          }
            case '-': { buffer.Clear(); return new JazonToken(JsonTokenType.tMinus, "-");          }
            case ':': { buffer.Clear(); return new JazonToken(JsonTokenType.tColon, ":");          }
            case '[': { buffer.Clear(); return new JazonToken(JsonTokenType.tSqBracketOpen, "[");  }
            case ']': { buffer.Clear(); return new JazonToken(JsonTokenType.tSqBracketClose, "]"); }
            case '{': { buffer.Clear(); return new JazonToken(JsonTokenType.tBraceOpen, "{");      }
            case '}': { buffer.Clear(); return new JazonToken(JsonTokenType.tBraceClose, "}");     }
          }

          text = buffer.ToString();
        }
        else if (blen==4)
        {
          if (buffer[0] == 'n' && buffer[1] == 'u' && buffer[2] == 'l' && buffer[3] == 'l') { buffer.Clear(); return new JazonToken(JsonTokenType.tNull, "null");}
          if (buffer[0] == 't' && buffer[1] == 'r' && buffer[2] == 'u' && buffer[3] == 'e') { buffer.Clear(); return new JazonToken(JsonTokenType.tTrue, "true");}

          text = buffer.ToString();
        }
        else
        {
          text = buffer.ToString();

      //    if (text == "null")    { buffer.Clear(); return new JazonToken(JsonTokenType.tNull, "null"); }
          if (text == "false") { buffer.Clear(); return new JazonToken(JsonTokenType.tFalse, "false"); }
      //    if (text == "true")  { buffer.Clear(); return new JazonToken(JsonTokenType.tTrue, "true"); }
        }
        buffer.Clear();


        try
        {
          if (JazonNumbers.ConvertInteger(text, out type, out var ulv))
          {
            return new JazonToken(type, text, ulv);
          }

          if (JazonNumbers.ConvertDouble(text, out type, out var dblv))
          {
            return new JazonToken(type, text, dblv);
          }
        }
        catch (OverflowException)
        {
          isError = true;
          return new JazonToken(JsonMsgCode.eValueTooBig, text);
        }

        type = JsonTokenType.tIdentifier;

        return new JazonToken(type, text);
      }//not comment

    }//flush

  }
}
