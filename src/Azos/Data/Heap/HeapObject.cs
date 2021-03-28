/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.Data.Heap
{
  /// <summary>
  /// Provides the very bas for objects stored in a distributed data heap
  /// </summary>
  public abstract class HeapObject : AmorphousTypedDoc
  {
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
    public State Sys_VerState { get; internal set; }

    [Field(Description = "Version UTC stamped at the time of set",
           StoreFlag = StoreFlag.LoadAndStore)]
    public ulong Sys_VerUtc { get; internal set; }

    [Field(Description = "Version Node - where the change originated on",
           StoreFlag = StoreFlag.LoadAndStore)]
    public Atom Sys_VerNode { get; internal set; }

    /// <summary>
    /// WARNING: Per CRDT definition, this operation must be COMMUTATIVE, ASSOCIATIVE, and IDEMPOTENT.
    /// Failure to comply with these requirements may result in an infinite inter-node rotary traffic pattern.
    /// </summary>
    /// <param name="others"></param>
    /// <returns>Object instance which results from merge, it can be one of the existing instances or a new one</returns>
    public (bool isnew, HeapObject obj) MergeCrdt(IEnumerable<HeapObject> others)
    {
      if (others==null) return (false, this);
      if (!others.Any()) return (false, this);

      others.IsTrue( v => v.All(one => one.GetType() == this.GetType() && one.Sys_Id == this.Sys_Id), "Non empty version for the same Sys_Id");

      var versions = others.AddOneAtStart(this);

      var result = this;
      foreach(var ver in versions)
      {
        if (ver.Sys_VerState == State.Deleted)
        {
          if (result.Sys_VerState != State.Deleted)
          {
            result = ver;
            continue;
          }
        }
        if (ver.Sys_VerUtc > result.Sys_VerUtc) result = ver;
      }
      return (false, result);
    }

  }


  [Heap("med", "doctor", ChannelName = "std")]
  public class Doctor : HeapObject
  {
    [Field] public string NPI{ get; set; }
  }


}
