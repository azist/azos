# Framework Testing

This directory contains testing code. Azos testing code is written using [Azos.Scripting](/src/Azos/Scripting) and
 [Aver.cs](/src/Azos/Aver.cs) "assertion" library. The testing philosophy is described here. TBD

There are **three test "tiers"** used in Azos framework:

1. [Azos.Tests.Nub](Azos.Tests.Nub) - tests the very base "crux" of the framework - the functionality 
 which everything else is based on (config, app chassis, modules etc.). The nub tests must be quick to 
 execute and they are used for basic regression testing while developing Azos itself. Nub testing does not
 (and it should not) provide full comprehensive coverage of functionality, rather it is purposed to
 **quickly ascertain the correctness** of framework "very core" (hence the name)
1. [Azos.Tests.Unit](Azos.Tests.Unit) - covers the Azos framework as a whole with unit tests some of which
 may take significant amount of time to execute. The tests are categorized using the "base" identifier to further
 narrow the testing scope
1. [Azos.Tests.Integration](Azos.Tests.Integration) - tests various components together, requiring external services
 (such as local DB server instance) for some tests. These tests are usually long running and concentrate on inter-service
 consumption and multi-threading

---
### Quick cheats:

##### Run Tests using CLI
Run nub tests from console on **Linux** or **Mac** (can only use `run-core` runtime):
```batch
~/azos/out/Debug/run-core
$ dotnet trun.dll Azos.Tests.Nub.dll
```

Run nub tests on **Windows** using `run-netf` or `run-core` runtimes:

```batch
C:\azos\out\Debug\run-netf> trun Azos.Tests.Nub.dll
C:\azos\out\Debug\run-core> dotnet trun.dll Azos.Tests.Nub.dll
```

The core vs. net test runner command args syntax is the same.

##### Get CLI Help

```batch
$ dotnet trun.dll -?
C:\azos\out\Debug\run-netf> trun -?
C:\azos\out\Debug\run-core> dotnet trun.dll -?
```

##### Run Specific Tests
Use `-r` switch to configure test script runner with pattern search expressions.

Search by namespace names:
```batch
~/azos/out/Debug/run-core
$ dotnet trun.dll Azos.Tests.Nub.dll -r namespaces=RunnerTests.Inject*;MyLogic.DB.*
```

You can combine pattern filters using `?` and `*`:

```batch
~/azos/out/Debug/run-core
$ dotnet trun.dll Azos.Tests.Nub.dll -r namespaces=RunnerTests.Inject* methods=*Json_Read?-* names=case?
```

Specific method names:
```batch
~/azos/out/Debug/run-core
$ dotnet trun.dll Azos.Tests.Nub.dll -r methods=*AsJson*
```

Use `names` parameter to invoke explicitly-named test cases:
```batch
~/azos/out/Debug/run-core
$ dotnet trun.dll MyTesting.dll -r names=MySpecialTest
```
the use the name in code:
```CSharp
[Run("!MySpecialTest"...]//note the use of "!" - unless you pass the
                         //test name explicitly it will not auto run
public void Special()
{
 ...Aver.IsTrue(...)...
}
```



##### Emulate Tests
This is needed to see what tests will run, but don't run them. Emulation helps identify the test
configuration/setup issues.
```batch
~/azos/out/Debug/run-core
$ dotnet trun.dll Azos.Tests.Integration.dll -r emulate=true
```

##### Save Results into File
Pass `out=<file>.xml|json|laconf` specifier to the `-host` switch:
```batch
~/azos/out/Debug/run-core
$ dotnet trun.dll Azos.Tests.Integration.dll -host out="~/azos/out/results.laconf"
```





