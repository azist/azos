/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Threading;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Log;
using Azos.Serialization.JSON;

namespace Azos.Apps.Hosting
{
  /// <summary>
  /// Provides services for managing subordinate processes
  /// </summary>
  public sealed class GovernorDaemon : DaemonWithInstrumentation<IApplicationComponent>
  {
    public const string CONFIG_ACTIVATOR_SECTION = "activator";
    public const string CONFIG_APP_SECTION = "app";

    public GovernorDaemon(IApplication app) : base(app)
    {
      m_Applications = new OrderedRegistry<App>();
    }

    private OrderedRegistry<App> m_Applications;
    private IAppActivator m_Activator;
    private GovernorSipcServer m_Server;
    private int m_ServerStartPort;
    private int m_ServerEndPort;
    private Thread m_Thread;
    private AutoResetEvent m_Wait;

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

        if (app.Name.IsNullOrWhiteSpace())
        {
          var etxt = "Config error: missing `$name` attribute on `{0}`".Args(napp.RootPath);
          WriteLogFromHere(MessageType.CatastrophicError, etxt);
          throw new AppHostingException(etxt);
        }


        if (!m_Applications.Register(app))
        {
          var etxt = "Config error: duplicate application id `{0}`".Args(app.Name);
          WriteLogFromHere(MessageType.CatastrophicError, etxt);
          throw new AppHostingException(etxt);
        }
      }
    }

    protected override void DoStart()
    {
      base.DoStart();
      m_Server = new GovernorSipcServer(this, m_ServerStartPort, m_ServerEndPort);
      m_Server.Start();

      m_Wait = new AutoResetEvent(false);

      m_Thread = new Thread(threadBody);
      m_Thread.IsBackground = false;
      m_Thread.Name = nameof(GovernorDaemon);
      m_Thread.Start();

      if (m_Applications.Count == 0)
        WriteLogFromHere(MessageType.Warning, "No applications registered");

      //App start happens Asynchronously in thread body
    }


    protected override void DoSignalStop()
    {
      m_Wait.Set();
      base.DoSignalStop();
    }

    protected override void DoWaitForCompleteStop()
    {
      if (m_Thread != null)
      {
        m_Thread.Join();
        m_Thread = null;
      }

      DisposeAndNull(ref m_Wait);
      DisposeAndNull(ref m_Server);

      base.DoWaitForCompleteStop();
    }


    private void threadBody()
    {
      const int SLICE_MS_LOW = 150;
      const int SLICE_MS_HIGH = 250;

      var rel = Guid.NewGuid();

      try
      {
        startAll(rel);
      }
      catch (Exception error)
      {
        WriteLogFromHere(MessageType.CatastrophicError, "..startAll() leaked: " + error.ToMessageWithType(), error, related: rel);
      }

      while (Running) //<--- LOOP ---
      {
        try
        {
          scanAllOnce(rel);
        }
        catch(Exception error)
        {
          WriteLogFromHere(MessageType.CatastrophicError, "..scanAllOnce() leaked: " + error.ToMessageWithType(), error, related: rel);
        }

        m_Wait.WaitOne(Ambient.Random.NextScaledRandomInteger(SLICE_MS_LOW, SLICE_MS_HIGH));
      }//while

      try
      {
        stopAll(rel);
      }
      catch (Exception error)
      {
        WriteLogFromHere(MessageType.CatastrophicError, "stopAll() leaked: " + error.ToMessageWithType(), error, related: rel);
      }
    }


    private void startAll(Guid rel)
    {
      const int SLICE_MS = 50;
      foreach(var app in m_Applications.OrderedValues)
      {
        for(var i=0; i < app.StartDelayMs; i+=SLICE_MS)
        {
          if (!Running) break;
          Thread.Sleep(SLICE_MS);
        }

        if (!Running) break;

        try
        {
          m_Activator.StartApplication(app);
        }
        catch(Exception error)
        {
          WriteLogFromHere(MessageType.CatastrophicError, "Activator.StartApplication(`{0}`) leaked: {1}".Args(app.Name, error.ToMessageWithType()), error, related: rel);
        }
      }
    }

    private void stopAll(Guid rel)
    {
      const int SLICE_MS = 50;

      foreach (var app in m_Applications.OrderedValues.Reverse())
      {
        try
        {
          //even if not Running we still need to stop
          m_Activator.StopApplication(app);
        }
        catch (Exception error)
        {
          WriteLogFromHere(MessageType.CatastrophicError, "Activator.StopApplication(`{0}`) leaked: {1}".Args(app.Name, error.ToMessageWithType()), error, related: rel);
        }

        //bypass delay if daemon is terminating
        for (var i = 0; Running && i < app.StopDelayMs; i += SLICE_MS)
        {
          Thread.Sleep(SLICE_MS);
        }
      }
    }//stopAll

    private void scanAllOnce(Guid rel)
    {
      foreach(var app in m_Applications.OrderedValues)
      {
        try
        {
          scanOne(rel, app);
        }
        catch (Exception error)
        {
          WriteLogFromHere(MessageType.CatastrophicError, "..scanOne(`{0}`) leaked: {1}".Args(app.Name, error.ToMessageWithType()), error, related: rel);
        }
      }
    }

    private void scanOne(Guid rel, App app)
    {
      //initiate global Application termination
      if (app.Failed && !app.Optional)
      {
        WriteLogFromHere(MessageType.CatastrophicError,
              "Initiating governor application chassis termination because of app failure",
              related: rel,
              pars: new {
                app = app.Name,
                opt = app.Optional,
                failUtc = app.FailUtc,
                failReason = app.FailReason
              }.ToJson());

        ((IApplicationImplementation)this.App).Stop();
      }

      // check for Limbo, Torn, etc...
      var now = App.TimeSource.UTCNow;
      var elapsedSecSinceStart = (now - app.LastStartAttemptUtc).TotalSeconds;
      var cnn = app.Connection;

      //never connected?
      if (cnn == null || cnn.State == IO.Sipc.ConnectionState.Undefined)
      {
        if (elapsedSecSinceStart > app.MaxTimeNeverConnectedSec)
        {
          restartOne(rel, app, "Unconnected for {0} sec > {1} sec".Args(elapsedSecSinceStart, app.MaxTimeNeverConnectedSec));//restart application
        }
        return;
      }

      var elapsedSecSinceLastActivity = (now - cnn.LastActivityUtc).TotalSeconds;

      //torn?
      if (cnn.State == IO.Sipc.ConnectionState.Torn && elapsedSecSinceLastActivity > app.MaxTimeTornSec)
      {
        restartOne(rel, app, "Torn for {0} sec > {1} sec".Args(elapsedSecSinceLastActivity, app.MaxTimeTornSec));//restart application
        return;
      }

      //in limbo?
      if (cnn.State == IO.Sipc.ConnectionState.Limbo && elapsedSecSinceLastActivity > app.MaxTimeInLimboSec)
      {
        restartOne(rel, app, "In limbo for {0} sec > {1} sec".Args(elapsedSecSinceLastActivity, app.MaxTimeInLimboSec));//restart application
        return;
      }

    }

    private void restartOne(Guid rel, App app, string reason)
    {
      WriteLogFromHere(MessageType.Critical,
              "Initiating app `{0}` restart. Reason: `{1}`".Args(app.Name, reason),
              related: rel,
              pars: new { app = app.Name, reason }.ToJson());

      m_Activator.StopApplication(app);

      var startOk = m_Activator.StartApplication(app);

      WriteLogFromHere(MessageType.Critical, "App `{0}` restarted: `{1}`".Args(app.Name, startOk), related: rel);
    }

  }
}
