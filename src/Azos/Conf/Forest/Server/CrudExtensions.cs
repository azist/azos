/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using Azos.Data;
using Azos.Data.Access;

namespace Azos.Conf.Forest.Server
{
  /// <summary>
  /// Extensions for accessing Crud forest tree data - used by server implementations
  /// </summary>
  public static class CrudExtensions
  {
    private const string DATA_STORE_CLAUSE = "tree data as ICrudDataStore";

    /// <summary>
    /// Gets ICrudDataStore context for Corporate Area
    /// </summary>
    public static ICrudDataStore GetCrudData(this IForestDataSource forestData, Atom idForest, Atom idTree)
    {
      var tds = forestData.NonNull(nameof(forestData)).TryGetTreeDataStore(idForest, idTree);
      if (tds == null)
        throw new ConfigException("Forest `{0}` tree `{1}` is not found".Args(idForest, idTree));

      return (tds as ICrudDataStore).NonNull(DATA_STORE_CLAUSE);
    }


    /// <summary>
    /// Extension which loads enumerable returned by query executed in tree data context
    /// </summary>
    public static ConfiguredTaskAwaitable<IEnumerable<TDoc>> CorporateLoadEnumerableAsync<TDoc>(this IForestDataSource forestData,
                      Atom idForest,
                      Atom idTree,
                      Query<TDoc> qry) where TDoc : Doc
      => forestData.GetCrudData(idForest, idTree).LoadEnumerableAsync(qry).ConfigureAwait(false);
  }
}
