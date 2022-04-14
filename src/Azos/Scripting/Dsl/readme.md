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