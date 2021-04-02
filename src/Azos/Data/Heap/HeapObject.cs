/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Serialization.JSON;
using Azos.Serialization.Bix;

namespace Azos.Data.Heap
{
  /// <summary>
  /// Provides the very base for objects stored in a distributed data heap.
  /// Heap objects are CvRDT (Convergent Replicated Data Types), their Merge() operation must be: Commutative, Associative, and Idempotent
  /// </summary>
  [BixJsonHandler]
  public abstract class HeapObject : AmorphousTypedDoc
  {
    /// <summary> Defines Sys_VerState values </summary>
    public enum State : sbyte
    {
      Undefined = 0,
      Created = 1,
      Modified = 2,
      Deleted = -1
    }

    /// <summary>
    /// Heap objects support amorphous data to implement gradual data schema changes
    /// </summary>
    public override bool AmorphousDataEnabled => true;


    [Field(Description = "A unique id of the object - immutable primary key",
           StoreFlag = StoreFlag.LoadAndStore,
           Key = true)]
    public GDID Sys_Id{ get; internal set; } //all entities referencing this field should start with "g" e.g. "gUser"


    public virtual object Sys_ShardingId => Sys_Id;

    /// <summary> Override to specify relative processing (e.g. replication) priority </summary>
    public virtual int Sys_Priority => 0;

    [Field(Description = "Version state: Created/Modified/Deleted",
           StoreFlag = StoreFlag.LoadAndStore)]
    public State Sys_VerState { get; private set; }

    [Field(Description = "Version UTC stamped at the time of set by server node",
           StoreFlag = StoreFlag.LoadAndStore)]
    public long Sys_VerUtc { get; private set; }

    [Field(Description = "Version Server Node id - where the change originated on",
           StoreFlag = StoreFlag.LoadAndStore)]
    public byte Sys_VerNode { get; private set; }

    /// <summary>
    /// Sync UTC stamp is set by server node at the time of device write. The system uses this value only for change log replication.
    /// This stamp is not to be confused with logical/CRDT version, as it only captures an absolute
    /// point in time which is used by other nodes for data gossip/replication.
    /// Every time data is written into device (e.g. a database) this value gets updated. Other heap nodes keep track
    /// of this value per source (node:collection) to keep track of how far along the remote change log they got.
    /// The value does not need to be precise as it is used only as a monotonic time counter per node (Unix milliseconds).
    /// </summary>
    [Field(Description = "Sync UTC stamp is set by server node at the time of device write." +
                         "The system uses this value for change log replication. " +
                         "This stamp is not to be confused with logical/CRDT version, as it only " +
                         "captures an absolute point in time which is used by other nodes for data gossip/replication",
           StoreFlag = StoreFlag.LoadAndStore)]
    public long Sys_SyncUtc { get; internal set; }

    /// <summary>
    /// Sync replication mask where every node 1..64 of a heap cluster is represented by a bit. If bit is set then
    /// the corresponding node did receive this version.
    /// </summary>
    [Field(Description = "Sync replication bit mask where every node 1..64 of a heap cluster is represented by a bit. " +
                         "If a bit is set to 1 then the corresponding node already has this object version",
           StoreFlag = StoreFlag.LoadAndStore)]
    public long Sys_ReplicationSet { get; internal set; }

    /// <summary>
    /// Called by the server node, "seals" the data version by stamping appropriate object attributes.
    /// The node calls this method when the data is SET(Updated), but NOT when it is synchronized/merged.
    /// You must always call the base implementation.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="collection"></param>
    /// <param name="newState">new State</param>
    /// <remarks>
    /// This is an extension point for any kind of CRDT mechanism, e.g. you can use vector clocks by
    /// storing the appropriate value in your derived type fields and then extend this method to populate the object version
    /// accordingly.
    /// </remarks>
    protected internal virtual void CrdtSet(IHeapNode node, IHeapCollection collection, State newState)
    {
      Sys_VerState = newState;
      Sys_VerUtc = node.UtcNow.ToMillisecondsSinceUnixEpochStart();
      Sys_VerNode = node.NodeId;
      Sys_ReplicationSet = 0L.SetReplicationNode(node.NodeId);
    }


    /// <summary>
    /// Called on the data heap server node, performs CRDT merge operation by
    /// taking the local state represented by this instance and merging another version(s) which came from other heap nodes.
    /// The function returns a result of the merge (conflict-free) by definition) which is then used as the most
    /// current state instance, or null if this instance already represents the most current instance.
    /// WARNING: Per CvRDT definition, this operation is COMMUTATIVE, ASSOCIATIVE, and IDEMPOTENT.
    /// Failure to comply with these requirements may result in an infinite inter-node rotary traffic pattern.
    /// </summary>
    /// <param name="others"></param>
    /// <returns>Object instance which results from merge or null if THIS instance already represents the latest state and no changes are necessary</returns>
    internal HeapObject CrdtMerge(IHeapNode node, IHeapCollection collection, IEnumerable<HeapObject> others)
    {
      if (others == null) return null;
      if (!others.Any()) return null;

      var t = this.GetType();

      others.IsTrue(v => v.All(one => one.GetType() == t && one.Sys_Id == this.Sys_Id), "Non empty version for the same Sys_Id");

      return DoCrdtMerge(node, collection, others).IsTrue(r => r == null || r.GetType() == t, "Returned type mismatch");
    }

    /// <summary>
    /// The default implementation is LWW (Last Write Wins) based on the accurate global time
    /// </summary>
    /// <param name="node"></param>
    /// <param name="collection"></param>
    /// <param name="others"></param>
    /// <returns></returns>
    protected virtual HeapObject DoCrdtMerge(IHeapNode node, IHeapCollection collection, IEnumerable<HeapObject> others)
    {
      var result = this;
      foreach (var ver in others)
      {
        if (ver.Sys_VerUtc > result.Sys_VerUtc) result = ver;
      }

      return result == this ? null : result;
    }


    protected override void AddJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, Dictionary<string, object> jsonMap, string name, object value)
    {
      if (def?.Order == 0)
      {
        BixJsonHandler.EmitJsonBixDiscriminator(this, jsonMap);
      }

      base.AddJsonSerializerField(def, options, jsonMap, name, value);
    }

  }


  [Heap("doc", "doc", ChannelName = "std")]  //16 servers 3 locations
  public class Doctor : HeapObject
  {
    [Field] public string NPI{ get; set; }
  }

}
