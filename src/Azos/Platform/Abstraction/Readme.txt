Platform Abstraction Layer
--------------------------

Azos supports Net Standards, therefore it can not use some platform-specific functions (e.g. compress graphical images) directly.
Instead, Azos delegates the work to PAL = Platform Abstraction Layer.

PAL(100% managed code) is a central interface hub containing references to platform-specific areas:
* Images + Compression
* Drawing / 2D Graphics
* Machine information: performance counters, CPU, RAM etc...
* FS Access rights
* C# runtime code compilation

When a particular EXE (entry point) is built, it is statically linked against some particular runtime, e.g. .NET Framework or .NET Core.
PlatformAbstractionLayer moves the details specific to the runtime into separate module which is injected at the application entry-point.
The ideology of Azos (and any other well-architected software) recommends to keep the entry modules (exes) as small as possible, having
all of the business logic in the class libraries built against NFX in a platform-agnostic way (.NET standard).
This way all of the code can be easily ported to the different platform/runtime

Example of ConsoleApp.Exe built against the full .NET Framework:
//statically reference Azos.DotNetFramework.dll (which uses full .net)

void Main()
{
  new Azos.DotNetFramework.DotNetFrameworkRuntime();//<--- call this before everything else

  //do things as you normally would
  using(var app = new ServiceBaseApplication())
  {
    Console.ReadLine();
  }
}



