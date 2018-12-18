/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Log;
using Azos.Apps;
using Azos.Conf;
using Azos.Data.Access;
using Azos.Wave;

using Azos.Sky;
using Azos.Sky.Identification;

namespace Azos.Apps
{
  /// <summary>
  /// Provides base implementation of ISkyApplication for applications like services and console apps.
  /// This class IS thread safe
  /// </summary>
  public class SkyApplication : AzosApplication, ISkyApplication
  {
    #region CONSTS

      public const string CONFIG_WEB_MANAGER_SECTION = "web-manager";

      public const string CONFIG_LOCK_MANAGER_SECTION = "lock-manager";

      public const string CONFIG_PROCESS_MANAGER_SECTION = "process-manager";

      public const string CONFIG_HOST_MANAGER_SECTION = "host-manager";

    #endregion

    #region .ctor

    //framework internal called by derivatives
    protected SkyApplication() : base() { }

    public SkyApplication(SystemApplicationType sysAppType, string[] args)
     : this(sysAppType, false, args, null)
    { }

    public SkyApplication(SystemApplicationType sysAppType, string[] args, ConfigSectionNode rootConfig)
      : this(sysAppType, false, args, rootConfig)
    { }

    public SkyApplication(SystemApplicationType sysAppType,
                          bool allowNesting,
                          string[] args,
                          ConfigSectionNode rootConfig) : base()
    {
      try
      {
        m_BootLoader = new BootConfLoader(sysAppType);
        m_NOPLockManager = new Locking.NOPLockManager(this);
        var cmdArgs = args == null ? null : new CommandArgsConfiguration(args);
        Constructor(allowNesting, cmdArgs, rootConfig);
        InitApplication();
      }
      catch
      {
        Destructor();
        throw;
      }
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_BootLoader);
      base.Destructor();
    }

    #endregion

    #region Fields

    private BootConfLoader m_BootLoader;
    private ConfigSectionNode m_BootConfigRoot;

    private WaveServer m_WebManagerServer;

    private Locking.ILockManagerImplementation m_LockManager;
    private Locking.ILockManagerImplementation m_NOPLockManager;
    private GdidGenerator m_GDIDProvider;
    private Workers.IProcessManagerImplementation m_ProcessManager;
    private Dynamic.IHostManagerImplementation m_DynamicHostManager;

    #endregion

    #region Properties

      /// <summary>
      /// Denotes system application/process type that this app container has, i.e.:  HostGovernor, WebServer, etc.
      /// The value is set in .ctor and kept in BootConfLoader.SystemApplicationType
      /// </summary>
      public SystemApplicationType SystemApplicationType => m_BootLoader.SystemApplicationType;


      public string MetabaseApplicationName { get{ return SkySystem.MetabaseApplicationName; } }


      public IConfigSectionNode BootConfigRoot { get { return m_BootConfigRoot; } }

      internal WaveServer WebManagerServer{ get{return m_WebManagerServer;}}

      public Locking.ILockManager LockManager => m_LockManager ?? m_NOPLockManager;

      public IGdidProvider GdidProvider { get {  return m_GDIDProvider; } }

      public Workers.IProcessManager ProcessManager { get { return m_ProcessManager; } }

      public Dynamic.IHostManager DynamicHostManager { get { return m_DynamicHostManager; } }
    #endregion

    #region Protected

      protected override Configuration GetConfiguration()
      {
        var localConfig = base.GetConfiguration();
        localConfig.Application = this;

        BootConfLoader.ProcessAllExistingIncludes(localConfig.Root, null, "boot");

        m_BootConfigRoot = localConfig.Root;

        var cmdArgs = new string[]{};

        if (CommandArgs.Configuration is CommandArgsConfiguration)
          cmdArgs = ((CommandArgsConfiguration)this.CommandArgs.Configuration).Arguments;

        var result = m_BootLoader.Load(cmdArgs, localConfig);
        result.Application = this;
        return result;
      }

