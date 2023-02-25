/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.CodeAnalysis.Source
{
  /// <summary>
  /// Represents position in source input
  /// </summary>
  public struct SourcePosition
  {
    public static readonly SourcePosition UNASSIGNED = new SourcePosition(-1, -1, -1);

    public readonly int LineNumber;
    public readonly int ColNumber;
    public readonly int CharNumber;

    public bool IsAssigned => LineNumber >= 0 && ColNumber >= 0 && CharNumber >= 0;

    public SourcePosition(int lineNum, int colNum, int charNum)
    {
      LineNumber = lineNum;
      ColNumber = colNum;
      CharNumber = charNum;
    }

    public override string ToString()
      => IsAssigned ? $"Line: {LineNumber} Col: {ColNumber} Char: {CharNumber}" : string.Empty;

  }
}
