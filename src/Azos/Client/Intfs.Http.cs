/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Net.Http;

namespace Azos.Client
{
  /// <summary>
  /// Marks services that have HTTP semantics - the ones based on HttpClient-like operations, REST, RPC, JSON etc...
  /// </summary>
  public interface IHttpService : IService
  {

  }


  /// <summary>
  /// Marks endpoints that have HTTP semantics - the ones based on HttpClient-like operations, REST, RPC, JSON etc...
  /// </summary>
  public interface IHttpEndpoint : IEndpoint
  {
    /// <summary>
    /// Uri of the Http call destination
    /// </summary>
    Uri Uri { get; }

    /// <summary>
    /// When true, enables attaching an HTTP header containing DistributedCallFlow object (if available) to outgoing calls
    /// </summary>
    bool EnableDistributedCallFlow {  get; }

    /// <summary>
    /// When set, overrides the HTTP_HDR_DEFAULT_CALL_FLOW header name
    /// </summary>
    string DistributedCallFlowHeader { get; }
  }


  /// <summary>
  /// Marks transports that have HTTP semantics - the ones based on HttpClient-like operations, REST, RPC, JSON etc...
  /// </summary>
  public interface IHttpTransport : ITransport
  {
    /// <summary>
    /// Returns HttpClient used for making calls
    /// </summary>
    HttpClient Client { get; }
  }
}
