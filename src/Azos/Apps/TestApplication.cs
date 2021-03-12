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
using Azos.Time;
using Azos.Apps.Volatile;
using Azos.Data.Access;
using Azos.Conf;
using Azos.Glue;
using Azos.Instrumentation;
using Azos.Log;
using Azos.Security;

namespace Azos.Apps
{
    /// <summary>
    /// Application designated for use in various unit test cases.
    /// This class is not intended for use in non-test systems
    /// </summary>
    public class TestApplication : DisposableObject, IApplication
    {
        protected Guid m_InstanceID = Guid.NewGuid();
        protected List<IConfigSettings> m_ConfigSettings = new List<IConfigSettings>();
        protected ConfigSectionNode m_ConfigRoot;
        protected ConfigSectionNode m_CommandArgs;

        public TestApplication(ConfigSectionNode cfgRoot = null)
        {
            this.ConfigRoot = cfgRoot;

            Singletons = new ApplicationSingletonManager();
            DependencyInjector = new ApplicationDependencyInjector(this);
            Active = true;
            StartTime = DateTime.Now;
            Log = new NOPLog(this);
            Instrumentation = new NOPInstrumentation(this);
            DataStore = new NOPDataStore(this);
            ObjectStore = new NOPObjectStore(this);
            Glue = new NOPGlue(this);
            ModuleRoot = new NOPModule(this);
            SecurityManager = new NOPSecurityManager(this);
            TimeSource = new DefaultTimeSource(this);
            TimeLocation = TimeLocation.Parent;
            EventTimer = new NOPEventTimer(this);

            Realm = new ApplicationRealmBase(this);

            Apps.ExecutionContext.__BindApplication(this);
        }

        protected override void Destructor()
        {
#warning Why do we not deallocate here all stuff allocated in ctor?
            Apps.ExecutionContext.__UnbindApplication(this);
        }

        public virtual IO.Console.IConsolePort ConsolePort { get; set; }

        public virtual string Copyright { get; set; }

        public virtual string Description { get; set;}

        public virtual int ExpectedComponentShutdownDurationMs {get; set;}

        public virtual bool IsUnitTest { get; set; }

        public virtual string EnvironmentName { get; set; }

        public virtual IApplicationRealm Realm{ get; set;}

        public IApplicationDependencyInjector DependencyInjector { get; set; }

        public virtual bool ForceInvariantCulture { get; set; }

        public virtual Atom AppId { get; set; }

        public virtual Guid InstanceId { get { return m_InstanceID;}}

        public virtual bool AllowNesting { get { return false;}}

        public virtual DateTime StartTime { get; set;}

        public virtual bool Active { get; set; }

        public virtual bool Stopping { get; set; }

        public virtual bool ShutdownStarted { get; set; }

        public virtual Log.ILog Log { get; set; }

        public virtual Instrumentation.IInstrumentation Instrumentation { get; set; }

        public virtual Platform.RandomGenerator Random => Platform.RandomGenerator.Instance;

        /// <summary>
        /// Enumerates all components of this application
        /// </summary>
        public IEnumerable<IApplicationComponent> AllComponents => ApplicationComponent.AllComponents(this);

        public virtual IConfigSectionNode ConfigRoot
        {
           get{return m_ConfigRoot;}
           set
           {
             if (value==null)
             {
                var conf = new MemoryConfiguration();
                conf.Create();
                value = conf.Root;
             }
             m_ConfigRoot = (ConfigSectionNode)value;
           }
        }

        public virtual IConfigSectionNode CommandArgs
        {
           get{return m_CommandArgs;}
           set
           {
             if (value==null)
             {
                var conf = new MemoryConfiguration();
                conf.Create();
                value = conf.Root;
             }
             m_CommandArgs = (ConfigSectionNode)value;
           }
        }


        public IApplicationSingletonManager Singletons { get; set; }

        public virtual IDataStore DataStore { get; set; }

        public virtual Volatile.IObjectStore ObjectStore { get; set; }

        public virtual Glue.IGlue Glue { get; set; }

        public virtual IModule ModuleRoot { get; set; }

        public virtual Security.ISecurityManager SecurityManager { get; set; }

        public virtual Time.ITimeSource TimeSource { get; set; }

        public virtual Time.IEventTimer EventTimer { get; set; }

        public virtual ISession MakeNewSessionInstance(Guid sessionID, Security.User user = null)
        {
            return NOPSession.Instance;
        }

        /// <summary>
        /// Registers an instance of IConfigSettings with application container to receive a call when
        ///  underlying app configuration changes
        /// </summary>
        public virtual bool RegisterConfigSettings(IConfigSettings settings)
        {
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
        public virtual bool UnregisterConfigSettings(IConfigSettings settings)
        {
            lock (m_ConfigSettings)
                return m_ConfigSettings.Remove(settings);
        }

        /// <summary>
        /// Forces notification of all registered IConfigSettings-implementers about configuration change
        /// </summary>
        public virtual void NotifyAllConfigSettingsAboutChange()
        {
            NotifyAllConfigSettingsAboutChange(m_ConfigRoot);
        }


        public virtual string Name { get; set; }

        public virtual Time.TimeLocation TimeLocation { get; set; }

        public virtual DateTime LocalizedTime
        {
            get { return UniversalTimeToLocalizedTime(TimeSource.UTCNow); }
            set { }
        }

            public DateTime UniversalTimeToLocalizedTime(DateTime utc)
            {
                if (utc.Kind!=DateTimeKind.Utc)
                 throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+".UniversalTimeToLocalizedTime(utc.Kind!=UTC)");

                var loc = TimeLocation;
                if (!loc.UseParentSetting)
                {
                   return DateTime.SpecifyKind(utc + TimeLocation.UTCOffset, DateTimeKind.Local);
                }
                else
                {
                   return TimeSource.UniversalTimeToLocalizedTime(utc);
                }
            }

            public DateTime LocalizedTimeToUniversalTime(DateTime local)
            {
                if (local.Kind!=DateTimeKind.Local)
                 throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+".LocalizedTimeToUniversalTime(utc.Kind!=Local)");

                var loc = TimeLocation;
                if (!loc.UseParentSetting)
                {
                   return DateTime.SpecifyKind(local - TimeLocation.UTCOffset, DateTimeKind.Utc);
                }
                else
                {
                   return TimeSource.LocalizedTimeToUniversalTime(local);
                }
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

        public void SetConsolePort(IO.Console.IConsolePort port) => this.ConsolePort = port;
  }
}
