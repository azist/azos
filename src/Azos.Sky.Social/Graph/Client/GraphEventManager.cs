using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Data;
using Azos.Sky.Coordination;

namespace Azos.Sky.Social.Graph
{
  public sealed class GraphEventManager : GraphEventManagerBase
  {
    public GraphEventManager(HostSet hostSet) : base(hostSet)
    {
    }

    public override void EmitEvent(Event evt)
    {
      var pair = HostSet.AssignHost(evt.G_EmitterNode);
      Contracts.ServiceClientHub
        .CallWithRetry<IGraphEventSystemClient>(eventSystem => eventSystem.EmitEvent(evt), pair.Select(host => host.RegionPath));
    }

    public override void Subscribe(GDID gRecipientNode, GDID gEmitterNode, byte[] parameters)
    {
      var pair = HostSet.AssignHost(gEmitterNode);
      Contracts.ServiceClientHub
        .CallWithRetry<IGraphEventSystemClient>(eventSystem => eventSystem.Subscribe(gRecipientNode, gEmitterNode, parameters), pair.Select(host => host.RegionPath));
    }

    public override void Unsubscribe(GDID gRecipientNode, GDID gEmitterNode)
    {
      var pair = HostSet.AssignHost(gEmitterNode);
      Contracts.ServiceClientHub
        .CallWithRetry<IGraphEventSystemClient>(eventSystem => eventSystem.Unsubscribe(gRecipientNode, gEmitterNode), pair.Select(host => host.RegionPath));
    }

    public override long EstimateSubscriberCount(GDID gEmitterNode)
    {
      var pair = HostSet.AssignHost(gEmitterNode);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphEventSystemClient, long>(eventSystem => eventSystem.EstimateSubscriberCount(gEmitterNode), pair.Select(host => host.RegionPath));
    }

    public override IEnumerable<GraphNode> GetSubscribers(GDID gEmitterNode, long start, int count)
    {
      var pair = HostSet.AssignHost(gEmitterNode);
      return Contracts.ServiceClientHub
        .CallWithRetry<IGraphEventSystemClient, IEnumerable<GraphNode>>(eventSystem => eventSystem.GetSubscribers(gEmitterNode, start, count), pair.Select(host => host.RegionPath));
    }
  }

  /// <summary>
  /// Заглушка для интерфейса IGraphEventSystem на клиенте
  /// </summary>

  public sealed class NOPGraphEventManager : GraphEventManagerBase
  {
    public NOPGraphEventManager(HostSet hostSet) : base(hostSet)
    {
    }

    public override void EmitEvent(Event evt)
    {
    }

    public override void Subscribe(GDID gRecipientNode, GDID gEmitterNode, byte[] parameters)
    {
    }

    public override void Unsubscribe(GDID gRecipientNode, GDID gEmitterNode)
    {
    }

    public override long EstimateSubscriberCount(GDID gEmitterNode)
    {
      return 0;
    }

    public override IEnumerable<GraphNode> GetSubscribers(GDID gEmitterNode, long start, int count)
    {
      yield break;
    }
  }
}