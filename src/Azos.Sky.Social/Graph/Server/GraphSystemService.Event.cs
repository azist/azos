using System;
using System.Collections.Generic;

using Azos.Data;
using Azos.Conf;
using Azos.Log;
using Azos.Serialization.JSON;

using Azos.Sky.Social.Graph.Server.Data;
using Azos.Sky.Social.Graph.Server.Workers;
using Azos.Sky.Workers;

namespace Azos.Sky.Social.Graph.Server
{
  public partial class GraphSystemService
  {
    /// <summary>
    /// Emits the event - notifies all subscribers (watchers, friends etc.) about the event.
    /// The physical notification happens via IGraphHost implementation
    /// </summary>
    void IGraphEventSystem.EmitEvent(Event evt)
    {
      try
      {
        IConfigSectionNode cfg = null;
        if (evt.Config.IsNotNullOrWhiteSpace()) cfg = evt.Config.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
        DoEmitEvent(evt);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "EmitEvent", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_EMIT_EVENT_ERROR.Args(evt.ToJSON()), ex);
      }
    }

    /// <summary>
    /// Subscribes recipient node to the emitter node. Unlike friends the susbscription connection is uni-directional
    /// </summary>
    void IGraphEventSystem.Subscribe(GDID gRecipientNode, GDID gEmitterNode, byte[] parameters)
    {
      try
      {
        DoSubscribe(gRecipientNode, gEmitterNode, parameters);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "Subscribe", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_SUBSCRIBE_ERROR.Args(gRecipientNode.ToString(), gEmitterNode.ToString()), ex);
      }
    }

    /// <summary>
    /// Removes the subscription. Unlike friends the subscription connection is uni-directional
    /// </summary>
    void IGraphEventSystem.Unsubscribe(GDID gRecipientNode, GDID gEmitterNode)
    {
      try
      {
        DoUnsubscribe(gRecipientNode, gEmitterNode);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error, "DoUnsubscribe", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_UNSUBSCRIBE_ERROR.Args(gRecipientNode.ToString(), gEmitterNode.ToString()), ex);
      }
    }

    /// <summary>
    /// Returns an estimated approximate number of subscribers that an emitter has
    /// </summary>
    long IGraphEventSystem.EstimateSubscriberCount(GDID gEmitterNode)
    {
      try
      {
        return DoEstimateSubscriberCount(gEmitterNode);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error,"EstimateSubscriberCount", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_ESTIMATE_SUBSCRIPTION_COUNT_ERROR.Args(gEmitterNode.ToString()), ex);
      }
    }

    /// <summary>
    /// Returns Subscribers for Emitter from start position
    /// </summary>
    IEnumerable<GraphNode> IGraphEventSystem.GetSubscribers(GDID gEmitterNode, long start, int count)
    {
      try
      {
        return DoGetSubscribers(gEmitterNode, start, count);
      }
      catch (Exception ex)
      {
        Log(MessageType.Error,"GetSubscribers", ex.ToMessageWithType(), ex);
        throw new GraphException(StringConsts.GS_GET_SUBSCRIBER_ERROR.Args(gEmitterNode.ToString()), ex);
      }
    }

    protected virtual void DoEmitEvent(Event evt)
    {
      DoGetNode(evt.G_EmitterNode); // Don't run todo if Emitter does not exist
      var qryVol = Queries.CountSubscriberVolumes<DynamicDoc>(evt.G_EmitterNode);
      var countVol = ForNode(evt.G_EmitterNode).LoadDoc(qryVol)["CNT"].AsInt();
      var count = countVol < EventDeliveryCohortSize ? countVol : EventDeliveryCohortSize;
      var todos = new List<EventDeliverTodo>();
      for(int i=0; i < count; i++)
      {
        var todo = Todo.MakeNew<EventDeliverTodo>();
        todo.Event = evt;
        todo.VolumeWorkerOffset  = count;
        todo.VolumeIndex = i;
        todo.ChunkIndex = 0;
        todo.G_Volume = GDID.Zero;
        todos.Add(todo);
      }
      SocialGraphTodos.EnqueueSubscribtion(todos);
    }

    protected virtual void DoSubscribe(GDID gRecipientNode, GDID gEmitterNode, byte[] parameters)
    {
      var emitter = DoGetNode(gEmitterNode);
      var gh = DoGetNode(gRecipientNode);
      if (!GraphHost.CanBeSubscribed(gh.NodeType, emitter.NodeType)) throw new GraphException(StringConsts.GS_CAN_NOT_BE_SUSBCRIBED_ERROR.Args(gh.NodeType, emitter.NodeType));
      var todo = Todo.MakeNew<EventSubscribeTodo>();
      todo.G_Owner = gEmitterNode;
      todo.G_Subscriber = gRecipientNode;
      todo.Subs_Type = gh.NodeType;
      todo.Parameters = parameters;
      SocialGraphTodos.EnqueueSubscribtion(todo);
    }

    protected virtual void DoUnsubscribe(GDID gRecipientNode, GDID gEmitterNode)
    {
      var todo =Todo.MakeNew<EventUnsubscribeTodo>();
      todo.G_Owner = gEmitterNode;
      todo.G_Subscriber = gRecipientNode;
      SocialGraphTodos.EnqueueSubscribtion(todo);
    }

    protected virtual long DoEstimateSubscriberCount(GDID gEmitterNode)
    {
      var count = ForNode(gEmitterNode).LoadOneDoc(Queries.CountSubscribers<Doc>(gEmitterNode));
      return count[0].AsLong();
    }

    protected virtual IEnumerable<GraphNode> DoGetSubscribers(GDID gEmitterNode, long start, int count)
    {
      var qryVol = Queries.FindSubscriberVolumes<SubscriberVolumeRow>(gEmitterNode);
      IEnumerable<SubscriberVolumeRow> volumes = ForNode(gEmitterNode).LoadEnumerable(qryVol);
      var sum = 0;
      SubscriberVolumeRow volume = null;
      var i = 0;
      foreach (var vol in volumes)
      {
        if (sum <= start && start <= sum + SocialConsts.GetVolumeMaxCountForPosition(i++))
        {
          volume = vol;
          break;
        }
        sum += vol.Count;
      }
      if(volume == null) yield break;
      var _start = start - sum;
      var qry = Queries.FindSubscribers<SubscriberRow>(volume.G_SubscriberVolume, _start, count);
      var subscribers = ForNode(volume.G_SubscriberVolume).LoadEnumerable(qry);
      foreach (var subscriber in subscribers)
      {
        yield return DoGetNode(subscriber.G_Subscriber);
      }
    }
  }
}