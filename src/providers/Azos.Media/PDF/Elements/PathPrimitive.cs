/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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