/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
    Connection = "default/keep alive",
    Title = "Amorphous Data Library Server data store",
    Authentication = "Token/Default",
    Description = @"Stores data for Adlib server",
    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE },
    TypeSchemas = new[] { typeof(AdlibPermission) }
  )]
  [AdlibPermission(AdlibAccessLevel.Read)]
  public sealed class Store : ApiProtocolController
  {
    [Inject] IAdlibLogic m_Logic;

    [ApiEndpointDoc(Title = "Get spaces",
      Uri = "spaces",
      Description = "Returns a list of all known data spaces on the server",
      Methods = new[] { "GET = gets all spaces on the server" },
      RequestHeaders = new[] { API_DOC_HDR_ACCEPT_JSON },
      ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE },
      ResponseContent = "JSON result - {OK: true, data: IEnumerable<Atom>}`")]
    [ActionOnGet(Name = "spaces"), AcceptsJson]
    [AdlibPermission(AdlibAccessLevel.Read)]
    public async Task<object> GetSpaceNames() => GetLogicResult(await m_Logic.GetSpaceNamesAsync().ConfigureAwait(false));

    [ApiEndpointDoc(Title = "Get collections",
      Uri = "spaces",
      Description = "Returns a list of all known collections within the space on the server. Note: these are server collections that have any data in them",
      Methods = new[] { "GET = gets all collections in a `space` on the server" },
      RequestHeaders = new[] { API_DOC_HDR_ACCEPT_JSON },
      ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE },
      RequestQueryParameters = new[] {"space = string, space name"},
      ResponseContent = "JSON result - {OK: true, data: IEnumerable<Atom>}`")]
    [ActionOnGet(Name = "collections"), AcceptsJson]
    [AdlibPermission(AdlibAccessLevel.Read)]
    public async Task<object> GetCollectionNames(Atom space) => GetLogicResult(await m_Logic.GetCollectionNamesAsync(space).ConfigureAwait(false));

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

    [ApiEndpointDoc(
      Uri = "item",
      Title = "POST - Saves a single `Item`",
      Description = "Persists a new `Item` by calling Save and returning a JSON result",
      Methods = new[] { "POST: post Json `Item`" },
      RequestHeaders = new[] { API_DOC_HDR_ACCEPT_JSON },
      RequestBody = "Json representation of an item",
      ResponseContent = "JSON Api Change Result",
      TypeSchemas = new[] { typeof(Item), typeof(ChangeResult) })]
    [ActionOnPost(Name = "item"), AcceptsJson]
    [AdlibPermission(AdlibAccessLevel.Change)]
    public async Task<object> Save(Item item) => await SaveNewAsync(item).ConfigureAwait(false);

    [ApiEndpointDoc(
      Uri = "item",
      Title = "PUT - Updates a single `Item`",
      Description = "Persists an updated `Item` by calling Save and returning a JSON result",
      Methods = new[] { "PUT: Updates user account entity" },
      RequestHeaders = new[] { API_DOC_HDR_ACCEPT_JSON },
      RequestBody = "Json representation of an item",
      ResponseContent = "JSON Api Change Result",
      TypeSchemas = new[] { typeof(Item), typeof(ChangeResult) })]
    [ActionOnPut(Name = "item"), AcceptsJson]
    [AdlibPermission(AdlibAccessLevel.Change)]
    public async Task<object> Update(Item item) => await SaveEditAsync(item).ConfigureAwait(false);

    [ApiEndpointDoc(
     Title = "DELETE - Deletes a single `Item`",
     Description = "Deletes a single `Item` by its EntityId and returns an Api Change Result",
     Methods = new[] { "DELETE: Deletes an item" },
     RequestQueryParameters = new[]{ "id=EntityId of the item to delete" },
     ResponseContent = "JSON representation of {OK: bool, data: ChangeResult}",
     TypeSchemas = new[]{ typeof(ChangeResult) })]
    [ActionOnDelete(Name = "item"), AcceptsJson]
    [AdlibPermission(AdlibAccessLevel.Delete)]
    public async Task<object> Delete(EntityId id) => GetLogicChangeResult(await m_Logic.DeleteAsync(id).ConfigureAwait(false));
  }
}
