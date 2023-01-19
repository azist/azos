/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;

using Azos.Log;
using Azos.Conf;
using Azos.Collections;
using Azos.Apps;

using Azos.Sky;
using Azos.Sky.Metabase;
using Azos.Sky.Contracts;

namespace Azos.Apps.HostGovernor
{
  /// <summary>
  /// Provides Host Governor Services - this is a singleton class
  /// </summary>
  public sealed class HostGovernorService : Daemon, Sky.Contracts.IPinger
  {
    #region CONSTS
      public const string THREAD_NAME = "HostGovernorService";
      public const int THREAD_GRANULARITY_MS = 3000;

      public const string CONFIG_HOST_GOVERNOR_SECTION = "host-governor";

      public const string CONFIG_DYNAMIC_HOST_ID_ATTR = "dynamic-host-id";
      public const string CONFIG_STARTUP_INSTALL_CHECK_ATTR = "startup-install-check";
    #endregion

    #region .ctor/.dctor
      /// <summary>
      /// Creates a singleton instance or throws if instance is already created
      /// </summary>
      public HostGovernorService(IApplication app, bool launchedByARD, bool ardUpdateProblem) : base(app)
      {
        if (!App.Singletons.GetOrCreate(() => this).created)
          throw new AHGOVException(Sky.ServerStringConsts.AHGOV_INSTANCE_ALREADY_ALLOCATED_ERROR);

        m_LaunchedByARD = launchedByARD;
        m_ARDUpdateProblem = ardUpdateProblem;

        var exeName = System.Reflection.Assembly.GetEntryAssembly().Location;
        m_RootPath = Directory.GetParent(Path.GetDirectoryName(exeName)).FullName;
      }

      protected override void Destructor()
      {
        base.Destructor();
        App.Singletons.Remove<HostGovernorService>();
      }
    #endregion

    #region Fields


      private string m_RootPath;

      private bool m_LaunchedByARD;
      private bool m_ARDUpdateProblem;
      private bool m_NeedsProcessRestart;
      private bool m_StartupInstallCheck = true;
      private Thread m_Thread;
      private AutoResetEvent m_WaitEvent;

      private List<ManagedApp> m_Apps = new List<ManagedApp>();

      private int m_AppStartOrder;

      private string m_DynamicHostID;
    #endregion

    #region Properties

    public override string ComponentLogTopic => SysConsts.LOG_TOPIC_HOST_GOV;
    public override string ComponentCommonName { get { return "hgov"; }}

      /// <summary>
      /// Returns true when this process was launched by Sky Root Daemon as opposed to being launched from console/script
      /// </summary>
      public bool LaunchedByARD {get{ return m_LaunchedByARD;}}

      /// <summary>
      /// Returns true when this process was launched by Azos Sky Root Daemon that could not perform update properly -
      /// most likely UPD and RUN folders locked by some other process
      /// </summary>
      public bool ARDUpdateProblem {get{ return m_ARDUpdateProblem;}}


      /// <summary>
      /// Indicates whether install version check is done on service start (vs being invoked by command)
      /// </summary>
      public bool StartupInstallCheck {get{ return m_StartupInstallCheck;}}

      /// <summary>
      /// Returns the very root path under which "ard","/run-netf","/run-core","/upd" are
      /// </summary>
      public string RootPath { get{return m_RootPath;}}

      /// <summary>
      /// Returns the full path to "/run" directory out of which the current AHGOV process was launched
      /// </summary>
      public string RunPath
      {
        get
        {
          var isCore = Platform.Abstraction.PlatformAbstractionLayer.IsNetCore;
          return Path.Combine(m_RootPath, isCore ? SysConsts.HGOV_RUN_CORE_DIR : SysConsts.HGOV_RUN_NETF_DIR);
        }
      }

      /// <summary>
      /// Returns the full path to "/upd" directory where AHGOV will place newer files
      /// </summary>
      public string UpdatePath { get{return Path.Combine(m_RootPath, SysConsts.HGOV_UPDATE_DIR);}}


      /// <summary>
      /// Returns a thread-safe copy of ManagedApps in the instance - applications managed by Azos Sky Host Governor
      /// </summary>
      public IEnumerable<ManagedApp> ManagedApps
      {
        get
        {
          lock(m_Apps)
           return m_Apps.ToList();
        }
      }

      /// <summary>
      /// Becomes true to indocate that the app;ication process should be restarted (by ARD)
      /// </summary>
      public bool NeedsProcessRestart
      {
        get { return m_NeedsProcessRestart;}
      }

      /// <summary>
      /// Gets all unique Metabank.SectionApplication.AppPackage(s) for all applications on this host
      /// </summary>
      public IEnumerable<Metabank.SectionApplication.AppPackage> AllPackages
      {
        get
        {
          var result = new List<Metabank.SectionApplication.AppPackage>();
          foreach(var application in m_Apps)
            foreach(var package in application.Packages)
              if (!result.Any(ap => ap.Name.EqualsIgnoreCase( package.Name )&&
                                    ap.MatchedPackage.Equals(package.MatchedPackage) &&
                                    ap.Path.EqualsIgnoreCase( package.Path ))) result.Add(package);
          return result;
        }
      }

