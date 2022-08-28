# Azos - A to Z Business Operating System

Supports: .Net 6.0 Server Components; .Net Standard 2.1 Azos, Azos.Sky clients and server components

<img src="/elm/design/logo/azos.png" alt="Logo" >

[![Build status](https://ci.appveyor.com/api/projects/status/v469s4pxwr5e0vox/branch/master?svg=true)](https://ci.appveyor.com/project/zhabis/azos/branch/master)
[![CodeFactor](https://www.codefactor.io/repository/github/azist/azos/badge/master)](https://www.codefactor.io/repository/github/azist/azos/overview/master)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=azist_azos&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=azist_azos)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=azist_azos&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=azist_azos)
 

[![Quality gate](https://sonarcloud.io/api/project_badges/quality_gate?project=azist_azos)](https://sonarcloud.io/summary/new_code?id=azist_azos)

-------

Azos is a **full stack** framework for distributed data-driven business applications
of any size. Unique feature of Azos is its self-sufficiency as it includes all of the components
necessary for creation of **typical business** to **Facebook-scale** applications.

**NEW!!!** [ (Under Construction) Azos Step-by-Step Tutorial](https://github.com/azist/tutorial-steps)

[Azos Documentation Index](/src/documentation-index.md)

Azos includes:
- Application chassis (component hierarchy container)
- Configuration (supports external cluster configuration)
- DI
- Logging
- Single instance or cross-sharded **cloud deployable log chronicle** client/server API services and log repository framework 
- Instrumentation (custom gauges/events/counters)
- Serialization: Binary, Bson, Json
- **Binary archiving stream** management for data persistence, instrumentation, and log archival (opt-in compression and encryption)
- Security: role based, permissions, annotations, inheritance etc.
- **Contract-based RPC** (Glue)
- **Mvc Web Stack** based on web server abstraction (may use Asp Core, HttpListener or any other server/stack)
- **Distributed cloud messaging and event hub** queue based pipeline services
- **Data documents** - model data for RDBMS/NoSQL/Service stores. Full **auto CRUD**/metadata/validation
- Hybrid Data Access Layer - **virtual queries** (e.g. query service instead of table)
- Pile: In memory pile of objects store 100s of millions of instances in-process
- Data cache based on Pile
- Virtual File System (e.g. SVN, Amazon S3, Google Drive)
- 100s of utilities: int/prime math, rnd, leaky bucket, keyed interlocked, object casts etc.

Azos is built for writing **Distributed systems** of **infinite scale**:
- **Distributed hierarchical cluster** topology
- **Todo queues** (a la serverless)
- **Virtual Actors**
- **Global Monotonic Unique ID** generation 2^96
- Distributed process model/process control signaling
- Load balancing/work sets
- Distributing locking/coordination
- Logging, Telemetry, Security Auditing works in cluster + archiving
- Real-time process/cluster admin panel
- Social Graph system: nodes, friendship, subscriptions/event notification
- Social trending: real-time trending system based on business entities

## Nuget Packages

[Azos Packages on Nuget](https://www.nuget.org/profiles/azist-group)

cmd | Description
 -------|------
 `pm> install-package Azos` | Azos Core Package (App Chassis, Pile, Glue, Log, Instr etc.)
 `pm> install-package Azos.Sky`| Azos Sky (Client, Protocols, Distributed cloud etc.) 
 `pm> install-package Azos.Wave`| Azos Wave Server + Mvc + Kestrel listener 
 `pm> install-package Azos.MsSql`| Azos Microsoft SQL Server Provider (CRUD etc.) 
 `pm> install-package Azos.Oracle`| Azos ORACLE RDBMS Provider (CRUD etc.)
 `pm> install-package Azos.MySQL`| Azos MySQL RDBMS Provider (CRUD etc.) + Native Client
 `pm> install-package Azos.MongoDb`| Azos MongoDb Proivder (CRUD etc.) + Native Client 
 `pm> install-package Azos.WinForms`| Azos WinForms Support (for legacy)
 `pm> install-package Azos.Media`| Azos Media Formats (PDF, QR Codes etc.) 
 `pm> install-package Azos.Sky.Server`| Azos Sky Distributed services implementations
 `pm> install-package Azos.Sky.Server.MongoDb`| Azos Sky MongoDb Services (Queues etc.)
 `pm> install-package Azos.AuthKit`| Azos IDP/Authorization Authority (IDP, OAuth)
 `pm> install-package Azos.AuthKit.Server`| Azos IDP/Authorization Authority server implementation
 `pm> install-package Azos.AuthKit.Server.MySql`| Azos IDP/Authorization Authority server MySql storage layer


See also:
- [Azos Design Philosophy](/src/philosophy.md)
- [Azos Documentation Index](/src/documentation-index.md)



External resources:
- [Monorepo Project Structure (Wikipedia)](https://en.wikipedia.org/wiki/Monorepo)
- [Pattern: Microservice Chassis](https://microservices.io/patterns/microservice-chassis.html)
