/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data.Idgen;

namespace Azos.Sky.EventHub
{
  /// <summary>
  /// Embodies data for event with raw byte[] payload.
  /// Events are equated using their immutable ID, thus every event must have a unique id.
  /// You generate event instances by calling <see cref="IEventProducer.MakeNew(byte[], EventHeader[])"/> which
  /// generates cluster-wide unique ids
  /// </summary>
  public struct Event : IEquatable<Event>, IDistributedStableHashProvider
  {
    /// <summary>
    /// Initializes event. This is more of an infrastructure method and most business
    /// applications should use <see cref="IEventProducer.MakeNew(byte[], EventHeader[])"/> instead
    /// </summary>
    public Event(EventId id, EventHeaders headers, byte[] payload)
    {
      Id = id;
      Headers = headers;
      Payload = payload;
    }

    /// <summary>Cluster-Unique event id with timestamp</summary>
    public readonly EventId Id;

    /// <summary>Optional headers </summary>
    public readonly EventHeaders Headers;

    /// <summary> Raw event payload </summary>
    public readonly byte[] Payload;

    public bool Assigned => Id.Assigned;

    public bool Equals(Event other) => Id == other.Id;
    public override bool Equals(object obj) => obj is Event other ? this.Equals(other) : false;
    public override int GetHashCode() => Id.GetHashCode();
    public ulong GetDistributedStableHash() => Id.GetDistributedStableHash();

    public static bool operator ==(Event left, Event right) => left.Equals(right);
    public static bool operator !=(Event left, Event right) => !left.Equals(right);
  }
}
