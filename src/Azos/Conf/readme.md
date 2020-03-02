# Azos Configuration Engine

Back to [Documentation Index](/src/documentation-index.md)

This section describes configuration engine in Azos which is used in many different ways. Many functions/components which need to get a structured dynamic data vector use config engine, because of this the **term "configuration" in Azos** does not  imply working with application config files only, but **has a much broader meaning/applicability**.

Here is a list of a few use-cases **besides** typical configuration:
- Application **command line arguments** are parsed into configuration, consequently various components may get injected/configured from command line, file or any other source in a uniform way
- **Security ACL/Rights** are stored as config tree (it is a security descriptor tree)
- Sky `cmdlets` (commandlets) are typed-in in application terminal. The commands are written in any supported config tree syntax (e.g. XML, JSON, Laconic)
- Metadata generator which generates API documentation from controllers, builds the **metadata tree into a "configuration" object** which can later be saved into various formats or served right from memory
- Testing script runner dumps **test results into a data object** which represents a results of a test session
- Complex **business logic uses IoC principle and performs DI of suitable strategies** at runtime having its **rules stored as config vectors in data store/db**
- **Abstract Expression Tree** nodes are fully inject-able from configuration thus allowing for complex expressions/logic (e.g. complex log sink filtering) construction from config vector with one line `.Configure(node)`


See also:
- [Laconic Format](laconic-format.md)
- [Json Format](json-format.md)
- [Xml Format](xml-format.md)
- [Command Args Format](cmdarg-format.md)

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
Configuration supports [**Laconic**](laconic-format.md), [**JSON**](json-format.md), [**XML**](xml-format.md) formats, all of which have the same eventual semantic expressed in a different syntax.

Although not technically a true format, a [**command-line argument**](cmdarg-format.md) parser also parses CLI args into a config tree.

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

> **Azos Config Framework is format-INDEPENDENT:** the config functionality such as macros, variable evaluation, navigation, property binding, etc. works over node trees in memory and does not depend on the content format which this tree was loaded from/saved into





-----------------------

See also:
- [Laconic Format](laconic-format.md)
- [Json Format](json-format.md)
- [Xml Format](xml-format.md)
- [Command Args Format](cmdarg-format.md)

Back to [Documentation Index](/src/documentation-index.md)


