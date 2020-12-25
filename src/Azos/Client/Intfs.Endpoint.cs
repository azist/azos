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
  /// Represents an abstraction of a remote service endpoint.
  /// Endpoints provide a connection point for services.
  /// Each endpoint represents a specific connection type via Binding(protocol)
  /// </summary>
  public interface IEndpoint : IApplicationComponent, IComponentDescription
  {
    /// <summary>
    /// Returns service which this endpoint represents
    /// </summary>
    IService Service { get; }

    /// <summary>
    /// Provides endpoint-level aspects which override by name the ones from service
    /// </summary>
    IOrderedRegistry<IAspect> Aspects { get; }

    /// <summary>
    /// Provides logical network name which this endpoint services, e.g. "noc","internoc","pub" etc.
    /// Depending on implementation, the actual physical remote endpoint address is calculated based on
    /// logical RemoteAddress, logical Network, binding/protocol, and contract - a logical "sub-service"/"port".
    /// </summary>
    Atom Network { get; }

    /// <summary>
    /// Provides logical binding name for this endpoint, for example "https". Bindings are protocols/connection methods supported.
    /// A typical REST-full system typically uses http/s bindings.
    /// </summary>
    Atom Binding { get; }

    /// <summary>
    /// Provides remote address routing/logical host/partition name which is used to match the callers address.
    /// For Sky apps this is a metabase host name (regional path) of the target server which provides the service.
    /// Some provider may implement pattern matching on remote addresses, for example `Primary*` would match
    /// multiple requested logical addressed (e.g. `Primary1, Primary2`) assigned to this endpoint
    /// </summary>
    string RemoteAddress { get; }

    /// <summary>
    /// Provides logical contract name for the functionality which this service endpoint covers.
    /// Contracts allow to provide more fine-grained assignment of different endpoints serving of otherwise the same logical address.
    /// For Http bindings this typically contains URI root path, such as "/user/admin".
    /// Some providers may implement pattern matching on contract names.
    /// </summary>
    string Contract {  get; }

    /// <summary>
    /// Groups endpoints by logical shard. Shard numbers are positive consecutive integers starting from 0 (e.g. 0,1,2,3...)
    /// If sharding is not used then all endpoints are set to the same shard of 0.
    /// Before service calls are made, the system takes "shardKey" and tries to find the partition based on sharding object,
    /// this way the load may be parallelized in "strands" of execution
    /// </summary>
    int Shard { get; }

    /// <summary>
    /// Relative order of endpoint per shard.
    /// Endpoints are tried in ascending order e.g. 0=Primary, 1=Secondary etc...
    /// When calculating the destination endpoint, the system uses RemoteAddress (and possibly other parameters such as current QOS/statistics) first
    /// (for the appropriate binding/contract), then shard, then ShardOrder within the shard, thus you may designate endpoints as
    /// primary/secondary/tertiary etc.. using this parameter
    /// </summary>
    int ShardOrder { get; }

    /// <summary>
    /// When set to a value &gt; 0 imposes a call timeout expressed in milliseconds
    /// </summary>
    int TimeoutMs { get; }

    /// <summary>
    /// When set, provides timestamp when circuit breaker has tripped on this endpoint.
    /// If this property is not null then the endpoint is in the "tripped" state and should not
    /// be tried for a call (until it auto resets - which is up to the implementation).
    /// </summary>
    DateTime? CircuitBreakerTimeStampUtc{ get; }

    /// <summary>
    /// When set, provides timestamp when this endpoint was brought offline.
    /// The difference from CircuitBreaker - offline/online endpoints are controlled manually (via code),
    /// the offline endpoint does NOT auto-reset unlike circuit breaker does
    /// </summary>
    DateTime? OfflineTimeStampUtc { get; }


    /// <summary>
    /// Returns true when endpoint was not purposely put offline and circuit breaker has not tripped
    /// </summary>
    bool IsAvailable{  get;}

    /// <summary>
    /// Returns a short status message of the endpoint, e.g. "Offline until Sun")
    /// </summary>
    string StatusMsg {  get; }
  }

  /// <summary>
  /// Classifies types of call errors, such as the ones that are caused by ServiceLogic and would NOT be retried vs
  /// errors related to MakingCall which indicate that call physically failed and must be re-tried.
  /// An example of MakingCall reason: TcpException,ProtocolException,TimeoutException etc... vs. ServiceLogic reasons:
  ///  BadRequestData, ValidationError, 404 Not Found, 403 Security etc...
  /// </summary>
  public enum CallErrorClass
  {
    MakingCall,
    ServiceLogic
  }

  public interface IEndpointImplementation : IEndpoint, IDisposable
  {
    /// <summary>
    /// Provides endpoint-level aspects which override by name the ones from service
    /// </summary>
    new OrderedRegistry<IAspect> Aspects { get; }


    /// <summary>
    /// Notifies endpoint of call success. This typically used to update call statistics
    /// </summary>
    void NotifyCallSuccess(ITransport transport);


    /// <summary>
    /// Notifies circuit breaker of error. The breaker may trip if error threshold
    /// is reached (as defined by endpoint/config).
    /// Returns true when the cause is deterministic connection/service CALL problem (WebException/Timeout) and it influenced the
    /// breaker state machine vs. business exception which is logical error (e.g. "Bad Request", "Access denied" etc.) which should not trip the
    /// breaker because it is a deterministic failure which does not indicate problems with network/server
    /// </summary>
    CallErrorClass NotifyCallError(ITransport transport, Exception error);

    /// <summary>
    /// Resets circuit breaker returning true if endpoint circuit was reset.
    /// False is returned if circuit breaker could not be reset (e.g. remote endpoint is still disabled)
    /// </summary>
    bool TryResetCircuitBreaker(string statusMessage);

    /// <summary>
    /// Puts endpoint online
    /// </summary>
    void PutOnline(string statusMsg);

    /// <summary>
    /// Puts endpoint offline
    /// </summary>
    void PutOffline(string statusMsg);
  }
}
