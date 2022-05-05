# Azos.Data.Adlib - [A]morphous [D]ata [Lib]rary

Amorphous data library defines a way for working with schema-less document-oriented data
having documents persistently stored in named collections along with indexable ad-hoc 
attributes.

The system is build for horizontal scaling with sharding, optional data expiration, querying 
and reflection.

The service provides simple CRUD-like functionality based around [`Item.cs`](Item.cs)
which is an entity that gets stored.

Use [`ItemFilter.cs`](ItemFilter.cs) to POST data filter requests to server `filter`
endpoint


[`AdlibWebClientLogic.cs`](AdlibWebClientLogic.cs) is an implementation of `IAdlibLogic` which
delegates execution to a remote Http server via the circuit breaker/balancer circuitry.
