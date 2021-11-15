using System;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Conf.Forest;
using Azos.Data;
using Azos.Data.Business;
using Azos.Security.Permissions.Data;
using Azos.Wave.Mvc;

namespace Azos.Data.Access.Rpc.Server
{
  /// <summary>
  /// Provides API controller service for configuration Tree management
  /// </summary>
  [NoCache]
  [ApiControllerDoc(
    BaseUri = "/data/rpc",
    Title = "Data RPC server",
    Description = @"Serves Data RPC",
    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE }
  )]
  [DataRpcPermission(DataRpcAccessLevel.Read)]
  public sealed class Handler : ApiProtocolController
  {
    [Inject] IRpcHandler m_Logic;

    [ApiEndpointDoc(
      Title = "POST - Executes read request",
      Description = "Executes read request by posting request body into this endpoint",
      ResponseContent = "Json data",
      RequestBody = "JSON payload {request: RequestBody}",
      Methods = new[] { "POST: posts ReadRequest body" },
      TypeSchemas = new[]{ typeof(ReadRequest)})]
    [ActionOnPost(Name = "reader"), AcceptsJson]
    public async Task<object> PostReadRequest(ReadRequest request) => await ApplyFilterAsync(request).ConfigureAwait(false);

    [ApiEndpointDoc(
      Title = "POST - Executes read request",
      Description = "Executes read request by posting request body into this endpoint",
      ResponseContent = "Json data",
      RequestBody = "JSON payload {request: RequestBody}",
      Methods = new[] { "POST: posts ReadRequest body" },
      TypeSchemas = new[] { typeof(ReadRequest) })]
    [ActionOnPost(Name = "reader"), AcceptsJson]
    public async Task<object> PostTransactRequest(TransactRequest request) => await SaveNewAsync(request).ConfigureAwait(false);


  }
}
