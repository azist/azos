/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Apps;
using Azos.Conf;

namespace Azos.Sky.Chronicle.Feed
{
  /// <summary>
  /// Pulls chronicle data feed from X number of channels into local receptacle
  /// </summary>
  public sealed class PullAgentModule : ModuleBase
  {
    public const string CONFIG_AGENT_SECTION = "agent";

    public PullAgentModule(IApplication application) : base(application) { }
    public PullAgentModule(IModule parent) : base(parent) { }

    private PullAgentDaemon m_Agent;

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.LOG_TOPIC;


    public PullAgentDaemon Agent => m_Agent;

    protected override void DoConfigure(IConfigSectionNode node)
    {
      var nAgent = node.NonEmpty(nameof(node))[CONFIG_AGENT_SECTION]
                       .NonEmpty($"`{CONFIG_AGENT_SECTION}` config section");

      DisposeAndNull(ref m_Agent);

      m_Agent = FactoryUtils.MakeAndConfigureDirectedComponent<PullAgentDaemon>(this, nAgent, typeof(PullAgentDaemon));

      base.DoConfigure(node);
    }

    protected override bool DoApplicationAfterInit()
    {
      m_Agent.NonNull("`{0}` not configured".Args(CONFIG_AGENT_SECTION));
      m_Agent.StartByApplication();
      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      DisposeAndNull(ref m_Agent);
      return base.DoApplicationBeforeCleanup();
    }
  }
}
