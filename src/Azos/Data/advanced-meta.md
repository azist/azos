# Advanced Data Document Metadata / Schemas
Back to [Documentation Index](/src/documentation-index.md)

This section describes accessing/working with Data in Azos.
See also:
- [Data Access Overview](readme.md) 
- [Metadata](metadata.md)
- [Data Validation with Domains](domains.md)
- [Data Modeling](modeling.md)


## Problem
Azos framework design is based on a 20+ year experience building large data processing/business applications. **Business systems
need to process complex data models**. Most of the times the models are **set by Government bodies** such as regulators or committees and
everyone in the industry is required to comply with a well defined standard.

**This discusses system metadata, such as the one defined by standards** (e.g. IRS1040 form), and not per-entity metadata (e.g. user preferences) which should be stored in a database.

A few examples of modeling requirements in the USA in the eCommerce and health care industries (just to name a few):

* Financial / IRS Forms 
* MCR/MCD general billing, CMS/HCFA 1500
* EDI 83* claims (834,835,837 etc.)
* PBM/NCPDP claims
* HL7
* LTC/MDS (Minimum Data Set), Case Mix, RUGs
* (many more...)

All of the above (used just as an example) have pretty much the same traits:

* Many models aka "Forms" (e.g. NCPDP Messages, MDS Sections, IRS1040 etc.)
* Most models broken further by segments (NCPDP Claims Segment, NCPDP Drug Segment etc.) 
* Each Segment has anywhere from a few to **more than a 100 fields** (e.g. MDS 2.0 Sections 'E'/'V', CMS/HCFA 802, CMS 1500)
* Required, min/max length, format, pick-list validations
* Custom data types such as **tristate fields** (e.g. dash, null/empty or numeric in MDS)
* **Conditional validation** dependency between forms/segments/sections (e.g. MDS sections do not apply if patient is comatose; NCPDP special drug dispensation apply for narcotics etc.)
* **Large body of knowledge** such as gov-defined descriptions/manuals/texts detailed to a field level (e.g. CMS MDS manual, NCPDP manual, IRS instructions etc.)
* Much **procedural/linear logic** defined by the gov body (sometimes as p-code) (e.g. RUG Case Mix)

The above standards are used just for context. There are thousands of various standards defined for various industries in 
different countries.

## Solution Overview
Azos data documents were created to address those common requirements:
1. Many forms/fields organized in a structure
2. Lots of metadata (including manuals/specs standardized in the industry)
3. Conditional validation logic, **multiple validation targets**
4. Custom metadata (e.g. attributes specific to certain industry/system)

Azos addresses **Requirement #1** above by providing a structured object-based type-safe data modeling using classic SOLID/OOP concepts with 
elements of functional (e.g. LINQ) programming where beneficial. You use classes and rich metadata facilitated by attributes.

**Requirement #2** is addressed by providing code-based convenient ways of metadata encoding and re-use using logical cloning/inheritance (described below) optional patterns which allow 
for metadata re-use between targets and model variations (e.g. data access model vs API contract model).

>*Some architects like to store much metadata externally in a file or database and we have tried that approach many times in the past.
This approach is problematic in large projects because it requires creation of special tools to maintain/manage this metadata in a proprietary format.
In Azos primary system metadata is stored in code, so we can use IDE/compiler as dev tools (including static typechecking) whereas
custom solutions do not have this benefit and rely on runtime errors as a sole checking method. This is not to say that external metadata storage
methods should not ever be used. There are valid cases for storing some metadata in database but this depends on a use-case*

**Requirement #3** is satisfied with **"multi-targeting" a truly unique feature in Azos framework**. You can provide metadata definitions as 
attributes targeted by name at a specific system/concept/use-case/technology (more details below).

**Requirement #4** is addressed using custom metadata configurations attach-able to a field level per target. Complex logical inheritance/derivation is possible (described below)

## Attribute-based Metadata Encoding, Cloning and Prototyping

## Multi-Targeting, Target Derivation

## Custom Metadata

## Metadata Indirection with Resource File References
Large government standards have much documentation which clarifies the use cases/rules and expectations, typically going down to a field level.
This creates a need to store multi-paragraph descriptions, possibly descriptions with some markdown (bold terms, references etc..).

Another typical consumption pattern is APIs which are typically called as (possibly micro) services by 3rd parties or other teams.
The API service consumers would benefit from rich metadata including plain language descriptions per entity/field.

Some validation rules span many lines, for example hard-coded pick lists may have 10+ items, while others can be semi-hard-coded and partially taken from database at runtime.
Pick lists are good examples as they feed the UI elements such as combo/drop down boxes. 

It would be problematic to store all of the aforementioned string metadata in the C# code directly, therefore Azos provides a **built-in
metadata indirection feature with resource references**. Instead of writing a long description, you can now store that description in a 
configuration file which is compiled as **"Embedded Resource"** alongside with your C# class. 

**You reference the value with a simple "./" indirection syntax.**

Example:
```csharp
  //instead of this

  [Field(description: @"
    ...
     3 paragraphs of text
    ...", valurList: "01:Custom processing code of 1 category; 02:Custom processing code of 2 category")]
  public string MyField1{ get; set;}

  //you can use resource reference
  [Field(description: @"./", valueList: "./")]
  public string MyField2{ get; set;}

  //and then include this in a config file MyClass.laconf
  MyField2
  { 
    description=" .. 3 paragraphs of text .. "  
    valueList="01:Custom processing code of 1 category; 02:Custom processing code of 2 category" 
  }

```










See also:
- [Data Access Overview](readme.md)
- [Metadata](metadata.md)
- [Data Validation with Domains](domains.md)
- [Data Modeling](modeling.md)

Back to [Documentation Index](/src/documentation-index.md)
