/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Serialization.JSON;

namespace Azos.Sky.EventHub
{
  /// <summary>
  /// Provides event-handling extensions
  /// </summary>
  public static class Extensions
  {
    private static readonly Encoding  JSON_ENCODING = new  UTF8Encoding(false);

    public static readonly Atom CONTENT_TYPE_JSON_DOC = Atom.Encode("docjson");

    /// <summary>
    /// Creates a <see cref="Event"/> instance out of <see cref="EventDocument"/> instance
    /// which can then be posted into IEventProducer. You can use this method to obtain raw event
    /// once in order to possibly re-try posting if post fails. The system maintains idempotency of
    /// Event posts, therefore a caller may hang to returned Event instance to retry failed posting
    /// </summary>
    /// <param name="producer">Producer to eventually post event into</param>
    /// <param name="evtDoc">EventDocument-derivative instance</param>
    /// <param name="headers">Event headers or null</param>
    /// <returns>
    ///   A tuple of (EventAttribute attr, ShardKey partition, Event evt) suitable for
    ///   making a call to<see cref="IEventProducer.PostAsync(Route, ShardKey, Event, DataLossMode)"/>
    /// </returns>
    public static (EventAttribute attr, ShardKey partition, Event evt) GetRawEventForDocument(this IEventProducer producer,
                                                                 EventDocument evtDoc,
                                                                 JsonDataMap headers = null)
    {
      var partition = evtDoc.NonNull(nameof(evtDoc)).GetEventPartition();
      var eventHeaders = evtDoc.GetEventHeaders(headers);

      var rawHeaders = eventHeaders != null ? JsonWriter.Write(eventHeaders, JsonWritingOptions.CompactRowsAsMap) : null;
      var rawJson = JsonWriter.WriteToBuffer(evtDoc, JsonWritingOptions.CompactRowsAsMap, JSON_ENCODING);

      var rawEvent = producer.NonNull(nameof(producer))
                             .MakeNew(CONTENT_TYPE_JSON_DOC, rawJson, rawHeaders);

      var attr = EventAttribute.GetFor(evtDoc.GetType());

      return (attr, partition, rawEvent);
    }

    /// <summary>
    /// Post event document using processing Route obtained from EventDocument instance and
    /// serializing document using json. The content type header is not set by this method.
    /// Note: When returned WriteResult does not satisfy caller requirements, this method does NOT
    /// give caller a chance to re-post the SAME raw <see cref="Event"/> instance as the onus
    /// of failure handling is on the caller.
    /// You can use <see cref="GetRawEventForDocument(IEventProducer, EventDocument, JsonDataMap)"/>
    /// if you want to re-post the SAME event instance
    /// </summary>
    /// <param name="producer">Producer to post event into</param>
    /// <param name="evtDoc">EventDocument-derivative instance</param>
    /// <param name="headers">Event headers or null</param>
    /// <param name="lossMode">Data loss mode, if null then default used from doc declaration</param>
    /// <returns>WriteResult - how many nodes tried/succeeded/failed</returns>
    public static async Task<WriteResult> PostEventDocumentAsync(this IEventProducer producer,
                                                                 EventDocument evtDoc,
                                                                 JsonDataMap headers = null,
                                                                 DataLossMode? lossMode = null)
    {
      var (attr, partition, rawEvent) = GetRawEventForDocument(producer, evtDoc, headers);
      var result = await producer.PostAsync(attr.Route, partition, rawEvent, lossMode ?? attr.LossMode).ConfigureAwait(false);
      return result;
    }

    /// <summary>
    /// Fetches raw events along with their deserialized EventDocument-derived instances when possible, returning an enumerable of
    /// (raw, doc, error) tuples
    /// </summary>
    /// <param name="consumer">Event consumer implementation</param>
    /// <param name="route">Queue designator</param>
    /// <param name="partition">Logical partition to fetch from <see cref="IEventConsumer.PartitionCount"/></param>
    /// <param name="checkpoint">A point in time as of which to fetch</param>
    /// <param name="count">Number of events to fetch</param>
    /// <param name="lossMode">Data loss tolerance</param>
    /// <returns>
    ///  A tuple of `raw` event representation, its converted EventDocument-derived instance `doc`, and an error (if any) which surfaced
    ///  during event doc deserialization attempt, thus `doc` and `err` are mutually exclusive
    /// </returns>
    public static async Task<IEnumerable<(Event raw, EventDocument doc, Exception err)>> FetchEventDocsAsync(this IEventConsumer consumer,
                                                                                           Route route,
                                                                                           int partition,
                                                                                           ulong checkpoint,
                                                                                           int count,
                                                                                           DataLossMode lossMode = DataLossMode.Default)
    {
      var got = await consumer.NonNull(nameof(consumer))
                              .FetchAsync(route, partition, checkpoint, count, lossMode);

      using(var ms = new IO.BufferSegmentReadingStream())
      {
        return got.Select(e => {

          EventDocument doc = null;
          Exception error = null;

          try
          {
            if (e.ContentType == CONTENT_TYPE_JSON_DOC && e.Content != null)
            {
              ms.UnsafeBindBuffer(e.Content, 0, e.Content.Length);
              var map = JsonReader.DeserializeDataObject(ms, JSON_ENCODING, true) as JsonDataMap;
              doc = JsonReader.ToDoc<EventDocument>(map, fromUI: false);
            }
          }
          catch(Exception err)
          {
            error = err;
          }

          return (raw: e, doc: doc, err: error);
        }).ToArray();
      }
    }


  }
}
