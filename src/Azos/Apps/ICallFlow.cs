/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;

namespace Azos.Apps
{
  /// <summary>
  /// Describes call flow - an entity which represents a logical call, such as service flow.
  /// This can be used as a correlation context
  /// </summary>
  public interface ICallFlow
  {
    /// <summary>
    /// Gets unique CallFlow identifier, you can use it for things like log correlation id
    /// </summary>
    Guid ID { get; }

    /// <summary>
    /// Provides information about the remote caller address, such as a Glue node or IP address of an a remote Http client making this call
    /// </summary>
    string CallerAddress { get; }

    /// <summary>
    /// Provides short info/description about the caller application/agent/device
    /// </summary>
    string CallerAgent { get; }

    /// <summary>
    /// Describes the port/entry point through which the caller made the call, e.g. this may be set to a web Uri
    /// that initiated the call
    /// </summary>
    string CallerPort { get; }

    /// <summary>
    /// Gets/sets an ad-hoc named item values. You can use this to store arbitrary correlation values.
    /// The names use case-sensitive ordinal comparison.
    /// Note: these item values are NOT automatically sent along the call chain to the next service host.
    /// If a named item is not found, returns null
    /// </summary>
    object this[string key] { get; set; }

    /// <summary>
    /// Enumerates all items in the flow or returns an empty enumerable
    /// </summary>
    IEnumerable<KeyValuePair<string, object>> Items{ get; }
  }

}
