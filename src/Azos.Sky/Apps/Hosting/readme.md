# Hosting

This namespace provides various services to facilitate process hosting:

* [BootArgs](BootArgs.cs) - helper class that parses boot arguments
* [GovernorDaemon](GovernorDaemon.cs) - manages the lifecycle of governed (managed) applications
* [ApplicationHostProgramBody](ApplicationHostProgramBody.cs) - static class providing `main()` logical process entry points

The `ApplicationHostProgramBody` class provides an entry point which is called by an exe `Main(string[] args)`
method. The determination of which entry point to call is made using `BootArgs` class - it 
pre-processes the supplied command line arguments. Depending on the `IsGoverned` and `IsDaemon` flags
and target runtime architecture the program starts as Windows service (.Net Fx on Windows), or
console app.

See [`BootArgs`](BootArgs.cs) summary for host boot config syntax.

## Examples

Linux examples:
```bash
# Run host governor on Linux as interactive console
$ ./azh hgov.laconf

# Run app1 governed by process on port 49123
$ ./azh gov://49123:app1 app1.laconf
```

Windows examples (using bash):
```bash
# Run host governor as interactive console 
/d/sky/bin> nws hgov.laconf

# Run app1 goverened by process on port 49123
/d/sky/bin> nws gov://49123:app1 app1.laconf

# Windows service registration
/d/sky/bin> sc create MY.HGOV obj=LocalSystem DisplayName=MY.HGOV start=auto binpath="d:\sky\bin\nws.exe daemon hgov.laconf"

# Windows service uninstall
/d/sky/bin> sc delete MY.HGOV
```
## Host Governor

Host governor (aka **`hgov`**) is a process which governs(manages) the runtime and execution of subordinate processes.
It uses your app process host executable with `hgov.laconf` app config to launch items under `/boot`
section.

The boot section contains an ordered list of applications which are spawned by host gov process.
Host gov uses injectable `IAppActivator` which is tasked with `Start/Stop` operations performed on
governed applications. 

By default a `ProcessAppActivator` implementation uses OS's `Process` class to launch governed app processes
on the same machine.

> VNext: The `DockerAppActivator` activates app processes using docker container runtime.

Host gov daemon observes the running processes using a [`SIPC`(Simple IPC)](/src/Azos/IO/Sipc) TCP socket communication
established from clients (governed apps) to the server (host gov process). When these aps run, the governed launches
their processes passing a `gov://PPPP:AAA` pragma via command line args having `P` represent listener 
port, and `A` application id/name from gov boot config 


