
using Azos.Media.PDF.DocumentModel;

namespace Azos.Media.PDF.Elements
{
  /// <summary>
  /// PDF path primitive (a line, Bezier curve,...) as a part of path
  /// </summary>
  public abstract class PathPrimitive : IPdfWritable
  {
    /// <summary>
    /// Returns PDF string representation
    /// </summary>
    public abstract string ToPdfString();
  }
}