/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.CompilerServices;

using Azos.Apps.Volatile;
using Azos.Conf;
using Azos.Data.Access;
using Azos.Glue;
using Azos.Instrumentation;
using Azos.Log;
using Azos.Security;
using Azos.Time;

namespace Azos.Apps
{
  /// <summary>
  /// Provides base implementation of IApplication for various application kinds
  /// </summary>
  [ConfigMacroContext]
  public abstract partial class CommonApplicationLogic : DisposableObject, IApplication
  {
    #region CONSTS
    public const string CONFIG_SWITCH = "config";

    public const string CONFIG_APP_NAME_ATTR = "application-name";
    public const string CONFIG_UNIT_TEST_ATTR = "unit-test";
    public const string CONFIG_FORCE_INVARIANT_CULTURE_ATTR = "force-invariant-culture";
    public const string CONFIG_ENVIRONMENT_NAME_ATTR = "environment-name";

    public const string CONFIG_MEMORY_MANAGEMENT_SECTION = "memory-management";

    public const string CONFIG_MODULES_SECTION = "modules";
    public const string CONFIG_MODULE_SECTION = "module";

    public const string CONFIG_STARTERS_SECTION = "starters";
    public const string CONFIG_STARTER_SECTION = "starter";

    public const string CONFIG_TIMESOURCE_SECTION = "time-source";
    public const string CONFIG_EVENT_TIMER_SECTION = "event-timer";
    public const string CONFIG_LOG_SECTION = "log";
    public const string CONFIG_INSTRUMENTATION_SECTION = "instrumentation";
    public const string CONFIG_DATA_STORE_SECTION = "data-store";
    public const string CONFIG_OBJECT_STORE_SECTION = "object-store";
    public const string CONFIG_GLUE_SECTION = "glue";
    public const string CONFIG_SECURITY_SECTION = "security";


    public const string CONFIG_PATH_ATTR = "path";
    public const string CONFIG_ENABLED_ATTR = "enabled";
    #endregion

    #region .ctor/.dctor
    protected CommonApplicationLogic(bool allowNesting, Configuration cmdLineArgs, ConfigSectionNode rootConfig)
    {
      m_AllowNesting = allowNesting;
      m_CommandArgs = (cmdLineArgs ?? new MemoryConfiguration()).Root;
      m_ConfigRoot  = rootConfig ?? GetConfiguration().Root;
      m_Singletons = new ApplicationSingletonManager();
      m_Realm = new ApplicationRealmBase(this);

      m_NOPLog = new NOPLog(this);
      m_NOPModule = new NOPModule(this);
      m_NOPInstrumentation = new NOPInstrumentation(this);
      m_NOPDataStore = new NOPDataStore(this);
      m_NOPObjectStore = new NOPObjectStore(this);
      m_NOPGlue = new NOPGlue(this);
      m_NOPSecurityManager = new NOPSecurityManager(this);
      m_DefaultTimeSource = new DefaultTimeSource(this);
      m_NOPEventTimer = new NOPEventTimer(this);
    }

    protected override void Destructor()
    {
      m_ShutdownStarted = true;

      DisposeAndNull(ref m_NOPEventTimer);
      DisposeAndNull(ref m_DefaultTimeSource);
      DisposeAndNull(ref m_NOPSecurityManager);
      DisposeAndNull(ref m_NOPGlue);
      DisposeAndNull(ref m_NOPObjectStore);
      DisposeAndNull(ref m_NOPDataStore);
      DisposeAndNull(ref m_NOPInstrumentation);
      DisposeAndNull(ref m_NOPModule);
      DisposeAndNull(ref m_NOPLog);

      DisposeAndNull(ref m_Realm);
      DisposeAndNull(ref m_Singletons);
      base.Destructor();
    }
    #endregion


    #region Fields

    private Guid m_InstanceID = Guid.NewGuid();
    protected DateTime m_StartTime;

    private string m_Name;
    private bool m_AllowNesting;

    protected volatile bool m_ShutdownStarted;
    private volatile bool m_Stopping;

    protected IApplicationRealmImplementation m_Realm;

    protected List<IConfigSettings> m_ConfigSettings = new List<IConfigSettings>();
    protected List<IApplicationFinishNotifiable> m_FinishNotifiables = new List<IApplicationFinishNotifiable>();

