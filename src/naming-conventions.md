# Azos - Naming Conventions & File Structure

This document describes the source code naming conventions for the Azos project.

## Constants
* Constants shall be in UPPER_CASE delimited by '_' underscores, e.g. "MAX_TCP_DELAY_SEC"
* The constant unit must be stated at the end of const name, e.g. "SOCKET_TIMEOUT_MSEC = 780"
* Common time units/suffixes are: _MSEC, _SEC, _HR, _MIN, _TIMES, _COUNT
* Complex type values used as constants via `static readonly` must stick to constant naming rules, e.g. `public static readonly Domain GLOBAL_DOMAIN = new Domain();`
* Configuration-related constants shall start with "CONFIG_" prefix, e.g. "CONFIG_PROVIDERS_SECTION"
* Configuration section names shall end with "_SECT" or "_SECTION"
* Configuration attribute names shall end with "_ATTR" or "_ATTRIBUTE", e.g. "CONFIG_APPLIED_ATTR"

## Public

* All public members (even in non-public types), must be in PascalCase, e.g. "DatabaseConnection", "UpdateRecord()", "button.MaxSize = ..."
* Public fields should not be exposed directly, shall a need arise to keep fields exposed - use PascalCase per rule above

## Non-Public
* Private members start with camelCase, e.g. `private void makeLogger(...)`
* Protected memebers shall be named as public, e.g. `protected void DoConnect()`
* Instance member fields shall start with "m_" prefix, e.g. `private int m_Width;`
* Thread static member fields shall start with "ts_" prefix, e.g. `private slot[] ts_SlotCache;`
* Async Local/static member fields shall start with "ats_" prefix, e.g. `private WorkContext ats_Context;`
* Members with names starting with one or more underscores "_" signify the special behavior which shall be avoided, e.g. `__setParent(p)`

## Protected/Template Methods
* Template Method design pattern, the virtual methods have "Do" prefix, e.g. `public void Connect()` does some checks and calls internally `protected virtual void DoConnect()`


## File Naming
* Types > 100 LOC should be kept in a file per type
* It is ok to combine **many related trivial** types in one file (when each type does not have much code, e.g. custom exceptions)
* Group related interfaces (if they are short and related) under "Intfs.cs"
* For type per file: file name should equal its single type
* For multiple types per file, use reasonable common name, e.g. "Exceptions.cs"
* Break large types *(should be a good reason to have a large type)* into partials, naming files accordingly: "Pile.Allocator.cs", "Pile.Properties.cs" etc.

## File Structure
The uniform file structure shall be adhered to, unless the described type is trivial, 
for example: there is not need to declare regions for a struct with 2 fields. 
The rules below apply to the classes with "body" over 100 LOC give or take. The following
structure is good for general class readability as inter-dependent items (such as ctor, dctor, fields) 
are co-located visually. 

Code fold/regions:

* COPY header
* Usings
* Inner Types (if any), if longer than 50 LOC consider moving into partial class in a separate file
* Static methods
* .ctors
* .dctors (override Destroy() if any)
* Private fields/state holder (near cotr visually = easier to read)
* Properties (ususally reference provate fields)
* Public methods
* Protected methods
* .pvt implementation (at the very end)



--------------------

**Note on Microsoft Code Analysis Rules** 
Azos project follows its own standard and MSFT-defined typical .Net/FxCop rules do not apply by design