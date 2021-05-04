/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Collections;
using Azos.Instrumentation;

namespace Azos.Client
{
  /// <summary>
  /// Represents a uniquely named service which provides services via its endpoints.
  /// A single service may serve more than one "Contract" - a logical sub-division of a service.
  /// For example, a "User" service may provide "List" and "Admin" contracts used for querying/listing users and adding/deleting users respectively.
  /// </summary>
  public interface IService : IApplicationComponent, INamed
  {
    /// <summary>
    /// Enumerates endpoints which provide this service. Depending on implementation this property may return
    /// all physical endpoints of higher-order regional endpoints for high-scale systems.
    /// Typically it is used with Http/s and returns actual endpoints which provide services which this instance
    /// represents
    /// </summary>
    IEnumerable<IEndpoint> Endpoints {  get; }

    /// <summary>
    /// Provides service-level aspects which cascade down on endpoints of this service
    /// </summary>
    IOrderedRegistry<IAspect> Aspects { get; }

    /// <summary>
    /// Gets default network name for this service if any, e.g. "internoc".
    /// Simple implementations typically do not use named logical networks, so this value is set to "default" or empty
    /// </summary>
    Atom DefaultNetwork { get; }

    /// <summary>
    /// Gets default binding name for this service, e.g. "https".
    /// </summary>
    Atom DefaultBinding {  get; }

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
    /// <param name="remoteAddress">
    ///   The remote service logical address, such as the regional host name for Sky applications.
    ///   The system resolves this address to physical address depending on binding and contract on the remote host
    /// </param>
    /// <param name="contract">Service contract name</param>
    /// <param name="shardKey">
    ///  Optional sharding parameter. The system will direct the call to the appropriate shard in the service partition if it is used.
    ///  You can use primitive values (such as integers/longs etc.) for sharding, as long as you do not change what value is used for
    ///  `shardKey` parameter, the call routing will remain deterministic
    /// </param>
    /// <param name="network">A name of the logical network to use for a call, or null to use the default network</param>
    /// <param name="binding">
    ///   The service binding to use, or null for default.
    ///   Bindings are connection technology/protocols (such as Http(s)/Glue/GRPC etc..) used to make the call
    /// </param>
    /// <returns>Endpoint(s) which should be (re)tried in the order of enumeration</returns>
    IEnumerable<EndpointAssignment> GetEndpointsForCall(string remoteAddress, string contract, ShardKey shardKey = default(ShardKey), Atom? network = null, Atom? binding = null);

    /// <summary>
    /// Returns endpoint set for all shards for a specific `remoteAddress/contract/network/binding`
    /// </summary>
    /// <param name="remoteAddress">
    ///   The remote service logical address, such as the regional host name for Sky applications.
    ///   The system resolves this address to physical address depending on binding and contract on the remote host
    /// </param>
    /// <param name="contract">Service contract name</param>
    /// <param name="network">A name of the logical network to use for a call, or null to use the default network</param>
    /// <param name="binding">
    ///   The service binding to use, or null for default.
    ///   Bindings are connection technology/protocols (such as Http(s)/Glue/GRPC etc..) used to make the call
    /// </param>
    /// <returns>
    /// An enumerable of enumerable of EndpointAssigments.
    /// A top level enumerable represents shards. Each shard is further represented by an enumerable of endpoint assignments which should be re-tried
    /// in case of failure in the order of their enumeration.
    /// Endpoint(s) which should be (re)tried in the order of enumeration
    /// </returns>
    IEnumerable<IEnumerable<EndpointAssignment>> GetEndpointsForAllShards(string remoteAddress, string contract, Atom? network = null, Atom? binding = null);

    /// <summary>
    /// Gets the physical transport used to make remote calls. Depending on implementation the system
    /// may return a pooled transport, re-use already acquired one (if transport supports multiplexing) etc. or
    /// allocate a new one. The call has to be paired with `ReleaseTransport(transport)`
    /// </summary>
    /// <param name="assignment">Endpoint to connect to</param>
    /// <param name="reserve">Pass true to reserve this transport for the caller. The caller must release the reserved transport</param>
    ITransportImplementation AcquireTransport(EndpointAssignment assignment, bool reserve = false);

    /// <summary>
    /// Releases the transport acquired by the AcquireTransport call
    /// </summary>
    void ReleaseTransport(ITransportImplementation transport);
  }


  /// <summary>
  /// Implements an IService, adding transport acquisition/release behavior
  /// </summary>
  public interface IServiceImplementation : IService, IDisposable, IInstrumentable
  {
    /// <summary>
    /// Provides service-level aspects which cascade down on endpoints of this service
    /// </summary>
    new OrderedRegistry<IAspect> Aspects {  get; }
  }
}
