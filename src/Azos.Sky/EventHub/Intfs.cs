/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;

namespace Azos.Sky.EventHub
{
  /// <summary>
  /// Produces events into named queues.
  /// An events gets written into multiple servers depending on the <see cref="DataLossMode"/> requirement.
  /// The queues are optionally partitioned(sharded) for scalability.
  /// </summary>
  public interface IEventProducer
  {
    /// <summary>
    /// This cluster region/zone id tag
    /// </summary>
    Atom Origin { get; }

    /// <summary>
    /// Creates an instance of Event initialized with payload and optional headers
    /// </summary>
    /// <param name="contentType">Event content type</param>
    /// <param name="content">Event content. You can specify up to <see cref="Event.MAX_CONTENT_LENGTH"/></param>
    /// <param name="headers">Optional event headers. You can specify up to <see cref="Event.MAX_HEADERS_LENGTH"/></param>
    /// <returns>New instance of Event initialized with cluster-unique ID/precision time stamp</returns>
    Event MakeNew(Atom contentType, byte[] content, string headers = null);

    /// <summary>
    /// Posts event into the queue. Throws if the cluster can not satisfy the requested DataLossMode requirement in principle.
    /// The <see cref="Event.Gdid"/> is treated as IDEMPOTENCY key - an attempt to post and event with the same GDID
    /// on the same node will have no effect
    /// </summary>
    /// <param name="route">Specifies what Namespace.Queue the event should be routed into <seealso cref="Route"/></param>
    /// <param name="partition">
    ///  A partition(a shard) is cohort (a group) of distinct servers which handle failover.
    ///  With the increase of <see cref="DataLossMode"/> requirement, the system tries to post message in more servers within partition
    /// </param>
    /// <param name="evt">Event to post</param>
    /// <param name="lossMode">
    ///   The acceptable data loss, the better the guarantees are the longer it takes to write event to more partitions.
    ///   Analyze the returned WriteResult
    /// </param>
    /// <returns>The result of write operation</returns>
    Task<WriteResult> PostAsync(Route route, ShardKey partition, Event evt, DataLossMode lossMode = DataLossMode.Default);
  }

  /// <summary>
  /// Implementation of IEventProducer
  /// </summary>
  public interface IEventProducerLogic : IEventProducer, IModuleImplementation
  {
  }

  /// <summary>
  /// Consumes events from queue
  /// </summary>
  public interface IEventConsumer
  {
    /// <summary>
    /// This cluster region/zone id tag
    /// </summary>
    Atom Origin { get; }

    /// <summary>
    /// Specifies how many partitions (shards) the system has per Origin.
    /// A partition is cohort (a group) of distinct servers which handle failover.
    /// Consumers (event subscribers) may poll events from queue in every partition up to the specified count
    /// of distinct partitions.
    /// </summary>
    int PartitionCount { get; }

    /// <summary>
    /// Fetches the count of events from the specified queue route starting at the specified checkpoint ordered by <see cref="Event.CheckpointUtc"/>
    /// optionally skipping a specified number of events at the beginning
    /// </summary>
    /// <param name="route">Specifies what Network, Namespace, Queue to fetch events from</param>
    /// <param name="partition">Partition number, from 0 to <see cref="PartitionCount"/></param>
    /// <param name="checkpoint">A point in time as of which to fetch</param>
    /// <param name="skip">How many events to skip before starting fetching as of the specified checkpoint, use 0 for most cases</param>
    /// <param name="count">How many events to fetch</param>
    /// <param name="lossMode">The amount of tolerable data loss</param>
    /// <returns>Enumerable of events</returns>
    Task<IEnumerable<Event>> FetchAsync(Route route, int partition, ulong checkpoint, int skip, int count, DataLossMode lossMode = DataLossMode.Default);

    /// <summary>
    /// Gets the checkpoint for the consumer
    /// </summary>
    Task<ulong> GetCheckpointAsync(Route route, int partition, string idConsumer, DataLossMode lossMode = DataLossMode.Default);

    /// <summary>
    /// Sets the checkpoint for the specified consumer
    /// </summary>
    Task<WriteResult> SetCheckpointAsync(Route route, int partition, string idConsumer, ulong checkpoint, DataLossMode lossMode = DataLossMode.Default);
  }

  /// <summary>
  /// Implementation of IEventConsumer
  /// </summary>
  public interface IEventConsumerLogic : IEventConsumer, IModuleImplementation
  {
  }
}