      protected override void DoInitApplication()
      {
        base.DoInitApplication();

        var FROM = GetType().FullName+".DoInitApplication()";

        var metabase = m_BootLoader.Metabase;

        try
        {
          m_GDIDProvider = new GdidGenerator(this, "Sky");

          foreach(var ah in metabase.GDIDAuthorities)
          {
            m_GDIDProvider.AuthorityHosts.Register(ah);
            WriteLog(MessageType.Info, FROM+"{GDIDProvider init}", "Registered GDID authority host: "+ah.ToString());
          }

          WriteLog(MessageType.Info, FROM, "GDIProvider made");
        }
        catch(Exception error)
        {
          WriteLog(MessageType.CatastrophicError, FROM+"{GDIDProvider init}", error.ToMessageWithType());
          try
          {
            m_GDIDProvider.Dispose();
          }
          catch{ }

          m_GDIDProvider = null;
        }

        var wmSection = ConfigRoot[CONFIG_WEB_MANAGER_SECTION];
        if (wmSection.Exists && wmSection.AttrByName(CONFIG_ENABLED_ATTR).ValueAsBool(false))
        try
        {
          m_WebManagerServer = new WaveServer(this);
          m_WebManagerServer.Configure(wmSection);
          m_WebManagerServer.Start();
        }
        catch(Exception error)
        {
          WriteLog(MessageType.CatastrophicError, FROM+"{WebManagerServer start}", error.ToMessageWithType());
          try
          {
            m_WebManagerServer.Dispose();
          }
          catch{}

          m_WebManagerServer = null;
        }

        var lockSection = ConfigRoot[CONFIG_LOCK_MANAGER_SECTION];
        try
        {
          m_LockManager = FactoryUtils.MakeAndConfigure<Locking.ILockManagerImplementation>(lockSection, typeof(Locking.LockManager));

          WriteLog(MessageType.Info, FROM, "Lock Manager made");

          if (m_LockManager is Daemon)
          {
            ((Daemon)m_LockManager).Start();
            WriteLog(MessageType.Info, FROM, "Lock Manager STARTED");
          }
        }
        catch(Exception error)
        {
          WriteLog(MessageType.CatastrophicError, FROM+"{LockManager start}", error.ToMessageWithType());
          try
          {
            m_LockManager.Dispose();
          }
          catch{}

          m_LockManager = null;
        }

        var procSection = ConfigRoot[CONFIG_PROCESS_MANAGER_SECTION];
        try
        {
          m_ProcessManager = FactoryUtils.MakeAndConfigure<Workers.IProcessManagerImplementation>(procSection, typeof(Workers.ProcessManager), new object[] { this });

          WriteLog(MessageType.Info, FROM, "Process Manager made");

          if (m_ProcessManager is Daemon)
          {
            ((Daemon)m_ProcessManager).Start();
            WriteLog(MessageType.Info, FROM, "Process Manager STARTED");
          }
        }
        catch (Exception error)
        {
          WriteLog(MessageType.CatastrophicError, FROM+"{ProcessManager start}", error.ToMessageWithType());
          try
          {
            m_ProcessManager.Dispose();
          }
          catch{}

          m_ProcessManager = null;
        }

        var hostSection = ConfigRoot[CONFIG_HOST_MANAGER_SECTION];
        try
        {
          m_DynamicHostManager = FactoryUtils.MakeAndConfigure<Dynamic.IHostManagerImplementation>(procSection, typeof(Dynamic.HostManager), new object[] { this });

          WriteLog(MessageType.Info, FROM, "Dynamic Host Manager made");

          if (m_DynamicHostManager is Daemon)
          {
            ((Daemon)m_DynamicHostManager).Start();
            WriteLog(MessageType.Info, FROM, "Dynamic Host Manager STARTED");
          }
        }
        catch (Exception error)
        {
          WriteLog(MessageType.CatastrophicError, FROM+ "{HostManager start}", error.ToMessageWithType());
          try
          {
            m_DynamicHostManager.Dispose();
          }
          catch{}

          m_DynamicHostManager = null;
        }
      }

      protected override void DoCleanupApplication()
      {
        var FROM = GetType().FullName+".DoCleanupApplication()";

        if (m_DynamicHostManager != null)
        {
          WriteLog(MessageType.Info, FROM, "Finalizing Dynamic Host Manager");
          try
          {
            if (m_DynamicHostManager is Daemon)
            {
              ((Daemon)m_DynamicHostManager).SignalStop();
              ((Daemon)m_DynamicHostManager).WaitForCompleteStop();
              WriteLog(MessageType.Info, FROM, "Dynamic Host Manager STOPPED");
            }

            DisposableObject.DisposeAndNull(ref m_DynamicHostManager);
            WriteLog(MessageType.Info, FROM, "Dynamic Host Manager DISPOSED");
          }
          catch(Exception error)
          {
            WriteLog(MessageType.Error, FROM, "ERROR finalizing Dynamic Host Manager: " + error.ToMessageWithType());
          }
        }

        if (m_ProcessManager!=null)
        {
          WriteLog(MessageType.Info, FROM, "Finalizing Process Manager");
          try
          {
            if (m_ProcessManager is Daemon)
            {
              ((Daemon)m_ProcessManager).SignalStop();
              ((Daemon)m_ProcessManager).WaitForCompleteStop();
              WriteLog(MessageType.Info, FROM, "Process Manager STOPPED");
            }

            DisposableObject.DisposeAndNull(ref m_ProcessManager);
            WriteLog(MessageType.Info, FROM, "Process Manager DISPOSED");
          }
          catch(Exception error)
          {
            WriteLog(MessageType.Error, FROM, "ERROR finalizing Process Manager: " + error.ToMessageWithType());
          }
        }

        if (m_LockManager!=null)
        {
          WriteLog(MessageType.Info, FROM, "Finalizing Lock Manager");
          try
          {
            if (m_LockManager is Daemon)
            {
                ((Daemon)m_LockManager).SignalStop();
                ((Daemon)m_LockManager).WaitForCompleteStop();
                WriteLog(MessageType.Info, FROM, "Lock Manager STOPPED");
            }

            DisposableObject.DisposeAndNull(ref m_LockManager);
            WriteLog(MessageType.Info, FROM, "lock manager DISPOSED");
          }
          catch(Exception error)
          {
            WriteLog(MessageType.Error, FROM, "ERROR finalizing Lock Manager: " + error.ToMessageWithType());
          }
        }

        if (m_WebManagerServer!=null)
        {
          WriteLog(MessageType.Info, FROM, "Finalizing Web Manager Server");
          try
          {
            DisposableObject.DisposeAndNull(ref m_WebManagerServer);
            WriteLog(MessageType.Info, FROM, "Web Manager Server DISPOSED");
          }
          catch (Exception error)
          {
            WriteLog(MessageType.CatastrophicError, FROM, "ERROR finalizing Web Manager Server: " + error.ToMessageWithType());
          }
        }

        if (m_GDIDProvider!=null)
        {
          WriteLog(MessageType.Info, FROM, "Finalizing GDIDProvider");
          try
          {
            DisposableObject.DisposeAndNull(ref m_GDIDProvider);
            WriteLog(MessageType.Info, FROM, "GDIDProvider DISPOSED");
          }
          catch(Exception error)
          {
            WriteLog(MessageType.Error, FROM, "ERROR finalizing GDIDProvider: " + error.ToMessageWithType());
          }
        }
        // Shutdown - must be last
        base.DoCleanupApplication();
      }

    #endregion
  }
}
