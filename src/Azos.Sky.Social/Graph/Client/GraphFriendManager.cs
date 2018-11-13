using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Data;
using Azos.Sky.Coordination;

namespace Azos.Sky.Social.Graph
{
  public class GraphFriendManager : GraphFriendManagerBase
  {
    public GraphFriendManager(HostSet hostSet) : base(hostSet)
    {
    }

    public override IEnumerable<string> GetFriendLists(GDID gNode)
    {
      var pair = HostSet.AssignHost(gNode);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphFriendSystemClient, IEnumerable<string>>(friendSystem => friendSystem.GetFriendLists(gNode), pair.Select(host => host.RegionPath));
    }

    public override GraphChangeStatus AddFriendList(GDID gNode, string list, string description)
    {
      var pair = HostSet.AssignHost(gNode);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphFriendSystemClient, GraphChangeStatus>(friendSystem => friendSystem.AddFriendList(gNode, list, description), pair.Select(host => host.RegionPath));
    }

    public override GraphChangeStatus DeleteFriendList(GDID gNode, string list)
    {
      var pair = HostSet.AssignHost(gNode);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphFriendSystemClient, GraphChangeStatus>(friendSystem => friendSystem.DeleteFriendList(gNode, list), pair.Select(host => host.RegionPath));
    }

    public override IEnumerable<FriendConnection> GetFriendConnections(FriendQuery query)
    {
      var pair = HostSet.AssignHost(query.G_Node);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphFriendSystemClient, IEnumerable<FriendConnection>>(friendSystem => friendSystem.GetFriendConnections(query), pair.Select(host => host.RegionPath));
    }

    public override GraphChangeStatus AddFriend(GDID gNode, GDID gFriendNode, bool? approve)
    {
      var pair = HostSet.AssignHost(gNode);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphFriendSystemClient, GraphChangeStatus>(friendSystem => friendSystem.AddFriend(gNode, gFriendNode, approve), pair.Select(host => host.RegionPath));
    }

    public override GraphChangeStatus AssignFriendLists(GDID gNode, GDID gFriendNode, string lists)
    {
      var pair = HostSet.AssignHost(gNode);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphFriendSystemClient, GraphChangeStatus>(friendSystem => friendSystem.AssignFriendLists(gNode, gFriendNode, lists), pair.Select(host => host.RegionPath));
    }

    public override GraphChangeStatus DeleteFriend(GDID gNode, GDID gFriendNode)
    {
      var pair = HostSet.AssignHost(gNode);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphFriendSystemClient, GraphChangeStatus>(friendSystem => friendSystem.DeleteFriend(gNode, gFriendNode), pair.Select(host => host.RegionPath));
    }
  }

  /// <summary>
  /// Заглушка для интерфейса IGraphFriendSystem на клиенте
  /// </summary>
  public class NOPGraphFriendManager : GraphFriendManagerBase
  {
    public NOPGraphFriendManager(HostSet hostSet) : base(hostSet)
    {
    }

    public override IEnumerable<string> GetFriendLists(GDID gNode)
    {
      yield break;
    }

    public override GraphChangeStatus AddFriendList(GDID gNode, string list, string description)
    {
      return GraphChangeStatus.NotFound;
    }

    public override GraphChangeStatus DeleteFriendList(GDID gNode, string list)
    {
      return GraphChangeStatus.NotFound;
    }

    public override IEnumerable<FriendConnection> GetFriendConnections(FriendQuery query)
    {
      yield break;
    }

    public override GraphChangeStatus AddFriend(GDID gNode, GDID gFriendNode, bool? approve)
    {
      return GraphChangeStatus.NotFound;
    }

    public override GraphChangeStatus AssignFriendLists(GDID gNode, GDID gFriendNode, string lists)
    {
      return GraphChangeStatus.NotFound;
    }

    public override GraphChangeStatus DeleteFriend(GDID gNode, GDID gFriendNode)
    {
      return GraphChangeStatus.NotFound;
    }
  }
}