/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Time;

namespace Azos.Apps
{
  /// <summary>
  /// Describes general application model - usually a root service locator with dependency injection container
  ///  that governs application initialization, state management, logging etc.
  /// An applications is usually implemented with a singleton class that has static
  ///  conduits to instance properties via App shortcut.
  /// Application instances may get passed by reference to simplify mocking
  /// </summary>
  /// <remarks>
  /// This pattern is used on purpose based on careful evaluation of various DI frameworks use-cases in various projects,
  /// both server and client-side. The central service/locator hub per process as facilitated by the IApplication is the most intuitive and simple
  /// dependency resolution facility for 90+% of various business applications - it significantly simplifies development and debugging as
  /// Application provides a common root for all ApplicationComponents regardless of the app types
  /// </remarks>
  public interface IApplication : Collections.INamed, ILocalizedTimeProvider
  {
     /// <summary>
     /// True if app is launched as a unit test as set by the app config "unit-test=true"
     /// The general use of this flag is discouraged as code constructs should not form special cases just for unit testing,
     /// however in some cases this flag is useful. It is not exposed via App. static accessors
     /// </summary>
     bool IsUnitTest{ get;}

      /// <summary>
     /// True when the app should force the process-wide invariant culture regardless of machine-level culture
     /// </summary>
     bool ForceInvariantCulture{ get;}

     /// <summary>
     /// Provides access to "environment-name" attribute
     /// </summary>
     string EnvironmentName { get; }

     /// <summary>
     /// Returns unique identifier of this running instance
     /// </summary>
     Guid InstanceID { get; }

     /// <summary>
     /// Returns true when this app container allows nesting of another one
     /// </summary>
     bool AllowNesting{ get; }

     /// <summary>
     /// Returns time stamp when application started as localized app time
     /// </summary>
     DateTime StartTime { get; }


     /// <summary>
     /// Returns true when application instance is active and working. This property returns false as soon as application finalization starts on shutdown or Stop() was called
     /// Use to exit long-running loops and such
     /// </summary>
     bool Active { get; }

     /// <summary>
     /// Returns true after Stop() was called
     /// </summary>
     bool Stopping { get;}

     /// <summary>
     /// Returns true after Dispose() was called to indicate that application is shutting down
     /// </summary>
     bool ShutdownStarted { get;}

     /// <summary>
     /// Initiates the stop of the application by setting its Stopping to true and Active to false so dependent services may start to terminate
     /// </summary>
     void Stop();

     /// <summary>
     /// References an accessor to the application surrounding environment (realm) in which app gets executed.
     /// This realm is sub-divided into uniquely-named areas each reporting their status.
     /// This is used by various app components and services to assess the environment status in which they execute, for example:
     /// a logger may suppress error messages from network in a cluster when the area is about to be upgraded to new software.
     /// One may consider this status as a "message board" where services/system check/report the planned or unexpected outages and
     /// adjust their behavior accordingly. Azos provides only the base implementation of such classes delegating the specifics to more
     /// concrete app containers.
     /// </summary>
     IApplicationRealm Realm { get; }

     /// <summary>
     /// References app log
     /// </summary>
     Log.ILog Log { get; }

     /// <summary>
     /// References instrumentation for this application instance
     /// </summary>
     Instrumentation.IInstrumentation Instrumentation { get; }

     /// <summary>
     /// References application configuration root
     /// </summary>
     Environment.IConfigSectionNode  ConfigRoot { get; }

     /// <summary>
     /// References application command arguments
     /// </summary>
     Environment.IConfigSectionNode  CommandArgs { get; }

     /// <summary>
     /// References application data store
     /// </summary>
     DataAccess.IDataStore DataStore { get; }

     /// <summary>
     /// References object store that may be used to persist object graphs between volatile application shutdown cycles
     /// </summary>
     Volatile.IObjectStore ObjectStore { get; }

     /// <summary>
     /// References glue implementation that may be used to "glue" remote instances/processes/contracts together
     /// </summary>
     Glue.IGlue Glue { get; }

     /// <summary>
     /// References security manager that performs user authentication based on passed credentials and other security-related global tasks
     /// </summary>
     Security.ISecurityManager SecurityManager { get; }

     /// <summary>
     /// References time source - an entity that supplies local and UTC times. The concrete implementation
     ///  may elect to get accurate times from the network or other external precision time sources (i.e. NASA atomic clock)
     /// </summary>
     Time.ITimeSource TimeSource { get; }

     /// <summary>
     /// References event timer - an entity that maintains and runs scheduled instances of Event
     /// </summary>
     Time.IEventTimer EventTimer { get; }

     /// <summary>
     /// References the root module (such as business domain logic) for this application. This is a dependency injection root
     /// provided for any application type
     /// </summary>
     IModule ModuleRoot{ get; }

     /// <summary>
     /// Factory method that creates new session object suitable for particular application type
     /// </summary>
     /// <param name="sessionID">Session identifier</param>
     /// <param name="user">Optional user object that the session is for</param>
     /// <returns>New session object</returns>
     ISession MakeNewSessionInstance(Guid sessionID, Security.User user = null);


     /// <summary>
     /// Registers an instance of IConfigSettings with application container to receive a call when
     ///  underlying app configuration changes
     /// </summary>
     /// <returns>True if settings instance was not found and was added</returns>
     bool RegisterConfigSettings(Conf.IConfigSettings settings);

     /// <summary>
     /// Removes the registration of IConfigSettings from application container
     /// </summary>
     /// <returns>True if settings instance was found and removed</returns>
     bool UnregisterConfigSettings(Conf.IConfigSettings settings);

     /// <summary>
     /// Forces notification of all registered IConfigSettings-implementers about configuration change
     /// </summary>
     void NotifyAllConfigSettingsAboutChange();

     /// <summary>
     /// Registers an instance of IApplicationFinishNotifiable with application container to receive a call when
     ///  underlying application instance will finish its life cycle.
     /// </summary>
     /// <returns>True if notifiable instance was not found and was added</returns>
     bool RegisterAppFinishNotifiable(IApplicationFinishNotifiable notifiable);

     /// <summary>
     /// Removes the registration of IApplicationFinishNotifiable from application container
     /// </summary>
     /// <returns>True if notifiable instance was found and removed</returns>
     bool UnregisterAppFinishNotifiable(IApplicationFinishNotifiable notifiable);
  }


}
