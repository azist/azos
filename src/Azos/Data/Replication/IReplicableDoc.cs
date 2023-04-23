/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Data.Replication
{
  /// <summary>
  /// Defines values for replicable data item state: Created|Modified|Deleted.
  /// Deleted is used as a tombstone state
  /// </summary>
  public enum ReplicationState : sbyte
  {
    Undefined = 0,
    Created = 1,
    Modified = 2,
    Deleted = -1
  }

  //Why Doc? Because it is universally "teleportable" using Bix or Json (or others).
  //DOC storage format may DIFFER between nodes due to their storage engine differences.
  //As replication does not have to match storage technology per node we may need to take data
  //say from Mongo and store data say in MySql.
  //We can not store data in a "envelope/frame" because we may need to get access to such data during
  //"merge" replication conflict resolution (e.g. is CRDT is used). A simple replicator will merge by applying
  //"the last wins" strategy, but complex will delegate to "Merge" method that needs all data fields available.

  /// <summary>
  /// Defines a unit of replication - a data document which can be replicated from/into another storage system/node instance.
  /// Replication is keyed on <see cref="Repl_Id"/> immutable surrogate primary key which is always universally present
  /// to define document instance identity. The system pulls document from the "other side" matching local ones by these ids
  /// and then compares the logical version information <see cref="Repl_VerUtc"/>
  /// </summary>
  public interface IReplicableDoc : IDataDoc
  {
    /// <summary>
    /// Gets primary key/id of the item being replicated. It is always required.
    /// This id may not change after item creation, it does not depend on version.
    /// An item must be uniquely-identifiable by such an id in a cross-origin cluster
    /// </summary>
    RGDID Repl_Id { get; }

    /// <summary>
    /// Specifies data version state: Created|Modified|Deleted.
    /// Deleted is used as a tombstone flag
    /// </summary>
    ReplicationState Repl_VerState { get; }

    /// <summary>
    /// Data version UTC with ms resolution stamped at the time of set/modify on origin server node.
    /// This stamp is NOT changed by replication unless merge yields yet another logical version
    /// </summary>
    long Repl_VerUtc { get; }

    /// <summary>
    /// The id of cluster origin region/zone where the item was first triggered, among other things
    /// this value is used to prevent circular traffic - in multi-master situations so the
    /// same event does not get replicated multiple times across regions (data centers)
    /// </summary>
    Atom Repl_VerOrigin { get; }

    /// <summary>
    /// Version Server Node id - what node within origin the change originated on
    /// </summary>
    Atom Repl_VerNode { get; }

    /// <summary>
    /// Sync UTC stamp is set by server node at the time of device write. The system uses this value ONLY for CHANGE LOG REPLication.
    /// This stamp is not to be confused with logical/CRDT version <see cref="Repl_VerUtc"/>, as it only captures an absolute
    /// point in time which is used by other nodes for data gossip/replication.
    /// Every time data is written into device (e.g. a database) this value gets updated. Other nodes keep track
    /// of this value per source (node:collection) to keep track of how far along the remote change log they got.
    /// The value does not need to be precise as it is used only as a monotonic time counter per node expressed in Unix milliseconds
    /// </summary>
    long Repl_VerSyncUtc { get; internal set; }

  }
}
