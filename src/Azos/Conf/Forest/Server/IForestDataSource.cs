/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data.Access;
using Azos.Pile;

namespace Azos.Conf.Forest.Server
{
  /// <summary>
  /// Abstraction of an entity which stores forest trees config data
  /// </summary>
  public interface IForestDataSource : IDaemon
  {
    /// <summary>
    /// Big memory cache used for node caching
    /// </summary>
    ICache Cache { get; }

    /// <summary>
    /// Tries to return all trees for the forest or null enumerable if the forest is not found
    /// </summary>
    Task<IEnumerable<Atom>> TryGetAllForestTreesAsync(Atom idForest);

    /// <summary>
    /// Returns a context for the specified forest tree or null if not found
    /// </summary>
    IDataStore TryGetTreeDataStore(Atom idForest, Atom idTree);
  }
}
