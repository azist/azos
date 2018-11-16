/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
  public class NOPApplication : IApplication
  {

     private static NOPApplication s_Instance = new NOPApplication();

     protected NOPApplication()
     {
        m_Configuration = new MemoryConfiguration();
        m_Configuration.Create();

        m_CommandArgsConfiguration = new MemoryConfiguration();
        m_CommandArgsConfiguration.Create();

        m_StartTime = DateTime.Now;
        m_Realm = new ApplicationRealmBase();
     }



     /// <summary>
     /// Returns a singleton instance of the NOPApplication
     /// </summary>
     public static NOPApplication Instance
     {
       get { return s_Instance; }
     }


    private Guid m_InstanceID = Guid.NewGuid();
    private DateTime m_StartTime;
    protected MemoryConfiguration m_Configuration;
    protected MemoryConfiguration m_CommandArgsConfiguration;
    protected IApplicationRealmImplementation m_Realm;


    #region IApplication Members


        public bool IsUnitTest{ get{ return false; } }

        public string EnvironmentName { get { return string.Empty; } }

        public bool ForceInvariantCulture { get { return false; } }

        public Guid InstanceID
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

        public bool Active
        {
          get { return false;}//20140128 DKh was true before
        }


        public IApplicationRealm Realm
        {
          get { return m_Realm;}
        }

        public bool Stopping
        {
          get { return false;}
        }

        public bool ShutdownStarted
        {
          get { return false;}
        }

        public string Name
        {
          get { return GetType().FullName; }
        }

        public ILog Log
        {
          get { return NOPLog.Instance; }
        }

        public IInstrumentation Instrumentation
        {
          get { return NOPInstrumentation.Instance; }
        }

        public IConfigSectionNode ConfigRoot
        {
          get { return m_Configuration.Root; }
        }

        public IConfigSectionNode CommandArgs
        {
          get { return m_CommandArgsConfiguration.Root; }
        }

        public IDataStore DataStore
        {
          get { return NOPDataStore.Instance; }
        }

        public IObjectStore ObjectStore
        {
          get { return NOPObjectStore.Instance; }
        }

        public IGlue Glue
        {
          get { return NOPGlue.Instance; }
        }

        public ISecurityManager SecurityManager
        {
          get { return NOPSecurityManager.Instance; }
        }

        public IModule ModuleRoot
        {
          get { return NOPModule.Instance; }
        }

        public ITimeSource TimeSource
        {
            get { return DefaultTimeSource.Instance; }
        }


        public IEventTimer EventTimer
        {
            get { return NOPEventTimer.Instance; }
        }


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

    #endregion

  }
}
