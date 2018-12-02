# Azos Design Philosophy
Back to [Documentation Index](/src/documentation-index.md)

This section describes design choices and patterns used in Azos framework. We also touch upon
a few common software [myths and truisms](/src/truisms.md).


## Overview
Azos project originated as **purposely "opinionated"** framework [~what is opinionated software?~](https://stackoverflow.com/questions/802050/what-is-opinionated-software)
as it prescribes **a certain way of application structuring**. This is done to reap the benefit of
 simplicity of the SDLC as a whole, and short time to market while writing real-world complex business/data-driven applications.


The philosophy of Azos is based on observations made in the past 15+ years of corporate code bases in 10s of large and medium scale organizations. 
It has been concluded that **modern framework landscape is still of a too low level
for average developer to grasp and apply properly**. Another detrimental factor is **fragmentation and too many choices**.

See [myths and truisms](/src/truisms.md).

Azos follows the following principles:
- Provide a full stack of services needed in a typical business application:
  - Logging
  - Instrumentation/Tracing/Perf counters
  - Configuration/Distributes/Overrides/Merges/Scripting/Validation/Class prop binding
  - DI
  - Serialization: Tight binary, JSON, BSON, XML
  - RPC/Microservices/Contract-based + Security
  - Security/Permissions/Identities/Password/Tokens/Authentication/Authorization
  - Web pipeline/Mvc
  - Data document modeling with rich constraints and metadata (for data access and protocol)
  - Auto data document mapping to hybrid data sources / Auto CRUD
  - Hybrid data access: RDBMS, NoSQL, FlatFile, Web Service data sources
  - Utilities: prime math, date math, leaky bucket, circuit breaker, 
- Reuse internal building blocks, thus reducing complexity and footprint
- Set guidelines for every app aspect implementation using the above


## Overall Application Structure
TBD...

Inification of app types: console, web service etc...

need for Chassis

Application root and component tree, logging + instrumentation built-in

...




----

External resources:
- [Opinionated Framework (SO)](https://stackoverflow.com/questions/802050/what-is-opinionated-software)
- [Pattern: Microservice Chassis](https://microservices.io/patterns/microservice-chassis.html)
- [Service Location (Wikipedia)](https://en.wikipedia.org/wiki/Service_locator_pattern)
- [Dependency Injection (Wikipedia)](https://en.wikipedia.org/wiki/Dependency_injection)
- [IoC with Service Location / Dependency Injection by Martin Fowler](https://martinfowler.com/articles/injection.html)

Back to [Documentation Index](/src/documentation-index.md)