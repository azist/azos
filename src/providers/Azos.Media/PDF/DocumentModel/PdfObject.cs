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
  public abstract class PdfObject : IPdfObject
  {
    private const string REFERENCE_FORMAT = "{0} 0 R";

    /// <summary>
    /// Document-wide unique object Id
    /// </summary>
    public int ObjectId { get; set; }

    /// <summary>
    /// Returns PDF object indirect reference
    /// </summary>
    public string GetReference()
    {
      return REFERENCE_FORMAT.Args(ObjectId);
    }
  }
}
