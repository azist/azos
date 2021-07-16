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
  /// <summary>
  /// Provides event-handling extensions
  /// </summary>
  public static class Extensions
  {
    /// <summary>
    /// Post event document using processing Route obtained from EventDocument instance
    /// </summary>
    /// <param name="producer">Producer to post event into</param>
    /// <param name="evtDoc">EventDocument-derivative instance</param>
    /// <returns>WriteResult - how many nodes tried/succeeded/failed</returns>
    public static async Task<PostResult> PostEventAsync(this IEventProducer producer, EventDocument evtDoc)
    {
      var route = evtDoc.NonNull(nameof(evtDoc)).ProcessingRoute;
      var rawEvent = new Event();//todo create event
      return await producer.NonNull(nameof(producer)).PostAsync(route, rawEvent).ConfigureAwait(false);
    }


    ////todo Fetch etc...
    //public static async Task<IEnumerable<(Event raw, EventDocument doc)>> FetchEventsAsync(Route route, ulong checkpoint, int count)
    //{

    //}


  }
}
