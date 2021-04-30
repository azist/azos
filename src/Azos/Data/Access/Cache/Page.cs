/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Data.Access.Cache
{
  /// <summary>
  /// Abstraction of an item that can be placed in hashing bucket
  /// </summary>
  public abstract class Bucketed : DisposableObject
  {

  }

  /// <summary>
  /// A composite item that stores collection of CacheRecs and can be placed into a hashing bucket
  /// </summary>
  internal sealed class Page : Bucketed
  {
    internal Page(int recPerPage)
    {
      m_Records = new CacheRec[recPerPage];
    }
    internal CacheRec[] m_Records;
  }

}
