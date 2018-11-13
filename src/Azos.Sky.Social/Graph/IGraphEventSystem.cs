using System.Collections.Generic;

using Azos.Data;
using Azos.Glue;

using Azos.Sky.Contracts;

namespace Azos.Sky.Social.Graph
{
  /// <summary>
  /// Handles social graph functionality dealing with event subscription and broadcasting
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface IGraphEventSystem : ISkyService
  {
    /// <summary>
    /// Emits the event - notifies all subscribers (watchers, friends etc.) about the event.
    /// The physical notification happens via IGraphHost implementation
    /// </summary>
    void EmitEvent(Event evt);

    /// <summary>
    /// Subscribes recipient node to the emitter node. Unlike friends the subscription connection is unidirectional
    /// </summary>
    void Subscribe(GDID gRecipientNode, GDID gEmitterNode, byte[] parameters);

    /// <summary>
    /// Removes the subscription. Unlike friends the subscription connection is unidirectional
    /// </summary>
    void Unsubscribe(GDID gRecipientNode, GDID gEmitterNode);

    /// <summary>
    /// Returns an estimated approximate number of subscribers that an emitter has
    /// </summary>
    long EstimateSubscriberCount(GDID gEmitterNode);

    /// <summary>
    /// Returns Subscribers for Emitter from start position
    /// </summary>
    IEnumerable<GraphNode> GetSubscribers(GDID gEmitterNode, long start, int count);
  }

  /// <summary>
  /// Contract for client of IGraphEventSystem svc
  /// </summary>
  public interface IGraphEventSystemClient : ISkyServiceClient, IGraphEventSystem
  {
    //todo Add async versions
  }
}
