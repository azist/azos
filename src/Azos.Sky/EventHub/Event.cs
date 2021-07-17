/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using Azos.Data;
using Azos.Data.Idgen;
using Azos.Serialization.JSON;

namespace Azos.Sky.EventHub
{
  /// <summary>
  /// Embodies data for event with raw byte[] content payload.
  /// Events are equated using their immutable ID, not reference.
  /// Obtain new event instances by calling <see cref="IEventProducer.MakeNew(Atom, byte[], string)"/>
  /// </summary>
  [Serializable]
  public sealed class Event : IEquatable<Event>, IDistributedStableHashProvider, IJsonReadable, IJsonWritable
  {
    public const int MAX_HEADERS_LENGTH = 8 * 1024;
    public const int MAX_CONTENT_LENGTH = 4 * 1024 * 1024;

    internal Event(){ }//serializer

    /// <summary>
    /// Initializes event. This is an infrastructure method and business applications
    /// should use <see cref="IEventProducer.MakeNew(Atom, byte[], string)"/> instead
    /// </summary>
    internal Event(GDID gdid, ulong createUtc, Atom origin, ulong checkpointUtc, string headers, Atom contentType, byte[] content)
    {
      Gdid = gdid;
      CreateUtc = createUtc;
      Origin = origin;
      CheckpointUtc = checkpointUtc;
      Headers = headers;
      ContentType = contentType;
      Content = content;
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
    /// The id of cluster origin region/zone where the event was first triggered, among other things
    /// this value is used to prevent circular traffic - in multi-master situations so the
    /// same event does not get replicated multiple times across regions (data centers)
    /// </summary>
    public Atom Origin { get; private set; }

    /// <summary>
    /// When event was filed - written to disk/storage - this may change
    /// between cluster regions. Checkpoints work of CheckpointUtc - a queue is a stream sorted by CheckpointUtc ascending.
    /// Clients consume events in queues sequentially in the order of production in the same <see cref="Origin"/>
    /// </summary>
    public ulong CheckpointUtc { get; private set; }

    /// <summary>Optional headers </summary>
    public string Headers { get; private set; }

    /// <summary> Content type </summary>
    public Atom ContentType { get; private set; }

    /// <summary> Raw event content </summary>
    public byte[] Content { get; private set; }


    public bool Equals(Event other) => other != null &&  this.Gdid == other.Gdid;
    public override bool Equals(object obj) => obj is Event other ? this.Equals(other) : false;
    public override int GetHashCode() => Gdid.GetHashCode();
    public ulong GetDistributedStableHash() => Gdid.GetDistributedStableHash();

    public static bool operator ==(Event left, Event right) => left.Equals(right);
    public static bool operator !=(Event left, Event right) => !left.Equals(right);


    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is JsonDataMap map)
      {
        Gdid      = map["id"].AsGDID();
        CreateUtc = map["crt"].AsULong();
        Origin        = map["ori"].AsAtom();
        CheckpointUtc = map["chk"].AsULong();
        Headers       = map["hdr"].AsString();
        ContentType   = map["ctp"].AsAtom();
        Content       = map["c"].AsString().TryFromWebSafeBase64();
        return (true, this);
      }
      return (false, this);
    }

    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
    {
      JsonWriter.WriteMap(wri,
        new JsonDataMap
        {
          {"id", Gdid},
          {"crt", CreateUtc},
          {"ori", Origin},
          {"chk", CheckpointUtc},
          {"hdr", Headers},
          {"ctp", ContentType},
          {"c", Content.ToWebSafeBase64()},
        },
        nestingLevel + 1, options);
    }
  }
}
