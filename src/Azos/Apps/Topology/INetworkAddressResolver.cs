/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Apps.Topology
{
  /// <summary>
  /// Resolves logical network addresses, such as SKY host names into lower-level
  /// servicing substrate addresses which depend on binding, e.g. a URI for Http service
  /// </summary>
  interface INetworkAddressResolver : IApplicationComponent
  {
    /// <summary>
    /// Resolves a logical network addresses, such as SKY host names into lower-level
    /// servicing substrate addresses which depend on binding, e.g. a URI for Http service
    /// </summary>
    /// <param name="net">
    /// Logical network name. The address resolution may take the network in consideration while calculating the destination address,
    /// for example, you may put all of the database-related traffic on a different network `data`
    /// </param>
    /// <param name="binding">
    /// A protocol/binding method to be used for intended connection. The resolver may elect the addressing scheme
    /// depending on this parameter, e.g. either a host name, IPv4 or IPv6 address may be returned
    /// </param>
    /// <param name="address">A logical address to resolve, such as SKY host name e.g. `\w\us\cle\db\h1`</param>
    /// <param name="fromHost">
    /// An optional logical address as-of which to resolve the physical address.
    /// If null (the default), the local host is assumed.
    /// This host may influence the addressing subnet used to connect to destination
    /// </param>
    /// <returns>Resolved address or null if resolution could not be made</returns>
    string Resolve(Atom net, Atom binding, string address, string fromHost = null);
  }
}
