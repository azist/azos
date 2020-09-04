/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Client
{
  /// <summary>
  /// Assigns a specific endpoint, network, binding, remote address, and contract.
  /// This structure gets produced by Service when resolving/routing call request parameters
  /// </summary>
  public struct EndpointAssignment : IEquatable<EndpointAssignment>
  {
    public EndpointAssignment(IEndpoint ep, string mappedAddr, string mappedContract)
    {
      Endpoint = ep.NonNull(nameof(ep));
      MappedRemoteAddress = mappedAddr;
      MappedContract = mappedContract;
    }

    public bool IsAssigned => Endpoint != null;

    /// <summary>
    /// An actual endpoint assigned to handle the call. The effective address, and contract are taken from the endpoint,
    /// whereas the rest of the field in this structure represent the requested value that got mapped/assigned into the real endpoint
    /// </summary>
    public readonly IEndpoint Endpoint;

    /// <summary>
    /// Returns the originally requested RemoteAddress that got mapped/assigned to this Endpoint
    /// </summary>
    public readonly string MappedRemoteAddress;

    /// <summary>
    /// Returns the originally requested Contract that got mapped/assigned to this Endpoint
    /// </summary>
    public readonly string MappedContract;

    public override string ToString() => "{0}(`{1}`->`{2}`)".Args((Endpoint?.GetType().Name).Default("[NONE]"), MappedRemoteAddress.Default("?"), (Endpoint?.RemoteAddress).Default("?"));

    public override int GetHashCode() => (Endpoint != null ? Endpoint.GetHashCode() : 0) ^ (MappedRemoteAddress != null ? MappedRemoteAddress.GetHashCode() : 0);
    public override bool Equals(object obj) => obj is EndpointAssignment epa ? this.Equals(epa) : false;
    public bool Equals(EndpointAssignment other)
     => this.Endpoint == other.Endpoint &&
        this.MappedRemoteAddress.EqualsOrdSenseCase(other.MappedRemoteAddress) &&
        this.MappedContract.EqualsOrdSenseCase(other.MappedContract);

    public static bool operator ==(EndpointAssignment a, EndpointAssignment b) => a.Equals(b);
    public static bool operator !=(EndpointAssignment a, EndpointAssignment b) => !a.Equals(b);
  }
}
