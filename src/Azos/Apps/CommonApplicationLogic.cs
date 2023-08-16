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
using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Data.Access;
using Azos.Glue;
using Azos.Instrumentation;
using Azos.Log;
using Azos.Security;
using Azos.Time;
using System.Threading;

namespace Azos.Apps
{
  /// <summary>
  /// Provides base implementation of IApplication for various application kinds
  /// </summary>
  [ConfigMacroContext]
  public abstract partial class CommonApplicationLogic : DisposableObject, IApplicationImplementation
  {
    #region CONSTS

    public const string CONFIG_SWITCH = "config";

    public const string CONFIG_NAME_ATTR = "name";
    public const string CONFIG_ID_ATTR = "id";
    public const string CONFIG_CLOUD_ORIGIN_ATTR = "cloud-origin";
    public const string CONFIG_NODE_DISCRIMINATOR_ATTR = "node-discriminator";
    public const string CONFIG_COPYRIGHT_ATTR = "copyright";
    public const string CONFIG_DESCRIPTION_ATTR = "description";
    public const string CONFIG_UNIT_TEST_ATTR = "unit-test";
    public const string CONFIG_FORCE_INVARIANT_CULTURE_ATTR = "force-invariant-culture";
    public const string CONFIG_ENVIRONMENT_NAME_ATTR = "environment-name";
    public const string CONFIG_PROCESS_INCLUDES = "process-includes";
    public const string CONFIG_PROCESS_EXCLUDES = "process-excludes";

    public const string CONFIG_EXPECTED_COMPONENT_SHUTDOWN_DURATION_MS = "expected-component-shutdown-duration-ms";
    public const int DFLT_EXPECTED_COMPONENT_SHUTDOWN_DURATION_MS = 1_250;

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
    public const string CONFIG_DEPENDENCY_INJECTOR_SECTION = "dependency-injector";
    public const string CONFIG_SECURITY_SECTION = "security";

    public const string CONFIG_PATH_ATTR = "path";
    public const string CONFIG_ENABLED_ATTR = "enabled";
    public const string CONFIG_CHASSIS_LOG_LEVEL_ATTR = "chassis-log-level";

    #endregion

    #region .ctor/.dctor

    /// <summary>
    /// Processes all include pragmas if they are specified in the root `process-includes` attribute.
    /// This method is called for auto-loaded entry point config automatically.
    /// You may call this method for configs acquired from external sources prior to
    /// passing it to application .ctor
    /// </summary>
    public static void ProcessAllExistingConfigurationIncludes(ConfigSectionNode appConfigRoot)
    {
      appConfigRoot.NonNull(nameof(appConfigRoot));
      var includePragma = appConfigRoot.AttrByName(CONFIG_PROCESS_INCLUDES).Value;
      if (includePragma.IsNotNullOrWhiteSpace())
      {
        appConfigRoot.ProcessAllExistingIncludes(nameof(CommonApplicationLogic), includePragma);
      }
    }

    /// <summary>
    /// Processes all exclude pragmas if they are specified in the root `process-exclude` attribute.
    /// This method is called for auto-loaded entry point config automatically.
    /// You may call this method for configs acquired from external sources prior to
    /// passing it to application .ctor
    /// </summary>
    public static void ProcessConfigurationExcludes(ConfigSectionNode appConfigRoot)
    {
      appConfigRoot.NonNull(nameof(appConfigRoot));
      var excludePragma = appConfigRoot.AttrByName(CONFIG_PROCESS_EXCLUDES).Value;
      if (excludePragma.IsNotNullOrWhiteSpace())
      {
        appConfigRoot.ProcessExcludes(true, true, nameof(CommonApplicationLogic), excludePragma);
      }
    }

    //fx internal, called by derivatives
    protected CommonApplicationLogic() { }

