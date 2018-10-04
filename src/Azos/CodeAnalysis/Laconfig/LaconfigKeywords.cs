
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.CodeAnalysis.Laconfig
{
  /// <summary>
  /// Provides Laconfig keyword resolution services, this class is thread safe
  /// </summary>
  public static class LaconfigKeywords
  {
    /// <summary>
    /// Resolves a Laconfig keyword
    /// </summary>
    public static LaconfigTokenType Resolve(string str)
    {
      if (str.Length!=1) return LaconfigTokenType.tIdentifier;

      var c = str[0];

      switch(c)
      {
        case '{': return LaconfigTokenType.tBraceOpen;
        case '}': return LaconfigTokenType.tBraceClose;
        case '=': return LaconfigTokenType.tEQ;
      }

      return LaconfigTokenType.tIdentifier;
    }

  }



}
