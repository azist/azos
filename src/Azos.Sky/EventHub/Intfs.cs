/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azos.Sky.EventHub
{

  public struct Event
  {
    public readonly EventId Id;
    public readonly byte[] Payload;
  }

  /// <summary>
  /// Produces events into named queues
  /// </summary>
  public interface IEventProducer
  {
    //EventBuilder Builder { get; }

    /// <summary>
    /// Posts a message into queue
    /// </summary>
    Task<PostResult> PostAsync(Route route, Event evt);
  }

  /// <summary>
  ///
  /// </summary>
  public interface IEventConsumer
  {
    /// <summary>
    /// Fetches the count of messages starting at the specified checkpoint
    /// </summary>
    Task<IEnumerable<Event>> FetchAsync(Route route, ulong checkpoint, int count);

    /// <summary>
    /// Gets the checkpoint for the consumer
    /// </summary>
    Task<ulong> GetCheckpoint(Route route, string idConsumer);

    /// <summary>
    /// Sets the checkpoint for the specified consumer
    /// </summary>
    Task<PostResult> SetCheckpoint(Route route, string idConsumer, ulong checkpoint);
  }
}
