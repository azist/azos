/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Azos.Client
{
  /// <summary>
  /// Provides extension methods for making service calls
  /// </summary>
  public static class HttpCallExtensions
  {
    public static async Task<TResult> Call<TResult>(this IHttpService service,
                                                    string remoteAddress,
                                                    string contract,
                                                    object shardKey,
                                                    Func<IHttpTransport, CancellationToken?, Task<TResult>> body,
                                                    CancellationToken? cancellation = null,
                                                    Atom? network = null,
                                                    Atom? binding = null)
    {
      body.NonNull(nameof(body));
      var assignments = service.NonNull(nameof(service))
                               .GetEndpointsForCall(remoteAddress, contract, shardKey, network, binding);

      var tries = 1;
      List<Exception> errors = null;
      foreach(var assigned in assignments)
      {
        if (!assigned.Endpoint.IsAvailable) continue;//offline or Circuit tripped  todo:  instrument

        var ep = assigned.Endpoint as IEndpointImplementation;

        var transport = service.AcquireTransport(assigned);
        try
        {
          var http = (transport as IHttpTransport).NonNull("Implementation error: cast to IHttpTransport");
          var result = await body(http, cancellation);
          CallGuardException.Protect(ep, _ => _.NotifyCallSuccess(transport));
          return result;
        }
        catch(Exception error)
        {
          //Implementation error
          if (error is CallGuardException) throw;

          //TaskCanceledException gets thrown on simple timeout even when cancellation was NOT requested
          if (error is TaskCanceledException && cancellation.HasValue && cancellation.Value.IsCancellationRequested) throw;

          var errorClass = ep.NotifyCallError(transport, error);
          //todo instrument

          if (errorClass == CallErrorClass.ServiceLogic) throw;//throw logical errors

          if (errors==null) errors = new List<Exception>();
          errors.Add(error);
        }
        finally
        {
          service.ReleaseTransport(transport);
        }

        tries++;
      }//foreach

      throw new ClientException("Call eventually failed; {0} endpoints tried; See .InnerException".Args(tries),
                                errors != null ? new AggregateException(errors) :
                                                 new AggregateException("No inner errors"));//todo LOG etc...
    }
  }
}
