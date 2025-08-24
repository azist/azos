# Laconic Config Format

Back to [Documentation Index](/src/documentation-index.md)

## Overview
Laconic format is somewhat similar to [HOCON](https://en.wikipedia.org/wiki/HOCON), however it probably predates it as it was devised in the early 2000s (though not called "Laconic" at that time).

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
- Optional quotes, both single and double
- Line comments
- Block comments
- Verbatim strings + strings escapes a-la C# (e.g. `"\n\rFunny: \u3748\u2423"` )

Laconic cons:

- This format is not understood by built-in DB parses, store DB configuration vectors in JSON or XML formats instead (this is rarely needed when strategy/entity config is stored in the database)


## New Format Purpose

Laconic format has been purposely created to address the following deficiencies in other formats:
- Lack of single line comments in Xml and classic Json
- Cryptic escape sequences in Xml
- Verbose markup, need extra quotes and other characters around names
- Xml duplicates tag names making content very verbose
- Json is an object data format, whereas configuration is hierarchical data tree format (so is Xml and Laconic)

>**USE-CASES:** If you need to store data in config database or other externally-consumable source, you should probably use Xml or Json as those formats are parse-able by database built-in tools. Laconic is an excellent choice for application configuration and by design one does not need to pre-process those config files by any external tools (e.g. Octopus Deploy) because the changing part should be re-factored into a Xml/Json include file and then reference variables from Laconic. This practice has been proven in various projects

## Examples
```csharp
 root
 {
   atr1=value1
   convention-is-to-use-dashes="In node names use dashes"
   atr-2="value 2" //notice the use of quotes because there is a space in value
   
   "atr 3"="Mc'Cloud"

   section{ /* empty block comment */ }
   
   section=may-have-value //just like attributes, a section is an attribute with { } block
   {
      |*another 
       block comment*| 
   }

   "crazy \n section name" = 'with even crazier \u4675\u4263 value'
   {
     text=$"
       It is possible to span text
       multiple lines if you need ""double quotes""
     "

     another-text=$'
       Now we will use ''single quotes''
     '
   }
 }
```
Example of block comment nesting:
```csharp
root
{
  a{ }
  |*a  //<--- notice outer comment scope
  { 
    /*  
      c{} 
      d{}
    */
  }*|  //<---- ending here
  a{ }
}
```



## Conventions
* Use dashes in names like `my-timeout-ms` instead of spaces, so you would not need to quote it
* Multiline content: Put opening brace on the next line
* Multiline content: use 2 space indenting
* Try not to use section values, this is because this feature is hard to deal with in Xml and Json, e.g. in Json this `a=1{ b{} }` looks like `{"a":{"-section-value": 1, "b": {}}}`, and in Xml `<a><b/>1</a>`
* Block comments should use `/* */` c-style first and then `|* *|` a secondary flavor should only be used when you need to comment out existing block comments without altering them

## Laconic Grammar
"Laconfig" is the name of configuration data tree format with a single defined root, just like xml. The grammar is based on a subset of C# lexer/parser and uses features such as string escapes from standard C# 2.

* Configuration content is a unicode textual content organized in nodes
* There are 2 types of nodes: sections and attributes
* A section may have any number of sub-sections 
* A section may have any number of attributes
* A section may have more than one node with the same name (section or attribute), so this is perfectly legal: `db{ device{type=primary } device{type=secondary } ... }`
* Sections establish scope and may contain children: sub sections and attributes
* Attributes may not have children as they do not create scope
* Attributes and section are delimited by spaces or keywords, consequently there is no need to use quotes for most names (e.g. unless they have space) as in `my-flag=2{ a-case=1 b-case=2 }`
* Laconic keywords are: `{`, `}`, `=`,`null`, everything else is identifiers, comments or strings
* Strings use single or double quotes. `$` denotes a verbatim string similar to C#
* Strings use C# escape codes, including unicode spec with `\u0000` syntax
* Comments are single line with `//` and block with `/* */` and `|* *|` the block comments of two kinds are good for commenting out regions which are already commented
* Laconic content must have at least one and at most one root node section

> NOTE: `.AsLaconicConfig()` function allows for section-less data, in which case it just wraps that data, for example: `.AsLaconicConfig("a=1")` is actually processed as `r{ a=1 }`


See also:
- [Configuration Overview](readme.md)
- [Json Config Format](json-format.md)
- [Xml Config Format](xml-format.md)
- [Command Args Format](cmdarg-format.md)

Back to [Documentation Index](/src/documentation-index.md)


