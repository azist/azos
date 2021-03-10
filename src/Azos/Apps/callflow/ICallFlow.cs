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
  /// This can be used as a correlation context. Typically this object is used on a single server,
  /// for multi-server correlation tracking see <see cref="DistributedCallFlow" />
  /// Call flow exposes a `DirectorName` property which may contain a name of the primary
  /// flow originator, which can be a remote/distributed process.
  /// </summary>
  public interface ICallFlow
  {
    /// <summary>
    /// Gets a unique CallFlow identifier, you can use it for things like log correlation id
    /// </summary>
    Guid ID { get; }

    /// <summary>
    /// Returns a logical name of the primary flow originator/controller which GOVERNS the execution of various
    /// steps of the flow. This logical name may point to a distributed process/activity.
    /// Subordinate components may check this property to determine, among other things, the level of their instrumentation
    /// </summary>
    string DirectorName { get; }

    /// <summary>
    /// Sets director name explicitly.
    /// This is a logical name of the primary flow originator/controller which governs the execution of various
    /// steps of the flow. This logical name may point to a distributed process/activity.
    /// Subordinate components may check this property to determine, among other things, the level of their instrumentation
    /// </summary>
    void SetDirectorName(string name);

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