    [Config(TimeLocation.CONFIG_TIMELOCATION_SECTION)]
    private TimeLocation m_TimeLocation = new TimeLocation();

    protected ConfigSectionNode m_CommandArgs;
    protected ConfigSectionNode m_ConfigRoot;

    protected ApplicationSingletonManager m_Singletons;

    protected ILogImplementation m_Log;
    protected ILogImplementation m_NOPLog;

    protected IModuleImplementation m_Module;
    protected IModuleImplementation m_NOPModule;

    protected IInstrumentationImplementation m_Instrumentation;
    protected IInstrumentationImplementation m_NOPInstrumentation;

    protected IDataStoreImplementation m_DataStore;
    protected IDataStoreImplementation m_NOPDataStore;

    protected IObjectStoreImplementation m_ObjectStore;
    protected IObjectStoreImplementation m_NOPObjectStore;

    protected IGlueImplementation m_Glue;
    protected IGlueImplementation m_NOPGlue;

    protected ISecurityManagerImplementation m_SecurityManager;
    protected ISecurityManagerImplementation m_NOPSecurityManager;

    protected ITimeSourceImplementation m_TimeSource;
    protected ITimeSourceImplementation m_DefaultTimeSource;

    protected IEventTimerImplementation m_EventTimer;
    protected IEventTimerImplementation m_NOPEventTimer;
    #endregion


    #region Properties

    /// <summary>True if this app chassis is a test rig </summary>
    public virtual bool IsUnitTest => m_ConfigRoot.AttrByName(CONFIG_UNIT_TEST_ATTR).ValueAsBool();

    /// <summary>Provides access to "environment-name" attribute, e.g. "DEV" vs "PROD"</summary>
    public string EnvironmentName => m_ConfigRoot.AttrByName(CONFIG_ENVIRONMENT_NAME_ATTR).Value;

    /// <summary>True to force app container set process-wide invariant culture on boot</summary>
    public virtual bool ForceInvariantCulture => m_ConfigRoot.AttrByName(CONFIG_FORCE_INVARIANT_CULTURE_ATTR).ValueAsBool();

    /// <summary>Returns unique identifier of this running instance</summary>
    public Guid InstanceID => m_InstanceID;

    /// <summary>Returns true if the app container allows nesting of another app container </summary>
    public virtual bool AllowNesting => m_AllowNesting;

    /// <summary>Returns timestamp when application started as localized app time </summary>
    public DateTime StartTime => m_StartTime;

    /// <summary>Returns the name of this application </summary>
    public string Name => m_Name ?? GetType().FullName;

    /// <summary>
    /// Returns true when application instance is active and working. This property returns false as soon as application finalization starts on shutdown
    /// Use to exit long-running loops and such as a cancellation flag
    /// </summary>
    public bool Active => !m_ShutdownStarted && !m_Stopping;

    /// <summary> Returns true to indicate that Stop() was called </summary>
    public bool Stopping => m_Stopping;

    /// <summary> Returns true to indicate that Dispose() has been called and shutdown has started</summary>
    public bool ShutdownStarted => m_ShutdownStarted;

    /// <summary>
    /// Returns an accessor to the application surrounding environment (realm) in which app gets executed.
    /// This realm is sub-divided into uniquely-named areas each reporting their status. Realms are used in distributed
    /// systems and represent zone/section of cluster
    /// </summary>
    public IApplicationRealm Realm => m_Realm;

    /// <summary>
    /// Initiates the stop of the application by setting its Stopping to true and Active to false so dependent services may start to terminate
    /// </summary>
    public void Stop() => m_Stopping = true;


    public IConfigSectionNode ConfigRoot          => m_ConfigRoot;
    public IConfigSectionNode CommandArgs         => m_CommandArgs;
    public IApplicationSingletonManager Singletons => m_Singletons;
    public ILog                Log                => m_Log             ??  m_NOPLog;
    public IInstrumentation    Instrumentation    => m_Instrumentation ??  m_NOPInstrumentation;
    public IDataStore          DataStore          => m_DataStore       ??  m_NOPDataStore;
    public IObjectStore        ObjectStore        => m_ObjectStore     ??  m_NOPObjectStore;
    public IGlue               Glue               => m_Glue            ??  m_NOPGlue;
    public ISecurityManager    SecurityManager    => m_SecurityManager ??  m_NOPSecurityManager;
    public ITimeSource         TimeSource         => m_TimeSource      ??  m_DefaultTimeSource;
    public IEventTimer         EventTimer         => m_EventTimer      ??  m_NOPEventTimer;
    public IModule             ModuleRoot         => m_Module          ??  m_NOPModule;

