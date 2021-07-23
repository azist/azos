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
using Azos.Conf;
using Azos.Data;
using Azos.Log;
using Azos.Serialization.JSON;
using Azos.Sky.Identification;
using Azos.Wave;

namespace Azos.Sky.EventHub.Server
{
  /// <summary>
  /// Provides server implementation for IEventHubServerLogic
  /// </summary>
  public sealed class EventHubServerLogic : ModuleBase, IEventHubServerLogic
  {
    //public const string CONFIG_STORE_SECTION = "store";
    public const int CONSUMER_ID_MAX_LEN = 255;

    public EventHubServerLogic(IApplication application) : base(application) { }
    public EventHubServerLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      //DisposeAndNull(ref m_*);
      base.Destructor();
    }

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.QUEUE_TOPIC;


    #region Protected plumbing
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      if (node == null) return;
    }

    protected override bool DoApplicationAfterInit()
    {
      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      return base.DoApplicationBeforeCleanup();
    }
    #endregion

    #region IEventHubServerLogic
    public Task<ChangeResult> WriteAsync(Atom ns, Atom queue, Event evt)
    {
      checkRoute(ns, queue);
      throw new NotImplementedException();
    }

    public Task<ChangeResult> FetchAsync(Atom ns, Atom queue, ulong checkpoint, int count, bool onlyid)
    {
      checkRoute(ns, queue);
      throw new NotImplementedException();
    }

    public Task<ulong> GetCheckpointAsync(Atom ns, Atom queue, string consumer)
    {
      checkRoute(ns, queue);
      consumer.NonBlankMax(512, nameof(consumer));
      throw new NotImplementedException();
    }

    public Task SetCheckpointAsync(Atom ns, Atom queue, string consumer, ulong checkpoint)
    {
      checkRoute(ns, queue);
      consumer.NonBlankMax(512, nameof(consumer));
      throw new NotImplementedException();
    }
    #endregion

    #region .pvt
    private void checkRoute(Atom ns, Atom queue)
    {
      if (ns.IsZero || !ns.IsValid) throw HTTPStatusException.BadRequest_400("invalid ns");
      if (queue.IsZero || !queue.IsValid) throw HTTPStatusException.BadRequest_400("invalid queue");
    }
    #endregion


  }
}

