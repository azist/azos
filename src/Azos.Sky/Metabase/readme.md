# Azos Sky Metabase

Sky OS makes an extensive use of metadata (data about data) to describe its’ instance. 
An instance is a distinct installation of Sky OS on a set of physical and/or virtual resources. 
One may operate an instance from many different data centers running various applications. 
The logical decision about the instance composition is dictated by a particular business solution.

There are many various factors to consider and configure in a distributed system. Sky takes 
approach akin to reflection - a set of data about the system with APIs that can programmatically 
extract that data for various OS functions. This concept is called **"Metabase"** - a special kind of
 database that stores metadata about the Sky OS system. 

Metabase is a hierarchical data structure that gets mounted by every Sky OS process via a 
**Virtual File System** (VFS) accessor. It can be depicted as a structured system of config files 
which describe various system entities. In code, metabase is represented by the 
`Azos.Sky.Metabase.Metabank` class. Application developers never create an instance of `Metabank`, 
as this is done by the `BootConfLoader` as a part of `ISkyApplication` chassis setup. 
The VFS is a software-mode component that abstracts the access to files on various
 PaaS and physical layers (i.e. SVN, Amazon S3, Google Drive etc.). 
 The VFS usually supports version control, so any changes to the whole metabase are version-controlled.
 This is beneficial for keeping track of releases and rolling-back changes in case of emergencies, so
 all changes in the cluster setup, packages and configuration are always available.


<img src="/doc/img/metabank.svg">


The **metabase is a read-only resource** - no process can write to it (think .NET reflection), apps can only
 read system data by **using a rich set of APIs** (see the Reference section).

Metabase file system root mapped to a local dev folder via version control (e.g. SVN): 

<img src="/doc/img/metabase-files.png">

Sky OS is designed to scale into a multi-thousand node range, consequently its metadata source must be
 "big" enough to handle many entities, on the other hand the metabase is loaded by every process and we 
 can not afford to preload lots of data structures into every process upon start, therefore 
 **metabase uses lazy-loading** patterns with extensive caching on multiple levels. For example, 
 when a program needs to make a service call to some remote host, it first needs to locate this host 
 in the system, check if the host is dynamic, resolve it’s address etc. The system may also consider 
 multiple hosts that can perform the service, in which case multiple metabase catalogs and sections may be accessed.
 If the process repeats, the data gets read from the in-memory cache. 

## Metabase Catalogs

Logically a metabase is organized as a set of Catalogs - a top-level folders accessed via VFS. 
Catalogs group related functionality in a single-addressable unit: 

Path|Catalog|Description
|-|-|-|
|/app|Application|App catalog lists applications along with their inner composition - packages and configuration. It also declares Roles - which are a named kits of applications with startup scripts
|/reg|Regional|Regional catalog defines the hierarchical topology of the system. It contains geo regions at the root, branching into sub-regions, NOCs, zones/sub-zones, and hosts. The general or app-specific configuration may be specified at every level for the structural override (discussed below)
|/bin|Binary|Contains named binary packages (folders) that get distributed to the destination hosts
|/inc|include|Technically not a catalog, but rather a pattern of re-using of include mixins in various configs
|/ |metabase root|Network service registry, platforms, common config mixins

Application and Regional catalogs consist of sections which represent a corresponding 
entity both logically and in code. For example `/app/applications/AHGov` is an application **"AHGov"** accessible in
 code using an instance of `SectionApplication` class.

## Config File Naming and Format

Metabase sections contain multiple config files, the **`$` symbol** in the name **represents that the file is for the level** where it is declared:

Pattern|Purpose
|-|-|
|$.amb|Metabase data (Azos Metabase) config file. Contains metabase data, not application config
|$.app.laconf|Provides application configuration override for any app
|$.XYZ.app.laconf|Provides application configuration override for app called XYZ

The configuration files can be in any of the formats which Azos supports: **JSON**, **Laconic**, **XML**.
 Laconic being the default format (*it is the most convenient format to use for configuration (see the Azos specification for detailed 
 Laconic format description)*). In order to use different configuration file format, it is sufficient to
 provide a different extension, for example `"$.app.xml"`. 

## AMM Tool

**Azos Metabase Manager** (AMM) command line tool performs static metabase content analysis and detects various conditions, such as: 

- Syntax errors in config files
- Duplications and Omissions, i.e. items referenced in the system but not declared (i.e. unknown applications, hosts, roles, regions etc.)
- Various logical errors, such as improperly mapped contracts, duplicate definitions, gaps in key mappings, network config errors etc. 


As a good practice, AMM tool is executed before metabase changes get committed into the 
version controlled backend, this is usually scripted as a part of CI/CD SDLC process, - this prevents the publication 
of bad metabase data that causes runtime errors. 


## Regional (Geo) Catalog
Regional catalog define the physical and logical topology of the Sky OS instance. It starts 
with `Region` sections defined in directories with `"*.r"` extension. Every region may have sub-regions and NOCs.

<img src="/doc/img/cluster-map-01.svg">

A NOC stands for **"Network Operation Center"**, represented by directories with `"*.noc"` extension.
NOCs are further broken into `Zones` `"*.z"` and sub-zones - zones within zones. 
Zones contain hosts - `"*.h"` directories. 


<img src="/doc/img/cluster-topology-01.svg">

Here is an example branch of regional catalog:

```CSharp
+ World.r
-    EU.r
-        Bern.noc
-             DB.z
                  Zgov1
                  Zgov2

  -               Usr.z
                      mongo01
                      mongo02
                      mysql1
                      mysql2
                      mysql3

  -               Fin.z
                      mysql1
                      mysql2
                      mysql3
                      mysql4
                      mysql5

-             WWW.z
                  Proxy1
                  Proxy2
                  Www1 — //full path example: "/World/EU/Bern/WWW/Www1"
                  Www2
                  Www3
                  Www4

-             Worker.z
                  Worker1
                  Worker2
                  Worker3
                  Worker4
....
```