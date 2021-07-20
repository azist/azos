/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Data.Idgen;

namespace Azos.Sky.EventHub
{
  /// <summary>
  /// Embodies data for event with raw byte[] content payload.
  /// Events are equated using their immutable ID, not reference.
  /// Obtain new event instances by calling <see cref="IEventProducer.MakeNew(Atom, byte[], string)"/>
  /// </summary>
  [Serializable]
  public sealed class Event : TypedDoc, IDistributedStableHashProvider
  {
    public const int MAX_HEADERS_LENGTH = 8 * 1024;
    public const int MAX_CONTENT_LENGTH = 4 * 1024 * 1024;

    internal Event(){ }//serializer

    /// <summary>
    /// Immutable event id, primary key, monotonically increasing
    /// </summary>
    [Field(key: true, required: true, Description = "Immutable event id, primary key, monotonically increasing")]
    public GDID Gdid { get; internal set; }

    /// <summary>
    /// Unix timestamp with ms resolution - when event was triggered at Origin
    /// </summary>
    [Field(required: true,  Description = "Unix timestamp with ms resolution - when event was triggered at Origin")]
    public ulong CreateUtc { get; internal set; }

    /// <summary>
    /// The id of cluster origin region/zone where the event was first triggered, among other things
    /// this value is used to prevent circular traffic - in multi-master situations so the
    /// same event does not get replicated multiple times across regions (data centers)
    /// </summary>
    [Field(required: true, Description = "Id of cluster origin zone/region")]
    public Atom Origin { get; internal set; }

    /// <summary>
    /// When event was filed - written to disk/storage - this may change
    /// between cluster regions. Checkpoints work of CheckpointUtc - a queue is a stream sorted by CheckpointUtc ascending.
    /// Clients consume events in queues sequentially in the order of production in the same <see cref="Origin"/>
    /// </summary>
    [Field(Description = "When event was filed - written to disk/storage - this may change between cluster regions")]
    public ulong CheckpointUtc { get; internal set; }

    /// <summary>Optional header content </summary>
    [Field(maxLength: MAX_HEADERS_LENGTH, Description = "Optional header content")]
    public string Headers { get; internal set; }

    /// <summary>Content type e.g. json</summary>
    [Field(Description = "Content type")]
    public Atom ContentType { get; internal set; }

    /// <summary> Raw event content </summary>
    [Field(required: true, maxLength: MAX_CONTENT_LENGTH, Description = "Raw event content")]
    public byte[] Content { get; internal set; }

    public override string ToString() => $"Event({Gdid})";
    public ulong GetDistributedStableHash() => Gdid.GetDistributedStableHash();
  }
}
