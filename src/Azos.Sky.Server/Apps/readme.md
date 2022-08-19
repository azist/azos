# Azos Sky App Model

## Application Services
Azos Sky provides a uniform feature set conducive to building distributed business applications, namely: 

* **Application Container** - provides a uniform way of running and controlling application instances, standardizes features like:

  * Application launch/shutdown cycle
  * Configuration (variable evaluation, structured overrides, macros, navigation)
  * Dependency Injection - driven by code or of the configuration - saturate the class dependencies using injections
  * Logging into various sinks with sink graphs SLA and failover
  * Cluster wide precise time source
    Event scheduler (programmatically or through configuration)
* **CLI app management** - ability to manage applications from command line interface (and GUI web console **AWM(Azos Web Manager)**). Ability to execute application-specific commands, specific to each application purpose, and the general ones, e.g. `"gc"` - forces full garbage collection *(see app command line reference)* 
* **Manage individual application components** - ability to manage individual components on the application component tree, set properties via remote commands. This is needed for real-time management, *(e.g. set log severity at runtime)*
* **Distributed configuration** of applications on 1000s of nodes - handled by metabase structured config override
* **Data Access** - data store partitioning, various models, CQRS/queues
* **Big Memory** - cache business domain objects - remove hot-spots from data access. Perform high-load social graph traversal in-ram 
* **Glue** - connect app components together as-if they were on one machine - location transparency, contract-based programming 
* **Instrumentation/Telemetry** - system and business-specific data, gather data by hosts, zones, and higher-level zones. Visualize the instrumentation as charts and tables. Trigger alert conditions 

## Process Topology
In the section above we described the topology of the Azos Sky system as a whole - as defined in the metabase regional catalog.

<img src="/doc/img/host-process-tree.svg">

Just like the cluster system as a whole, every host has its own process tree, while every application instance has a set of addressable components *(described farther down)*. The process tree starts of at the Azos Host Governor (AHGOV) process which runs first, then invokes all of the necessary processes under it. How does the AHGOV know what software to run? The following outlines the process: 

* AHGOV process starts
* AHGOV mounts the metabase (via an injectable FS like any process)
* AHGOV determines what host is it on, and gets its ROLE
* The ROLE (defined in the Application catalog) lists the applications that the roles consists of
* An application is physically represented by a number of binary packages that come from the binary catalog
* The metabase system meatched the most appropriate packages for the platform and operating system version which runs on the given machine
* If the required packages are not present locally, then they get installed *(downloaded from Metabase via injected VFS)*
* The AHGOV runs the applications that need to be auto-started in the defined sequence 


## Metabase Application Container Configuration

Azos Sky applications execute in the the `ISkyApplication` container which is fed configuration content from the [Metabase](/mbase). The effective configuration gets calculated by traversing the graph of metabase sections, each subsequent level overriding the result, in the following order:

* Metabank.RootAppConfig is used as a base (very root of the metabase - "any application")
* Role.AnyAppConfig
* Application[applicationName].AnyAppConfig
*  Regional override: parent sections on path, from left to right, ending with the host, each: AnyAppConfig GetAppConfig(applicationName)
   * AnyAppConfig
   * GetAppConfig(applicationName)
* Include GetOSConfNode(this.OS)

The configuration gets computed for the particular `applicationName`, running on the particular `host` which runs the concrete OS.

The hierarchical structure of the config is very useful as it allows to specify the configuration prototype at the very root, then **cascade down on thousands of hosts**, only having to **override parts where necessary** which is a rare need. This way developers do not need to maintain many files and metabase does not have to store them. 

A typical application config declared at the metabase root includes basic mixins: 

```CSharp
application
{
  _override="all"
   trace-disable=true

  _include { name=qty             provider{ file="/inc/gv/default.laconf" } }
  _include { name=qlue            provider{ file="/inc/glue.laconf" } }
  _include { name=instrumentation provider{ file="/inc/instrumentation.laconf" } }
  _include { name=object-store    provider{ file="/inc/object-store.laconf" } }
  _include { name=security     provider{ file="/inc/security/default.laconf" } }
  _include { name=web-manager  provider{ file="/inc/web-manager/default.laconf" } }
}
```

**Mixins** represent a library of configuration blocks, they get referenced from app config via "_include" pragmas. This modular approach simplifies the configuration of various applications in the whole cluster. 

```CSharp
glue
{
  bindings
  {
    binding { name=$(~SysConsts.ASYNC_BINDING) type=$(/gv/types/$glue-async) }
    binding { name=$(~SysConsts.SYNC_BINDING)  type=$(/gv/types/$glue-sync) }
  }
}
```
The snippet above demonstrates the use of system constants and environment variables. See Configuration Reference section for more details. 