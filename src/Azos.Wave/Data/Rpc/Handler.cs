/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Apps.Injection;
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
    [ApiEndpointDoc(
      Title = "POST - Executes read request",
      Description = "Executes read request by posting request body into this endpoint",
      ResponseContent = "Json data",
      RequestBody = "JSON payload {request: ReadRequest}",
      Methods = new[] { "POST: posts ReadRequest body" },
      TypeSchemas = new[]{ typeof(ReadRequest)})]
    [ActionOnPost(Name = "reader"), AcceptsJson]
    public async Task<object> PostReadRequest(ReadRequest request) => await ApplyFilterAsync(request).ConfigureAwait(false);

    [ApiEndpointDoc(
      Title = "POST - Executes transaction request",
      Description = "Executes transaction request by posting request body into this endpoint",
      ResponseContent = "Json data",
      RequestBody = "JSON payload {request: TransactRequest}",
      Methods = new[] { "POST: posts TransactRequest body" },
      TypeSchemas = new[] { typeof(TransactRequest) })]
    [ActionOnPost(Name = "transaction"), AcceptsJson]
    public async Task<object> PostTransactRequest(TransactRequest request) => await SaveNewAsync(request).ConfigureAwait(false);


  }
}
