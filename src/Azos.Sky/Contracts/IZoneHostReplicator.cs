/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Reflection;

using Azos.Glue;
using Azos.Glue.Protocol;

namespace Azos.Sky.Contracts
{
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface IZoneHostReplicator : ISkyService
  {
    [OneWay]
    [ArgsMarshalling(typeof(RequestMsg_IZoneHostReplicator_PostDynamicHostInfo))]
    void PostDynamicHostInfo(DynamicHostID id, DateTime stamp, string owner, int votes);
    DynamicHostInfo GetDynamicHostInfo(DynamicHostID hid);
    [OneWay]
    void PostHostInfo(HostInfo host, DynamicHostID? hid);
  }

  public interface IZoneHostReplicatorClient : ISkyServiceClient, IZoneHostReplicator
  {
    CallSlot Async_PostDynamicHostInfo(DynamicHostID hid, DateTime stamp, string owner, int votes);
    CallSlot Async_PostHostInfo(HostInfo host, DynamicHostID? hid);
  }

  public sealed class RequestMsg_IZoneHostReplicator_PostDynamicHostInfo : RequestMsg
  {
    public RequestMsg_IZoneHostReplicator_PostDynamicHostInfo(MethodInfo method, Guid? instance) : base(method, instance) { }
    public RequestMsg_IZoneHostReplicator_PostDynamicHostInfo(TypeSpec contract, MethodSpec method, bool oneWay, Guid? instance)
                                                             : base(contract, method, oneWay, instance) { }

    public DynamicHostID MethodArg_0_id;
    public DateTime      MethodArg_1_stamp;
    public string        MethodArg_2_owner;
    public int           MethodArg_3_votes;
  }
}
