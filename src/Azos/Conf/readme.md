# Azos Configuration Engine

Back to [Documentation Index](/src/documentation-index.md)

This section describes configuration engine in Azos which is used in many different ways not only for file-based configuration. Many functions/components which need to get a structured dynamic data vector use config engine.
Here is a list of a few use-cases **besides** typical configuration:
- Application command line arguments are parsed into configuration, consequently various components may get injected/configured from command line, file or any other source in a uniform way
- Sky `cmdlets` (commandlets) are typed-in in application terminal. The commands are written in any supported config tree syntax (e.g. XML, JSON, Laconic)
- Metadata generator which generates API documentation from controllers, builds the metadata tree into a "configuration" object which can later be saved into various formats or served right from memory
- Testing script runner dumps test results into configuration
- Complex business logic uses IoC principle and performs DI of suitable strategies at runtime having its rules stored as config vectors in data store/db
- Abstract Expression Tree nodes are fully inject-able from configuration thus allowing for complex expressions/logic (e.g. complex log sink filtering) construction from config vector with one line `.Configure(node)`


See also:
- [Data Schema](metadata.md)
- [Advanced Metadata](advanced-meta.md)
- [Data Validation with Domains](domains.md)
- [Data Modeling](modeling.md)

## Overview
The configuration framework is built for contemporary apps which are usually deployed on multiple hosts having different
roles in a system. The framework has to support both the **classical "conf file" approach** and **centralized network
configuration** for distributed application scenarios.

The following outlines the features of Azos config:

- Transparent integration in app chassis - **uniformity of configuration for any application type**
- **Format-abstract configuration tree** in-memory
- Support for **navigation** (similar to x-path) within the tree
- Read from **local files** (the classic model), or (logically)**centralized network/source control** location *(this does not mean that conf is fed from physically central location)*
- Unified model to configure classes, services, properties, fields etc. from configuration
- **Variable evaluation**, node inter-referencing, infinite cycles are detected and stopped
- **External Environment Variable** (external vars) evaluation built-in - used heavily to differentiate between environments, e.g. DEV vs .PROD
- **Pluggable variable** value providers - e.g. used for **externalizing security sensitive data** such as connect strings
- **Section includes** - take content form other files accessible using pluggable virtual file system (e.g. take from a local file, SVN, Amazon S3 or Google Drive etc.)
- **Structural merging, overrides** with rules. Prohibition (sealed sections) of overrides
- **Custom macro plugins** (e.g. `::NOW`, `::AS-TYPE` etc.)
- Support for **XML**, **Laconic**, **JSON**, **Command-Line Args** (CLI) formats
- Full support for **imperative scripting** constructs (macros) - loops, vars, Ifs, blocks
- **Aspect injection** with configuration `Behavior` - named kits of config values that may be applied to different nodes
indirectly. This approach addresses cross-cutting concerns on the configuration level
- Multiple value getters for different nodes and data types (i.e. `ValueAs: String/Date/Enum/Int`...) with defaults

## Reading Configuration Content

You rarely if ever need to read configuration files directly, as application chassis does this automatically:

- Application process starts (typically calling Program.Main())
- App chassis is allocated using pattern: `using(var app = new AzosApplication(args, null)){...}`
- The app .ctor is passed a command line args and optional root config node
- If the root config node is null (the default), the application will probe for local conf files
- Probing works by **trying to load a file co-located** with the application entry point (`dll` or `exe` file)
- The config **file name has to be the same as the entry point**, e.g. `"mytool.exe" -> "mytool.laconf"`
- The system will try to load the config file in ANY supported format (formats are described below)
- After the config file gets read by the app chassis, it parses it and makes it accessible as a structured tree via `App.ConfigRoot: IConfigSectionNode` property
- You rarely need to access this property while writing business code, as the containing components/modules are already configured and you access values using local class property/member access

