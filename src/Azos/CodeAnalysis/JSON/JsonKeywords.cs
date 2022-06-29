/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

namespace Azos.CodeAnalysis.JSON
{
  /// <summary>
  /// Provides JSON keyword resolution services, this class is thread safe
  /// </summary>
  public static class JsonKeywords
  {
    private static Dictionary<string, JsonTokenType> s_KeywordList = new Dictionary<string, JsonTokenType>(StringComparer.Ordinal);

    static JsonKeywords()
    {
       s_KeywordList["true"]  = JsonTokenType.tTrue;
       s_KeywordList["false"] = JsonTokenType.tFalse;
       s_KeywordList["null"]  = JsonTokenType.tNull;

       s_KeywordList["+"]  = JsonTokenType.tPlus;
       s_KeywordList["-"]  = JsonTokenType.tMinus;

       s_KeywordList["{"] = JsonTokenType.tBraceOpen;
       s_KeywordList["}"] = JsonTokenType.tBraceClose;
       s_KeywordList["["] = JsonTokenType.tSqBracketOpen;
       s_KeywordList["]"] = JsonTokenType.tSqBracketClose;
       s_KeywordList[","] = JsonTokenType.tComma;
       s_KeywordList[":"] = JsonTokenType.tColon;
    }

    /// <summary>
    /// Resolves a JSON keyword - this method IS thread safe
    /// </summary>
    public static JsonTokenType Resolve(string str)
    {

      if (s_KeywordList.TryGetValue(str, out JsonTokenType tt)) return tt;

      return JsonTokenType.tIdentifier;
    }

  }
}

