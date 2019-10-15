using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Collections;
using Azos.Instrumentation;

namespace Azos.Client
{
  /// <summary>
  /// Represents a uniquely named service instance
  /// </summary>
  public interface IService : IApplicationComponent, INamed
  {
    IEnumerable<IEndpoint> Endpoints {  get; }

    /// <summary>
    /// Gets default network name for this service if any, e.g. "internoc"
    /// </summary>
    string DefaultNetwork { get; }

    /// <summary>
    /// Gets default binding name for this service, e.g. "https"
    /// </summary>
    string DefaultBinding {  get; }

    /// <summary>
    /// When &gt; 0 imposes a call timeout expressed in milliseconds, otherwise the system uses hard-coded timeout (e.g. 10 sec)
    /// </summary>
    int DefaultTimeoutMs { get; }

    /// <summary>
    /// Returns endpoints which should be re-tried subsequently on failure.
    /// The endpoints are returned in the sequence which depend on implementation.
    /// Typically the sequence is based on network routing efficiency and least/loaded resources.
    /// The optional shardingKey parameter may be passed for multi-sharding scenarios.
    /// </summary>
    /// <param name="shardKey">Optional sharding parameter. The system will direct the call to the appropriate shard in the service partition if it is used</param>
    /// <returns>Endpoints which should be (re)tried in the order of enumeration</returns>
    IEnumerable<IEndpoint> GetEndpointsForCall(string contract, object shardKey = null, string network = null, string binding = null);
  }


  public interface IServiceImplementation : IService, IDisposable, IInstrumentable
  {
    ITransportImplementation AcquireTransport(IEndpoint endpoint);
    void ReleaseTransport(ITransportImplementation transport);
  }

  public interface ITransport
  {
    /// <summary>
    /// Returns endpoint which this transport connects to
    /// </summary>
    IEndpoint Endpoint { get; }
  }

  public interface ITransportImplementation : ITransport, IDisposable
  {
  }


  /// <summary>
  /// Represents an abstraction of a remote service endpoint.
  /// Each endpoint is uniquely identified using its mnemonic name
  /// </summary>
  public interface IEndpoint
  {
    /// <summary>
    /// Returns service which this endpoint represents
    /// </summary>
    IService Service { get; }

    /// <summary>
    /// Provides routing/logical host/partition name which is used to match the callers address.
    /// For Sky apps this is a metabase host path of the target server which provides the service
    /// </summary>
    string Route { get; }

    /// <summary>
    /// Provides logical network name which this endpoint services, e.g. "noc","internoc","pub" etc.
    /// </summary>
    string Network { get; }

    /// <summary>
    /// Provides logical binding name for this endpoint, for example "https"
    /// </summary>
    string Binding { get; }

    /// <summary>
    /// Provides logical contract name for the functionality which this service endpoint covers.
    /// For Http this is typically a HttpClient configured with default headers and protocol handlers
    /// </summary>
    string Contract {  get; }

    /// <summary>
    /// Groups endpoints by logical shard. If sharding is not used then all endpoints have the same shard of 0
    /// </summary>
    int Shard { get; }

    /// <summary>
    /// Relative order of endpoint per shard. Endpoints are tried in ascending order e.g. 0=Primary, 1=Secondary etc...
    /// </summary>
    int ShardOrder { get; }

    /// <summary>
    /// When &gt; 0 imposes a call timeout expressed in milliseconds
    /// </summary>
    int TimeoutMs { get; }

    /// <summary>
    /// When set, provides timestamp when circuit breaker has tripped on this endpoint.
    /// If this property is not null then the endpoint is in the "tripped" state and should not
    /// be tried for a call (until it auto resets - which is up to implementation).
    /// </summary>
    DateTime? CircuitBreakerTimeStampUtc{ get; }
  }

  public interface IEndpointImplementation : IEndpoint, IDisposable
  {
  }
}
