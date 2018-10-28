/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using Azos.Media.PDF.DocumentModel;

namespace Azos.Media.PDF
{
  /// <summary>
  /// Class that generates document-wide unique resource Id-s
  /// (the class is not thread-safe)
  /// </summary>
  internal class ResourceRepository
  {
    public ResourceRepository()
    {
      m_CurrentId = 0;
      m_Resources = new Dictionary<int, IPdfResource>();
    }

    private readonly Dictionary<int, IPdfResource> m_Resources;

    private int m_CurrentId;

    public int CurrentId
    {
      get { return m_CurrentId; }
    }

    public IPdfResource GetObject(int id)
    {
      IPdfResource result;
      m_Resources.TryGetValue(id, out result);
      return result;
    }

    public void Register(IPdfResource pdfResource)
    {
      pdfResource.ResourceId = ++m_CurrentId;
      m_Resources[m_CurrentId] = pdfResource;
    }
  }
}