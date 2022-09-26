/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Security.Data;
using Azos.Serialization.JSON;
using Azos.Wave.Mvc;

namespace Azos.Data.Access.Rpc.Server
{
  /// <summary>
  /// Provides API controller service for data RPC
  /// </summary>
  [NoCache]
  [ApiControllerDoc(
    BaseUri = "/data/rpc/handler",
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
    public async Task<object> PostReadRequest(ReadRequest request)
    {
      var resultData =  await ApplyFilterAsync(request).ConfigureAwait(false);
      var opts = new JsonWritingOptions
      {
        RowsetMetadata = true,
        RowsAsMap = false,
        Purpose = JsonSerializationPurpose.Marshalling,
        MapSkipNulls = false
      };

      if (request.RequestHeaders != null)
      {
        opts.RowsetMetadata = !request.RequestHeaders[StandardHeaders.NO_SCHEMA].AsBool(false);
        opts.RowsAsMap = request.RequestHeaders[StandardHeaders.ROWS_AS_MAP].AsBool(false);
        if (request.RequestHeaders[StandardHeaders.PRETTY].AsBool(false))
        {
          opts.SpaceSymbols = true;
          opts.IndentWidth = 2;
          opts.ObjectLineBreak = true;
          opts.MemberLineBreak = true;
        }
      }

      return new JsonResult(resultData, opts);
    }

    [ApiEndpointDoc(
      Title = "POST - Executes transaction request",
      Description = "Executes transaction request by posting request body into this endpoint",
      ResponseContent = "Json data",
      RequestBody = "JSON payload {request: TransactRequest}",
      Methods = new[] { "POST: posts TransactRequest body" },
      TypeSchemas = new[] { typeof(TransactRequest) })]
    [ActionOnPost(Name = "transaction"), AcceptsJson]
    [DataRpcPermission(DataRpcAccessLevel.Transact)]
    public async Task<object> PostTransactRequest(TransactRequest request) => await SaveNewAsync(request).ConfigureAwait(false);


  }
}
