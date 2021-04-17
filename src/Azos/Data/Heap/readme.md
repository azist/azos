# HEAP = [H]uge [E]nterprise(Business) [A]pplication [P]ersistence

(akin to memory heap)

Azos.Data.Heap is a multi-master replicated geo-aware document database with horizontal
scaling via sharding with data locality zoning.
The multi-master eventual consistency state is achieved using Conflict Free Replicated Data Types
(CRDT) which converge their state into a single deterministic result regardless of ordering. 

The heap is organized in areas which contain collections of similarly-typed objects.
Areas represent data partitioning and storage technology selection for every use case, 
for example: master index may not need to have many shards, whereas customer transactions
may need to store lots of data and be split on 10s of shards.

Data modeling is performed using any CLI-capable language, C# being the default.
The Heap stores data schema in a form of CLI  assemblies and is built for
gradual transparent data schema change as it is virtually unaffected by object counts (see below).

The Heap uses REST Api to communicate from any language which supports JSON `{map} / [array] / scalar` types (any language).
The Heap support BIX(biz info exchange) format natively via corresponding content type header `application/bix`.


The following table describes chief heap traits:

* Data Model
  * Document-oriented, entity reference-oriented (a kin to RDBMS) via uniform object/entity address space
  * CRDT(Conflict-Free Replicated Data Types) data documents
  * Schema-first/Design first approach
    * Heap uses CLI (.Net) instead of SQL or other proprietary languages (e.g. BSON in MongoDb)
    * Consume from any language via REST interface (e.g. Python, Java, etc.)
    * Native model+language: any CLR (e.g. C#) language capable of producing CLI-compliant assemblies
    * Model data using POCO data documents with attributes
    * Schema version upgrades performed transparently - on read/write as data being used
    * CLR Queries (stored procedures) - named command handlers stored on server
    * Use LINQ
  * Built for complex business models 
    * Government standards with 100s of data fields
    * Rich field types: NLS strings, currency amounts, LatLng, DateRange etc.
    * Complex validation logic: required, min/max, pick lists, dynamic lookup etc.
    * Validation rule chaining-inheritance used for policy-based validation
    * Rich custom metadata (e.g. add business-specific attributes)
    * Built-in documentation engine supports all data definitions including validation logic
    * Unlimited extendability with any .CLR language
* Unlimited horizontal scalability
  * Built-in horizontal partitioning aka "Sharding"
  * Locality of reference via co-related data zoning using `ShardKey`
* Storage-agnostic
  * Data Heap does not provide any specific storage engine of its own, by default uses MongoDb
  * Use any storage engine, be it document (e.g. Mongo Db) or RDBMS (e.g. MySQL)
  * Avoid licensing fees and vendor lock-in as the heap performs data partitioning, replication on its own without relying on proprietary storage engine technology
  * Custom queries from hybrid data sources (e.g. local files + ws calls)
* Multi-master replication 
  * Channel priorities, ability to override priority per document (e.g. give "hot" data a boost)
  * Replication policies - confine sensitive data within owner's perimeter (e.g. keep your customer data within the country of origin)
* Geo-aware
* Eventual global consistency. Fast local consistency
  * CRDT - convergent data models (LWW, one-way flags, registers, grow-only set, tombstones etc.)
  * The `merge()` is implemented in domain model class - for you specific case making system infinitely flexible and easy to use
* Automatic failover
  * On network outage - traffic is routed to next closest data center
  * Split brain reparation - when network coverage is restored, data node converge automatically using CvRDT
  * Physical node replacement: add/remove nodes; the nodes catch-up from others automatically


- Heap is made of areas
- Area defines types of objects and queries
- Same software needs to be installed on replicated-from machines, until it is replication is paused
- Nodes use pull replication via "Pull uplink"
- Each Node belongs only to ONE area (possible to run multiple nodes on same computer)
- Each node eventually has all of the data as others (except for data perimeter policy)
- Each Node has its own storage engine
- Each Node has its own sharding
- A node may be replicate-only backup copy with archive storage engine (captures all changes)


## Features

### Optional TTL

////HeapObject:   ITTL
make optional interface


## Notes

ObjectVersion is not needed because area engine may not know how to handle specific replication
needed for every object type, therefore those replication-controlling fields a-la CRDT are really needed
in the data buffer itself.

State-based CRDTs are called convergent replicated data types, or CvRDTs. In contrast to CmRDTs, CvRDTs send their full local state
to other replicas, where the states are merged by a function which must be[COMMUTATIVE, ASSOCIATIVE, AND IDEMPOTENT].
The merge function provides a join for any pair of replica states, so the set of all states forms a semilattice.The update function must monotonically increase the internal state, according to the same partial order rules as the semilattice.

http://jtfmumm.com/blog/2015/11/17/crdt-primer-1-defanging-order-theory/
https://lars.hupel.info/topics/crdt/07-deletion/