    //perform the core construction of app instance,
    //this is a method because of C# inability to control ctor chaining sequence
    //this is framework internal code, developers do not call
    protected void Constructor(bool allowNesting,
                               string[] cmdLineArgs,
                               ConfigSectionNode rootConfig,
                               IApplicationDependencyInjectorImplementation defaultDI = null)
    {
      m_AllowNesting = allowNesting;

      if (cmdLineArgs != null && cmdLineArgs.Length > 0)
      {
        var acfg = new CommandArgsConfiguration(cmdLineArgs) { Application = this };
        m_CommandArgs = acfg.Root;
      }
      else
      {
        var acfg = new MemoryConfiguration { Application = this };
        m_CommandArgs = acfg.Root;
      }

      m_ConfigRoot = rootConfig ?? GetConfiguration().Root;
      m_Singletons = new ApplicationSingletonManager();
      m_NOPApplicationSingletonManager = new NOPApplicationSingletonManager();
      m_DefaultDependencyInjector = defaultDI ?? new ApplicationDependencyInjector(this);
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
      SetShutdownStarted();

      DisposeAndNull(ref m_Realm);
      DisposeAndNull(ref m_Singletons);
      DisposeAndNull(ref m_DefaultDependencyInjector);

      DisposeAndNull(ref m_NOPEventTimer);
      DisposeAndNull(ref m_DefaultTimeSource);
      DisposeAndNull(ref m_NOPSecurityManager);
      DisposeAndNull(ref m_NOPGlue);
      DisposeAndNull(ref m_NOPObjectStore);
      DisposeAndNull(ref m_NOPDataStore);
      DisposeAndNull(ref m_NOPInstrumentation);
      DisposeAndNull(ref m_NOPModule);
      DisposeAndNull(ref m_NOPLog);

      base.Destructor();
      DisposeAndNull(ref m_ShutdownEvent);
    }

    #endregion

    #region Fields

    private Atom m_AppId;
    private Guid m_InstanceId = Guid.NewGuid();
    protected DateTime m_StartTime = DateTime.UtcNow;//Fix #494
    private Atom m_CloudOrigin;
    private ushort m_NodeDiscriminator;
    private IO.Console.IConsolePort m_ConsolePort;

    private string m_Name;
    private bool m_AllowNesting;

    private ManualResetEventSlim m_ShutdownEvent = new ManualResetEventSlim(false, spinCount: 2);
    private volatile bool m_ShutdownStarted;
    private volatile bool m_Stopping;

    protected IApplicationRealmImplementation m_Realm;

    protected List<IConfigSettings> m_ConfigSettings = new List<IConfigSettings>();
    protected List<IApplicationFinishNotifiable> m_FinishNotifiables = new List<IApplicationFinishNotifiable>();

    [Config(TimeLocation.CONFIG_TIMELOCATION_SECTION)]
    private TimeLocation m_TimeLocation = new TimeLocation();

    protected ConfigSectionNode m_CommandArgs;
    protected ConfigSectionNode m_ConfigRoot;

    protected ApplicationSingletonManager m_Singletons;
    protected IApplicationSingletonManager m_NOPApplicationSingletonManager;

    protected IApplicationDependencyInjectorImplementation m_DependencyInjector;
    protected IApplicationDependencyInjectorImplementation m_DefaultDependencyInjector;

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

    /// <summary> References ConsolePort for this app or null </summary>
    public IO.Console.IConsolePort ConsolePort => m_ConsolePort;

    /// <summary>True if this app chassis is a test rig </summary>
    public virtual bool IsUnitTest => m_ConfigRoot.AttrByName(CONFIG_UNIT_TEST_ATTR).ValueAsBool();

    /// <summary>Provides access to "environment-name" attribute, e.g. "DEV" vs "PROD"</summary>
    public string EnvironmentName => m_ConfigRoot.AttrByName(CONFIG_ENVIRONMENT_NAME_ATTR).Value;

    /// <summary>Provides access to "copyright" attribute, e.g. "(c) 2023 Azist Group"</summary>
    public string Copyright => m_ConfigRoot.AttrByName(CONFIG_COPYRIGHT_ATTR).Value.Default("2023 Azist Group");

    /// <summary>Provides access to "description" attribute, e.g. "xyz application"</summary>
    public string Description => m_ConfigRoot.AttrByName(CONFIG_DESCRIPTION_ATTR).Value.Default("AZ OS application");

    /// <summary>True to force app container set process-wide invariant culture on boot</summary>
    public virtual bool ForceInvariantCulture => m_ConfigRoot.AttrByName(CONFIG_FORCE_INVARIANT_CULTURE_ATTR).ValueAsBool();

    /// <summary>Application chassis logging log level, default is `Info`</summary>    //#884
    public MessageType ChassisLogLevel => m_ConfigRoot.AttrByName(CONFIG_CHASSIS_LOG_LEVEL_ATTR).ValueAsEnum(MessageType.Info);

