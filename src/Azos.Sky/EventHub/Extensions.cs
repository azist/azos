/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
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
    /// Post event document using processing Route obtained from EventDocument instance and
    /// serializing document using json. The content type header is not set by this method
    /// </summary>
    /// <param name="producer">Producer to post event into</param>
    /// <param name="evtDoc">EventDocument-derivative instance</param>
    /// <param name="target">An optional target context name/id</param>
    /// <returns>WriteResult - how many nodes tried/succeeded/failed</returns>
    public static async Task<WriteResult> PostEventJsonAsync(this IEventProducer producer, EventDocument evtDoc, string target = null)
    {
      var route = evtDoc.NonNull(nameof(evtDoc)).GetEventRoute(target);
      var hdrs = evtDoc.GetEventHeaders(target);

      var rawJson = JsonWriter.WriteToBuffer(evtDoc, JsonWritingOptions.CompactRowsAsMap, JSON_ENCODING);

      var rawEvent = producer.NonNull(nameof(producer)).MakeNew(CONTENT_TYPE_JSON_DOC, rawJson, hdrs);

      return await producer.PostAsync(route, rawEvent).ConfigureAwait(false);
    }


    ////todo Fetch etc...
    //public static async Task<IEnumerable<(Event raw, EventDocument doc)>> FetchEventsAsync(Route route, ulong checkpoint, int count)
    //{

    //}


  }
}