There are times when config content needs to be processed in business code, for example when we store a config vector for business-related entity in a data store, we can use a handy set of accessor extensions: `AsJSONConfig(string)`, `AsXMLConfig(string)` etc. see the **Config Vector Pattern** below.


## Sky Cluster Configuration
Cluster app configuration is accessed using `Sky.Metabase` which provides both geo and logical configurational hierarchies.

The Sky applications use `SkyApplication` chassis which first boots from local config then mounts config from metabase store which is mounted using a virtual file system. See [Metabase Documentation](/src/Azos.Sky/Metabase)

## Configuration Content Formats
Configuration supports **Laconic**, **JSON**, **XML** formats, all of which have the same eventual semantic expressed in a different syntax.

> **Practical Recommendation:** Use Laconic format for all of the app configuration except for the stored content (e.g. in DB) and interop where JSON should be used instead

### Formats comparison
 The following snippet contains 3 logically identical structures in different formats:
```csharp
private const string LACONIC_SOURCE =
 @"root=-900{a=1 b=2 sub{z='my\nmessage!'}}";

private const string XML_SOURCE =
 @"<root a=""1"" b=""2"">-900<sub z=""my&#10;message!"" /></root>";

private const string JSON_SOURCE =
 @"{ ""root"":{ ""-section-value"":""-900"",""a"":""1"",""b"":""2"",""sub"":{ ""z"":""my\nmessage!""} } }";
```

### Laconic Format

Laconic format is somewhat similar to [HOCON](https://en.wikipedia.org/wiki/HOCON), however it probably predates them as it was devised in the early 2000s (though not called "Laconic" at that time).

> **LACONIC** means "terse" or "concise" in Greek, as in a "Laconic speech"

```csharp
 root=-900
 {
   a=1 //single line comment
   b=2 /* block comment */
   |* another nested /* block */ comment *|
   sub{z='my\nmessage! "OK"'}
   c=$"
   verbatim
   strings
   "
 }
```
This is the **default choice for writing config files** as it is the simplest format that has the following pros:

- Terse format (e.g. XML is much more verbose, so is JSON)
- Optional quotes, both single and dbl
- Line comments
- Block comments
- Verbatim strings + strings escapes a-la C# (e.g. `"\n\rFunny: \u3748\u2423"` )

Laconic cons:

- This format is not understood by built-in DB parses, store DB configuration vectors in JSON or XML formats instead (this is rarely needed when strategy/entity config is stored in the database)


## JSON
```json
{
  "root": {
  "-section-value": "-900",
  "a": "1",
  "b": "2",
  "sub":{
    "z": "my\nmessage! \"OK\""
  }
  "c": "\n verbatim \n strings "
 }
}
```

The only advantage of JSON over Laconic is if you need to read it somewhere else which rarely happens, e.g. if you need to read in in a SQL query. JSON is used for cases when config vector is stored in DBs or sent to client. Other than that, JSON is much harder to use because it does not support comments (classic json does not), and has much longer footprint/more typing and extra quotes.

Consider the difference:
```csharp
 a=1{b=one{} b=two{} b=three{} c{atr=val}}

 - vs -

 {"a":{"-section-value": "1","b":[{"-section-value": "one"},{"-section-value": "two"},{"-section-value": "three"}],"c":{"atr":"val"} }}

```

## XML
```xml
  <!-- only block comments -->
  <root a="1" b="2" c="&#10; verbatim &#10; strings ">
   -900
   <sub z="my\nmessage! &quot;OK&quot;">
   </sub>
  </root>
```
XML is very similar to JSON in its limitations. It is verbose, does not have line comments (which is very inconvenient) and requires additional escape sequences. In practice this format should only be used for persisting config snippets in DBs and other external systems.






-----------------------

See also:
- [Data Schema](metadata.md)
- [Advanced Metadata](advanced-meta.md)
- [Data Validation with Domains](domains.md)
- [Data Modeling](modeling.md)

Back to [Documentation Index](/src/documentation-index.md)


