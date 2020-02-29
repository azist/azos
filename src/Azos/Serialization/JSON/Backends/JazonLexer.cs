using System;
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
    internal JazonToken(JsonMsgCode code, SourcePosition position, string text)
    {
      Type = (JsonTokenType)(-1);
      Position = position;
      Text = text;
      ULValue = (ulong)code;
      DValue = 0d;
    }

    // Identifier
    internal JazonToken(JsonTokenType type, SourcePosition position, string text)
    {
      Type = type;
      Position = position;
      Text = text;
      ULValue = 0ul;
      DValue = 0d;
    }

    //Int, or Long value
    internal JazonToken(JsonTokenType type, SourcePosition position, string text, ulong ulValue)
    {
      Type = type;
      Position = position;
      Text = text;
      ULValue = ulValue;
      DValue = 0d;
    }

    //Double value
    internal JazonToken(JsonTokenType type, SourcePosition position, string text, double dValue)
    {
      Type = type;
      Position = position;
      Text = text;
      ULValue = 0ul;
      DValue = dValue;
    }

    public readonly JsonTokenType Type;
    public readonly SourcePosition Position;

    public readonly string Text;
    //To avoid boxing the primitives are stored in-place
    public readonly ulong ULValue;//bool is stored as 1
    public readonly double DValue;

    public bool IsError => Type < 0;
    public JsonMsgCode MsgCode => IsError ? (JsonMsgCode)ULValue : JsonMsgCode.INFOS;
  }



  internal class JazonLexer
  {
    public JazonLexer(ISourceText src)
    {
      source = src;
    }

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

    StringBuilder buffer = new StringBuilder(128);



    private void moveNext()
    {
      posChar++;
      posCol++;
      chr = source.ReadChar();
      nchr = source.PeekChar();
    }

    private SourcePosition srcPos()
    {
      return new SourcePosition(posLine, posCol, posChar);
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
      if (buffer.Length == 0) tagStartPos = srcPos();
      buffer.Append(c);
    }



    public IEnumerable<JazonToken> Scan()
    {

      yield return new JazonToken(JsonTokenType.tBOF, srcPos(), string.Empty);

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
            yield return new JazonToken(JsonMsgCode.eUnterminatedString, srcPos(), "Unterminated string");
            yield break;//no further parsing
          }


          if ((isString && isVerbatim) || (isCommentBlock))
            bufferAdd(chr);


          if (chr == '\n')
          {
            if ((!isString) && (!isCommentBlock))
            {
              yield return flush();
              if (isError) yield break;
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
                yield return new JazonToken(JsonMsgCode.eUnterminatedString, srcPos(), "Unterminated string");
                yield break;//stop further processing, as string did not terminate but EOF reached
              }
            }
            else if (chr == stringEnding)
            {
              yield return flush();
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
                yield return flush();
                if (isError) yield break;
                isCommentBlock = true;
                commentBlockEnding = chr;
                moveNext();
                continue;
              }

              //turn on comment line
              if ((chr == '/') && (nchr == '/'))
              {
                yield return flush();
                if (isError) yield break;
                isCommentLine = true;
                moveNext();
                continue;
              }

              //turn on comment line mode for directive
              //directives MUST be the first non-white char on the line
              if (freshLine && chr == '#')
              {
                yield return flush();
                if (isError) yield break;
                isCommentLine = true;
                isDirective = true;
                continue;
              }


              #endregion

              #region Turn On Strings
              if ((chr == '$') && ((nchr == '"') || (nchr == '\'')))
              {
                yield return flush();
                if (isError) yield break;
                isString = true;
                isVerbatim = true;
                stringEnding = nchr;
                moveNext();
                continue;
              }
              if ((chr == '"') || (chr == '\''))
              {
                yield return flush();
                if (isError) yield break;
                isString = true;
                isVerbatim = false;
                stringEnding = chr;
                continue;
              }

              #endregion

              #region Syntactic Separators - Space, colons and Symbols

              if ((chr == ' ') || (chr == '\t')) //space or TAB
              {
                yield return flush();
                if (isError) yield break;
                continue; //eat it
              }

              if (
                  (chr == ';') ||
                  (chr == '{') ||
                  (chr == '}') ||
                  (chr == '(') ||
                  (chr == ')') ||
                  (chr == '[') ||
                  (chr == ']') ||
                  (chr == ',') ||
                  (chr == ':') ||
                  ((chr == '.') && (!JsonIdentifiers.ValidateDigit(nchr)))
                 )
              {
                yield return flush();
                if (isError) yield break;

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


      yield return flush(); //flush any remains
      if (isError) yield break;

      #region Post-walk check
      if (!wasFlush)
        yield return new JazonToken(JsonMsgCode.ePrematureEOF, srcPos(), "Premature EOF");


      if (isCommentBlock)
        yield return new JazonToken(JsonMsgCode.eUnterminatedComment, srcPos(), "Unterminated comment");


      if (isString)
        yield return new JazonToken(JsonMsgCode.eUnterminatedString, srcPos(), "Unterminated string");


      #endregion

      yield return new JazonToken(JsonTokenType.tEOF, new SourcePosition(posLine, posCol, posChar), string.Empty);
    }//Scan



    private JazonToken flush()
    {
      if (
          (!isString) &&
          (!isCommentBlock) &&
          (!isCommentLine) &&
          (buffer.Length == 0)
         ) return new JazonToken(JsonTokenType.tUnknown, tagStartPos, null);

      var text = buffer.ToString();
      buffer.Length = 0;
      wasFlush = true;

      var type = JsonTokenType.tUnknown;

      if (isString)
      {
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
            return new JazonToken(JsonMsgCode.eInvalidStringEscape, tagStartPos, err.ErroredEscape);
          }
        }

        return new JazonToken(JsonTokenType.tStringLiteral, tagStartPos, text);
      }
      else if (isCommentLine && isDirective)//directives treated similar to line comments
      {
        return new JazonToken(JsonTokenType.tDirective, tagStartPos, text);
      }
      else if (isCommentBlock || isCommentLine)
      {
        return new JazonToken(JsonTokenType.tComment, tagStartPos, text);
      }
      else
      {
        try
        {
          if (JazonNumbers.ConvertInteger(text, out type, out var ulv))
          {
            return new JazonToken(type, tagStartPos, text, ulv);
          }

          if (JazonNumbers.ConvertDouble(text, out type, out var dblv))
          {
            return new JazonToken(type, tagStartPos, text, dblv);
          }
        }
        catch (OverflowException)
        {
          isError = true;
          return new JazonToken(JsonMsgCode.eValueTooBig, tagStartPos, text);
        }

        type = JsonKeywords.Resolve(text);

        if (type == JsonTokenType.tIdentifier)
        {
          //////if (text.StartsWith("$"))
          //////{
          //////  text = text.Remove(0, 1); //take care of verbatim names like: $class, $method, $var etc..
          //////  tagStartPos = new SourcePosition(tagStartPos.LineNumber, tagStartPos.ColNumber + 1, tagStartPos.CharNumber + 1);
          //////}

          if (!JsonIdentifiers.Validate(text))
          {
            isError = true;
            return new JazonToken(JsonMsgCode.eInvalidIdentifier, tagStartPos, text);
          }
        }

        return new JazonToken(type, tagStartPos, text);
      }//not comment

    }//flush


  }//FSM






}
