/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Azos.Data.Access.MongoDb.Client
{
  /// <summary>
  /// Provides extension methods for making service calls to Mongo db
  /// </summary>
  public static class MongoDbCallExtensions
  {
    /// <summary>
    /// Orchestrates an Mongo/s call to a remote service pointed to by a logical `remoteAddress`.
    /// This algorithm provides sharding, fail-over and circuit breaker functionality built-in.
    /// An actual call is performed in a passed-in call body functor.
    /// </summary>
    /// <typeparam name="TResult">The resulting type of the call, as obtained from call body</typeparam>
    /// <param name="service">A service to run against</param>
    /// <param name="remoteAddress">Logical address of the remote service, not to be confused with physical. Physical addressed are resolved from logical addresses</param>
    /// <param name="contract">Logical contract name, such as * for any contract</param>
    /// <param name="shardKey">A key value used for sharding (such as GDID, GUID) traffic</param>
    /// <param name="body">Call body functor. May not be null</param>
    /// <param name="cancellation">Optional CancellationToken</param>
    /// <param name="network">Logical network name used for endpoint address resolution, e.g. this is used to segregate traffic by physical channels</param>
    /// <param name="binding">Logical binding (sub-protocol) name (e.g. json/bix)</param>
    /// <returns>TResult call result or throws `ClientException` if call eventually failed after all failovers tried</returns>
    public static Task<TResult> Call<TResult>(this IMongoDbService service,
                                                    string remoteAddress,
                                                    string contract,
                                                    object shardKey,
                                                    Func<IMongoDbTransport, CancellationToken?, Task<TResult>> body,
                                                    CancellationToken? cancellation = null,
                                                    Atom? network = null,
                                                    Atom? binding = null)
    {
      body.NonNull(nameof(body));
      var assignments = service.NonNull(nameof(service))
                               .GetEndpointsForCall(remoteAddress, contract, shardKey, network, binding);

      return assignments.Call(body, cancellation);
    }

    public static async Task<TResult> Call<TResult>(this IEnumerable<EndpointAssignment> assignments,
                                                    Func<IMongoDbTransport, CancellationToken?, Task<TResult>> body,
                                                    CancellationToken? cancellation = null)
    {
      body.NonNull(nameof(body));

      var first = assignments.NonNull(nameof(assignments)).FirstOrDefault();
      if (!first.IsAssigned) throw new ClientException(StringConsts.MONGO_CLIENT_CALL_ASSIGMENT_ERROR.Args("No assignments provided"));

      var service = first.Endpoint.Service as IHttpService;
      if (service==null) throw new ClientException(StringConsts.MONGO_CLIENT_CALL_ASSIGMENT_ERROR.Args("Wrong service type assignments"));

      if (!assignments.All(a => a.Endpoint.Service == service &&
                                a.MappedContract == first.MappedContract &&
                                a.MappedRemoteAddress == first.MappedRemoteAddress &&
                                a.Endpoint.Network == first.Endpoint.Network &&
                                a.Endpoint.Binding == first.Endpoint.Binding))
       throw new ClientException(StringConsts.MONGO_CLIENT_CALL_ASSIGMENT_ERROR.Args("Inconsistent endpoint assignments"));

      var tries = 0;
      List<Exception> errors = null;
      foreach(var assigned in assignments)
      {
        if (!assigned.Endpoint.IsAvailable) continue;//offline or Circuit tripped  todo:  instrument

        tries++;

        var ep = assigned.Endpoint as IEndpointImplementation;

        var transport = service.AcquireTransport(assigned);
        try
        {
          var tx = (transport as IMongoDbTransport).NonNull("Implementation error: cast to IMongoDbTransport");
          var result = await body(tx, cancellation);
          CallGuardException.Protect(ep, _ => _.NotifyCallSuccess(transport));
          return result;
        }
        catch(Exception error)
        {
          //Implementation error
          if (error is CallGuardException) throw;

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
      }//foreach

      throw new ClientException(StringConsts.MONGO_CLIENT_CALL_FAILED.Args(service.GetType().Name, first.MappedRemoteAddress.TakeLastChars(32, "..."), tries),
                                errors != null ? new AggregateException(errors) :
                                                 new AggregateException("No inner errors"));//todo LOG etc...
    }
  }
}
