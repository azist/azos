/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Net.Sockets;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.IO.Sipc;
using Azos.Log;

namespace Azos.Apps.Hosting
{
  /// <summary>
  /// Provides services for managing subordinate processes
  /// </summary>
  public class GovernorDaemon : DaemonWithInstrumentation<IApplicationComponent>
  {
    public const string CONFIG_ACTIVATOR_SECTION = "activator";

    protected GovernorDaemon(IApplication app) : base(app)
    {
      m_Applications = new Registry<App>();
    }

    private Registry<App> m_Applications;
    private IAppActivator m_Activator;
    private GovernorSipcServer m_Server;
    private int m_ServerStartPort;
    private int m_ServerEndPort;

    public override bool InstrumentationEnabled { get; set; }
    public override string ComponentLogTopic => Sky.SysConsts.LOG_TOPIC_HOST_GOV;

    public IRegistry<App> Applications => m_Applications;

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE)]
    public int ServerStartPort
    {
      get => m_ServerStartPort;
      set => m_ServerStartPort = SetOnInactiveDaemon(value);
    }

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE)]
    public int ServerEndPort
    {
      get => m_ServerEndPort;
      set => m_ServerEndPort = SetOnInactiveDaemon(value);
    }



    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      if (node == null) return;

      var nActivator = node[CONFIG_ACTIVATOR_SECTION];
      m_Activator = FactoryUtils.MakeDirectedComponent<IAppActivator>(this, nActivator, typeof(ProcessAppActivator), new []{ nActivator });

    }

    protected override void DoStart()
    {
      base.DoStart();
      m_Server = new GovernorSipcServer(this, m_ServerStartPort, m_ServerEndPort);
      m_Server.Start();

      m_Applications.ForEach(app => m_Activator.StartApplication(app));
    }
    protected override void DoSignalStop()
    {
      m_Applications.ForEach(app => m_Activator.StopApplication(app));
      base.DoSignalStop();
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();
    }
  }
}
