/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Media.PDF.DocumentModel
{
  /// <summary>
  /// PDF document header
  /// </summary>
  internal class PdfRoot : PdfObject
  {
    /// <summary>
    /// Document outlines' object Id
    /// </summary>
    public PdfOutlines Outlines { get; set; }

    /// <summary>
    /// Document info's object Id
    /// </summary>
    public PdfInfo Info { get; set; }

    /// <summary>
    /// Document page tree's object Id
    /// </summary>
    public PdfPageTree PageTree { get; set; }
  }
}
