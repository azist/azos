/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Log;

namespace Azos.Apps.Hosting
{
  /// <summary>
  /// Provides services for managing subordinate processes
  /// </summary>
  public class GovernorDaemon : DaemonWithInstrumentation<IApplicationComponent>
  {
    public const string CONFIG_ACTIVATOR_SECTION = "activator";
    public const string CONFIG_APP_SECTION = "app";

    protected GovernorDaemon(IApplication app) : base(app)
    {
      m_Applications = new OrderedRegistry<App>();
    }

    private OrderedRegistry<App> m_Applications;
    private IAppActivator m_Activator;
    private GovernorSipcServer m_Server;
    private int m_ServerStartPort;
    private int m_ServerEndPort;

    public override bool InstrumentationEnabled { get; set; }
    public override string ComponentLogTopic => Sky.SysConsts.LOG_TOPIC_HOST_GOV;

    public IRegistry<App> Applications => m_Applications;

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_APP)]
    public int ServerStartPort
    {
      get => m_ServerStartPort;
      set => m_ServerStartPort = SetOnInactiveDaemon(value);
    }

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_APP)]
    public int ServerEndPort
    {
      get => m_ServerEndPort;
      set => m_ServerEndPort = SetOnInactiveDaemon(value);
    }

    /// <summary>
    /// Returns the assigned IPC port for active server or zero
    /// </summary>
    public int AssignedSipcServerPort
    {
      get
      {
        var srv = m_Server;
        if (Running && srv != null) return srv.AssignedPort;
        return 0;
      }
    }


    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      m_Applications.Clear();

      if (node == null) return;

      var nActivator = node[CONFIG_ACTIVATOR_SECTION];
      m_Activator = FactoryUtils.MakeDirectedComponent<IAppActivator>(this, nActivator, typeof(ProcessAppActivator), new []{ nActivator });

      foreach(var napp in node.ChildrenNamed(CONFIG_APP_SECTION))
      {
        var app = FactoryUtils.MakeDirectedComponent<App>(this, napp, typeof(App), new []{ napp });
        if (!m_Applications.Register(app))
        {
          var etxt = "Config error: duplicate application id `{0}`".Args(app.Name);
          WriteLogFromHere(MessageType.CatastrophicError, etxt);
          throw new AppHostingException(etxt);
        }
      }
    }

    private Task m_AsyncStartBody;
    protected override void DoStart()
    {
      base.DoStart();
      m_Server = new GovernorSipcServer(this, m_ServerStartPort, m_ServerEndPort);
      m_Server.Start();

      if (m_Applications.Count == 0)
        WriteLogFromHere(MessageType.Warning, "No applications registered");

      //Asynchronously start
      m_AsyncStartBody = Task.Factory.StartNew(() =>
      {
        try
        {
          startAll().GetAwaiter().GetResult();
        }
        catch(Exception error)
        {
          WriteLogFromHere(MessageType.CatastrophicError, "DoStart()..startAll() leaked: "+error.ToMessageWithType(), error);
        }
      }
      , TaskCreationOptions.LongRunning);
    }


    protected override void DoSignalStop()
    {
      base.DoSignalStop();
    }

    protected override void DoWaitForCompleteStop()
    {
      if (m_AsyncStartBody != null)
      {
        var towait = m_AsyncStartBody;
        m_AsyncStartBody = null;
        towait.Wait();
      }

      try
      {
        stopAll().GetAwaiter().GetResult();
      }
      catch(Exception error)
      {
        WriteLogFromHere(MessageType.CatastrophicError, "stopAll() leaked: " + error.ToMessageWithType(), error);
      }

      DisposeAndNull(ref m_Server);
      base.DoWaitForCompleteStop();
    }

    private async Task startAll()
    {
      const int SLICE_MS = 100;
      foreach(var app in m_Applications.OrderedValues)
      {
        for(var i=0; i < app.StartDelayMs; i+=SLICE_MS)
        {
          if (!Running) break;
          await Task.Delay(SLICE_MS);
        }

        if (!Running) break;

        try
        {
          m_Activator.StartApplication(app);
        }
        catch(Exception error)
        {
          WriteLogFromHere(MessageType.CatastrophicError, "Activator.StartApplication(`{0}`) leaked: {1}".Args(app.Name, error.ToMessageWithType()), error);
        }
      }
    }

    private async Task stopAll()
    {
      const int SLICE_MS = 100;
      foreach (var app in m_Applications.OrderedValues.Reverse())
      {
        try
        {
          //even if not Running we still need to stop
          m_Activator.StopApplication(app);
        }
        catch (Exception error)
        {
          WriteLogFromHere(MessageType.CatastrophicError, "Activator.StopApplication(`{0}`) leaked: {1}".Args(app.Name, error.ToMessageWithType()), error);
        }

        //bypass delay if daemon is terminating
        for (var i = 0; Running && i < app.StopDelayMs; i += SLICE_MS)
        {
          await Task.Delay(SLICE_MS);
        }
      }
    }//stopAll

  }
}
