# AZOS Documentation Index 

## General Overview
* [Azos Design Philosophy](philosophy.md)
* [Naming Conventions](naming-conventions.md)
* Building
* Testing
  * Unit Testing overview + `Aver` library
  * [Scripting](/src/Azos/Scripting) is used for writing tests
  * [Azos Framework Tests](/src/testing)
* [Runtimes and Platform Abstraction](/src/runtimes.md)

## Azos Core Functionality
* [Application Chassis](/src/Azos/Apps)
  * App Components
  * [Dependency Injection](/src/Azos/Apps/Injection)
  * Modules
  * Singleton Instances
  * Volatile Object Store
  * Daemon Model
  * User Sessions
* [Configuration](/src/Azos/Conf)
  * Laconic configuration
  * XML configuration
  * Json configuration
* [Data Access](/src/Azos/Data)
  * [Data Access Overview](/src/Azos/Data) 
  * [Schema Metadata](/src/Azos/Data/metadata.md)
  * [Data Validation with Domains](/src/Azos/Data/domains.md)
  * [Data Modeling](/src/Azos/Data/modeling.md)
* [Glue](/src/Azos/Glue) - interprocess communication
* [Logging](/src/Azos/Log) - logging and sinks
* [Instrumentation](/src/Azos/Instrumentation) - telemetry with gauges and events
* IO
  * [Virtual File Systems](/src/Azos/IO/FileSystem) - pluggable file system
* [Big Memory Pile](/src/Azos/Pile) - utilization of large amounts of RAM
* [Scripting](/src/Azos/Scripting)
* [Security](/src/Azos/Security) - user identity, authentication, authorization, permissions
* [Serialization](/src/Azos/Serialization)
  * Slim
  * Json
  * Arow
  * Bson
* Standards and Conversions
* Templatization
* Time Services and Event Scheduling
* General-purpose [Utilities](utilities.md)

## Sky Distributed OS
* Sky App Chassis
* Distributed Worker Coordination
* [Locking](/src/Azos.Sky/Locking)
* [Global ID Generation/GDID](/src/Azos.Sky/Identification)
* [MDB - Database Router](/src/Azos.Sky/Mdb)
* [Metabase](/src/Azos.Sky/Metabase)
* [Distributed Workers](/src/Azos.Sky/Workers)
  * [Processes](/src/Azos.Sky/Workers/processes.md) 
  * [Todos](/src/Azos.Sky/Workers/todos.md)

