/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Data.Heap
{
  /// <summary>
  /// Provides the very bas for objects stored in a distributed data heap
  /// </summary>
  public abstract class HeapObject : AmorphousTypedDoc
  {

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

#warning Add JsonHandler for provider-specific version type
    [Field(Description = "Version assigned by heap implementation",
           StoreFlag = StoreFlag.LoadAndStore)]
    public ObjectVersion Sys_Ver { get; set; }
  }


  [Heap("med", "doctor", ChannelName = "std")]
  public class Doctor : HeapObject
  {
    [Field] public string NPI{ get; set; }
  }


}