      /// <summary>
      /// Returns current application start order, the sequence # of the next app start
      /// </summary>
      public int AppStartOrder
      {
        get { return m_AppStartOrder;}
      }

      public DynamicHostID? DynamicHostID
      {
        get
        {
          if (m_DynamicHostID.IsNullOrWhiteSpace()) return null;
          return new DynamicHostID(m_DynamicHostID, App.GetThisHostMetabaseSection().ParentZone.RegionPath);
        }
      }

    #endregion

    #region Public

      public HostInfo GetHostInfo()
      {
        return HostInfo.ForThisHost(App);
      }

      public void Ping()
      {
        //does nothing. just comes back
      }

      /// <summary>
      /// Initiates check of locally installed packages and if they are different than install set, reinstalls in UpdatePath
      /// </summary>
      /// <returns>True if physical install was performed and AHGOV needs to restart so ARD may respawn it</returns>
      public bool CheckAndPerformLocalSoftwareInstallation(IList<string> progress, bool force = false)
      {
        IOUtils.EnsureDirectoryDeleted(UpdatePath);

        var anew =  App.AsSky().Metabase.CatalogBin.CheckAndPerformLocalSoftwareInstallation(progress, force);
        if (!anew) return false;
        //Flag the end of successful installation
        File.WriteAllText(Path.Combine(UpdatePath, SysConsts.HGOV_UPDATE_FINISHED_FILE), SysConsts.HGOV_UPDATE_FINISHED_FILE_OK_CONTENT);

        m_NeedsProcessRestart = true;

        return true;
      }



    #endregion

    #region Protected

      protected override void DoConfigure(IConfigSectionNode node)
      {
        if (node==null)
          node = App.ConfigRoot[CONFIG_HOST_GOVERNOR_SECTION];


        m_StartupInstallCheck = node.AttrByName(CONFIG_STARTUP_INSTALL_CHECK_ATTR).ValueAsBool(true);
        m_DynamicHostID = node.AttrByName(CONFIG_DYNAMIC_HOST_ID_ATTR).ValueAsString();

        base.DoConfigure(node);
      }

      protected override void DoStart()
      {
            try
            {
                lock(m_Apps)
                {
                  m_Apps.Clear();

                  //add managed apps as specified by host's role
                  foreach(var appInfo in App.GetThisHostMetabaseSection().Role.Applications)
                  {
                    var app = new ManagedApp(this, appInfo);
                    m_Apps.Add(app);//the app is started later when it is ready
                  }
                }


                m_WaitEvent = new AutoResetEvent(false);

                m_Thread = new Thread(threadSpin);
                m_Thread.Name = THREAD_NAME;
                m_Thread.Start();


            }
            catch
            {
                AbortStart();
                throw;
            }
      }

      protected override void DoSignalStop()
      {
         stopAllApps(false);
      }

      protected override void DoWaitForCompleteStop()
      {
            m_WaitEvent.Set();

            m_Thread.Join();
            m_Thread = null;

            m_WaitEvent.Close();
            m_WaitEvent = null;

            base.DoWaitForCompleteStop();
      }

    #endregion


    #region .pvt .impl
        private void threadSpin()
        {
            const string FROM = "threadSpin()";
            try
            {
              if (m_ARDUpdateProblem)
               log(MessageType.CatastrophicError, FROM, Sky.ServerStringConsts.AHGOV_ARD_UPDATE_PROBLEM_ERROR);
              else
              {
                 if (StartupInstallCheck)
                  if (checkInstall()) return;
              }
            }
            catch(Exception error)
            {
              log(MessageType.CatastrophicError, FROM, "checkInstall() leaked: " + error.ToMessageWithType(), error);
            }

            try
            {
              autoStartApps();
            }
            catch(Exception error)
            {
              log(MessageType.CatastrophicError, FROM, "autoStartApps() leaked: " + error.ToMessageWithType(), error);
            }

            try
            {
              var first = true;
              while (Running)
              {
                try
                {
                  if (!first) m_WaitEvent.WaitOne(THREAD_GRANULARITY_MS);
                  else first = false;

                  var now = App.TimeSource.UTCNow;
                  //check for stopped processes that supposed to be auto-started

                  //Notify Zone governor about this host(Host Registry service)
                  registerWithZGov(now);
                }
                catch(Exception error)
                {
                  log(MessageType.CatastrophicError, FROM, "while(Running){<body>} leaked: " + error.ToMessageWithType(), error);
                }
              }
            }
            finally
            {
              stopAllApps(true);//block
            }
        }

        private DateTime m_ScheduledZGovRegistration;
        private int m_ConsecutiveZGovRegFailures;
        private const int CONSECUTIVE_ZGOV_REG_FAIL_LONG_RETRY_THRESHOLD = 7;

