/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azos.Sky.Bus
{
  public struct QueueSpec
  {
    public readonly Atom Namespace;
    public readonly Atom Queue;
    public readonly ShardKey Partition;
  }


  public struct EventId
  {
    public readonly ulong CreateUtc;
    public readonly int Counter;
    public readonly byte NodeDiscriminator;

    public DateTime CreateDateTimeUtc => CreateUtc.FromMillisecondsSinceUnixEpochStart();//todo: Use different epoch (e.g. 1/1/2021) to make timestamps smaller
  }

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
    Task<double> PostAsync(QueueSpec queue, Event evt);
  }

  /// <summary>
  ///
  /// </summary>
  public interface IEventConsumer
  {
    /// <summary>
    /// Fetches the count of messages starting at the specified checkpoint
    /// </summary>
    /// <param name="ns"></param>
    /// <param name="queue"></param>
    /// <param name="shard"></param>
    /// <param name="checkpoint"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    Task<IEnumerable<Event>> FetchAsync(QueueSpec queue, ulong checkpoint, int count);

    /// <summary>
    /// Gets the checkpoint for the consumer
    /// </summary>
    /// <param name="ns"></param>
    /// <param name="queue"></param>
    /// <param name="shard"></param>
    /// <param name="idConsumer"></param>
    /// <returns></returns>
    Task<ulong> GetCheckpoint(QueueSpec queue, string idConsumer);

    /// <summary>
    /// Sets the checkpoint for the specified consumer
    /// </summary>
    /// <param name="ns"></param>
    /// <param name="queue"></param>
    /// <param name="shard"></param>
    /// <param name="idConsumer"></param>
    /// <param name="checkpoint"></param>
    /// <returns></returns>
    Task<double> SetCheckpoint(QueueSpec queue, string idConsumer, ulong checkpoint);
  }
}
