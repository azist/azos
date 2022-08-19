# Azos - A to Z Business Operating System

**NET6**

<img src="/elm/design/logo/azos.png" alt="Logo" >

[![Build status](https://ci.appveyor.com/api/projects/status/v469s4pxwr5e0vox/branch/master?svg=true)](https://ci.appveyor.com/project/zhabis/azos/branch/master)
[![CodeFactor](https://www.codefactor.io/repository/github/azist/azos/badge/master)](https://www.codefactor.io/repository/github/azist/azos/overview/master)



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
 `pm> install-package Azos.Web`| Azos Web (Client, Protocols etc.) 
 `pm> install-package Azos.Wave`| Azos Wave Server + Mvc 
 `pm> install-package Azos.MsSql`| Azos Microsoft SQL Server Provider (CRUD etc.) 
 `pm> install-package Azos.Oracle`| Azos ORACLE RDBMS Provider (CRUD etc.)
 `pm> install-package Azos.MySQL`| Azos MySQL RDBMS Provider (CRUD etc.) + Native Client
 `pm> install-package Azos.MongoDb`| Azos MongoDb Proivder (CRUD etc.) + Native Client 
 `pm> install-package Azos.WinForms`| Azos WinForms Support (for legacy)
 `pm> install-package Azos.Media`| Azos Media Formats (PDF, QR Codes etc.) 
 `pm> install-package Azos.Sky`| Azos Sky Distributed Cloud OS
 `pm> install-package Azos.Sky.MongoDb`| Azos Sky MongoDb Services (Queues etc.)


See also:
- [Azos Design Philosophy](/src/philosophy.md)
- [Azos Documentation Index](/src/documentation-index.md)



External resources:
- [Monorepo Project Structure (Wikipedia)](https://en.wikipedia.org/wiki/Monorepo)
- [Pattern: Microservice Chassis](https://microservices.io/patterns/microservice-chassis.html)
