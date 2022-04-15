# DSL Step Script Runner

Facilitates execution of DOMAIN-SPECIFIC language C# code snippets embodied id [`Step`](BaseSteps.cs)-derived classes.

Step Scripts are composed of series of configured steps that can be supplied as [Configuration](/src/Azos/Conf) with all of the scripting features of the 
Azos Configuration Engine. Each `Step` is a C# coded concrete implementation of the `Azos.Scripting.Steps.Step` abstract class that you want to 
create for logic encapsulation. Simply build your own library of steps for execution and then provide that as configuration to a [`StepRunner`](StepRunner.cs)
which will run each step's code execution in sequence until all steps have completed (Control Flow steps such as `Goto` may change execution path).

## Step Configuration Examples:

### 1. Simple Configuration:

A simple [Laconic Format](/src/Azos/Conf/laconic-format.md) example:

```csharp
  script
  {
    //We do not want to repeat this part in various type references down below,
    //so we set it in the root of run step section for all child section down below
    type-path='Azos.Scripting.Dsl, Azos'

    do{ type='See' text='Step number one' name='loop'} //loop label
    do{ type='See' text='Step number two'}
    do{ type='Delay' seconds=0.5 }
    do{ type='Goto' label='loop' name='goto1'}
  }
```

A simple [Json Format](/src/Azos/Conf/json-format.md) example:

```json
{
    "script": {
      "type-path": "Azos.Scripting.Dsl, Azos",
      "timeout-sec": 1.25,
      "do": [
        {
          "type": "See",
          "text": "Step number one",
          "name": "loop"
        },
        {
          "type": "See",
          "text": "Step number two"
        },
        {
          "type": "Delay",
          "seconds": 0.5
        },
        {
          "type": "Goto",
          "label": "loop",
          "name": "goto1"
        }
      ]
    }
}
```


### 2. Configuration with Inclusion of Multiple `Sub` Routine Files:

A [Laconic Format](/src/Azos/Conf/laconic-format.md) example including `sub-routine-one.laconf` and `sub-routine-two.laconf` steps:

```csharp
setup-auth-kit
{
  process-includes="_include"
  type-path="Azos.Scripting.Dsl, Azos; Azos.MySql.ConfForest.Steps, Azos.MySql;"

  do
  {
    type="Sub"
    name="sub-routine-one"
    source{ _include{ file="./scripts/tools/$(../../$name).laconf" } }
  }

  do
  {
    type="Sub"
    name="sub-routine-two"
    source{ _include{ file="./scripts/tools/$(../../$name).laconf" } }
  }

}
```

## Anatomy of a Step

Steps are inherited from the abstract `Step` class that provides a C# stateful container of runnable logic that is run in order of within the executing script configuration. 
Each step provides local and global state, `RunStatus` (e.g. Init, Running, Finished, Crashed, Terminated), 
order within the containing script, timeout, exception, result, and optional name logic.

### Creating your own `Step` implementations

When creating your own steps you can simply provide a constructor that chains the values to base. 
Add additional `[Config]` attributed public properties (you should declare properties as `string` if you intend to evaluate variable replacements from JSON input or command arguments).
Then override the `DoRun` base abstract method with your concrete step logic.

> **Note*** the `Eval(string value, JsonDataMap state)` method calls in the `DoRun` method, this is done to evaluate variable replacements from an external JSON input file or command arguments. 

```csharp
  /// <summary>
  /// Emits a log message
  /// </summary>
  public sealed class Log : Step
  {
    public Log(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx){ }

    [Config] public Azos.Log.MessageType MsgType{ get; set;}
    [Config] public string From { get; set; }
    [Config] public string Text { get; set; }
    [Config] public string Pars { get; set; }
    [Config] public string Rel  { get; set; }

    protected override string DoRun(JsonDataMap state)
    {
      var guid = WriteLog(MsgType, Eval(From, state), Eval(Text, state), null, Eval(Rel, state).AsNullableGUID(), Eval(Pars, state));
      Runner.SetResult(guid);
      return null;
    }
  }
```

