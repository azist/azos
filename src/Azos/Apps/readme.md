# Azos Application Chassis
Back to [Documentation Index](/src/documentation-index.md)
This section describes Application Chassis
See also:
- [Dependency Injection](/src/Azos/Apps/Injection)
- [Configuration](/src/Azos/Conf)

## Overview
`Azos.Apps` namespace provides uniform application chassis model for all application kinds be it console apps, web or any other servers.
 
 Application chassis is a global singleton object which acts as a "motherboard"/"chassis" for your process. 
 It implements [`IApplication`](/src/Azos/IAppplication.cs) contract which provides service like: configuration, DI, log, console port, 
 instrumentation, data store etc.

### Allocation
 An application instance is allocated once, at the entry point, typically using a `using{}` block.
 ```csharp
//from TRUN test runner container app
public static int Main(string[] args)
{
  try
  {
    using(var app = new AzosApplication(true, args, null))//allow nesting
    {
      //Handle CTL+C - stop application
      Console.CancelKeyPress += (_, e) => { app.Stop(); e.Cancel = true; };

      return run(app);
    }
  }
  catch(Exception error)
  {
    ConsoleUtils.Error(error.ToMessageWithType());
    ConsoleUtils.Warning(error.StackTrace);
    Console.WriteLine();
    return -1;
  }
}
 ```

### Configuration Boot
Upon its allocation, the `IApplication` object tries to acquire its configuration root node (conf vector). 
You can optionally pass a prefabricated custom configuration vector to the `AzosApplication` constructor, in which case you will need to 
get/make its content yourself. When `null` is passed instead (the default), the application would go and search for any 
file/format supported by the configuration provider, by default it is either `Laconic`, `Json`, or `Xml` formats.
The file is searched in location co-located with the executing process entry point, so for example, if your app is called "app1", then
the system will searched for "app1.laconf", "app1.json", "app1.xml", "app1.configuration" (xml).
If no specific file found, then an empty config node is allocated in memory as `App.ConfigRoot`.

> **Sky distributed applications** get their configuration from the `Metadabase` as mounted via `Metabank` class by the app boot loader. This relies on app chassis nesting


### App Chassis Nesting
An application chassis may support a concept of nesting - an ability to allocate and deallocate an 
inner application scope within another. This feature is needed in many cases. 

Take **unit testing as an example** of nesting app chassis.

The **Test container `trun`** allocates in it own `AzosApplication` context, consequently it uses
its own logging, instrumentation etc. While tests run (e.g. `Runnable` harnesses), they may need to allocate
their own application configurations, which are specific to the test. Test runner `trun` container app
is configured with `AzosApplication` instance which allows for nesting (because of `new AzosApplication(allowNesting: true, ...)`, therefore all sub-tests can operate
under a different context of log, DI, modules etc...  For more details on testing [see Testing topic](/src/testing/).





See also:
- [Configuration](/src/Azos/Conf)
- [Dependency Injection](/src/Azos/Apps/Injection)
- [Logging](/src/Azos/Log)
- [Instrumentation](/src/Azos/Instrumentation)
- [Data Access](/src/Azos/Data)
- [Security](/src/Azos/Security)

External resources:
- [Pattern: Microservice Chassis](https://microservices.io/patterns/microservice-chassis.html)
- [Service Location (Wikipedia)](https://en.wikipedia.org/wiki/Service_locator_pattern)
- [Dependency Injection (Wikipedia)](https://en.wikipedia.org/wiki/Dependency_injection)
- [IoC with Service Location / Dependency Injection by Martin Fowler](https://martinfowler.com/articles/injection.html)

Back to [Documentation Index](/src/documentation-index.md)
