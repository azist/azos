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
  public static class HttpCallExtensions
  {
    public static async Task<TResult> CallWithRetry<TResult>(this IHttpService service, string network, string binding, string remoteAddress, string contract, object shardKey, Func<IHttpTransport, Task<TResult>> body)
    {
      body.NonNull(nameof(body));
      var assignments = service.NonNull(nameof(service))
                               .GetEndpointsForCall(remoteAddress, contract, shardKey, network, binding);

      var tries = 1;
      foreach(var assigned in assignments)
      {
        if (!assigned.Endpoint.IsAvailable) continue;//offline or Circuit tripped  todo:  instrument

        var ep = assigned.Endpoint as IEndpointImplementation;

        var transport = service.AcquireTransport(assigned);
        try
        {
          var http = (transport as IHttpTransport).NonNull("Implementation error: cast to IHttpTransport");
          var result = await body(http);  //todo detect Offline/Circuit breaker trip; who trips circuit breaker?
          return result;
        }
        catch(Exception error)
        {
          if (error is CallGuardException) throw;

          var isServiceCallError = ep.NotifyCircuitBreakerError(error);
          //todo instrument

          if (!isServiceCallError) throw;
        }
        finally
        {
          service.ReleaseTransport(transport);
        }

        tries++;
      }//foreach

      throw new ClientException("Call eventually failed; {0} endpoints tried".Args(tries));//todo LOG etc...
    }
  }
}
