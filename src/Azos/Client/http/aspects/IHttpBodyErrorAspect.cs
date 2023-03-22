/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Azos.Serialization.JSON;

namespace Azos.Client
{
  /// <summary>
  /// Reacts to HTTP request body errors detected by server.
  /// WAVE server uses a special response header `wv-body-error` by default
  /// which communicates a problem back to the calling client.
  /// A typical use case is to log the client body which was sent to server but the server was
  /// not able to parse it, responding with 400 and additional details in the aforementioned header
  /// </summary>
  public interface IHttpBodyErrorAspect : IAspect
  {
    /// <summary>
    /// The name of the header to interpret as the one containing JSON error
    /// details supplied by the server. Null = turn handling off
    /// </summary>
    string BodyErrorHeader { get; }

    /// <summary>
    /// Called after client receives a named header signaling a presence of body processing problem.
    /// The implementor typically captures the original requested object by logging its contents elsewhere
    /// </summary>
    /// <param name="uri">Originally called URI</param>
    /// <param name="method">Original HTTP method</param>
    /// <param name="body">The original body object that was supplied by client caller and caused server error</param>
    /// <param name="contentType">The original content type passed by the caller</param>
    /// <param name="options">The original JsonWritingOptions supplied by the caller</param>
    /// <param name="request">Request sent to server</param>
    /// <param name="response">Response gotten from server</param>
    /// <param name="isSuccess">True if server responded with 200</param>
    /// <param name="rawResponseContent">Fetched raw response content (such as error) or null</param>
    /// <param name="bodyErrorValues">Collection of values for header named by `BodyErrorHeader`</param>
    Task ProcessBodyErrorAsync(string uri,
                      HttpMethod method,
                      object body,
                      string contentType,
                      JsonWritingOptions options,
                      HttpRequestMessage request,
                      HttpResponseMessage response,
                      bool isSuccess,
                      string rawResponseContent,
                      IEnumerable<string> bodyErrorValues);

  }
}
