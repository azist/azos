# Scripting.Packaging

Provides facilities to pack, introspect and install file bundles, aka "software packages".
The functionality is similar to `Nuget` et.al. however it is geared towards software
image installation in cloud systems with capability to **add custom setup steps** while avoiding 
any 3rd party dependencies.

A **package is an archive of `Command`-derived objects**, written out using `Azos.IO.Archiving` framework,
which supports **optional compression, encryption and authenticity checking** via **crypto API** integration.
This is a requirement since binary distributables need to be protected from tampering, be encrypted and compressed in most cases.

The packaging API offers:

* A binary stream/file format for packages, based on `Azos.IO.Archiving` (optional encryption, authenticity, and compression)
* A stream of `Command`-derived objects, such as `CreateFileCommand`, `CdCommand`, `ChmodCommand` etc.
* The package contents is read by `Installer` class on a target machine and commands are executed sequentially
* A series of DSL package creation scripting steps which allow `Packer` to write a binary representation of the package into a stream/file

The usage **of DSL steps** during package creation allow for fine-grained control of package content, for example,
you can include/exclude files using globing patterns. You can also include **custom `Command`-derivatives**
for specific control of package installation options. A series of built-in commands provide common functionality such as `CreateDirectoryCommand`, `ChmodCommand` etc..

> Due to the fact that installation is not a transactional high-throughput activity, we decided to keep all I/O APIs as
> classic sync-only for simplicity and readability


> Q: **Why cant I use ZIP/tar and copy files over as a tar-ball?**
> A: You could, however you would need to script the ZIP creation yourself and you would have no custom control
> during an "unzip" on file properties such as applying `chmod`. You would also lose conditional logic which is sometimes needed
> to pre-generate resources on a target machine such as pre-populate cached files.


> Q: **Can I add custom commands which run during install?**
> A: Yes, just derive your class from `Command`, add a `PackageCommand` attribute with a new guid and override `Serialize,Deserialize and Execute` methods.
> You will need to include your assembly reference for the installer (see examples).
> You can also invoke OS script with a built-in `ExecOsCommand`


