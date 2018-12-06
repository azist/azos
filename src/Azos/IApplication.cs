/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using Azos.Time;

using Azos.Apps;

namespace Azos
{
  /// <summary>
  /// Establishes a general model for applications - a "chassis": root service composite governs app initialization,
  /// module linking (service location), state management, logging, security, and other app/process-wide activities.
  /// An applications is usually allocated at program entry point and provides common "ambient" context - a "chassis pattern".
  /// </summary>
  public interface IApplication : Collections.INamed, ILocalizedTimeProvider
  {
     /// <summary>
     /// True if app is launched as a unit test as set by the app config "unit-test=true"
     /// The general use of this flag is discouraged as code constructs should not form special cases just for unit testing,
     /// however in some cases this flag is useful. It is not exposed via App. static accessors
     /// </summary>
     bool IsUnitTest{ get; }

     /// <summary>
     /// True when the app should force the process-wide invariant culture regardless of machine-level culture.
     /// This is used in server applications
     /// </summary>
     bool ForceInvariantCulture{ get; }

     /// <summary>
     /// Provides access to "environment-name" attribute, e.g. "DEV" vs "PROD"
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
     /// Returns true when application instance is active and working. This property returns false as soon as
     /// application finalization starts on shutdown or Stop() was called.
     /// Used to exit background workers gracefully (e.g. reactor completion threads, abort pending tasks etc.)
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
     /// Initiates the stop of the application by setting its Stopping to true and Active to false so
     /// dependent workers may start to complete
     /// </summary>
     void Stop();


     /// <summary>
     /// References application dependency injector which injects app-context rooted dependencies into
     /// objects. The frameworks calls this service automatically for most places (Mvc models/views/controllers, Glue servers)
     /// so business application developers should rarely (if ever) use this facility
     /// </summary>
     Apps.Injection.IApplicationDependencyInjector DependencyInjector { get; }

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
     /// References app primary logging facility
     /// </summary>
     Log.ILog Log { get; }

     /// <summary>
     /// References instrumentation for this application instance
     /// </summary>
     Instrumentation.IInstrumentation Instrumentation { get; }

     /// <summary>
     /// References application configuration root
     /// </summary>
     Conf.IConfigSectionNode  ConfigRoot { get; }

     /// <summary>
     /// References application launch command arguments
     /// </summary>
     Conf.IConfigSectionNode  CommandArgs { get; }

     /// <summary>
     /// References application data store
     /// </summary>
     Data.Access.IDataStore DataStore { get; }

     /// <summary>
     /// References object store that may be used to persist object graphs between volatile application shutdown cycles
     /// </summary>
     Apps.Volatile.IObjectStore ObjectStore { get; }

     /// <summary>
     /// References glue implementation that is used to "glue" remote instances/processes/contracts together (IPC)
     /// </summary>
     Glue.IGlue Glue { get; }

     /// <summary>
     /// References security manager that performs user authentication based on passed credentials and other security-related general tasks
     /// </summary>
     Security.ISecurityManager SecurityManager { get; }

     /// <summary>
     /// References time source - an entity that supplies local and UTC times. The concrete implementation
     ///  may elect to get accurate times from the network or other external precision time sources (i.e. NASA atomic clock)
     /// </summary>
     Time.ITimeSource TimeSource { get; }

     /// <summary>
     /// References event timer - an entity that maintains and runs scheduled instances of Event class
     /// </summary>
     Time.IEventTimer EventTimer { get; }

     /// <summary>
     /// References the root module (such as business domain logic) for this application. This is a dependency injection root
     /// provided for any application type
     /// </summary>
     IModule ModuleRoot{ get; }

     /// <summary>
     /// Returns all components that this application contains
     /// </summary>
     IEnumerable<IApplicationComponent> AllComponents {  get; }

     /// <summary>
     /// Returns random generator used by the application
     /// </summary>
     Platform.RandomGenerator Random {  get; }

     /// <summary>
     /// Factory method that creates new session object suitable for particular application type
     /// </summary>
     /// <param name="sessionID">Session identifier</param>
     /// <param name="user">Optional user object that the session is for</param>
     /// <returns>New session object</returns>
     ISession MakeNewSessionInstance(Guid sessionID, Security.User user = null);

     /// <summary>
     /// Returns a component by SID or null
     /// </summary>
     IApplicationComponent GetComponentBySID(ulong sid);

     /// <summary>
     /// Returns an existing application component instance by its ComponentCommonName or null. The search is case-insensitive
     /// </summary>
     IApplicationComponent GetComponentByCommonName(string name);

     /// <summary>
     /// Manages singleton instances per application
     /// </summary>
     IApplicationSingletonManager Singletons {  get; }

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
