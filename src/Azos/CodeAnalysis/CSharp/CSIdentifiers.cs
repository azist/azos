/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

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
      => (c >= '0' && c <= '9');

    /// <summary>
    /// Checks whether supplied char is suitable for a part of C# id
    /// </summary>
    public static bool ValidateChar(char c)
      => Char.IsLetter(c) || (c == '_');

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
