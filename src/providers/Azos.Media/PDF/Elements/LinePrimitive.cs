/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Media.PDF.Text;

namespace Azos.Media.PDF.Elements
{
  /// <summary>
  /// PDF line primitive as a part of path
  /// </summary>
  public class LinePrimitive : PathPrimitive
  {
    private const string LINE_TO_FORMAT = "{0} {1} l";

    public LinePrimitive()
    {
    }

    public LinePrimitive(float endX, float endY)
    {
      EndX = endX;
      EndY = endY;
    }

    /// <summary>
    /// Line's end X coordinate
    /// </summary>
    public float EndX { get; set; }

    /// <summary>
    /// Line's end Y coordinate
    /// </summary>
    public float EndY { get; set; }

    /// <summary>
    /// Returns PDF string representation on the line primitive
    /// </summary>
    /// <returns></returns>
    public override string ToPdfString()
    {
      return LINE_TO_FORMAT.Args(TextAdapter.FormatFloat(EndX), TextAdapter.FormatFloat(EndY));
    }
  }
}