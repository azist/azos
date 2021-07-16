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
  /// Embodies data for event with raw byte[] payload.
  /// Events are equated using their immutable ID, thus every event must have a unique id.
  /// You generate event instances by calling <see cref="IEventProducer.MakeNew(byte[], EventHeader[])"/> which
  /// generates cluster-wide unique ids
  /// </summary>
  [Serializable]
  public sealed class Event : IEquatable<Event>, IDistributedStableHashProvider
  {
    internal Event(){ }//serializer

    /// <summary>
    /// Initializes event. This is an infrastructure method and business applications
    /// should use <see cref="IEventProducer.MakeNew(byte[], EventHeader[])"/> instead
    /// </summary>
    public Event(GDID gdid, ulong createUtc, Atom origin, ushort node, ulong fileUtc, EventHeaders headers, byte[] payload)
    {
      Gdid = gdid;
      CreateUtc = createUtc;
      OriginRegion = origin;
      OriginNode = node;
      Headers = headers;
      Payload = payload;
    }



    /// <summary>
    /// Immutable event id, primary key, monotonically increasing
    /// </summary>
    public GDID Gdid { get; private set; }

    /// <summary>
    /// When event was triggered
    /// </summary>
    public ulong CreateUtc { get; private set; }

    /// <summary>
    /// The id of cluster origin region where the event was first triggered, among other things
    /// this value is used to prevent circular traffic - in multi-master situations so the
    /// same event does not get replicated multiple times across regions (data centers)
    /// </summary>
    public Atom OriginRegion { get; private set; }

    /// <summary>
    /// The cluster node id within region
    /// </summary>
    public ushort OriginNode { get; private set; }

    /// <summary>
    /// When event was filed - written to disk/storage - this may change
    /// between cluster regions. Checkpoints work of FileUtc - a queue is a stream sorted by FileUtc ascending
    /// </summary>
    public ulong FileUtc { get; private set; }

    /// <summary>Optional headers </summary>
    public EventHeaders Headers { get; private set; }

    /// <summary> Raw event payload </summary>
    public byte[] Payload { get; private set; }


    public bool Equals(Event other) => other != null &&  this.Gdid == other.Gdid;
    public override bool Equals(object obj) => obj is Event other ? this.Equals(other) : false;
    public override int GetHashCode() => Gdid.GetHashCode();
    public ulong GetDistributedStableHash() => Gdid.GetDistributedStableHash();

    public static bool operator ==(Event left, Event right) => left.Equals(right);
    public static bool operator !=(Event left, Event right) => !left.Equals(right);
  }
}
