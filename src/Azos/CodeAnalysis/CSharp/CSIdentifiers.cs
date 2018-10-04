using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.CodeAnalysis.CSharp
{
  /// <summary>
  /// Identifier validation in the context of C# grammar
  /// </summary>
  public static class CSIdentifiers
  {

    /// <summary>
    /// Checks to see whether supplied char is a digit
    /// </summary>
    public static bool ValidateDigit(char c)
    {
      return (c >= '0' && c <= '9');
    }

    /// <summary>
    /// Checks whether supplied char is suitable for a part of C# id
    /// </summary>
    public static bool ValidateChar(char c)
    {
      return Char.IsLetter(c) || (c == '_');
    }

    /// <summary>
    /// Checks whether supplied string is a valid C# ident
    /// </summary>
    public static bool Validate(string id)
    {
      if (id==null) return false;
      if (id.Length < 1) return false;
      if (!ValidateChar(id[0])) return false;

      for (int i = 1; i < id.Length; i++)
      {
        if (! (ValidateChar(id[i]) || ValidateDigit(id[i]))) return false;
      }

      return true;
    }



  }
}