### Azos Provided Steps

The Azos framework includes several basic utility and evaluation steps that you can use in addition to your custom step implementations.

#### Azos Utility Steps:

- **Log : Step** - Writes a log message for this run step; returns the new log msg GDID for correlation, or GDID.Empty if no message was logged.

- **See : Step** - Writes to Console output the `Text` (evaluated for variables) or `Format` 
(evaluated for variables and Expands a format string of a form: `"Hello {~global.name}, see you in {~local.x} minutes!"`).

- **DumpGlobalState : Step** - Dumps the global state to console (by default) and optionally writes to file if `FileName` is provided.

- **DumpLocalState : Step** - Dumps the local state to console (by default) and optionally writes to file if `FileName` is provided.

- **Delay : Step** - Runs a step with a delay in `Seconds`

- **LoadModule : Step** - Loads a module and resolves dependencies. The module gets registered with container scope context which will clean up all of the owned resources upon its exit

- **Impersonate : Step** - Impersonates a session with credentials using `Auth` (Basic Auth) or by `Id` and `Pwd`.

- **SetDataContextName : Step** - Sets the ambient session data context name.

#### Azos Eval Steps:

- **Set : Step** - Sets a global or local value to the specified expression.

    - Example:
    ```csharp
    do{ type="Set" global=a to='((x * global.a) / global.b) + 23'}
    do{ type="Set" local=x to='x+1' name="inc x"}
    ```

- **SetResult : Step** - Sets `runner.Result` to the specified expression. You can later retrieve it in C# as `Runner.Result: object` or in script expression using `runner.result`

    - Example:
    ```csharp
    do{ type="SetResult" to='((x * global.a) / global.b) + 23'}
    do{ type="SetResult" to='x+1' name="inc x"}
    ```

#### Azos Control Flow Steps:

- **EntryPoint : Step** - Defines an entry point. Entry points need to be used as independently-addressable
named steps which execution can start from **(Name property is Required!)**.

- **Sub : EntryPoint** - Defines a subroutine which is a `StepRunner` sub-tree **(the `source{ ... }` section is Required!)**.

- **Halt : Step** - Signals that the underlying `StepRunner` should no longer continue processing subsequent steps.
Can be paired with the "If" step provide "Halt and catch fire" stop execution logic.

- **Goto : Step** - Transfers control to another named step.

    - Example:
    ```csharp
    do{ type="See" text='Beginning of the loop!' name='loopStart' }
    ...
    do{ type="GoTo" step='loopStart'}
    ```

- **Call : Step** - Calls a named subroutine **(Name property is Required!)**.

- **If : Step** - Provides an If statement that evaluates a condition and 
executes the `then` section (if **True**) or the `else` section (if **False**).

    - Example:
    ```csharp
    do
    {
        type='If' condition='2 + 2 == 5'
        then
        {
            do{ type='See' text='Yessss'}
        }
        else
        {
            do{ type='See' text='Maybe Not?'}
        }
    }
    ```

## Executing Scripts

Step scripts are executed by calling an instance of a `StepRunner` or through calling a concrete implementation 
of a `Multisource<StepRunner>`. The Azos framework provides a `ScriptSource` concrete class implementation of `Multisource<StepRunner>`
in `Azos.Tools.Srun` namespace that provides a wrapper for creating a `StepRunner` from either an `IConfigSectionNode` OR creating directly
from a file. You can implement your own custom logic by inheriting and overriding the `StepRunner` and `Multisource<StepRunner>` as needed.

