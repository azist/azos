/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Serialization.JSON;

namespace Azos.Web
{
  public static partial class WebCallExtensions
  {
    /// <summary> Marker interface for traits applicable to making web calls</summary>
    public interface ICallerAspect { }

    /// <summary> Indicates that the caller is capable of providing DistributedCallFlow context </summary>
    public interface IDistributedCallFlowAspect : ICallerAspect
    {
      /// <summary> If non-empty provides header name to be used for sending DistributedCallFlow object, otherwise a default name is used</summary>
      string DistributedCallFlowHeader {  get; }

      /// <summary>
      /// Gets the distributed call flow or NULL if not present or not enabled
      /// </summary>
      DistributedCallFlow GetDistributedCallFlow();
    }

    /// <summary> Decorates caller which supply <see cref="JsonReadingOptions"/> which control payload processing </summary>
    public interface IJsonReadingOptionsAspect : ICallerAspect//AZ#909
    {
      /// <summary>
      /// Gets the options object or NULL which is treated as an absence of specific options so the system can use the defaults
      /// </summary>
      JsonReadingOptions GetJsonReadingOptions(bool needTypeHints);
    }

    /// <summary>
    /// Designates the caller as authentication provider for remote impersonation - provides
    /// header name and header value if caller impersonation is used
    /// </summary>
    public interface IAuthImpersonationAspect : ICallerAspect
    {
      /// <summary>
      /// When non empty provides header name used for impersonation, otherwise standard HTTP `Authorization` header is used
      /// </summary>
      string AuthImpersonationHeader {  get; }

      /// <summary>
      /// Gets auth header value if impersonation is used or null.
      /// Optional functor to get identity context of the caller
      /// </summary>
      Task<string> GetAuthImpersonationHeaderAsync(Func<object> fGetIdentityContext);
    }

    /// <summary>
    /// Designates the caller as an entity reacting to request body errors detected by server.
    /// Typically an aspect will log the body that could not be processed.
    /// Body errors arise when client-supplied body is not processable e.g. when requested JSON can not be parsed
    /// </summary>
    public interface IRequestBodyErrorAspect : ICallerAspect
    {
      /// <summary>
      /// When non empty provides header name used for body error detection, otherwise request body errors are not detected
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
}
