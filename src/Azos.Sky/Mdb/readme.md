# MDB - Multi Database Router


MDB is for storing (usually) related/relational data organized by sharding ID in "briefcases" within areas.
*(we can also store documents as easily using Mongo Db*).

The data is organized in areas. Areas are Named.
The purpose of each area is to provide the best possible load accommodation for particular data type.
For example: User area holds user-related data (profile, transactions, balances) for users partitioning it on GDID.
This approach allows to use traditional query methods within one "briefcase" of data.

Within an area, a "briefcase" is a segment of relational tables that is keyed on the same partition GDID - a "briefcase key".
It represents a logical set of data needed for business logic.
For example: User briefcase stores all user-related tables on the same RDBMS server.
This allows for quick traversal/joins of data within the briefcase identified by GDID ID.
Briefcases are not physical entities, they are logical sets of rows in various tables within the same DB server.
An area may hold MORE THAN ONE type of briefcases (as its name should reflect).

Internally, briefcases are usually hierarchical structures of tables. For example:
```
 - User
   - Pay Accounts
     - Account Authorization History
   - Addresses
   - Orders
     - Order Line-items
   - Transactions
   - Balances
```


All areas except "CENTRAL" store data in partitions that are range-based on GDID, then within a partition data is sharded among the
shard count.
The SHARD COUNT within partition is normally immutable (pre-arranged).
If the shard count needs to change within a partition, briefcase resharding within that partition has to be done (see below).

```
Area "User"
+------------------+   +-----------------------------+        +-----------------------------+
+ Partition 1 	   +   + Partition 2                 +        + Partition X                 +
+ Range Start 0	   +   + Range Start 250000          +        + Range Start >250000         +
+--------+---------+   +--------+----------+---------+ . . .  +--------+----------+---------+
+Shard 1 | Shard 2 +   +Shard 1 | Shard 2  | Shard 3 |        +Shard 1 |   ...    | Shard N |
+--------+---------+   +--------+----------+---------+        +--------+----------+---------+
```

Partitioning IDs are always GDID (global unique identifier), not strings; this is because
MDB uses range partitioning in all but CENTRAL area.

CENTRAL Area DOES NOT use any partitioning. It is a special Area used for global definitions/indexing.
It still uses sub-shards. CENTRAL Area is the only area that has SHARDING KEY: object (not gdid), as it allows to lookup
 by strings and other shard keys.

When a STRING ID (i.e. user email) needs to be mapped to GDID (i.e. user briefcase GDID), the CENTRAL Area should be queried (index tables).
Most of the data in Central area is static, so it gets aggressively cached.

All other areas use Range partitioning. Range partitioning works as follows:
 All sharding IDS are monotonically-increasing GDIDs (with authority bits discarded: Era 32bit + Counter 60 bit)
 Sharding GDID is requested. The system needs to locate a shard that holds that data within the area.
 System looks-up the system config that maps GDID start {Era, Counter} to the partition.

 The partition is further broken-down by shards, this is needed so that write/read load of current
 data does not create hotspot on one server. The Number of shards within partition is not expected to be changed
  (or briefcase data rebalancing would need to take place with 3rd party tools, see below).


 Benefits:
  1. Data does not need to be physically moved between partitions on partition addition. Once a partition has been assigned, data remains there
  2. Quick mapping of GDID_SHARDING_ID -> physical server
  3. Ability to gradually increase capacity: start business with one partition, assess the load and add more partitions when necessary
  4. Fast in-area queries - as data is physically pre-grouped in "briefcases" by GDID_SHARDING_ID (all briefcase-related data is on the same physical server)

 Drawbacks:
  1. If "older GDID" data gets stale*, the older shards experience less load
  2. Possibly uneven distribution of "newer/hotter" data goes towards the end
  3. Theoretically not 100% even distribution as some USERS(or other briefcases) may have more
   data than others, 100 users on one server!=100 users on another. Because of it, MORE CAPACITY has to be reserved in partition.
   Mitigation may be: scale particular server up (faster CPU+more ram/disk)

  * - keep in mind, "older" users still have new transactions comming into their shard,
  as transactions/balances are co-located with user


 ## Future/Data Support/Archiving tasks
 
  With time (after X years), some data may get deleted from the MDB. Older customer data may get archived and moved-out into a long-term storage.
  Instead of adding new partitions, we can set a GDID brakepoint (one number) after which the range partitioning tables will start over - that is
   the GDIDs below the brakepoint will get routed according to the first range set, after brakepoint, to another.. and so on

 ## How to re-shard the data whithin the partition	(briefcase move)
 
 The business logic-aware tool(script/utility) would need to be constructed to move briefcases (all logically-grouped data) between DB servers.
 It is important to note that AUTO-INC ids SHOULD NOT be used because of possible collisions, instead GDIDs need to be used throughout
 all tables








