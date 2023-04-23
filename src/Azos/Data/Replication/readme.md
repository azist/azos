# Data Replication

Azos library provides definition and base implementation for **Asynchronous Pull Document Replication** 
data management pattern. This pattern was selected for the following traits:

Pros:
- Horizontal scaling
- Multi-master replication
- Large volumes of online "live" data
- Documents with schema/data shape version gradual upgrade (e.g. add/drop a field as we go)
- Autonomous cluster operation/Appliance mode "Fire and forget"
  - Eventual auto recovery from sporadic temporal failures/split brain
  - No limits for replication log size as pull replication does not need it
  - Auto backup
- Different nodes may use different DB storage technology, e.g. can use file technology for data archiving nodes
- Optional use of CRDT data modeling
- No vendor lock-in (e.g. cloud/db tech)

Cons:
- Eventual consistency only
- No support for instant synchronous change across origins (however it can be added as OOB updates)
- Large node catch-up takes time (e.g. days for 100s mill records)
- Higher competency of app architects and engineers