    /// <summary>
    /// Returns time location of this LocalizedTimeProvider implementation
    /// </summary>
    public TimeLocation TimeLocation
    {
        get { return m_TimeLocation ?? TimeLocation.Parent;}
        set { m_TimeLocation = value; }
    }

    /// <summary>Returns current time localized per TimeLocation</summary>
    public DateTime LocalizedTime => UniversalTimeToLocalizedTime(TimeSource.UTCNow);

    /// <summary>Enumerates all components of this application</summary>
    public IEnumerable<IApplicationComponent> AllComponents => ApplicationComponent.AllComponents(this);

    /// <summary> Returns app random generator </summary>
    public Platform.RandomGenerator Random => Ambient.Random;//just a shortcut to process-wide random generator

    #endregion


    #region Public

    /// <summary>
    /// Converts universal time to local time as of TimeLocation property
    /// </summary>
    public DateTime UniversalTimeToLocalizedTime(DateTime utc)
    {
        if (utc.Kind!=DateTimeKind.Utc)
          throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+".UniversalTimeToLocalizedTime(utc.Kind!=UTC)");

        var loc = TimeLocation;
        if (!loc.UseParentSetting)
        {
            return DateTime.SpecifyKind(utc + loc.UTCOffset, DateTimeKind.Local);
        }
        else
        {
            return TimeSource.UniversalTimeToLocalizedTime(utc);
        }
    }

    /// <summary>
    /// Converts localized time to UTC time as of TimeLocation property
    /// </summary>
    public DateTime LocalizedTimeToUniversalTime(DateTime local)
    {
        if (local.Kind!=DateTimeKind.Local)
          throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+".LocalizedTimeToUniversalTime(utc.Kind!=Local)");

