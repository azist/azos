/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Client;
using Azos.Conf;
using Azos.Log;
using Azos.Serialization.JSON;
using Azos.Sky.Identification;
using Azos.Web;

namespace Azos.Sky.EventHub
{
  /// <summary>
  /// Provides client implementation for IEventProducerLogic and IEventConsumerLogic
  /// </summary>
  public sealed class EventHubClientLogic : ModuleBase, IEventProducerLogic, IEventConsumerLogic
  {
    public const string CONFIG_SERVICE_SECTION = "service";

    public EventHubClientLogic(IApplication application) : base(application) { }
    public EventHubClientLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Server);
      base.Destructor();
    }


    [Inject] IGdidProviderModule m_Gdid;

    private HttpService m_Server;


    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.QUEUE_TOPIC;


    /// <summary>
    /// Logical service address of queue
    /// </summary>
    [Config]
    public string QueueServiceAddress { get; set; }

    /// <summary>
    /// Logical cluster region/zone id
    /// </summary>
    [Config]
    public Atom Origin {  get; set; }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      DisposeAndNull(ref m_Server);
      if (node == null) return;

      var nServer = node[CONFIG_SERVICE_SECTION];
      m_Server = FactoryUtils.MakeDirectedComponent<HttpService>(this,
                                                                 nServer,
                                                                 typeof(HttpService),
                                                                 new object[] { nServer });
    }

    protected override bool DoApplicationAfterInit()
    {
      m_Server.NonNull("Not configured Server of config section `{0}`".Args(CONFIG_SERVICE_SECTION));
      Origin.IsTrue(v => !v.IsZero && v.IsValid, "Origin");
      QueueServiceAddress.NonBlank(nameof(QueueServiceAddress));

      return base.DoApplicationAfterInit();
    }

    public Event MakeNew(Atom contentType, byte[] content, string headers = null)
    {
      contentType.IsTrue(v => v.IsZero || v.IsValid);
      content.NonNull(nameof(content)).IsTrue( v => content.Length < Event.MAX_CONTENT_LENGTH, "content.len < MAX_LEN");
      if (headers != null) headers.IsTrue(v => v.Length < Event.MAX_HEADERS_LENGTH, "header.len < MAX_LEN");

      var gdid = m_Gdid.Provider.GenerateOneGdid(SysConsts.GDID_NS_EVENTHUB, SysConsts.GDID_SEQ_EVENTHUB);

      var result = new Event
      {
        Gdid = gdid,
        Origin = this.Origin,
        CheckpointUtc = 0, //not written to disk yet as of right now
        CreateUtc = App.TimeSource.UTCNow.ToUnsignedMillisecondsSinceUnixEpochStart(),
        Headers = headers,
        ContentType = contentType,
        Content = content
      };

      return result;
    }

    public async Task<WriteResult> PostAsync(Route route, Event evt)
    {
      route.IsTrue(v => v.Assigned, "assigned Route");
      var ve = evt.NonNull(nameof(evt)).Validate();
      if (ve != null) throw ve;

      //send
      return new WriteResult();
    }

    public Task<IEnumerable<Event>> FetchAsync(Route route, ulong checkpoint, int count)
    {
      throw new NotImplementedException();
    }

    public Task<ulong> GetCheckpoint(Route route, string idConsumer)
    {
      throw new NotImplementedException();
    }

    public Task<WriteResult> SetCheckpoint(Route route, string idConsumer, ulong checkpoint)
    {
      throw new NotImplementedException();
    }
  }
}