        private void registerWithZGov(DateTime now)
        {
          const string FROM = "registerWithZGov()";

          if (now<m_ScheduledZGovRegistration) return;

          var thisHost = App.GetThisHostMetabaseSection();

          var thisHostHasZGov = thisHost.IsZGov;

          var zgovs = thisHost.ParentZone.FindNearestParentZoneGovernors( thisHostHasZGov, transcendNOC: false);//only within NOC

          var ok = false;
          var any = false;
          var logid = Guid.NewGuid();
          foreach (var zgov in zgovs)
          {
            any = true;
            try
            {
              using (var cl = App.GetServiceClientHub().MakeNew<IZoneHostRegistryClient>(zgov.RegionPath))
              {
                cl.RegisterSubordinateHost(HostInfo.ForThisHost(App), DynamicHostID);
                ok = true;
                break;
              }
            }
            catch (Exception error)
            {
              log(MessageType.Error, FROM, "RegisterSubordinateHost('{0}') svc call threw: {1} ".Args(zgov.RegionPath, error.ToMessageWithType()), error, related: logid);
            }
          }

          if (!ok && !thisHostHasZGov)
            log(MessageType.Error, FROM, "Could not send this host registration to any of the ZGovs tried", related: logid);

          //20151016 DKh added IF so if there is no ZGov it does not get bombarded
          if (ok)
          {
            m_ScheduledZGovRegistration = now.AddMilliseconds(5000 + App.Random.NextScaledRandomInteger(0, 25000));
            m_ConsecutiveZGovRegFailures = 0;
          }
          else
          {//20151016 DKh added IF so if there is no ZGov it does not get bombarded
            if (any)
            {
              m_ConsecutiveZGovRegFailures++;
              if (m_ConsecutiveZGovRegFailures<CONSECUTIVE_ZGOV_REG_FAIL_LONG_RETRY_THRESHOLD)
                m_ScheduledZGovRegistration = now.AddSeconds(25 + App.Random.NextScaledRandomInteger(0, 30));
              else
                m_ScheduledZGovRegistration = now.AddSeconds(60 + App.Random.NextScaledRandomInteger(10, 300));
            }
            else//try to re-read Metabase once an hour +
             m_ScheduledZGovRegistration = now.AddMinutes(60 + App.Random.NextScaledRandomInteger(0, 120));
          }
        }

        private bool checkInstall()
        {
            var id = Guid.NewGuid();
            try
            {
              var list = new EventedList<string, object>();
              list.GetReadOnlyEvent = (c)=>false;
              list.ChangeEvent = delegate(EventedList<string, object> cl, EventedList<string, object>.ChangeType change, EventPhase phase, int idx, string item)
              {
                  if (phase==EventPhase.After)
                    log(MessageType.Trace, "checkInstall()", " * " + item, related: id);
              };
              var anew = CheckAndPerformLocalSoftwareInstallation(list, false);
              if (anew)
              {
                log(MessageType.Info, "checkInstall()", "CheckAndPerformLocalSoftwareInstallation() returned true. Will be restarting", related: id);
                return true; //needs restart
              }
            }
            catch(Exception error)
            {
              log(MessageType.CatastrophicError, "checkInstall()", "CheckAndPerformLocalSoftwareInstallation() leaked: " + error.ToMessageWithType(), error, related: id);
            }
            return false;
        }

        private void autoStartApps()
        {
            var autoApps = m_Apps.Where(a => !a.AppInfo.Name.EqualsIgnoreCase(SysConsts.APP_NAME_HGOV)  &&
                                             a.AppInfo.AutoRun.HasValue);
            foreach(var app in autoApps.OrderBy(a=>a.AppInfo.AutoRun.Value))
              try
              {
                app.Start();
                app.m_StartOrder = m_AppStartOrder;
                m_AppStartOrder++;
              }
              catch(Exception error)
              {
                log(MessageType.CatastrophicError, "autoStartApps()", "App '{0}' Leaked: {1}".Args(app.Name, error.ToMessageWithType()), error);
              }
        }

        private void stopAllApps(bool block)
        {
            foreach(var app in m_Apps.OrderBy(p=>-p.StartOrder))//in reverse order of start
              try
              {
                 if (block)
                   app.WaitForCompleteStop();
                 else
                   app.SignalStop();
              }
              catch(Exception error)
              {
                log(MessageType.CatastrophicError, "stopAllApps(block:{0})".Args(block), "Svc stop leaked: " + error.ToMessageWithType(), error);
              }
        }


        internal void log(MessageType type, string from, string text, Exception error = null, Guid? related = null)
        {
           var msg = new Message
              {
                 Type = type,
                 Topic = SysConsts.LOG_TOPIC_APP_MANAGEMENT,
                 From = "{0}.{1}".Args(GetType().FullName, from),
                 Text = text,
                 Exception = error
              };

              if (related.HasValue) msg.RelatedTo = related.Value;

           App.Log.Write( msg );
        }
    #endregion
  }
}
