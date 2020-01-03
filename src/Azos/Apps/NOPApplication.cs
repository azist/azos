/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Apps.Injection;
using Azos.Log;
using Azos.Instrumentation;
using Azos.Conf;
using Azos.Data.Access;
using Azos.Apps.Volatile;
using Azos.Glue;
using Azos.Security;
using Azos.Time;

namespace Azos.Apps
{
  /// <summary>
  /// Represents an application that consists of pure-nop providers, consequently
  ///  this application does not log, does not store data and does not do anything else
  /// still satisfying its contract
  /// </summary>
  public class NOPApplication : IApplicationImplementation
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



     /// <summary>
     /// Returns a singleton instance of the NOPApplication
     /// </summary>
     public static NOPApplication Instance
     {
       get { return s_Instance; }
     }


    protected Guid m_InstanceID = Guid.NewGuid();
    protected DateTime m_StartTime;
    protected MemoryConfiguration m_Configuration;
    protected MemoryConfiguration m_CommandArgsConfiguration;
    protected IApplicationDependencyInjectorImplementation m_DependencyInjector;
    protected IApplicationSingletonManager m_Singletons;
    protected IApplicationRealmImplementation m_Realm;
    protected ILog               m_Log;
    protected IInstrumentation   m_Instrumentation;
    protected IObjectStore       m_ObjectStore;
    protected IGlue              m_Glue;
    protected IDataStore         m_DataStore;
    protected ISecurityManager   m_SecurityManager;
    protected IModule            m_Module;
    protected ITimeSource        m_TimeSource;
    protected IEventTimer        m_EventTimer;





    #region IApplication Members


        public bool IsUnitTest{ get{ return false; } }

        public string EnvironmentName { get { return string.Empty; } }

        public bool ForceInvariantCulture { get { return false; } }


        public IO.Console.IConsolePort ConsolePort => null;

        public Atom AppId => Atom.ZERO;

        public Guid InstanceId
        {
            get { return m_InstanceID; }
        }

        public bool AllowNesting
        {
            get { return false; }
        }

        public DateTime StartTime
        {
            get { return m_StartTime; }
        }

        public bool Active => false;//20140128 DKh was true before

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

        public IConfigSectionNode ConfigRoot
        {
          get { return m_Configuration.Root; }
        }

        public IConfigSectionNode CommandArgs
        {
          get { return m_CommandArgsConfiguration.Root; }
        }

        public IDataStore DataStore => m_DataStore;
        public IObjectStore ObjectStore => m_ObjectStore;
        public IGlue Glue => m_Glue;
        public ISecurityManager SecurityManager => m_SecurityManager;
        public IModule ModuleRoot => m_Module;
        public ITimeSource TimeSource => m_TimeSource;
        public IEventTimer EventTimer => m_EventTimer;

        public Platform.RandomGenerator Random => Platform.RandomGenerator.Instance;

        public TimeLocation TimeLocation
        {
            get { return TimeLocation.Parent; }
        }

        public DateTime LocalizedTime
        {
            get { return TimeSource.Now; }
        }

        public DateTime UniversalTimeToLocalizedTime(DateTime utc)
        {
            return TimeSource.UniversalTimeToLocalizedTime(utc);
        }

        public DateTime LocalizedTimeToUniversalTime(DateTime local)
        {
            return TimeSource.LocalizedTimeToUniversalTime(local);
        }

        public ISession MakeNewSessionInstance(Guid sessionID, Security.User user = null)
        {
            return NOPSession.Instance;
        }

        public bool RegisterConfigSettings(IConfigSettings settings)
        {
            return false;
        }

        public bool UnregisterConfigSettings(IConfigSettings settings)
        {
            return false;
        }

        public void NotifyAllConfigSettingsAboutChange()
        {

        }

        public bool RegisterAppFinishNotifiable(IApplicationFinishNotifiable notifiable)
        {
            return false;
        }

        public bool UnregisterAppFinishNotifiable(IApplicationFinishNotifiable notifiable)
        {
            return false;
        }

        public void Stop()
        {

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

        public bool ResolveNamedVar(string name, out string value)
        {
          return DefaultAppVarResolver.ResolveNamedVar(this, name, out value);
        }

        public void SetConsolePort(IO.Console.IConsolePort port){ }

    #endregion

  }
}