    /// <summary>
    /// Provides a default expected shutdown duration for various constituent components of the application.
    /// The value is applied to entities which do not specify their own expected shutdown duration
    /// </summary>
    public int ExpectedComponentShutdownDurationMs
      => m_ConfigRoot.AttrByName(CONFIG_EXPECTED_COMPONENT_SHUTDOWN_DURATION_MS)
                     .ValueAsInt(DFLT_EXPECTED_COMPONENT_SHUTDOWN_DURATION_MS);

    /// <summary> Uniquely identifies this application type </summary>
    public Atom AppId => m_AppId;


    /// <summary>
    /// Provides an efficient global unique identifier of the cloud partition of a distributed system in which this application instance executes.
    /// Origins typically represents data centers or regions, for example, on AWS CloudOrigins are typically mapped to AWS regions which
    /// asynchronously replicate data
    /// </summary>
    public Atom CloudOrigin => m_CloudOrigin;

    /// <summary>
    /// Provides a short value which uniquely identifies the logical cluster network node.
    /// In most cases a node is the same as the host, however it is possible to launch multiple nodes - instances
    /// of the same logical application type on the same host. NodeDiscriminator is used by some
    /// services to differentiate these node instances.
    /// </summary>
    public ushort NodeDiscriminator => m_NodeDiscriminator;

    /// <summary>Returns unique identifier of this running instance</summary>
    public Guid InstanceId => m_InstanceId;

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
    public void Stop()
    {
      m_Stopping = true;
      NotifyPendingStopOrShutdown();
    }

    public IConfigSectionNode ConfigRoot => m_ConfigRoot;
    public IConfigSectionNode CommandArgs => m_CommandArgs;
    public IApplicationSingletonManager Singletons => m_Singletons ?? m_NOPApplicationSingletonManager;
    public IApplicationDependencyInjector DependencyInjector => m_DependencyInjector ?? m_DefaultDependencyInjector;
    public ILog Log => m_Log ?? m_NOPLog;
    public IInstrumentation Instrumentation => m_Instrumentation ?? m_NOPInstrumentation;
    public IDataStore DataStore => m_DataStore ?? m_NOPDataStore;
    public IObjectStore ObjectStore => m_ObjectStore ?? m_NOPObjectStore;
    public IGlue Glue => m_Glue ?? m_NOPGlue;
    public ISecurityManager SecurityManager => m_SecurityManager ?? m_NOPSecurityManager;
    public ITimeSource TimeSource => m_TimeSource ?? m_DefaultTimeSource;
    public IEventTimer EventTimer => m_EventTimer ?? m_NOPEventTimer;
    public IModule ModuleRoot => m_Module ?? m_NOPModule;

    /// <summary>
    /// Returns time location of this LocalizedTimeProvider implementation
    /// </summary>
    public TimeLocation TimeLocation
    {
      get { return m_TimeLocation ?? TimeLocation.Parent; }
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
    /// Sets app-wide console out port
    /// </summary>
    public void SetConsolePort(IO.Console.IConsolePort port) => m_ConsolePort = port;

    /// <summary>
    /// Converts universal time to local time as of TimeLocation property
    /// </summary>
    public DateTime UniversalTimeToLocalizedTime(DateTime utc)
    {
      if (utc.Kind != DateTimeKind.Utc)
        throw new TimeException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".UniversalTimeToLocalizedTime(utc.Kind!=UTC)");

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
      if (local.Kind != DateTimeKind.Local)
        throw new TimeException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".LocalizedTimeToUniversalTime(utc.Kind!=Local)");

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
      if (m_ShutdownStarted || settings == null) return false;
      lock (m_ConfigSettings)
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
      if (m_ShutdownStarted || settings == null) return false;
      lock (m_ConfigSettings)
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
      if (m_ShutdownStarted || notifiable == null) return false;

      lock (m_FinishNotifiables)
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
      if (m_ShutdownStarted || notifiable == null) return false;

      lock (m_FinishNotifiables)
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

    /// <summary>
    /// Performs resolution of the named application variable into its value.
    /// This mechanism is referenced by Configuration environment vars which start with app prefix
    /// </summary>
    public virtual bool ResolveNamedVar(string name, out string value)
    {
      return DefaultAppVarResolver.ResolveNamedVar(this, name, out value);
    }

