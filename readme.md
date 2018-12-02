# Azos - A to Z Application Operating System
<img src="/elm/design/icons/azos.png" alt="Logo" >

Azos is a **full stack** framework for distributed data-driven business applications
of any size. Unique feature of Azos is its self-sufficiency as it includes all of the components
necessary for creation of **typical business** to **Facebook-scale** applications.

Azos includes:
- Application chassis (component hierarchy container)
- Configuration (supports external cluster configuration)
- DI
- Logging 
- Instrumentation (custom gauges/events/counters)
- Serialization: Binary, Bson, Json
- Security: role based, permissions, annotations, inheritance etc.
- **Contract-based RPC** (Glue)
- **Mvc Web Stack** based on web server abstraction (may use Asp Core, HttpListener or any other server/stack)
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


[![CodeFactor](https://www.codefactor.io/repository/github/azist/azos/badge/master)](https://www.codefactor.io/repository/github/azist/azos/overview/master)

See also:
- [Azos Design Philosophy](/src/philosophy.md)
- [Azos Documentation Index](/src/documentation-index.md)



External resources:
- [Monorepo Project Structure (Wikipedia)](https://en.wikipedia.org/wiki/Monorepo)
- [Pattern: Microservice Chassis](https://microservices.io/patterns/microservice-chassis.html)
