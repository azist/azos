/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Media.PDF.DocumentModel
{
  /// <summary>
  /// Object that can be placed in PDF document as a resource
  /// </summary>
  internal interface IPdfResource
  {
    /// <summary>
    /// Document-wide unique resource Id
    /// </summary>
    int ResourceId { get; set; }

    /// <summary>
    /// Returns PDF object indirect reference
    /// </summary>
    string GetResourceReference();
  }
}