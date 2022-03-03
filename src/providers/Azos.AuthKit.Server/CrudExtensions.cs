/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Data.Access;

namespace Azos.AuthKit.Server
{
  public static class CrudExtensions
  {

    /// <summary>
    /// Extension which executes a query in IDP data context and does not fetch any result
    /// </summary>
    public static ConfiguredTaskAwaitable<TDoc> IdpExecuteAsync<TDoc>(this ICrudDataStore data, Query<TDoc> qry) where TDoc : Doc
      => data.idpExecuteAsync(qry).ConfigureAwait(false);

    private static async Task<TDoc> idpExecuteAsync<TDoc>(this ICrudDataStore data, Query<TDoc> qry) where TDoc : Doc
    {
      var doc = await data.ExecuteAsync(qry).ConfigureAwait(false);
      if (doc == null) return null;
      var result = doc.CastTo<TDoc>($"Query returned value of type `{doc.GetType().DisplayNameWithExpandedGenericArgs()}`");
      return result;
    }

  }
}
