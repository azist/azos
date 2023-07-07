/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Data;
using Azos.Security.FileGateway;
using Azos.Wave.Mvc;

namespace Azos.Sky.FileGateway.Server.Web
{
  [NoCache]
  [FileGatewayPermission]
  [ApiControllerDoc(
    BaseUri = "/file/gateway",
    Connection = "default/keep alive",
    Title = "File Gateway",
    Authentication = "Token/Default",
    Description = "Provides REST API for working with remote files",
    TypeSchemas = new[]{typeof(FileGatewayPermission) }
  )]
  [Release(ReleaseType.Preview, 2023, 07, 07, "Initial Release", Description = "First release of API")]
  public class Gateway : ApiProtocolController
  {
    [Inject]
    IFileGatewayLogic m_Logic;

    [ApiEndpointDoc(Title = "Systems",
                    Uri = "systems",
                    Description = "Gets list of systems",
                    Methods = new[] { "GET = gets list of systems" },
                    RequestHeaders = new[] { API_DOC_HDR_ACCEPT_JSON },
                    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE },
                    ResponseContent = "JSON enumerable of `{@Atom}`",
                    TypeSchemas = new[] { typeof(Atom) })]
    [ActionOnGet(Name = "systems"), AcceptsJson]
    public async Task<object> GetSystems() => GetLogicResult(await m_Logic.GetSystemsAsync().ConfigureAwait(false));

    [ApiEndpointDoc(Title = "Volumes",
                    Uri = "volumes",
                    Description = "Gets list of volumes per system",
                    Methods = new[] { "GET = gets list of volumes" },
                    RequestQueryParameters = new[] { "system = Atom system identifier" },
                    RequestHeaders = new[] { API_DOC_HDR_ACCEPT_JSON },
                    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE },
                    ResponseContent = "JSON enumerable of `{@Atom}`",
                    TypeSchemas = new[] { typeof(Atom) })]
    [ActionOnGet(Name = "systems"), AcceptsJson]
    public async Task<object> GetVolumes(Atom system) => GetLogicResult(await m_Logic.GetVolumesAsync(system).ConfigureAwait(false));


    [ApiEndpointDoc(Title = "Get Item List",
                    Uri = "item-list",
                    Description = "Gets a list of `{@ItemInfo}`",
                    Methods = new[] { "POST = {path: EntityId, recurse: bool}" },
                    RequestHeaders = new[] { API_DOC_HDR_ACCEPT_JSON },
                    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE },
                    RequestBody = "JSON map {path: EntityId, recurse: bool}`",
                    ResponseContent = "JSON enumerable of `{@ItemInfo}`",
                    TypeSchemas = new[] { typeof(EntityId), typeof(ItemInfo) })]
    [ActionOnPost(Name = "item-list"), AcceptsJson]
    public async Task<object> GetItemList(EntityId path, bool recurse = false)
      => GetLogicResult(await m_Logic.GetItemListAsync(path, recurse).ConfigureAwait(false));


    [ApiEndpointDoc(Title = "Get ItemInfo",
                    Uri = "item-info",
                    Description = "Gets `{@ItemInfo}` for the specified path",
                    Methods = new[] { "GET = gets item info by path" },
                    RequestQueryParameters = new[] { "path = EntityId path" },
                    RequestHeaders = new[] { API_DOC_HDR_ACCEPT_JSON },
                    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE },
                    ResponseContent = "JSON enumerable of `{@ItemInfo}`",
                    TypeSchemas = new[] { typeof(EntityId), typeof(ItemInfo) })]
    [ActionOnGet(Name = "systems"), AcceptsJson]
    public async Task<object> GetItemInfo(EntityId path) => GetLogicResult(await m_Logic.GetItemInfoAsync(path).ConfigureAwait(false));

  }
}
