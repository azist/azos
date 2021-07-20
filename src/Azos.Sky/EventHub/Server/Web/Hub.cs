/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Wave;
using Azos.Wave.Mvc;

using Azos.Sky.Security.Permissions.EventHub;

namespace Azos.Sky.EventHub.Server.Web
{
  [NoCache]
  [ApiControllerDoc(
    BaseUri = "/event/hub",
    Connection = "default/keep alive",
    Title = "Event Hub Controller",
    Authentication = "Token/Default",
    Description = "Provides REST API for IEventProducer/IEventConsumer contracts",
    TypeSchemas = new[]{typeof(EventProducerPermission), typeof(EventConsumerPermission) }
  )]
  [Release(ReleaseType.Preview, 2021, 07, 20, "Initial Release", Description = "Preview release of API")]
  public class Hub : ApiProtocolController
  {

    [Inject] IEventHubServerLogic m_Server;

    [ApiEndpointDoc(Title = "Event",
                    Uri = "event",
                    Description = "Posts an event into event hub queue for consumption `{@Event}`",
                    Methods = new[] { "POST = post `{@Event}` for insertion into archive" },
                    RequestHeaders = new[] { API_DOC_HDR_ACCEPT_JSON },
                    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE },
                    RequestBody = "JSON representation of (Atom ns, Atom queue, `{@Event}`)",
                    ResponseContent = "Api Change Result")]
    [ActionOnPost(Name = "event"), AcceptsJson]
    [EventProducerPermission]
    public async Task<object> PostEvent(Atom ns, Atom queue, Event evt)
    {
      if (ns.IsZero || !ns.IsValid) throw HTTPStatusException.BadRequest_400("invalid ns");
      if (queue.IsZero || !queue.IsValid) throw HTTPStatusException.BadRequest_400("invalid queue");

      var changeResult = await m_Server.WriteAsync(ns, queue, evt).ConfigureAwait(false);

      return new {OK = true, data = changeResult};
    }

    [ApiEndpointDoc(Title = "Feed",
                    Uri = "feed",
                    Description = "Retrieves the events from the specified namespace/queue",
                    Methods = new[] { "POST = post request object for fetch execution", "GET = get specified events" },
                    RequestHeaders = new[] { API_DOC_HDR_ACCEPT_JSON },
                    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE },
                    RequestBody = "JSON body or parameters: (Atom ns, Atom queue, ulong checkpoint, int count, bool onlyid)",
                    RequestQueryParameters = new[] {"ns = Atom, namepsace",
                                                    "queue = Atom, queue id",
                                                    "checkpoint = ulong, point in time",
                                                    "count = int, how many top return",
                                                    "onlyid = bool, when true returns only event ids, not payload"},
                    ResponseContent = "JSON filter result - enumerable of `{@Event}`",
                    TypeSchemas = new[] { typeof(Event) })]
    [Action(Name = "feed"), AcceptsJson]
    [EventConsumerPermission]
    public async Task<object> Feed(Atom ns, Atom queue, ulong checkpoint, int count, bool onlyid)
    {
      if (ns.IsZero || !ns.IsValid) throw HTTPStatusException.BadRequest_400("invalid ns");
      if (queue.IsZero || !queue.IsValid) throw HTTPStatusException.BadRequest_400("invalid queue");

      return null;

      //var changeResult = await m_Server.WriteAsync(ns, queue, evt).ConfigureAwait(false);

      //return new { OK = true, data = changeResult };
    }
  }

}
