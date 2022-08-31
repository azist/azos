# Sky Operating Daemon

`Skyod` is a top level process component deployed on every node of sky cluster.

It performs the following tasks:
- Works in either local node or cluster controller mode
- Fetches software installation packages
- Installs software package
- Respawns manage software set
- Provides command-based interface via app remote terminal via Glue/Web
- Cluster controller:
  - Broadcasts commands to subordinate nodes
  - Keeps track of all cluster nodes
  - Provides uniform web management interface

 `Skyod` is the component which:
 - Is installed separately from main software set as it does NOT auto-update e.g. `~/skyod/bin`
 - Gets installed as system service (e.g. `systemd`) and controlled using OS start/stop facilities
 - Stops, Swaps and Re-Spawns software set in a dedicated root folders

## Definitions
* **Managed** software - is installed, updated, spawn/re-spawn/ stopped/controlled using the `skyod` daemon
* A **software set** is a logically-isolated set of **software set components** which get managed
* Every software set component has an entry point. 
* For managed system components an entry point is a `governor daemon` instance which spawns subordinate applications
* For unmanaged components (e.g. MongoDb server) an entry point is batch/shell script
* Every component has an `IComponentActivator`
* Software sets are NOT inter-dependent
* SetComponents ARE inter-dependent (ordered)

Example tree, using a fictional `Z9` system moniker:
```
 skyod
   - SoftwareSet(`z9-sys`)
     - SetComponent(`mongo-db-v5`)  start/stop  activators
     - SetComponent(`z9-sys-core`)
       - hgov
         - idp
         - gdid
         - evh
         - sysapp1
   - SoftwareSet(`z9-biz`)
     - SetComponent(`hadoop`)  start/stop  activators
     - SetComponent(`mongo-db-v5`)  start/stop  activators
     - SetComponent(`z9-biz-apps`)
       - hgov
         - market
         - bill
         - ar
         - ap
         - iv
```


