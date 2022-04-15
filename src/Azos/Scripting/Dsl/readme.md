# DSL Step Script Runner

Facilitates execution of DOMAIN-SPECIFIC language C# code snippets embodied id [`Step`](BaseSteps.cs)-derived classes.

Step Scripts are composed of series of configured steps that can be supplied as [Configuration](/src/Azos/Conf) with all of the scripting features of the 
Azos Configuration Engine. Each `Step` is a C# coded concrete implementation of the [Azos.Scripting.Steps.Step] abstract class that you want to 
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


    protected override string DoRun(JsonDataMap state)
    {
      WriteLog(MsgType, Eval(From, state), Eval(Text, state), null, null, Eval(Pars, state));
      return null;
    }
  }
```

### Azos Provided Steps

The Azos framework includes several basic utility and evaluation steps that you can use in addition to your custom step implementations.

#### Utility Steps:

- **Log : Step** - Writes a log message for this run step; returns the new log msg GDID for correlation, or GDID.Empty if no message was logged.


- **See : Step** - Writes to Console output the `Text` (evaluated for variables) or `Format` 
(evaluated for variables and Expands a format string of a form: `"Hello {~global.name}, see you in {~local.x} minutes!"`).

- **Delay : Step** - Runs a step with a delay in `Seconds`

- **LoadModule : Step** - Loads a module and resolves dependencies.

- **Impersonate : Step** - Impersonates a session with credentials using `Auth` (Basic Auth) or by `Id` and `Pwd`.

- **SetDataContextName : Step** - 