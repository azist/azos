/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using Azos.Media.PDF.Styling;

namespace Azos.Media.PDF.DocumentModel
{
  /// <summary>
  /// PDF Page Tree document object
  /// </summary>
  internal class PdfPageTree : PdfObject
  {
    public PdfPageTree()
    {
      m_Pages = new List<PdfPage>();
    }

    private readonly List<PdfPage> m_Pages;

    public List<PdfPage> Pages
    {
      get { return m_Pages; }
    }

    /// <summary>
    /// Creates new page and adds it to the page tree
    /// </summary>
    /// <returns></returns>
    public PdfPage CreatePage(PdfSize size)
    {
      var page = new PdfPage(this, size);
      m_Pages.Add(page);

      return page;
    }
  }
}
