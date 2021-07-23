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
      var result = await m_Server.FetchAsync(ns, queue, checkpoint, count, onlyid).ConfigureAwait(false);
      return new { OK = true, data = result };
    }


    [ApiEndpointDoc(Title = "Get checkpoint",
                    Uri = "checkpoint",
                    Description = "Retrieves checkpoint from the specified namespace/queue",
                    Methods = new[] { "GET = gets checkpoint value" },
                    RequestHeaders = new[] { API_DOC_HDR_ACCEPT_JSON },
                    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE },
                    RequestBody = "JSON body or parameters: (Atom ns, Atom queue, string consumer)",
                    RequestQueryParameters = new[] {"ns = Atom, namepsace",
                                                    "queue = Atom, queue id",
                                                    "consumer = string, consumer identifier"},
                    ResponseContent = "JSON checkpoint result - {OK: true, data: ulong}`")]
    [ActionOnGet(Name = "checkpoint"), AcceptsJson]
    [EventConsumerPermission]
    public async Task<object> GetCheckpoint(Atom ns, Atom queue, string consumer)
    {
      var result = await m_Server.GetCheckpointAsync(ns, queue, consumer).ConfigureAwait(false);
      return new { OK = true, data = result };
    }


    [ApiEndpointDoc(Title = "Post checkpoint",
                    Uri = "checkpoint",
                    Description = "Sets checkpoint for the specified namespace/queue/consumer",
                    Methods = new[] { "POST = sets checkpoint value" },
                    RequestHeaders = new[] { API_DOC_HDR_ACCEPT_JSON },
                    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE },
                    RequestBody = "JSON body or parameters: (Atom ns, Atom queue, string consumer, ulong checkpoint)",
                    RequestQueryParameters = new[] {"ns = Atom, namepsace",
                                                    "queue = Atom, queue id",
                                                    "consumer = string, consumer identifier",
                                                    "checkpoint = ulong, checkpoint value"},
                    ResponseContent = "JSON checkpoint result - {OK: true}")]
    [ActionOnPost(Name = "checkpoint"), AcceptsJson]
    [EventConsumerPermission]
    public async Task<object> PostCheckpoint(Atom ns, Atom queue, string consumer, ulong checkpoint)
    {
      await m_Server.SetCheckpointAsync(ns, queue, consumer, checkpoint).ConfigureAwait(false);
      return new { OK = true };
    }
  }
}
