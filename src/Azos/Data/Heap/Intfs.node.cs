/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;
using System;
using System.Collections.Generic;

namespace Azos.Data.Heap
{
  /// <summary>
  /// Selects the suiting nodes relative to the specified host (such as local host)
  /// ordered by their relevance (such as proximity, network congestion etc.)
  /// </summary>
  public interface INodeSelector : IApplicationComponent
  {
    /// <summary>
    /// Area of the heap which defines nodes
    /// </summary>
    IArea Area { get; }

    /// <summary>
    /// Returns an ordered enumerable of nodes relative to the local calling (this) host.
    /// The returned nodes are ordered by their relevance
    /// </summary>
    IEnumerable<INode> ForLocal { get; }

    /// <summary>
    /// Returns an ordered enumerable of nodes relative to the specified host name.
    /// An empty/null host name denotes a local host.
    /// The returned nodes are ordered by their relevance
    /// </summary>
    IEnumerable<INode> RelativeTo(string hostName);
  }

  /// <summary>
  /// Provides node status information
  /// </summary>
  public interface INodeStatusInfo
  {
    /// <summary>
    /// Utc timestamp as of which the status was determined
    /// </summary>
    DateTime AsOfUtc { get; }

    /// <summary>
    /// Returns the version of type schema deployed on the node.
    /// If this value does not correspond to local value then auto-replication is paused until
    /// the values match
    /// </summary>
    ulong SchemaVersion { get; }

    /// <summary>
    /// Node status
    /// </summary>
    NodeStatus Status {  get; }

    /// <summary>
    /// Terse description of status(if any)
    /// </summary>
    string Description {  get; }
  }

  /// <summary>
  /// Represents a heap node logical process. Heap clients connect to heap nodes to get data
  /// using queries or direct object references, mutate data using Set() or Delete(). Internally,
  /// nodes host storage engines and perform inter-node replication and CRDT-style conflict resolution.
  /// A heap node services one and only one area.
  /// </summary>
  /// <remarks>
  /// Depending on implementation it may be possible to run multiple node types on the same physical host.
  /// A node is a logical process
  /// </remarks>
  public interface INode
  {
    /// <summary>
    /// Area of the heap
    /// </summary>
    IArea Area { get; }

    /// <summary>
    /// Globally-unique cluster node identifier. Every HeapObject instance gets Sys_VerNode stamp by the server
    /// </summary>
    Atom NodeId { get; }

    /// <summary>
    /// Returns the latest/current status of the node
    /// </summary>
    INodeStatusInfo CurrentStatus {  get; }

    /// <summary>
    /// Returns node status history which is typically used for admin purposes
    /// </summary>
    IEnumerable<INodeStatusInfo> StatusHistory {  get; }

    /// <summary>
    /// Node properties like whether a node supports data changes or a read-only node
    /// </summary>
    NodeFlags Flags { get; }

    /// <summary>
    /// Node client server logical address which gets translated into a
    /// physical address using <see cref="IArea.ServiceClient"/>
    /// </summary>
    string ServiceAddress {  get;}

    /// <summary>
    /// Node client server logical contract which influences endpoint binding
    /// during physical address resolution using <see cref="IArea.ServiceClient"/>
    /// </summary>
    string ServiceContract { get; }
  }

  /// <summary>
  /// Represents data heap server node
  /// </summary>
  public interface IServerNodeContext
  {
    /// <summary>
    /// Logical node
    /// </summary>
    INode Node     { get; }

    /// <summary>
    /// Cluster host name, such as sky regional catalog path e.g. `/world/us/east/cle/db/z1/lmed002.h`
    /// </summary>
    string HostName { get; }

    /// <summary>
    /// The high precision Universal Time Coordinate(d)
    /// </summary>
    DateTime UtcNow { get; }

    ///// <summary>
    ///// Returns the data partitioning handler which performs shard routing of heap data
    ///// </summary>
    //Router Sharding { get; }

    //todo: Storage backend - each node may have its own storage engine, e.g. we can have a node which is used only as backup/archive
    //so it does no take writes or queries, but only replicates data to a file-based log locally
    //this should probably go to more detailed server interface
  }


}