Additionally included in the `Azos.Tools.Srun` namespace is a console application `ProgramBody` bootstrapper that can be wrapped in a console
application. By using the `ProgramBody` bootstrapper you can expose a fully functioning console application that allows for JSON arguments to be
loaded and evaluated, overriding or directly supplying arguments, loading the configuration script by file name, and much more. See the Srun 
[Help.txt](/src/Azos.Tools/Srun/Help.txt), [ProgramBody.cs](/src/Azos.Tools/Srun/ProgramBody.cs), and 
[ScriptSource.cs](/src/Azos.Tools/Srun/ScriptSource.cs) to review the bootstrap logic.

#### StepRunner Laconic Example:

```csharp

    var steps = "script { do{ type='See' text='Yessss'} }";
    
    var runnable = new StepRunner(NOPApplication.Instance, steps.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
    runnable.Run();
```

#### StepRunner JSON Example:

```csharp

    var steps = "{ \"script\": { do{ \"type\":\"See\", \"text\":\"Yessss\"} }";
    
    var runnable = new StepRunner(NOPApplication.Instance, steps.AsJSONConfig(handling: Data.ConvertErrorHandling.Throw));
    runnable.Run();
```



## Creating a Bootstrapped Console Application for .Net Core 2.0 Example:

```csharp
using System;
using System.Reflection;
using Azos;

namespace srunc
{
  class Program
  {
    static void Main(string[] args)
    {
      System.Runtime.Loader.AssemblyLoadContext.Default.Resolving += refAssemblyResolver;

      new Azos.Platform.Abstraction.NetCore.NetCore20Runtime();
      Azos.Tools.Srun.ProgramBody.Main(args);
    }

    //so we can resolve ad-hoc assembly inclusions in scripting
    private static Assembly refAssemblyResolver(System.Runtime.Loader.AssemblyLoadContext loadContext, AssemblyName asmName)
      => Assembly.LoadFrom("{0}.dll".Args(asmName.Name));
  }
}
```

## Executing Step Scripts from you Console Application:

Optionally create a `script_args.json` JSON file with your supplied argument mix-ins (`-vars`):

```json
{ 
  "args": {
    "a": 4,
    "b": 3
    }
  }
} 
```

Then use the arguments (`-vars`) in your script file:

```csharp
script
{
    do{ type='See' text='~global.args.a'}
    do{ type='See' text='~global.args.b'}
}
```

Then execute the script with the `script_args.json` arguments:

```sh
$ dotnet srunc.dll scripts/authkit/setup.laconf -state script_args.json
```

You can override arguments -vars during execution:

```sh
$ dotnet srunc.dll scripts/authkit/setup.laconf -vars a=1 b=2
```

or partial replacements.

```sh
$ dotnet srunc.dll scripts/authkit/setup.laconf -state json_file_name -vars a=3
```

or for **HELP** execute

```sh
dotnet srunc.dll /?
```

that prints the help details:
```sh
Azos Script Step Runner
Copyright (c) 2022 Azist Group
Version 1.0 / Azos as of April 2022

 Usage:

   srun script_file [entry_point_sub_name] [/h | /? | /help]
              [/runner
                  [type=type_name]
                  [... runner type-specific attributes]
              ]
              [-r|-result]
              [-g|-global]
              [-s|-silent]
              [-state json_file_name]
              [-vars var1=val1 [varX=valX]]
              [-dump-source]


Executes a script contained in a script_file. The system performs "_include" expansion.
You can specify optional "entry_point_sub_name".

 Specifiers:

script_file - fully qualified script source path
entry_point_sub_name - optional entry point of the script
-r|result - dumps step runner result object
-g|global - dumps global JSON object
-s|silent - suppresses info
-state json_file_name - loads json data into runner globals
-vars var1=val1 [varX=valX] - sets global variables by name
-dump-source - if present dumps all source code into console

 Examples:

 srun d:\scr\devops\setup.laconf network
Runs a sub "network" from the specified script file

 srun d:\scr\devops\setup.laconf network -state data.json
Runs a sub "network" from the specified script file loading json data into globals

 srun d:\scr\devops\setup.laconf -vars port=123
Runs the specified script file pre-setting global "port" to "123"
```