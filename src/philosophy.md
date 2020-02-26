# Azos Design Philosophy
Back to [Documentation Index](/src/documentation-index.md)

This section describes design choices and patterns used in Azos framework. We also touch upon
a few common software [myths and truisms](/src/truisms.md).


## Overview
The philosophy of Azos is based on observations made in the past 20+ years of corporate code bases in 10s of large and medium scale organizations. 
It has been concluded that **modern framework landscape is still of a too low level
for an average developer to grasp and apply properly**. Another detrimental factor is **fragmentation and too many choices**. There are 100s 
(in not 1000s) of ways these days to do the same thing only a bit differently, consequentially the solution as a whole becomes very complex
trying to integrate different pieces together *(e.g. use 3 different log frameworks in the same project, each requiring its own configuration)*.

See [myths and truisms](/src/truisms.md).

Azos follows the following design principles:
- **Provide a full stack of services** needed in a typical business application aka **"Application Chassis"**:
  - Uniform Configuration engine with variables, structural navigation/merging/overrides, file includes, local or distributed, format agnostic, object binding
  - Logging: sink graphs, SLA, filtering, multi-channel support (e.g. sec vs app)- Instrumentation/Tracing/Performance counters + TeleVision/TeleMetry for cloud systems
  - DI (Dependency Injection) while curbing unnecessary object allocation abuse
  - Service client pattern: multiple endpoints, sharding, balancing, fail-over, circuit breaker, bulkhead, name/contract resolution, throttling
  - Serialization: Tight binary, JSON, BSON, XML. Culture-sensitive serialization (e.g. only write iso-lang keys)
  - **RPC**/Microservices/Contract-based + Security
  - **Security**/Permissions/Identities/Password/Tokens/Authentication/Authorization
  - Web pipeline/MVC/ APIs
  - **Custom Metadata** sources + **Documentation Generation**
  - **Data document** modeling with rich constraints and metadata (for data access and distributed protocols/RPC/REST/API)
  - Auto **data document mapping** to hybrid data sources / Auto CRUD
  - **Hybrid data access**: RDBMS, NoSQL, flat file, Web Service data sources
  - **Virtual File System** abstraction - work with Local, SVN, Amazon S3, Google Drive and other file systems via the same API
  - Precise TimeSource and **EventScheduler** - run scheduled jobs/events
  - **Daemons** - create lightweight in-app "processes" controllable with Start/Stop commands
  - **In-process large cache** (capable of storing 100s of millions of objects in memory)
  - **i18n/culture sensitive data** structures, NLS, culture-aware serialization, multi-language metadata etc.
  - Utilities: prime math, date math, leaky bucket, circuit breaker etc.
- **Reuse internal building blocks**, thus reducing complexity and footprint
- Achieve higher performance due to use of intrinsics and optimizations for internal types (e.g. StringMap direct serialization)
- **Sets guidelines for every app aspect** implementation using the above


## Overall Application Structure

Azos application structure unifies different app types: console/CLI, web server, RPC service(not necessarily web), UI.
Unlike the approach traditionally taken in .Net framework *(which is changing now towards using 
[universal generic host/HostBuilder process model](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-2.1)*
for any kind of app), Azos uses the same app hosting chassis for all application types.

Any Azos application starts from allocation of the [`AzosApplication : IApplication`](/src/Azos/Apps/AzosApplication.cs) (or its derivative) chassis, 
which in turn boots all of the root services. The following are the root services of [`IApplication`](/src/Azos/IApplication.cs):



Application is a composition root for the [`IApplicationComponent`](/src/Azos/Apps/ApplicationComponent.cs) entities.
<table>
<tr><th>Root Entity / Type</th><th>Description</th></tr>

<tr>
<td>EnvironmentName <br><sup>string<sup></td>
<td>
 Provides access to "environment-name" attribute, e.g. "DEV" vs "PROD" read of application configuration tree root
</td>
</tr>


<tr>
<td>DependencyInjector <br><sup>IApplicationDependencyInjector<sup></td>
<td>
 Injects app-context rooted dependencies into object fields decorated with `[Inject]` attribute. 
 This component is mostly used by other system processes like MVC handler and Glue servers, so app developers 
 rarely use it
</td>
</tr>

