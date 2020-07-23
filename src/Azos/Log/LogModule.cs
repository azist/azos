/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Log
{
  /// <summary>
  /// Default implementation of log service as module
  /// </summary>
  public class LogModule : ModuleBase, ILogModuleImplementation
  {
    protected LogModule(IApplication application) : base(application) { }
    protected LogModule(IModule parent) : base(parent) { }

    private ILogImplementation m_Log;

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.LOG_TOPIC;

    [Config]
    public Atom DefaultChannel { get; private set; }

    public ILog Log => m_Log ?? new NOPLog(App);

    protected override void DoConfigure(IConfigSectionNode node)
    {
      var nlog = node.NonEmpty(nameof(node))[CommonApplicationLogic.CONFIG_LOG_SECTION]
                     .NonEmpty($"`{CommonApplicationLogic.CONFIG_LOG_SECTION}` config section");

      DisposeAndNull(ref m_Log);

      m_Log = FactoryUtils.MakeAndConfigureDirectedComponent<ILogImplementation>(this, nlog, typeof(LogDaemon));

      base.DoConfigure(node);
    }

    protected override bool DoApplicationAfterInit()
    {
      m_Log.NonNull("`{0}` not configured".Args(CommonApplicationLogic.CONFIG_LOG_SECTION));

      if (m_Log is Daemon d)
        d.StartByApplication();

      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      DisposeAndNull(ref m_Log);
      return base.DoApplicationBeforeCleanup();
    }
  }
}
