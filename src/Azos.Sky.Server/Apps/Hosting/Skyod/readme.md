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
* Every software set component has one entry point
* For managed system components an entry point is a `governor daemon` instance which spawns subordinate applications
* For unmanaged components (e.g. MongoDb server) an entry point is batch/shell script
* A component has `is-local: bool` when true - requires to set adapters; when false requires subordinate nodes
* A component has `auto-start: bool`, if false it must be started manually otherwise activated via activator
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

> Note: Skyod does not care about pkg version comparison. It does not "understand" verion structure be it semantical version or timestamp based.
> All `skyod` does is download and install packages by their name WHETEVER these names imply, so it is up to the sysop to determine and select the
> most current version for the installation

Installation on-disk structure:
```
 ~/sky     <==== $SKY_HOME
   /skyod - skyod install directory
     /bin
        /cfg...
        skyod.exe
        Azos.dll, Azos.Sky.dll, Azos.Wave.dll, Azos.Sky.Server.dll
     /logs
        skyod
          yyymmdd-skyod-gen-csv.log
     /data
        /install //<=== Installer root directory
          x-package-id.apar // downloaded package files
          package-list.json // list of locally installed packages AND current package

   /software-set-x
     /cmp-x // e.g. "z9-sys" for "Z9" system-related component
       /bin === link ==> ./bin-current-pkg-id (Symbolic link)
       /bin-x-pkg-id  // <===== This is where skyod unpacks apar files
          /cfg...
          azh.exe
          hgov.sky
          Azos.dll, Azos.Sky.dll, Azos.Wave.dll...
          Biz.dll, Biz.Server.dll, Cmp.dll...
          skyod.install.manifest.json 
       /logs
          app-x
            yyymmdd-app-x-gen-csv.log
       /data
       /gdid
```

See also [OS System Setup](setup.md) which covers the steps of container/image/or manual setup of the host OS.


