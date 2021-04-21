/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

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


    [Field(Description = "A unique id of the object - immutable primary key aka 'heap pointer'",
           StoreFlag = StoreFlag.LoadAndStore,
           Key = true)]
    public GDID Sys_Id{ get; internal set; } //all entities referencing this field should start with "G_" e.g. "G_User"

    /// <summary>
    /// The sharding key is used to locate the shard aka 'segment' of the data heap
    /// </summary>
    public virtual ShardKey Sys_ShardKey => new ShardKey(Sys_Id);

    /// <summary>
    /// Override to specify relative processing (e.g. replication) priority dependent on the instance.
    /// The higher the number - the more priority is given. The default is ZERO = normal priority.
    /// You can boost priority for objects which contain data for important/frequently used items
    /// such as celebrity profiles etc.
    /// </summary>
    public virtual int Sys_Priority => 0;

    /// <summary>
    /// Latest version state: Created/Modified/Deleted
    /// </summary>
    [Field(Description = "Version state: Created/Modified/Deleted",
           StoreFlag = StoreFlag.LoadAndStore)]
    public State Sys_VerState { get; private set; }

    /// <summary>
    /// Latest version UTC stamped at the time of set on origin server node.
    /// This stamp is not changed by replication unless merge yields yet another version
    /// </summary>
    [Field(Description = "Version UTC stamped at the time of set by the server node." +
                         " This stamp is not changed by replication unless merge yields yet another version",
           StoreFlag = StoreFlag.LoadAndStore)]
    public long Sys_VerUtc { get; private set; }

    /// <summary>
    /// Version Server Node id - where the change originated on
    /// </summary>
    [Field(Description = "Version Server Node id - where the change originated on",
           StoreFlag = StoreFlag.LoadAndStore)]
    public Atom Sys_VerNode { get; private set; }

    /// <summary>
    /// Sync UTC stamp is set by server node at the time of device write. The system uses this value only for change log replication.
    /// This stamp is not to be confused with logical/CRDT version, as it only captures an absolute
    /// point in time which is used by other nodes for data gossip/replication.
    /// Every time data is written into device (e.g. a database) this value gets updated. Other heap nodes keep track
    /// of this value per source (node:collection) to keep track of how far along the remote change log they got.
    /// The value does not need to be precise as it is used only as a monotonic time counter per node expressed in Unix milliseconds
    /// </summary>
    [Field(Description = "Sync UTC stamp is set by server node at the time of device write." +
                         "The system uses this value for change log replication. " +
                         "This stamp is not to be confused with logical/CRDT version, as it only " +
                         "captures an absolute point in time which is used by other nodes for data gossip/replication",
           StoreFlag = StoreFlag.LoadAndStore)]
    public long Sys_SyncUtc { get; internal set; }

    /// <summary>
    /// Called by the server node, "seals" the data version by stamping appropriate object attributes.
    /// The node calls this method when the data is SET(Updated), but typically NOT when it is synchronized/merged
    /// unless a merge yields a brand new version of data.
    /// You must always call the base implementation
    /// </summary>
    /// <param name="node">Node where data change takes place</param>
    /// <param name="space">Space where this object is stored</param>
    /// <param name="newState">The new version state</param>
    /// <remarks>
    /// This is an extension point for any kind of CRDT mechanism, e.g. you can use vector clocks by
    /// storing the appropriate value in your derived type fields and then extend this method to populate the object version
    /// accordingly.
    /// </remarks>
    protected internal virtual void Crdt_Set(IServerNode node, ISpace space, State newState)
    {
      Sys_VerState = newState;
      Sys_VerUtc = node.UtcNow.ToMillisecondsSinceUnixEpochStart();
      Sys_VerNode = node.NodeId;
    }

    /// <summary>
    /// Called by the data heap server nodes during replication, performs CRDT merge operation by
    /// taking the local state represented by this instance and merging another version(s) which came from other heap nodes.
    /// The function returns a result of the merge (conflict-free by definition) which is then used as the most
    /// current state instance, or null if this instance already represents the most current instance.
    /// WARNING: Per CvRDT definition, this operation is COMMUTATIVE, ASSOCIATIVE, and IDEMPOTENT.
    /// Failure to comply with these requirements may result in an infinite inter-node rotary traffic pattern.
    /// </summary>
    /// <param name="node">Node where data change takes place</param>
    /// <param name="space">Space where this object is stored</param>
    /// <param name="others">Other versions got form other nodes</param>
    /// <returns>
    /// Object instance which results from merge or null if THIS instance already represents the latest eventual state and no changes are necessary.
    /// You either return null, or one of "others" OR you can return a brand new object (not this or others), in which case the system treats it a as
    /// a brand new version performing necessary version stamping via `Crdt_Set()`
    /// </returns>
    internal HeapObject Crdt_Merge(IServerNode node, ISpace space, IEnumerable<HeapObject> others)
    {
      if (others == null) return null;
      if (!others.Any()) return null;

      var t = this.GetType();

      others.IsTrue(v => v.All(one => one.GetType() == t && one.Sys_Id == this.Sys_Id), "Non empty version for the same Sys_Id");

      return DoCrdt_Merge(node, space, others).IsTrue(r => r == null || r.GetType() == t, "Returned type mismatch");
    }

    /// <summary>
    /// Called on the data heap server nodes during replication, performs CRDT merge operation by
    /// taking the local state represented by this instance and merging another version(s) which came from other heap nodes.
    /// The function returns a result of the merge (conflict-free by definition) which is then used as the most
    /// current state instance, or null if this instance already represents the most current instance.
    /// WARNING: Per CvRDT definition, this operation is COMMUTATIVE, ASSOCIATIVE, and IDEMPOTENT.
    /// Failure to comply with these requirements may result in an infinite inter-node rotary traffic pattern.
    /// </summary>
    /// <param name="node">Node where data change takes place</param>
    /// <param name="space">Space where this object is stored</param>
    /// <param name="others">Other versions got form other nodes</param>
    /// <returns>Object instance which results from merge or null if THIS instance already represents the latest eventual state and no changes are necessary</returns>
    protected virtual HeapObject DoCrdt_Merge(IServerNode node, ISpace space, IEnumerable<HeapObject> others)
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

    public override ValidState ValidateField(ValidState state, Schema.FieldDef fdef, string scope = null)
    {
      if (fdef.Name == nameof(Sys_Id))
      {
        if (Sys_VerState != State.Undefined && Sys_Id.IsZero)
        {
          state = new ValidState(state, new FieldValidationException(this, nameof(Sys_Id), StringConsts.CRUD_FIELD_VALUE_REQUIRED_ERROR));
          if (state.ShouldStop) return state;
        }
      }
      return base.ValidateField(state, fdef, scope);
    }

  }


  [HeapSpace(area: "clinical", space: "doc")]//, ChannelName = "std")]  //16 servers 3 locations
  public class Doctor : HeapObject
  {
    [Field] public string NPI{ get; set; }
  }

  [HeapProc(area: "clinical", name: "doctor.getList")]
  public class DoctorListByNpi : HeapQuery//<DoctorInfo><List<int>>
  {
    //public static async void A()
    //{
    //  IHeap heap = null;
    //  var lst = await heap.ExecAsync( new DoctorListByNpi{});
    //}

    [Field] public string Npi { get; set; }
  }

}
