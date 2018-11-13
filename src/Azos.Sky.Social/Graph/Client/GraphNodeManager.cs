using System.Linq;

using Azos.Apps;
using Azos.Conf;
using Azos.Data;

using Azos.Sky.Coordination;

namespace Azos.Sky.Social.Graph
{
  public sealed class GraphNodeManager : GraphNodeManagerBase
  {

    public GraphNodeManager(HostSet hostSet) : base(hostSet)
    {

    }

    public override GraphChangeStatus SaveNode(GraphNode node)
    {
      var pair = HostSet.AssignHost(node.GDID);
      return Contracts.ServiceClientHub.CallWithRetry<IGraphNodeSystemClient, GraphChangeStatus>(nodeSystem => nodeSystem.SaveNode(node), pair.Select(host => host.RegionPath));
    }

    public override GraphNode GetNode(GDID gNode)
    {
      var pair = HostSet.AssignHost(gNode);
      return Contracts.ServiceClientHub.CallWithRetry<IGraphNodeSystemClient, GraphNode>(nodeSystem => nodeSystem.GetNode(gNode), pair.Select(host => host.RegionPath));
    }

    public override GraphChangeStatus DeleteNode(GDID gNode)
    {
      var pair = HostSet.AssignHost(gNode);
      return Contracts.ServiceClientHub.CallWithRetry<IGraphNodeSystemClient, GraphChangeStatus>(nodeSystem => nodeSystem.DeleteNode(gNode), pair.Select(host => host.RegionPath));
    }

    public override GraphChangeStatus UndeleteNode(GDID gNode)
    {
      var pair = HostSet.AssignHost(gNode);
      return Contracts.ServiceClientHub.CallWithRetry<IGraphNodeSystemClient, GraphChangeStatus>(nodeSystem => nodeSystem.UndeleteNode(gNode), pair.Select(host => host.RegionPath));
    }

    public override GraphChangeStatus RemoveNode(GDID gNode)
    {
      var pair = HostSet.AssignHost(gNode);
      return Contracts.ServiceClientHub.CallWithRetry<IGraphNodeSystemClient, GraphChangeStatus>(nodeSystem => nodeSystem.RemoveNode(gNode), pair.Select(host => host.RegionPath));
    }

  }

  /// <summary>
  /// Заглушка для интерфейса IGraphNodeSystem на клиенте
  /// </summary>
  public sealed class NOPGraphNodeManager : GraphNodeManagerBase
  {
    public NOPGraphNodeManager(HostSet hostSet) : base(hostSet)
    {
    }

    public override GraphChangeStatus SaveNode(GraphNode node)
    {
      return GraphChangeStatus.NotFound;
    }

    public override GraphNode GetNode(GDID gNode)
    {
      return default(GraphNode);
    }

    public override GraphChangeStatus DeleteNode(GDID gNode)
    {
      return GraphChangeStatus.NotFound;
    }

    public override GraphChangeStatus UndeleteNode(GDID gNode)
    {
      return GraphChangeStatus.NotFound;
    }

    public override GraphChangeStatus RemoveNode(GDID gNode)
    {
      return GraphChangeStatus.NotFound;
    }
  }
}