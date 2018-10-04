
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

    public bool IsAssigned
    {
        get { return LineNumber >=0 && ColNumber>=0 && CharNumber >=0; }
    }

    public SourcePosition(int lineNum, int colNum, int charNum)
    {
      LineNumber = lineNum;
      ColNumber = colNum;
      CharNumber = charNum;
    }

    public override string ToString()
    {
      return IsAssigned ?  "Line: {0} Col: {1} Char: {2}".Args(LineNumber, ColNumber, CharNumber) : string.Empty;
    }

  }

}
