/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Collections;

namespace Azos.Data.Heap
{
  /// <summary>
  /// Represents a heap node logical process. Heap clients connect to heap nodes to get data using queries or direct object references,
  /// mutate data using via Set() or Delete(). Internally nodes host storage engines and perform inter-node replication and CRDT-style
  /// conflict resolution.
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

    //collection of peers
    /// <summary>
    /// Cluster host name, such as sky regional catalog path e.g. `/world/us/east/cle/db/z1/lmed002.h`
    /// </summary>
    string HostName { get; }

    /// <summary>
    /// Node client
    /// </summary>
    string ServiceAddress {  get;}
    string ServiceContract { get; }
  }

  /// <summary>
  /// Represents data heap server node
  /// </summary>
  public interface IServerNode
  {
    /// <summary>
    /// Globally-unique cluster node identifier. Every HeapObject instance gets Sys_VerNode stamp by the server
    /// </summary>
    Atom NodeId     { get; }

    /// <summary>
    /// Cluster host name, such as sky regional catalog path e.g. `/world/us/east/cle/db/z1/lmed002.h`
    /// </summary>
    string HostName { get; }

    /// <summary>
    /// The high precision Universal Time Coordinate(d)
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Returns the data partitioning handler which performs shard routing of heap data
    /// </summary>
    Router Sharding { get; }

    //todo: Storage backend - each node may have its own storage engine, e.g. we can have a node which is used only as backup/archive
    //so it does no take writes or queries, but only replicates data to a file-based log locally
    //this should probably go to more detailed server interface
  }


}





