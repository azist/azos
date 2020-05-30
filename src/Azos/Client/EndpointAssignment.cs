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
    public EndpointAssignment(IEndpoint ep, Atom net, Atom binding, string addr, string contract)
    {
      Endpoint = ep;
      Network = net;
      Binding = binding;
      RemoteAddress = addr;
      Contract = contract;
    }

    public readonly IEndpoint Endpoint;
    public readonly Atom Network;
    public readonly Atom Binding;
    public readonly string RemoteAddress;
    public readonly string Contract;

    public override int GetHashCode() => (Endpoint != null ? Endpoint.GetHashCode() : 0) ^ (RemoteAddress != null ? RemoteAddress.GetHashCode() : 0);
    public override bool Equals(object obj) => obj is EndpointAssignment epa ? this.Equals(epa) : false;
    public bool Equals(EndpointAssignment other)
     => this.RemoteAddress == other.RemoteAddress &&
        this.Endpoint == other.Endpoint &&
        this.Contract == other.Contract &&
        this.Binding == other.Binding &&
        this.Network == other.Network;

    public static bool operator ==(EndpointAssignment a, EndpointAssignment b) => a.Equals(b);
    public static bool operator !=(EndpointAssignment a, EndpointAssignment b) => !a.Equals(b);
  }
}
