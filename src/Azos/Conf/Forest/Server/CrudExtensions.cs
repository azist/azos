/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
    public static ICrudDataStore GetCrudData(this IForestDataSource forestData, TreePtr tree)
    {
      var tds = forestData.NonNull(nameof(forestData)).TryGetTreeDataStore(tree.IdForest, tree.IdTree);
      if (tds == null)
      {
        var tnmsg = "Forest tree `{0}` is not found".Args(tree);
        throw new ValidationException(tnmsg) { HttpStatusCode = 404, HttpStatusDescription = tnmsg };
      }

      return (tds as ICrudDataStore).NonNull(DATA_STORE_CLAUSE);
    }


    /// <summary>
    /// Extension which loads enumerable returned by query executed in tree data context
    /// </summary>
    public static ConfiguredTaskAwaitable<IEnumerable<TDoc>> TreeLoadEnumerableAsync<TDoc>(this IForestDataSource forestData,
                      TreePtr tree,
                      Query<TDoc> qry) where TDoc : Doc
      => forestData.GetCrudData(tree).LoadEnumerableAsync(qry).ConfigureAwait(false);


    /// <summary>
    /// Extension which loads a document returned by query executed in tree data context
    /// </summary>
    public static ConfiguredTaskAwaitable<TDoc> TreeLoadDocAsync<TDoc>(this IForestDataSource forestData,
                      TreePtr tree,
                      Query<TDoc> qry) where TDoc : Doc
      => forestData.GetCrudData(tree).LoadDocAsync(qry).ConfigureAwait(false);


    /// <summary>
    /// Extension which executes a query in tree data context and does not fetch any result
    /// </summary>
    public static ConfiguredTaskAwaitable<TDoc> TreeExecuteAsync<TDoc>(this IForestDataSource forestData,
                    TreePtr tree,
                    Query<TDoc> qry) where TDoc : Doc
      => forestData.treeExecuteAsync(tree, qry).ConfigureAwait(false);

    public static async Task<TDoc> treeExecuteAsync<TDoc>(this IForestDataSource forestData,
                    TreePtr tree,
                    Query<TDoc> qry) where TDoc : Doc
    {
      var doc = await forestData.GetCrudData(tree).ExecuteAsync(qry).ConfigureAwait(false);
      if (doc == null) return null;
      var result = doc.CastTo<TDoc>($"Query returned value of type `{doc.GetType().DisplayNameWithExpandedGenericArgs()}`");
      return result;
    }

  }
}
