/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Azos.Client
{
  /// <summary>
  /// Provides extension methods for making service calls
  /// </summary>
  public static class ServiceCallExtensions
  {

    public static async Task<TResult> CallWithRetry<TResult>(this IService service, string network, string binding, string remoteAddress, string contract, object shardKey, Func<EndpointAssignment, ITransport, Task<TResult>> body)
    {
      body.NonNull(nameof(body));
      var endpoints = service.NonNull(nameof(service)).GetEndpointsForCall(remoteAddress, contract, shardKey, network, binding);

      foreach(var ep in endpoints)
      {
        var transport = service.AcquireTransport(ep);
        try
        {
          var result = await body(ep, transport);  //todo detect Offline/Circuit breaker trip; who trips circuit breaker?
          return result;
        }
        finally
        {
          service.ReleaseTransport(transport);
        }
      }

      throw new ClientException("Call eventually failed; {0} endpoints tried");//todo LOG etc...
    }

    public static async Task<TResult> HttpCallWithRetry<TResult>(this IHttpService service, string network, string binding, string remoteAddress, string contract, object shardKey, Func<EndpointAssignment, HttpClient, Task<TResult>> body)
    {
      body.NonNull(nameof(body));
      var endpoints = service.NonNull(nameof(service)).GetEndpointsForCall(remoteAddress, contract, shardKey, network, binding);

      foreach (var ep in endpoints)
      {
        var transport = service.AcquireTransport(ep);
        var http = (transport as IHttpTransport).NonNull("Implementation error: cast to IHttpTransport");
        try
        {
          var result = await body(ep, http.Client);  //todo detect Offline/Circuit breaker trip; who trips circuit breaker?
          return result;
        }
        finally
        {
          service.ReleaseTransport(transport);
        }
      }

      throw new ClientException("Call eventually failed; {0} endpoints tried");//todo LOG etc...
    }
  }
}
