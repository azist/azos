# Sharding

Provides abstractions for working with sharded data.

See [Shard Database Architecture (Wikipedia)](https://en.wikipedia.org/wiki/Shard_(database_architecture))

Sharding is a technique which breaks a large monolithic corpus of data 
into fragments aka "shards".

The data schema is not sharded, the whole database is. In other words every shard 
in a shard set has all the same tables/collections of the data schema.

Applications have to deal with different sorts of data, therefore **not ALL application datasets need to be sharded**.
>For example, in medical applications, codes and classifications (such as drugs and diagnoses) do not
>need to be sharded as there are counted in hundreds of thousands of entries, not in hundreds of millions, however
>patient transactions in a nationwide multi-tenant hospital system would probably benefit from sharding its patient data.
>*(every patient record may contain thousands of detail record such as "visits", "procedures" etc.)*


**IMPORTANT NOTE:** Sharding adds a great deal of complexity to your application design, especially at the
application data modeling phase. Designing proper sharding architecture is NOT an easy task. Sharding is not
needed for most applications which can store data in a single database, however modern day applications need
to service millions of customers/orders - this is where sharding becomes exceptionally beneficial.
We recommend that application data sharding is only implemented by teams that have highly skilled software engineers.

> Note: Data sharding concept does NOT require the use of RDBMS (relational), you can shard any `ICrudDataStore`
> such as Mongo, ErlangOtp, Text-based etc.

Application data sharding is a way of building your application and its data access layer cognizant of data
partitioning. The disadvantage are complexity and stricter requirements for better data architecture at the project outset (which is not really a disadvantage after all).
However the advantages are really impossible to match otherwise:

- Virtually unlimited horizontal database scalability
- Zero vendor lock-in. You can migrate data from cloud A to cloud B - as all proprietary systems are avoided
- Low cost - save on license fees
- Much better performance than "automatic partitioning" proprietary systems

While it is true that database sharding increases general query time due to cross-shard requests, it is **possible to mitigate this
disadvantage almost to zero with careful logical sharded database design**. For this purpose, data is organized in
logical "areas" similar to "bounded contexts"/"aggregation roots" in Domain-Driven design.
Azos calls such **data contexts - "briefcases"**.

> A **briefcase is a group of logically-dependent data** which typically resides in multiple tables
> and requires joining. We shard data using a main key for the aggregate root object aka "Briefcase Handle"
> or `ShardKey`. Now, because all of the **related tables are co-located on the same server** you can easily **join
> data within a shard**, avoiding complex cross-shard querying.
 
A briefcase is identified by a "aggregate root" key, such as "CustomerId"
 which all of the grouped tables depend on. The briefcase key is passed around via `ShardKey`
 value. Shard router (e.g. such as [ShardedCrudDataStore](ShardedCrudDataStore.cs)) uses some form of consistent mapping
 algorithm (e.g. Rendezvous Hashing) to map `ShardKey` into `IShard`. `IShard` provides actual data store connect string.

Shards are part of a [ShardSet](Shardset.cs). Data store supports multiple instances of `ShardSet` which are called 
"generations". This is sometimes needed if you change data portioning in future and need to keep track of
prior shard sets to locate the data which has not yet been re-partitioned.