        var loc = TimeLocation;
        if (!loc.UseParentSetting)
        {
            return DateTime.SpecifyKind(local - loc.UTCOffset, DateTimeKind.Utc);
        }
        else
        {
            return TimeSource.LocalizedTimeToUniversalTime(local);
        }
    }


    /// <summary>
    /// Makes BaseSession instance
    /// </summary>
    public virtual ISession MakeNewSessionInstance(Guid sessionID, Security.User user = null)
    {
        var result = new BaseSession(sessionID, Random.NextRandomUnsignedLong);
        result.User = user;

        return result;
    }


    /// <summary>
    /// Registers an instance of IConfigSettings with application container to receive a call when
    ///  underlying app configuration changes
    /// </summary>
    /// <returns>True if settings instance was not found and was added</returns>
    public bool RegisterConfigSettings(IConfigSettings settings)
    {
        if (m_ShutdownStarted || settings==null) return false;
        lock(m_ConfigSettings)
          if (!m_ConfigSettings.Contains(settings, Collections.ReferenceEqualityComparer<IConfigSettings>.Instance))
          {
              m_ConfigSettings.Add(settings);
              return true;
          }
        return false;
    }

    /// <summary>
    /// Removes the registration of IConfigSettings from application container
    /// </summary>
    /// <returns>True if settings instance was found and removed</returns>
    public bool UnregisterConfigSettings(IConfigSettings settings)
    {
        if (m_ShutdownStarted || settings==null) return false;
        lock(m_ConfigSettings)
          return m_ConfigSettings.Remove(settings);
    }

    /// <summary>
    /// Forces notification of all registered IConfigSettings-implementers about configuration change
    /// </summary>
    public void NotifyAllConfigSettingsAboutChange()
    {
        NotifyAllConfigSettingsAboutChange(m_ConfigRoot);
    }


    /// <summary>
    /// Registers an instance of IApplicationFinishNotifiable with application container to receive a call when
    ///  underlying application instance will finish its life cycle
    /// </summary>
    /// <returns>True if notifiable instance was not found and was added</returns>
    public bool RegisterAppFinishNotifiable(IApplicationFinishNotifiable notifiable)
    {
        if (m_ShutdownStarted || notifiable==null) return false;

        lock(m_FinishNotifiables)
          if (!m_FinishNotifiables.Contains(notifiable, Collections.ReferenceEqualityComparer<IApplicationFinishNotifiable>.Instance))
          {
              m_FinishNotifiables.Add(notifiable);
              return true;
          }
        return false;
    }

    /// <summary>
    /// Removes the registration of IConfigSettings from application container
    /// </summary>
    /// <returns>True if notifiable instance was found and removed</returns>
    public bool UnregisterAppFinishNotifiable(IApplicationFinishNotifiable notifiable)
    {
        if (m_ShutdownStarted || notifiable==null) return false;

        lock(m_FinishNotifiables)
          return m_FinishNotifiables.Remove(notifiable);
    }


    /// <summary>
    /// Returns a component by SID or null
    /// </summary>
    public IApplicationComponent GetComponentBySID(ulong sid)
    {
        return ApplicationComponent.GetAppComponentBySID(this, sid);
    }

    /// <summary>
    /// Returns an existing application component instance by its ComponentCommonName or null. The search is case-insensitive
    /// </summary>
    public IApplicationComponent GetComponentByCommonName(string name)
    {
      return ApplicationComponent.GetAppComponentByCommonName(this, name);
    }

    #endregion


    #region Protected


    protected Guid WriteLog(MessageType type,
                            string from,
                            string msgText,
                            Exception error = null,
                            [CallerFilePath]string file = "",
                            [CallerLineNumber]int line = 0,
                            object pars = null,
                            Guid? related = null)
    {
      var log = m_Log;
      if (log==null) return Guid.Empty;

      var msg = new Message
                 {
                    Topic = CoreConsts.APPLICATION_TOPIC,
                    Type = type,
                    From = from,
                    Text = msgText,
                    Exception = error,
                 }.SetParamsAsObject( Message.FormatCallerParams(pars, file, line) );

      if (related.HasValue) msg.RelatedTo = related.Value;

      log.Write(msg);

      return msg.Guid;
    }

    protected virtual Configuration GetConfiguration()
    {
        //try to read from  /config file
        var configFile = m_CommandArgs[CONFIG_SWITCH].AttrByIndex(0).Value;

        if (string.IsNullOrEmpty(configFile))
            configFile = GetDefaultConfigFileName();


        Configuration conf;

        if (File.Exists(configFile))
            conf = Configuration.ProviderLoadFromFile(configFile);
        else
            conf = new MemoryConfiguration();

        return conf;
    }


    protected IEnumerable<IApplicationStarter> GetStarters()
    {
      var snodes = m_ConfigRoot[CONFIG_STARTERS_SECTION].Children.Where(n=>n.IsSameName(CONFIG_STARTER_SECTION));
      foreach(var snode in snodes)
      {
          var starter = FactoryUtils.MakeAndConfigure<IApplicationStarter>(snode);
          yield return starter;
      }
    }

    /// <summary>
    /// Tries to find a configuration file name looping through various supported extensions
    /// </summary>
    /// <returns>File name that exists or empty string</returns>
    protected string GetDefaultConfigFileName()
    {
        var exeName = System.Reflection.Assembly.GetEntryAssembly().Location;
        var exeNameWoExt = Path.Combine(Path.GetDirectoryName(exeName), Path.GetFileNameWithoutExtension(exeName));
//Console.WriteLine("EXENAME:" +exeName);
//Console.WriteLine("EXENAME wo extension:" +exeNameWoExt);
        var extensions = Configuration.AllSupportedFormats.Select(fmt => '.'+fmt);
        foreach(var ext in extensions)
        {
            var configFile = exeName + ext;
//Console.WriteLine("Probing:" +configFile);
            if (File.Exists(configFile)) return configFile;
            configFile = exeNameWoExt + ext;
//Console.WriteLine("Probing:" +configFile);
            if (File.Exists(configFile)) return configFile;

        }
        return string.Empty;
    }


    /// <summary>
    /// Forces notification of all registered IConfigSettings-implementers about configuration change
    /// </summary>
    protected void NotifyAllConfigSettingsAboutChange(IConfigSectionNode node)
    {
        node = node ?? m_ConfigRoot;

        lock(m_ConfigSettings)
          foreach(var s in m_ConfigSettings) s.ConfigChanged(this, node);
    }

    #endregion

  }

}
