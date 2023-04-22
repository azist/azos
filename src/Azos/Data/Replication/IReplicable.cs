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

  /// <summary>
  /// Denotes a data entity which can be replicated from/into another storage system/node instance
  /// </summary>
  public interface IReplicable
  {
    /// <summary>
    /// Gets primary key/id of the item being replicated. It is always required.
    /// This id may not change after item creation, it does not depend on version.
    /// An item must be uniquely-identifiable by such an id in a cross-origin cluster
    /// </summary>
    RGDID Repl_Id {  get; }

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
    Atom Repl_VerOrigin {  get; }

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
