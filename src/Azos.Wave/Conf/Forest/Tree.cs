using System;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Conf.Forest;
using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Business;
using Azos.Security.ConfigForest;
using Azos.Wave.Mvc;

namespace Azos.Conf.Forest.Server
{
  /// <summary>
  /// Provides API controller service for configuration Tree management
  /// </summary>
  [NoCache]
  [ApiControllerDoc(
    BaseUri = "/conf/forest/tree",
    Title = "Config Tree management",
    Description = @"Provides API controller service for configuration Tree management",
    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE }
  )]
  //[TreePermission(TreeAccessLevel.Read)] // TODO: determine if there needs to be another permission? ForestPermission???
  public sealed class Tree : ApiProtocolController
  {
    public const string ACT_VERSION = "version";
    public const string ACT_NODE = "node";
    public const string ACT_TREE_LIST = "tree-list";
    public const string ACT_NODE_LIST = "node-list";

    [Inject] IForestSetupLogic m_Logic;

    [ApiEndpointDoc(
      Title = "GET - Retrieves a list of trees",
      Description = "Gets a list of tree names of a specified forest as of the specified timestamp.",
      DocAnchor = "### /conf/forest/tree-list/ GET",
      RequestQueryParameters = new[]{
            "forest=Forest Atom of the node to retrieve"},
      ResponseContent = "Http 200 / JSON representation of {OK: true, data: [Atom]} or Http 404 {OK: false, data: null}",
      Methods = new[] { "GET: list of [Atom]" },
      TypeSchemas = new[]{
            typeof(Atom)
      })]
    //[TreePermission(TreeAccessLevel.View)]  // TODO: determine if there needs to be another permission? ForestPermission???
    [ActionOnGet(Name = ACT_TREE_LIST)]
    public async Task<object> GetTreeListAsync(Atom forest)
      => GetLogicResult(await m_Logic.GetTreeListAsync(forest).ConfigureAwait(false));


    [ApiEndpointDoc(
      Title = "GET - Retrieves a list of child nodes for the specified tree node",
      Description = "Retrieves a list of child nodes headers (node info without config content) for the specified tree node as of the specified timestamp.",
      DocAnchor = "### /conf/forest/node-list/ GET",
      RequestQueryParameters = new[]{
        "idparent=EntytyId of a parent node to retrieve child nodes for",
        "asofutc=Nullable timestamp as of which to retrieve the list. Null denotes UTC now (default)"},
      ResponseContent = "Http 200 / JSON representation of {OK: true, data: [TreeNodeHeader]} or Http 404 {OK: false, data: null}",
      Methods = new[] { "GET: list of [TreeNodeHeader]" },
      TypeSchemas = new[]{
        typeof(TreeNodeHeader)
      })]
    //[TreePermission(TreeAccessLevel.View)]  // TODO: determine if there needs to be another permission? ForestPermission???
    [ActionOnGet(Name = ACT_NODE_LIST)]
    public async Task<object> NodeListGet(EntityId idparent, DateTime? asofutc = null, bool nocache = false)
      => GetLogicResult(await m_Logic.GetChildNodeListAsync(idparent, asofutc, nocache ? CacheParams.NoCache : (ICacheParams)null).ConfigureAwait(false));



    //////[ApiEndpointDoc(
    //////  Title = "GET - Retrieve Node Version list",
    //////  Description = "Retrieves the list of all node versions. ",
    //////  DocAnchor = "### /corporate/hierarchy/version-list/ GET",
    //////  RequestQueryParameters = new[]{
    //////        "id=EntityId of the node to retrieve"},
    //////  ResponseContent = "Http 200 / JSON representation of {OK: true, data: [VersionLogBlock]} or Http 404 {OK: false, data: null}",
    //////  Methods = new[] { "GET: List of versions of the specified entity" },
    //////  TypeSchemas = new[]{
    //////        typeof(VersionInfo)
    //////  })]
    //////[CorporatePermission(CorporateAccessLevel.View)]
    //////[ActionOnGet(Name = ACT_VERSION_LIST)]
    //////public async Task<object> NodeVersionsGet(EntityId id)
    //////  => GetLogicResult(await m_Logic.GetNodeVersionListAsync(id).ConfigureAwait(false));


    [ApiEndpointDoc(
      Title = "GET - Retrieves a specific Tree Node Info version.",
      Description = "Retrieves tree node information of the specified version. " +
      "Note: you must use 'gver' address schema:  `region.gver@geo::0:0:345`",
      DocAnchor = "### /conf/forest/tree/version/ GET",
      RequestQueryParameters = new[]{ "id=EntityId of the node to retrieve" },
      ResponseContent = "Http 200 / JSON representation of {OK: true, data: [TreeNodeInfo]} or Http 404 {OK: false, data: null}",
      Methods = new[] { "GET: TreeNodeInfo of the specified version" },
      TypeSchemas = new[]{ typeof(TreeNodeInfo) })]
    //[TreePermission(TreeAccessLevel.View)]  // TODO: determine if there needs to be another permission? ForestPermission???
    [ActionOnGet(Name = ACT_VERSION)]
    public async Task<object> GetNodeInfoVersionAsync(EntityId id)
      => GetLogicResult(await m_Logic.GetNodeInfoVersionAsync(id).ConfigureAwait(false));



    [ApiEndpointDoc(
      Title = "GET - Retrieves the Tree Node Info",
      Description = "Retrieves a node of TreeNodeInfo by its id as of certain point in time" +
      "This action returns a TreeNodeInfo object or null if such item is not found",
      DocAnchor = "### /conf/forest/tree/node/ GET",
      RequestQueryParameters = new[]{
            "id=EntityId of the node to retrieve",
            "asofutc=Nullable timestamp as of which to retrieve the info. Null denotes UTC now (default)",
            "nocache=Bool flag indicating whether to suppress the use of server cache and read from data store. False by default"},
      ResponseContent = "Http 200 / JSON representation of {OK: true, data: TreeNodeInfo} or Http 404 {OK: false, data: null}",
      Methods = new[] { "GET: Gets the TreeNodeInfo by its id as of certain point in time" },
      TypeSchemas = new[]{ typeof(TreeNodeInfo) })]
    [ActionOnGet(Name = ACT_NODE)]
    public async Task<object> GetNodeInfoAsync(EntityId id, DateTime? asofutc = null, bool nocache = false)
      => GetLogicResult(await m_Logic.GetNodeInfoAsync(id, asofutc, nocache ? CacheParams.NoCache : (ICacheParams)null).ConfigureAwait(false));

  }
}
