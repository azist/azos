# Runtimes and Platform Abstraction

Back to [Documentation Index](/src/documentation-index.md)

See also:
- [Platform Abstraction](/src/Azos/Platform/Abstraction)

## Targeted Runtimes
All system code in **Azos is built against .NET Standard** so it can
target different runtimes. 

**Note:** Azos purposely does not use built-in MSBuild multi-targeting and builds platform-specific entry points instead. This
is done on purpose to control the exact behavior of the platform-specific EXE produced (see below).

Azos supports two CLR (Common Language Runtime) runtimes by default:
- **.Net Framework 4.7.2 and above** - runs on Windows
- **.Net Core 2.0 and above** - runs on Windows, Linux and Mac

It is possible to run Azos applications on other OSs that support .Net Core,
however we have not "officially" tried it.

System code resides in `Azos.*` assemblies adheres to .Net assembly naming using
`PascalCase`, e.g. `Azos.dll`, `Azos.Web.dll`. The fact that the output files are called `*.dll`
does not mean that they are Windows-specific as they contain CLR metadata and CIL - please see [.Net Framework and
 .Net Core documentation](https://docs.microsoft.com/en-us/dotnet/).

## Entry Points
Azos builds entry points for every specific runtime. The following shows the solution structure:
```
 src/
    /runtimes
        /core
           Azos.Platform.NetCore20
           trun (NetSdk Csproj)
        /netf
           Azos.Platform.NetFramework
           Azos.WinForms
           trun (Classic .Net Csproj)
 Azos/
   ....
    Class.cs
   ....
 Azos.Tools/
   ....
    trun/
       ProgramBody.cs
   .....
 Azos.sln
```

The per-platform code resides under `/src/runtimes/<runtime>` directory. Note how both runtimes have a test runner console tool
*(used just as an example)*. The body of `trun` tool resides in `Azos.Tools.trun.ProgramBody.cs` which is a platform-
independent code in a .Net Standard library.

Note that `Azos.WinForms` library is under `/netf` runtime only, there is no counterpart under `/core` - this is because
WinForms legacy technology is not supported/needed on .Net Core.

The build output is directed into `/out` directory co-located with `/src` (cloaked from source control). All
build artifacts are emitted in that folder: `/out/<Configuration>/<runtime>` as depicted:
```
 out/
    Debug/
       run-netf
         trun.exe
         Azos.dll
         Azos.Tools.dll
        ....
       run-core
         trun.dll
         Azos.dll
         Azos.Tools.dll
        ....
    Release/
       ..... similar structure ....
```
To run tests using `trun` cli *(just as an example)* you would need to go into corresponding runtime directory
and invoke the executable entry point:
```batch
C:\azos\out\Debug\run-core> dotnet trun.dll -?
.....
C:\azos\out\Debug\run-core> cd ..\run-netf
C:\azos\out\Debug\run-netf> trun.exe -?
.....
```

## Memory Considerations

**Azos is built for 64-bit operation on servers**, while it is possible to build the code
for 32 bit apps, we do not officially support 32 bit code.

**Note**: *Always set Garbage Collector mode to "SERVER" in your process config files*

The snippet below sets the Server GC mode on .Net Core project:
```xml
...
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.0</TargetFramework>
    <ServerGarbageCollection>true</ServerGarbageCollection>
  </PropertyGroup>
...
```
Use the following code in `App.config` for .Net Framework executable files:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <runtime>
    <gcServer enabled="true"/>
  </runtime>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7.1"/>
  </startup>
</configuration>
```

## Platform Abstraction Layer

Azos is built on Net Standards, therefore **it can not use some platform-specific functions** *(e.g. compress graphical images)* directly.
Instead, Azos **delegates** the work to PAL = **Platform Abstraction Layer**.

PAL *(100% managed code)* is a central interface hub containing references to platform-specific areas:
* Images + Compression
* Drawing / 2D Graphics
* Machine information: performance counters, CPU, RAM etc...
* FS Access rights
* C# runtime code compilation

When a particular EXE (entry point) is built, it is **statically linked against some particular runtime**, e.g. .NET Framework or .NET Core.
PlatformAbstractionLayer moves the details specific to the runtime into separate module which is injected at the application entry-point.
The ideology of Azos (and any other well-architected software) recommends to keep the entry modules (exes) as small as possible, having
all of the business logic in the class libraries built against Azos in a platform-agnostic way (.NET Standard).
This way all of the code can be easily ported to the different platform/runtime

Example of the aforementioned `trun` CLI tool built against the full .NET Framework:
```CSharp
//statically reference Azos.Platform.NetFramework.dll (which uses full .net framework)

class Program
{
  static void Main(string[] args)
  {
    new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
    Azos.Tools.Trun.ProgramBody.Main(args);
  }
}
```

See [Platform Abstraction](/src/Azos/Platform/Abstraction) for more details

## Full Alt Stack

Since **Azos is a full-stack solution**, it **does not use any 3rd-party libraries** besides
standard BCL and Microsoft-supplied ones (e.g. `Microsoft.CSharp.dll`). This makes **Azos an alt-stack
choice** for working with .Net. Azos provides all of the core services itself - something which is 
usually accomplished using myriads of 3rd party disjoint systems.

---

Back to [Documentation Index](/src/documentation-index.md)

