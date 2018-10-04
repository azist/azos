

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Azos.CodeAnalysis.JSON
{
  /// <summary>
  /// Provides JSON keyword resolution services, this class is thread safe
  /// </summary>
  public static class JSONKeywords
  {
    private static Dictionary<string, JSONTokenType> s_KeywordList = new Dictionary<string, JSONTokenType>();

    static JSONKeywords()
    {
       s_KeywordList["true"]  = JSONTokenType.tTrue;
       s_KeywordList["false"] = JSONTokenType.tFalse;
       s_KeywordList["null"]  = JSONTokenType.tNull;

       s_KeywordList["+"]  = JSONTokenType.tPlus;
       s_KeywordList["-"]  = JSONTokenType.tMinus;

       s_KeywordList["{"] = JSONTokenType.tBraceOpen;
       s_KeywordList["}"] = JSONTokenType.tBraceClose;
       s_KeywordList["["] = JSONTokenType.tSqBracketOpen;
       s_KeywordList["]"] = JSONTokenType.tSqBracketClose;
       s_KeywordList[","] = JSONTokenType.tComma;
       s_KeywordList[":"] = JSONTokenType.tColon;
    }

    /// <summary>
    /// Resolves a JSON keyword - this method IS thread safe
    /// </summary>
    public static JSONTokenType Resolve(string str)
    {
      JSONTokenType tt;

      s_KeywordList.TryGetValue(str, out tt);

      return (tt!=JSONTokenType.tUnknown) ? tt : JSONTokenType.tIdentifier;
    }

  }



}

