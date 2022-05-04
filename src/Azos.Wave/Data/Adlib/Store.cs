/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Security.Adlib;
using Azos.Wave.Mvc;

namespace Azos.Data.Adlib.Server
{
  /// <summary>
  /// Amorphous Data Library Server data store
  /// </summary>
  [NoCache]
  [ApiControllerDoc(
    BaseUri = "/adlib/store",
    Title = "Amorphous Data Library Server data store",
    Description = @"Stores data for Adlib server",
    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE }
  )]
  [AdlibPermission(AdlibAccessLevel.Read)]
  public sealed class Store : ApiProtocolController
  {
    [Inject] IAdlibLogic m_Logic;


    [ActionOnGet(Name = "spaces"), AcceptsJson]
    [AdlibPermission(AdlibAccessLevel.Read)]
    public async Task<object> GetSpaceNames() => GetLogicResult(await m_Logic.GetSpaceNamesAsync().ConfigureAwait(false));

    [ActionOnGet(Name = "collections"), AcceptsJson]
    [AdlibPermission(AdlibAccessLevel.Read)]
    public async Task<object> GetCollectionNames(string space) => GetLogicResult(await m_Logic.GetCollectionNamesAsync(space).ConfigureAwait(false));


    [ApiEndpointDoc(Title = "Filter",
                    Uri = "filter",
                    Description = "Queries Adlib server by applying a structured filter `{@ItemFilter}`",
                    Methods = new[] { "POST = post filter object for query execution" },
                    RequestHeaders = new[] { API_DOC_HDR_ACCEPT_JSON },
                    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE },
                    RequestBody = "JSON representation of `{@ItemFilter}`",
                    ResponseContent = "JSON filter result - enumerable of `{@Item}`",
                    TypeSchemas = new[] { typeof(Item) })]
    [ActionOnPost(Name = "filter"), AcceptsJson]
    [AdlibPermission(AdlibAccessLevel.Read)]
    public async Task<object> Filter(ItemFilter filter) => await ApplyFilterAsync(filter).ConfigureAwait(false);


    [ActionOnPost(Name = "item"), AcceptsJson]
    [AdlibPermission(AdlibAccessLevel.Change)]
    public async Task<object> Save(Item item) => await SaveNewAsync(item).ConfigureAwait(false);

    [ActionOnPut(Name = "item"), AcceptsJson]
    [AdlibPermission(AdlibAccessLevel.Change)]
    public async Task<object> Update(Item item) => await SaveEditAsync(item).ConfigureAwait(false);

    [ActionOnDelete(Name = "item"), AcceptsJson]
    [AdlibPermission(AdlibAccessLevel.Delete)]
    public async Task<object> Delete(EntityId id) => GetLogicChangeResult(await m_Logic.DeleteAsync(id).ConfigureAwait(false));
  }
}
