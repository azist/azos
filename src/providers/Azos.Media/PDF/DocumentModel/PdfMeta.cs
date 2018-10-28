/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Media.PDF.DocumentModel
{
  /// <summary>
  /// PDF document metadata
  /// </summary>
  public class PdfMeta
  {
    public PdfMeta()
    {
      Version = Constants.DEFAULT_DOCUMENT_VERSION;
    }

    /// <summary>
    /// PDF document version
    /// </summary>
    public string Version { get; set; }
  }
}