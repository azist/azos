# AZOS Documentation Index 

## General Overview
* [Azos Design Philosophy](philosophy.md)
* [Naming Conventions](naming-conventions.md)
* [Utility and Extension Methods](utils-extensions-reference.md)
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
  * [Strategy Pattern and Business rule-driven IOC](/src/Azos/Apps/Strategies)
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
  * [Advanced Metadata](/src/Azos/Data/advanced-meta.md)
  * [Data Validation with Domains](/src/Azos/Data/domains.md)
  * [Data Modeling](/src/Azos/Data/modeling.md)
  * [Advanced Query APIs with AST (Abstract Syntax Trees)](/src/Azos/Data/AST)
  * [Data RPC](/src/Azos/Data/Access/Rpc)
* [Glue](/src/Azos/Glue) - interprocess communication
* [Logging](/src/Azos/Log) - logging and sinks
    * [Chronicle Logging Framework](/src/Azos.Sky/Chronicle)
* [Instrumentation](/src/Azos/Instrumentation) - telemetry with gauges and events
* IO
  * [Virtual File Systems](/src/Azos/IO/FileSystem) - pluggable file system
* [Big Memory Pile](/src/Azos/Pile) - utilization of large amounts of RAM
* [Scripting](/src/Azos/Scripting)
* [Security](/src/Azos/Security) - user identity, authentication, authorization, permissions
* [Serialization](/src/Azos/Serialization)
  * Slim
  * Json
  * Bix
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
* [Event Hub](/src/Azos.Sky/EventHub)
* [MDB - Database Router](/src/Azos.Sky/Mdb)
* [Metabase](/src/Azos.Sky/Metabase)
* [Distributed Workers](/src/Azos.Sky/Workers)
  * [Processes](/src/Azos.Sky/Workers/processes.md) 
  * [Todos](/src/Azos.Sky/Workers/todos.md)

