/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;

namespace Azos
{
    /// <summary>
    /// Provides a shortcut access to app-global context Azos.Apps.ExecutionContext.Application.*
    /// </summary>
    public static class App
    {
      /// <summary>
      /// Denotes memory utilization modes
      /// </summary>
      public enum MemoryUtilizationModel
      {
        /// <summary>
        /// The application may use memory in a regular way without restraints. For example, a component
        /// may pre-allocate a few hundred megabayte lookup table on stratup trading space for speed.
        /// </summary>
        Regular = 0,

        /// <summary>
        /// The application must try to use memory sparingly and not allocate large cache and buffers.
        /// This mode is typically used in a constrained 32bit apps and smaller servers. This mode gives
        /// a hint to components not to pre-allocate too much (e.g. do not pre-load 100 mb ZIP code database on startup)
        /// on startup. Trades performance for lower memory consumption
        /// </summary>
        Compact = -1,

        /// <summary>
        /// The application must try not to use extra memory for caches and temp buffers.
        /// This mode is typically used in a constrained 32bit apps and smaller servers.
        /// This mode is stricter than Compact
        /// </summary>
        Tiny = -2
      }


      private static MemoryUtilizationModel s_MemoryModel;


      /// <summary>
      /// Returns the memory utilization model for the application.
      /// This property is NOT configurable. It may be set at process entry point via a call to
      /// App.SetMemoryModel() before the app container spawns.
      /// Typical applications should not change the defaults.
      /// Some system service providers examine this property to allocate less cache and temp buffers
      /// in the memory-constrained environments
      /// </summary>
      public static MemoryUtilizationModel MemoryModel => s_MemoryModel;

      /// <summary>
      /// Sets the memory utilization model for the whole app.
      /// This setting is NOT configurable. It may be set at process entrypoint via a call to
      /// App.SetMemoryModel() before the app contrainer spawns.
      /// Typical applications should not change the defaults.
      /// Some system service providers may examine this property to allocate less cache and temp buffers
      /// in the memory-constrained environments
      /// </summary>
      public static void SetMemoryModel(MemoryUtilizationModel model)
      {
        if (Available)
          throw new AzosException(StringConsts.APP_SET_MEMORY_MODEL_ERROR);
        s_MemoryModel = model;
      }



      /// <summary>
      /// Shortcut access to  ExecutionContext.Application;
      /// </summary>
      public static IApplication Instance =>  ExecutionContext.Application;


      /// <summary>
      /// Returns current session, this is a shortuct to ExecutionContext.Session
      /// </summary>
      public static ISession Session =>  ExecutionContext.Session;

      /// <summary>
      /// Returns the current call context user
      /// </summary>
      public static Security.User CurrentCallUser => Session.User;

      /// <summary>
      /// Returns unique identifier of this running instance
      /// </summary>
      public static Guid InstanceID => Instance.InstanceID;

      /// <summary>
      /// Returns timestamp when application started as localized app time
      /// </summary>
      public static DateTime StartTime => Instance.StartTime;

      /// <summary>
      /// Returns true when application container is active non-NOPApplication instance
      /// </summary>
      public static bool Available
      {
        get
        {
              var inst = Instance;
              return inst!=null &&
                    !(inst is NOPApplication) &&
                    inst.Active;
        }
      }


      /// <summary>
      /// Returns true when application instance is active and working. This property returns false as soon as application finalization starts on shutdown
      /// Use to exit long-running loops and such
      /// </summary>
      public static bool Active => Instance.Active;


      /// <summary>
      /// Returns application name
      /// </summary>
      public static string Name => Instance.Name;

      /// <summary>
      /// Returns environment name
      /// </summary>
      public static string EnvironmentName => Instance.EnvironmentName;

      /// <summary>
      /// References app log
      /// </summary>
      public static Log.ILog Log => Instance.Log;

      /// <summary>
      /// References instrumentation for this application instance
      /// </summary>
      public static Instrumentation.IInstrumentation Instrumentation => Instance.Instrumentation;

      /// <summary>
      /// References application configuration root
      /// </summary>
      public static Conf.IConfigSectionNode  ConfigRoot => Instance.ConfigRoot;

      /// <summary>
      /// References application data store
      /// </summary>
      public static Data.Access.IDataStore DataStore => Instance.DataStore;

      /// <summary>
      /// References object store that may be used to persist object graphs between volatile application shutdown cycles
      /// </summary>
      public static Azos.Apps.Volatile.IObjectStore ObjectStore => Instance.ObjectStore;

      /// <summary>
      /// References glue implementation that may be used to "glue" remote instances/processes/contracts together
      /// </summary>
      public static Glue.IGlue Glue => Instance.Glue;

      /// <summary>
      /// References security manager that performs user authentication based on passed credentials and other security-related global tasks
      /// </summary>
      public static Security.ISecurityManager SecurityManager => Instance.SecurityManager;

      /// <summary>
      /// References time source - an entity that supplies local and UTC times. The concrete implementation
      ///  may elect to get accurate times from the network or other external precision time sources (i.e. NASA atomic clock)
      /// </summary>
      public static Time.ITimeSource TimeSource => Instance.TimeSource;


      /// <summary>
      /// References event timer that maintains and runs scheduled Event instances
      /// </summary>
      public static Time.IEventTimer EventTimer => Instance.EventTimer;

      /// <summary>
      /// References the root module (such as business domain logic root) for this application. This is a dependency injection root
      /// provided for any application type
      /// </summary>
      public static IModule ModuleRoot => Instance.ModuleRoot;


      /// <summary>
      /// References application-wide RandomGenerator.
      /// By default, it is a shortcut to the default instance of Platform.RandomGenerator.Instance
      /// </summary>
      public static Platform.RandomGenerator Random => Platform.RandomGenerator.Instance;


      /// <summary>
      /// Returns the location
      /// </summary>
      public static Time.TimeLocation TimeLocation => Instance.TimeLocation;


      /// <summary>
      /// Returns current time localized per TimeLocation
      /// </summary>
      public static DateTime LocalizedTime => Instance.LocalizedTime;

      /// <summary>
      /// Converts universal time to local time as of TimeLocation property
      /// </summary>
      public static DateTime UniversalTimeToLocalizedTime(DateTime utc)
      {
        return Instance.UniversalTimeToLocalizedTime(utc);
      }

      /// <summary>
      /// Converts localized time to universal time as of TimeLocation property
      /// </summary>
      public static DateTime LocalizedTimeToUniversalTime(DateTime local)
      {
        return Instance.LocalizedTimeToUniversalTime(local);
      }



    }
}
