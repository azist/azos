/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Apps.Injection;
using Azos.Log;
using Azos.Instrumentation;
using Azos.Conf;
using Azos.Data.Access;
using Azos.Apps.Volatile;
using Azos.Glue;
using Azos.Security;
using Azos.Time;
using System.Threading;

namespace Azos.Apps
{
  /// <summary>
  /// Represents an application that consists of pure-nop providers, consequently
  ///  this application does not log, does not store data and does not do anything else
  /// still satisfying its contract
  /// </summary>
  public class NOPApplication : DisposableObject, IApplicationImplementation
  {

    private static readonly NOPApplication s_Instance = new NOPApplication();

    protected NOPApplication()
    {
      m_Configuration = new MemoryConfiguration();
      m_Configuration.Create();

      m_CommandArgsConfiguration = new MemoryConfiguration();
      m_CommandArgsConfiguration.Create();

      m_StartTime = DateTime.Now;
      m_Singletons = new NOPApplicationSingletonManager();
      m_DependencyInjector = new ApplicationDependencyInjector(this);
      m_Realm = new ApplicationRealmBase(this);

      m_Log = new NOPLog(this);
      m_Instrumentation = new NOPInstrumentation(this);
      m_ObjectStore = new NOPObjectStore(this);
      m_Glue = new NOPGlue(this);
      m_DataStore = new NOPDataStore(this);
      m_SecurityManager = new NOPSecurityManager(this);
      m_Module = new NOPModule(this);
      m_TimeSource = new DefaultTimeSource(this);
      m_EventTimer = new EventTimer(this);
    }

    //Added for symmetry, as NOPApplication is never going to be disposed anyway as it is a process singleton
    //which is ALWAYS PRESENT, even when outside of any concrete application scope
    protected override void Destructor()
    {
      DisposeIfDisposableAndNull(ref m_EventTimer);
      DisposeIfDisposableAndNull(ref m_TimeSource);
      DisposeIfDisposableAndNull(ref m_Module);
      DisposeIfDisposableAndNull(ref m_SecurityManager);
      DisposeIfDisposableAndNull(ref m_DataStore);
      DisposeIfDisposableAndNull(ref m_Glue);
      DisposeIfDisposableAndNull(ref m_ObjectStore);
      DisposeIfDisposableAndNull(ref m_Instrumentation);
      DisposeIfDisposableAndNull(ref m_Log);
      DisposeIfDisposableAndNull(ref m_Realm);
      DisposeIfDisposableAndNull(ref m_DependencyInjector);
      DisposeIfDisposableAndNull(ref m_Singletons);

      base.Destructor();
    }

    /// <summary>
    /// Returns a singleton instance of the NOPApplication
    /// </summary>
    public static IApplication Instance => s_Instance;

    protected Guid m_InstanceID = Guid.NewGuid();
    protected DateTime m_StartTime;
    protected MemoryConfiguration m_Configuration;
    protected MemoryConfiguration m_CommandArgsConfiguration;
    protected IApplicationDependencyInjectorImplementation m_DependencyInjector;
    protected IApplicationSingletonManager m_Singletons;
    protected IApplicationRealmImplementation m_Realm;
    protected ILog m_Log;
    protected IInstrumentation m_Instrumentation;
    protected IObjectStore m_ObjectStore;
    protected IGlue m_Glue;
    protected IDataStore m_DataStore;
    protected ISecurityManager m_SecurityManager;
    protected IModule m_Module;
    protected ITimeSource m_TimeSource;
    protected IEventTimer m_EventTimer;

    #region IApplication Members

    public bool IsUnitTest => false;

    public string EnvironmentName => string.Empty;

    public bool ForceInvariantCulture => false;

    public string Description => "NOP Application";

    public int ExpectedComponentShutdownDurationMs => 0;

    public string Copyright => "2020 Framework";

    public IO.Console.IConsolePort ConsolePort => null;

    public Atom AppId => Atom.ZERO;

    public Atom CloudOrigin => Atom.ZERO;

    public ushort NodeDiscriminator => 0;

    public Guid InstanceId => m_InstanceID;

    public bool AllowNesting => false;

    public DateTime StartTime => m_StartTime;

    public bool Active => false;//20140128 DKh was true before

    public CancellationToken  ShutdownToken => new CancellationToken(false);

    public IApplicationRealm Realm => m_Realm;

    public IApplicationDependencyInjector DependencyInjector => m_DependencyInjector;

    public bool Stopping => false;

    public bool ShutdownStarted => false;

    public string Name => GetType().FullName;

    /// <summary>
    /// Enumerates all components of this application
    /// </summary>
    public IEnumerable<IApplicationComponent> AllComponents => ApplicationComponent.AllComponents(this);

    public IApplicationSingletonManager Singletons => m_Singletons;

    public ILog Log => m_Log;

    public IInstrumentation Instrumentation => m_Instrumentation;

    public IConfigSectionNode ConfigRoot => m_Configuration.Root;

    public IConfigSectionNode CommandArgs => m_CommandArgsConfiguration.Root;

    public IDataStore DataStore => m_DataStore;

    public IObjectStore ObjectStore => m_ObjectStore;

    public IGlue Glue => m_Glue;

    public ISecurityManager SecurityManager => m_SecurityManager;

    public IModule ModuleRoot => m_Module;

    public ITimeSource TimeSource => m_TimeSource;

    public IEventTimer EventTimer => m_EventTimer;

    public Platform.RandomGenerator Random => Platform.RandomGenerator.Instance;

    public TimeLocation TimeLocation => TimeLocation.Parent;

    public DateTime LocalizedTime => TimeSource.Now;

    public DateTime UniversalTimeToLocalizedTime(DateTime utc)
      => TimeSource.UniversalTimeToLocalizedTime(utc);

    public DateTime LocalizedTimeToUniversalTime(DateTime local)
      => TimeSource.LocalizedTimeToUniversalTime(local);

    public ISession MakeNewSessionInstance(Guid sessionID, Security.User user = null)
      => NOPSession.Instance;

    public bool RegisterConfigSettings(IConfigSettings settings)
      => false;

    public bool UnregisterConfigSettings(IConfigSettings settings)
      => false;

    public void NotifyAllConfigSettingsAboutChange() { }

    public bool RegisterAppFinishNotifiable(IApplicationFinishNotifiable notifiable)
      => false;

    public bool UnregisterAppFinishNotifiable(IApplicationFinishNotifiable notifiable)
    => false;

    public void Stop() { }

    /// <summary>
    /// Returns a component by SID or null
    /// </summary>
    public IApplicationComponent GetComponentBySID(ulong sid)
      => ApplicationComponent.GetAppComponentBySID(this, sid);

    /// <summary>
    /// Returns an existing application component instance by its ComponentCommonName or null. The search is case-insensitive
    /// </summary>
    public IApplicationComponent GetComponentByCommonName(string name)
      => ApplicationComponent.GetAppComponentByCommonName(this, name);

    public bool ResolveNamedVar(string name, out string value)
      => DefaultAppVarResolver.ResolveNamedVar(this, name, out value);

    public void SetConsolePort(IO.Console.IConsolePort port) { }

    public bool WaitForStopOrShutdown(int waitIntervalMs)
    {
      if (waitIntervalMs > 0) System.Threading.Thread.Sleep(waitIntervalMs);
      return false;
    }

    #endregion

  }
}
