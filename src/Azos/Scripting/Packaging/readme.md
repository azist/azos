# Scripting.Packaging

Provides facilities to pack, introspect and install file bundles, aka "packages".
The functionality is similar to `Nuget` et.al. however it is geared towards software
image installation in cloud systems without any 3rd party dependencies.

A **package is an archive of `Instruction` objects**, written out using `Azos.IO.Archiving` framework.
Support **optional compression, encryption and authenticity checking** via **crypto API** integration.
This is a requirement since binary distributables need to be protected from tampering, encrypted and compressed in most cases.

The packaging offers:

* A binary stream/file format for packages, based on `Azos.IO.Archiving` (optional encryption, authenticity, and compression)
* A stream of `Command` objects, such as `WriteFile`, `Chmod` etc.
* The package contents is read by `Installer` class on a target machine and commands are applied sequentially
* A series of DSL scripting steps which allow for `Packer` to create a package binary representation in stream/file

The usage **of DSL steps** allow for fine-grained control of package building, for example,
you can include/exclude files using globing patterns. You can also include **custom `Command`-derivatives**
for specific control of package installation options. A series of built-in commands provide common functionality such as `chmod`, `mkdir` etc..

> Due to the fact that installation is not a transactional high-throughput activity, we decided to keep all I/O APIs as
> classic sync-only for simplicity and readability


> Q: **Why cant I use ZIP/tar and copy files over as a tar-ball?**
> A: You could, however you would need to script the ZIP creation yourself and you would have no custom control
> during an "unzip" on file properties such as applying `chmod`. You would also lose conditional logic which is sometimes needed
> to pre-generate resources on a target machine such as pre-populate cached files.
