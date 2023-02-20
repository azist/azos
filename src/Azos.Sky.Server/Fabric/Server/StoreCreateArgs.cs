/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Data.Business;
using Azos.Serialization.Bix;

namespace Azos.Sky.Fabric
{
  [Schema(Description = "Create shard store fiber record args")]
  [Bix("a081d76f-bff7-4edd-9b02-faf03ef0a5ed")]
  public sealed class StoreCreateArgs : TransientModel
  {
    public StoreCreateArgs(){ }
    public StoreCreateArgs(FiberStartArgs proto, byte[] binParams)
    {
      proto.CopyFields(this);
      BinParameters = binParams;
    }
    [Field(typeof(FiberStartArgs))]  public FiberId    Id              { get; set; }
    [Field(typeof(FiberStartArgs))]  public Atom       Origin          { get; set; }
    [Field(typeof(FiberStartArgs))]  public Guid       ImageTypeId     { get; set; }
    [Field(typeof(FiberStartArgs))]  public string     Group           { get; set; }
    [Field(typeof(FiberStartArgs))]  public float?     Priority        { get; set; }
    [Field(typeof(FiberStartArgs))]  public EntityId?  ImpersonateAs   { get; set; }
    [Field(typeof(FiberStartArgs))]  public DateTime?  ScheduledStartUtc { get; set; }
    [Field(typeof(FiberStartArgs))]  public EntityId?  Initiator       { get; set; }
    [Field(typeof(FiberStartArgs))]  public EntityId?  Owner           { get; set; }
    [Field(typeof(FiberStartArgs))]  public string     Description     { get; set; }
    [Field(typeof(FiberStartArgs))]  public List<Data.Adlib.Tag>  Tags { get; set; }

    [Field(Description = "Binary representation of parameter payload to be stored as-is")]
    public byte[] BinParameters{ get; set; }
  }
}
