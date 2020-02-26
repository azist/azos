# Command Args Config Format (CLI Args Parser)

Back to [Documentation Index](/src/documentation-index.md)

## Overview

> **IMPORTANT** In Azos you do not need any special library to parse complex command lines with switches as the application parses args into a config tree which can then be applied to any entity (e.g. classes) just like any other config tree would. You do not need to create special models just for command line options, in fact **any tool code base must NOT be written for CLI use only**. This is because any such code (e.g. a utility) may need to be called from a script/batch or web activator.


Parsing of command line args is done using `CommandArgsConfiguration` which builds a tree of the supplied `args`. This allows
for using a **uniform configuration mechanism of objects** in code from file, distributed config source or command line arguments.

This section explains how it works, but you rarely if ever need to parse command args using manual allocation of `CommandArgsConfiguration` , because it is a built-in property of `IApplication.CommandArgs: IConfigSectionNode`

> **Note** as of Dec 2019 the parser does not adhere to *nix standards parsing short vs long switches, in other words `-long` does not create 4 switches, but acts as a `--long` in Linux. There is a task to have a Linux compatible mode

Given a command line:
```bash
c:\>dosomething "c:\input.file" "d:\output.file" -compress level=100 method=zip -shadow fast -large
```

the following config tree is built (shown logically):

```
[args ?1="c:\input.file" ?2="c:\output.file"]
  [compress level="100" method="zip"]
  [shadow ?1="fast"]
  [large]
```
which can be saved to JSON or Laconic with one line `.ToLaconic()`:

```csharp
args
{
  "?1"="c:\\input.file"
  "?2"="c:\\output.file"
  compress{level=100 method=zip}
  shadow{"?1"=fast}
  large{}
}
```

## CLI Program Example
An simple CLI tool skeleton example using runtime multi-targeting:
```csharp
// call: mytool -?|h|help
 class Program
 {
    int Main(string[] args)
    {
      try
      {
        new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
        //.NET Core copy will have this instead:
        //new Azos.Platform.Abstraction.NetCore.NetCore20Runtime();
        return ProgramBody.run(args);
      }
      catch(Exception error)
      {
        ConsoleUtils.WriteError(error.ToMessageWithType());
        return -100;
      }
    }
 }
  //MyTools assembly, .NET Standard (works on all platforms)
  class ProgramBody
  {
    public int Run(string[] args)
    {
    using(var app = new AzosApplication(args))
    {
    if (app.CommandArgs["?", "h", "help"].Exists)
    {
    ConsoleUtils.WriteMarkupContent( typeof(ProgramBody).GetText("Help.txt") );
    return 0;
    }
    ...... do you stuff ......
    }
  }
 }
```


See also:
- [Configuration Overview](readme.md)
- [Json Config Format](json-format.md)
- [Laconic Config Format](laconic-format.md)

Back to [Documentation Index](/src/documentation-index.md)


