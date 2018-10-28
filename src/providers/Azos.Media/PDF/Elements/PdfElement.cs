/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Media.PDF.DocumentModel;

namespace Azos.Media.PDF.Elements
{
  /// <summary>
  /// Base class for all PDF primitives
  /// </summary>
  public abstract class PdfElement : PdfObject
  {
    /// <summary>
    /// X-coordinate
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Y-coordinate
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// Writes element into file stream
    /// </summary>
    /// <param name="writer">PDF writer</param>
    public abstract void Write(PdfWriter writer);
  }
}
