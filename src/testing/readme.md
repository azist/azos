# Framework Testing

Back to [Documentation Index](/src/documentation-index.md)


This directory contains testing code. Azos testing code is written using [Azos.Scripting](/src/Azos/Scripting) and
 [Aver.cs](/src/Azos/Aver.cs) "assertion" library. The testing philosophy is described here. TBD

**Warning**: Do not use `System.Console` in your tests, instead use [`Conout`](/src/Azos/Scripting) which has similar signature but is 
redirectable in a thread-safe way. Internally, `Conout` redirects output to [`IConsolePort`](/src/Azos/IO/Console) which 
sends output to either local console or can be connected to an external console a.k.a. **TV (TeleVision) system** 
for remote/cloud debugging.

There are **three test "tiers"** used in Azos framework:

1. [Azos.Tests.Nub](Azos.Tests.Nub) - tests the very base "core" of the framework - the functionality 
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
```bash
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
Use the `-r` switch to configure test script runner with **pattern search predicate expressions**.
Pattern expressions use `?` for a single character match and `*` for multiple char match. 
The `~` character denotes "not"/inversion and may appear only at the very beginning of a pattern expression.

For example:
- `*Session` will match strings ending with "Session"
- `~*Session` will match anything BUT strings ending with "Session"
- `Test_?3` matches on any single character before "3" at the end

You can apply pattern search to the following properties of runnable classes/methods:
- Categories - defined via `[Runnable(category: "mycategory1"]` attribute decoration (see categories note below)
- Namespace names - pattern applied to namespace names which contain `[Runnable]` classes
- Method names - pattern applied to class/method names
- Names - named test cases


You can **combine multiple pattern** filters using `,` `;` or `|` delimiters.
When processing patterns the system applies logic depending on the pattern types: direct, or inverted.
Non-inverted (direct) patterns are combined using `ORs` as in "match any of": `a*;b*;c*`.
The inverted patterns starting with `~` are combined with `ANDs` as in "match none of": `~a*;~b*;~c*`.
If both pattern types are specified, then the system applies all `ORs` first then all `ANDs`:  
 `(direct1 [|| directX])[&& inverted1][&& invertedX]` etc.

Search by namespace names (using OR):
```batch
~/azos/out/Debug/run-core
$ dotnet trun.dll Azos.Tests.Nub.dll -r namespaces=RunnerTests.Inject*;MyLogic.DB.*
```

Search by namespace names and method names:

```bash
~/azos/out/Debug/run-core
$ dotnet trun.dll Azos.Tests.Nub.dll -r namespaces=RunnerTests.Inject* methods=*Json_Read?-* names=case?
# with AND NOT
$ dotnet trun.dll Azos.Tests.Nub.dll -r namespaces=*Inject*;*Session*;~*Secur* methods=~*Json* 
```

Specific method names:
```bash
~/azos/out/Debug/run-core
$ dotnet trun.dll Azos.Tests.Nub.dll -r methods=*AsJson*
```

Cases of specific category:
```bash
# hub or draw categories
$ dotnet trun.dll MyTests.dll -r categories=hub,draw

# but not i/o tests
$ dotnet trun.dll MyTests.dll -r categories=hub,draw,~io

# hub category, namespace "Serialization", any method but for the ones called "_Fail"
$ dotnet trun.dll MyTests.dll -r categories=hub namespaces=*Serialization* methods=~*_Fail
```

> If categories filter is specified then it expects that category on the `[Runnable]` declaration, then if there is at least one method with `[Run]` spcifying the category then 
> the rest of method declarations must also specify category. In other words: **if none of the runnable methods have category set**, then the **category specification cascades down from 
> runnable class level to its individual methods**

Use `names` parameter to invoke **explicitly-named test cases**:
```bash
~/azos/out/Debug/run-core
$ dotnet trun.dll MyTesting.dll -r names=MySpecialTest
```
then use of the **explicitly-named run case** in code:
```CSharp
[Run("!MySpecialTest"...]//note the use of "!" - unless you pass the
                         //test name explicitly it will not auto run
public void Special()
{
 ...Aver.IsTrue(...)...
}
```



##### Emulate Tests
This is needed to see what tests will run, but don't run them. Emulation helps to identify the test
configuration/setup issues.
```bash
~/azos/out/Debug/run-core
$ dotnet trun.dll Azos.Tests.Integration.dll -r emulate=true
```

##### Save Results into File
Pass `out=<file>.xml|json|laconf` specifier to the `-host` switch:
```bash
~/azos/out/Debug/run-core
$ dotnet trun.dll Azos.Tests.Integration.dll -host out="~/azos/out/results.laconf"
```

##### Custom Testing Host
In some CI/CD environments (e.g. [Azure DevOps](https://azure.microsoft.com/en-us/services/devops/)) you may need to run the tests in a different hosting environment, for example
Azos uses [Appveyor](https://www.appveyor.com/) service for its CI auto build.

The `-host` switch is used to inject a different text host type (taken from `appveyor.yml')`:
```yml
 test_script:
 - cd ..\out\Release\run-netf
 - trun Azos.Tests.Nub.dll -ec -host type="Azos.Scripting.TestRunnerAppVeyorHost, Azos"
```

In the example above, the `Azos.Scripting.TestRunnerAppVeyorHost` uses special hooks
specific to [Appveyor Build Worker API](https://www.appveyor.com/docs/build-worker-api/) 
to report testing results in real time

---
Back to [Documentation Index](/src/documentation-index.md)




