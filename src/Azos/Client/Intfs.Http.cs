/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Net.Http;

using Azos.Apps;
using Azos.Collections;
using Azos.Instrumentation;

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
    Uri Uri { get; }
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