<tr>
<td>Realm <br><sup>IApplicationRealm</sup></td>
<td>
 References an accessor to the application surrounding environment (realm) in which app gets executed.
 This realm is sub-divided into uniquely-named areas each reporting their status.
 This is used by various app components and services to assess the environment status in which they execute, for example:
 a logger may suppress error messages from network in a cluster when the area is about to be upgraded to new software.
 One may consider this status as a "message board" where services/system check/report the planned or unexpected outages and
 adjust their behavior accordingly. Azos provides only the base implementation of such classes delegating the specifics to more
 concrete app containers (e.g. Azos.Sky).
</td>
</tr>

<tr>
<td>Log <br><sup>ILog</sup></td>
<td>
 References application logging component. Component-specific logging is done via <code>WriteLog(...)</code> component instance method
</td>
</tr>


<tr>
<td>Instrumentation <br><sup>IInstrumentation</sup> </td>
<td>
 Returns instrumentation facade. The primary method is <code>instrumentation.Write(Datum)</code> where Datum <i>(singular of "data")</i> is a common ancestor 
 for <strong>gauges</strong> (measurements that have a value) and <strong>events</strong> (measurements that just happen)
</td>
</tr>


<tr>
<td>ConfigRoot<br><sup>IConfigSectionNode</sup></td>
<td>
 References application configuration tree root
</td>
</tr>

<tr>
<td>CommandArgs<br><sup>IConfigSectionNode</sup></td>
<td>
 References application command-line arguments parsed into configuration tree (separate from primary configuration)
</td>
</tr>

<tr>
<td>DataStore<br><sup>IDataStore</sup></td>
<td>
 References primary app data store. This is just a marker interface. It is up to the app to decide what constitutes its data store
</td>
</tr>

<tr>
<td>ObjectStore<br><sup>IObjectStore</sup></td>
<td>
 References primary app object store which persist object instances out-of-process (e.g. on disk). The store has an interface akin to 
the source control system with <code>Checkin()/Checkout()</code> methods. The store is used to store state-full instances (sessions, glue servers etc.)
</td>
</tr>

<tr>
<td>Glue<br><sup>Glue</sup></td>
<td>
 References Glue implementation - a contract-based RPC technology which allows for inter-process/object communication
</td>
</tr>


<tr>
<td>SecurityManager<br><sup>ISecurityManager</sup></td>
<td>
 References component which provides authentication/authorization/auditing/and password management services
</td>
</tr>

<tr>
<td>TimeSource<br><sup>ITimeSource</sup></td>
<td>
 Supplies exact local and UTC times. An implementation may use external precision sources like NASA atomic clock.
 Never use <code>DataTime.Now</code> if the value is stored, instead always obtain precise time from application like" <code>App.TimeSource.UTCNow</code>.
 The <code>DefaultTimeSource</code> is based on local Date class.
</td>
</tr>

<tr>
<td>EventTimer<br><sup>IEventTimer</sup></td>
<td>
 Maintains and executes instances of <code>Time.Event</code> class. This is a process-wide scheduler. Many services use custom
events to schedule some periodic update work, for example to dump performance statistics into instrumentation. Events are configurable,
so one may script events like <code>DeleteFilesJob</code> which drops old log files.
</td>
</tr>

<tr>
<td>ModuleRoot<br><sup>IModule</sup></td>
<td>
Returns the root of modules used by the app. Typically a <code>HubModule</code> is used to host multiple sub-modules.
Modules are used to organize system (e.g. platform-specific functions like image rendering) and app logic (business logic).
App objects may request module dependency injection using <code>[InjectModule]</code> attribute on a field
</td>
</tr>

<tr>
<td>Random<br><sup>RandomGenerator</sup></td>
<td>
Provides random generation services (e.g. numbers, web-safe strings, salts and nonces). The generator is internally fed
entropy from network stack and other sources of unpredictable data (e.g. various statistics)
</td>
</tr>


<tr>
<td>InstanceID <br><sup>Guid<sup></td>
<td>
 Uniquely identifies this running application instance. Regenerated on every run
</td>
</tr>

<tr>
<td>AllowNesting <br><sup>bool<sup></td>
<td>
 True if this application chassis allows another one to allocate - used for testing and multi app hosting in the same process
</td>
</tr>





</table>


...




----

Back to [Documentation Index](/src/documentation-index.md)

External resources:
- [Pattern: Microservice Chassis](https://microservices.io/patterns/microservice-chassis.html)

