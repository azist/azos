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

namespace Azos.Sky.EventHub.Server
{
  /// <summary>
  /// Provides server implementation for ILogChronicle and  IInstrumentationChronicle
  /// </summary>
  public sealed class EventHubServerLogic : ModuleBase, IEventHubServerLogic
  {
    //public const string CONFIG_STORE_SECTION = "store";

    public EventHubServerLogic(IApplication application) : base(application) { }
    public EventHubServerLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      //DisposeAndNull(ref m_*);
      base.Destructor();
    }

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.QUEUE_TOPIC;



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

    public Task<ChangeResult> WriteAsync(Atom ns, Atom queue, Event evt)
    {
      throw new NotImplementedException("Under construction");
    }
  }
}