    /// <summary>
    /// Completes the call returning true as soon as application stop or shutdown starts.
    /// The stop/shutdown is initiated by a different call flow/thread via a call to `Stop()`
    /// or deterministic application finalization via a call to `Dispose()`.
    /// False is returned if application has not yet started shutdown during the
    /// specified millisecond interval. Indefinite intervals are not allowed
    /// </summary>
    /// <param name="waitIntervalMs">
    /// Millisecond interval to wait for shutdown or stop. You may not pass values less than 1 ms as
    /// indefinite intervals are NOT supported
    /// </param>
    public bool WaitForStopOrShutdown(int waitIntervalMs)
    {
      var wait = m_ShutdownEvent;
      if (wait == null) return true;

      waitIntervalMs.IsTrue(v => v > 0, "{0} > 0".Args(nameof(waitIntervalMs)));
      var result = wait.Wait(waitIntervalMs);
      return result;
    }
    #endregion

    #region Protected

    [Config] private bool m_LogCallerFileLines;

    protected Guid WriteLog(MessageType type,
                            string from,
                            string msgText,
                            Exception error = null,
                            [CallerFilePath] string file = "",
                            [CallerLineNumber] int line = 0,
                            object pars = null,
                            Guid? related = null)
    {
      var log = m_Log;
      if (log == null || type < ChassisLogLevel) return Guid.Empty;

      var msg = new Message
      {
        Topic = CoreConsts.APPLICATION_TOPIC,
        Type = type,
        From = from,
        Text = msgText,
        Exception = error,
      };

      if (m_LogCallerFileLines)
        msg.SetParamsAsObject(Message.FormatCallerParams(pars, file, line));

      if (related.HasValue) msg.RelatedTo = related.Value;

      log.Write(msg);

      return msg.Guid;
    }

    /// <summary>
    /// Gets application configuration processing all includes (if required).
    /// The default implementation takes a file co-located with entry point in any of the supported formats
    /// </summary>
    protected virtual Configuration GetConfiguration()
    {
      //try to read from  /config file
      var configFile = m_CommandArgs[CONFIG_SWITCH].AttrByIndex(0).Value;

      var confWasSpecified = true;
      if (configFile.IsNullOrWhiteSpace())
      {
        configFile = GetDefaultConfigFileName();
        confWasSpecified = false;
      }

      Configuration conf;

      if (File.Exists(configFile))
      {
        conf = Configuration.ProviderLoadFromFile(configFile);
      }
      else
      {
        if (confWasSpecified)
          throw new AzosException(StringConsts.APP_INJECTED_CONFIG_FILE_NOT_FOUND_ERROR.Args(configFile, CONFIG_SWITCH));

        conf = new MemoryConfiguration();
      }

      conf.Application = this;

      //20190416 DKh added support for root config pragma includes
      ProcessAllExistingConfigurationIncludes(conf.Root);

      //20230722 DKh added support for root pragma excludes #888
      ProcessConfigurationExcludes(conf.Root);

      return conf;
    }

    protected IEnumerable<IApplicationStarter> GetStarters()
    {
      var snodes = m_ConfigRoot[CONFIG_STARTERS_SECTION].Children.Where(n => n.IsSameName(CONFIG_STARTER_SECTION));
      foreach (var snode in snodes)
      {
        var starter = FactoryUtils.MakeAndConfigure<IApplicationStarter>(snode);
        yield return starter;
      }
    }

    /// <summary>
    /// Tries to find a configuration file name looping through various supported extensions.
    /// The method respects `Ambient.ProcessName` if it is set
    /// </summary>
    /// <returns>File name that exists or empty string</returns>
    protected string GetDefaultConfigFileName()
    {
      var exeName = System.Reflection.Assembly.GetEntryAssembly().Location;
      var exeNameWoExt = Path.Combine(Path.GetDirectoryName(exeName), Ambient.ProcessName.Default(Path.GetFileNameWithoutExtension(exeName)));
      //Console.WriteLine("EXENAME:" +exeName);
      //Console.WriteLine("EXENAME wo extension:" +exeNameWoExt);
      var extensions = Configuration.AllSupportedFormats.Select(fmt => '.' + fmt);
      foreach (var ext in extensions)
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

      lock (m_ConfigSettings)
        foreach (var s in m_ConfigSettings) s.ConfigChanged(this, node);
    }

    /// <summary>
    /// Sets shutdown event wait handle
    /// </summary>
    protected void NotifyPendingStopOrShutdown()
    {
      var wait = m_ShutdownEvent;
      if (wait != null)
      {
        if (!wait.IsSet) wait.Set();
      }
    }

    /// <summary>
    /// Triggers ShutdownStarted and notifies shutdown event handle
    /// </summary>
    protected void SetShutdownStarted()
    {
      m_ShutdownStarted = true;
      NotifyPendingStopOrShutdown();
    }


    #endregion

  }

}
