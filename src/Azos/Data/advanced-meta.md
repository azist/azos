# Advanced Data Document Metadata / Schemas
Back to [Documentation Index](/src/documentation-index.md)

This section describes advanced metadata field decorations used on data document Schema

See also:
- [Data Access Overview](readme.md) 
- [Metadata](metadata.md)
- [Data Validation with Domains](domains.md)
- [Data Modeling](modeling.md)


## Problem
Azos framework design is based on a 20+ year experience building large data processing/business applications. **Business systems
need to process complex data models**. Most of the times those models are **established by various Government bodies** such as 
regulators or committees and everyone in the industry is expected to comply with well defined standard.

**Here we talk about system metadata, such as the one defined by various standards** (e.g. IRS1040 form), 
and not entity-specific metadata (e.g. user preferences) which are typically stored in a database.

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
* Most models are further broken down by segments (NCPDP Claims Segment, NCPDP Drug Segment etc.) 
* Each Segment has anywhere from a few to **couple hundreds of fields** (e.g. MDS 2.0 Sections 'E'/'V', CMS/HCFA 802, CMS 1500)
* Required, min/max length, format, pick-list validations
* Custom data types such as **tristate fields** (e.g. dash, null/empty or numeric in MDS)
* **Conditional validation** dependency between forms/segments/sections (e.g. MDS sections do not apply if patient is comatose; NCPDP special drug dispensation apply for narcotics etc.)
* **Large body of knowledge** such as gov-defined descriptions/manuals/texts detailed to a field level (e.g. CMS MDS manual, NCPDP manual, IRS instructions etc.)
* Much **procedural/linear logic** defined by the industry (sometimes as p-code) (e.g. RUG Case Mix)

The above standards are used just as a sample context. There are thousands of various standards defined for various industries in 
different countries.

## Solution Overview
Azos data documents were created to address those common requirements:

1. 100s+ forms/fields organized in a structure
2. Lots of metadata (including manual/help/texts standardized in the industry)
3. Conditional validation logic, **multiple validation targets**
4. Custom metadata (e.g. attributes specific to certain industry/system)

Azos addresses **Requirement #1** above by providing a structured object-based type-safe data modeling using classic SOLID/OOP concepts with 
elements of functional (e.g. LINQ) programming where beneficial. You use classes and properties decorated with rich metadata.

**Requirement #2** is addressed by providing code-based **convenient** ways of metadata encoding and re-use (to minimize repetition and support)
using logical cloning/inheritance (described below) optional patterns which allow for metadata re-use between targets and model variations (e.g. data access model vs API contract model).

>*Some architects like to store much metadata externally in a file or database and we have tried that approach many times in the past.
This approach is problematic in large projects because it requires creation of special tools to maintain/manage this metadata in a proprietary format.
In Azos primary system metadata is stored in code, so we **can re-use IDE/compiler as metadata dev tools** (including static typechecking) whereas
custom solutions do not have this benefit and rely on runtime errors as the sole checking method. This is not to say that external metadata storage
methods should not ever be used. There are valid cases for storing some metadata in database but this depends on a use-case*

**Requirement #3** is satisfied with **"multi-targeting" capability - a truly unique feature in Azos framework**. You can provide metadata definitions as 
attributes targeted by name at a specific system/concept/use-case/technology (more details below).

**Requirement #4** is addressed using custom metadata configurations attach-able to a field level per target. Complex logical inheritance/derivation is possible (described below)


## Attribute-based Metadata Encoding, Multi-Targeting
Azos encodes metadata as a series of `[Field]` attribute declarations on data document properties. Any single logic field must have at least one 
`[Field]` declaration to be considered as a part of `Schema`.

Properties **not** decorated with '[Field]' attribute are **NOT** a part of `Schema`. This trait is used to create helper accessors to fields as the 
helper property values may be computed of the other properties marked with `[Field]` attribute.

Any logical document **field may be marked with more than one `[Field]` attribute**. This is needed for **multi-targeting**. For example, this code snippet
declares a field that has different column name in ORCL and all other databases:
```csharp
[Schema(name: "customer")] //notice no target declaration
[Schema(targetName: "mongo", name: "cust")]
[Schema(targetName: "ORCL", name: "tbl_cust")]
public class Customer : TypedDoc
{
  [Field(backendName: "name")] //notice no target declaration
  [Field(targetName: "ORCL", backendName: "nm")]
  public string Name { get; set; }
}
```

Multi targeting is also supported by `[Schema]` attribute, in the above example the table/collection names are varied by target.

> QUESTION: why does Azos use a single [Field] attribute instead of many separate attributes like data annotations does? The answer is - convenience and multi-targeting.
> It is convenient **in practice** to include things like "description", "required" etc. together (less symbols to type) but the true reason is multi targeting,
> as all of the field properties are **targeted together**. This would have not been practically possible to implement had every trait of every Field been a separate attribute declaration 

#### What do Target Names represent?
The naming/purpose of different targets is completely up to an application being designed. Targets can represent a backend technology, such as
"ORACLE" vs "ms-SQL" etc.., or they can represent a **logical system name**.

An **empty target** name, or **null target** name are logically equivalent to `const FieldAttribute.ANY_TARGET = "*"` so you can use those interchangeably. 

The following are the equivalent:
```
  [Field]
  [Field(targetName: null)]
  [Field(targetName: ANY_TARGET)]
```

An example of this would be an **upgrade/migration** pattern involving a legacy system which has some specific (possibly not consistent) 
naming pattern. You might need to interface with that older system and also extract a good consistent API contract for new customers. In this instance
a name like "LEGACY" may be used as a target name for the old system.

Target names are passed in from data stores - a concrete data store has `"TargetName"` property, so **all operations directed into that data store
would use metadata targeted at that use case** by its logical target name.

This design is NOT SQL-specific. Multi-targeting capabilities are as beneficial in general data contract modeling, NoSQL modeling, Service Layer/API modeling
and any kind of modeling in general.

You **should use target name constant values** which are typically declared in the very core ring (in terms of onion architecture) of your solution:
```csharp
  public static class Data
  {
    public const string LEGACY = "legacy";
    public const string LEVEL1 = "L1";
    public const string LEVEL2 = "L2";
    ...
  }
  ...
  public class Patient : TypedDoc
  {
    [Field(targetName: Data.LEGACY.....)]
    [Field(targetName: Data.LEVEL1.....)]
    [Field(targetName: Data.LEVEL2.....)]
    public int Prop { get; set; }
  }
```


## Target Derivation / Inheritance Pattern
Azos `FieldAttribute` has a constructor variant that implements an inheritance pattern which greatly **reduces the number of copious re-declarations**
of the same metadata. Let us look at the example:

```csharp
10  [Field(required: true, 
11         minLength: 5, 
12         description: "Name of insurance claim adjuster (agencies - use business names)")]
13  [Field("edi", null, MetadataContent = "segment='AM01' field='2Z'")]
14  [Field("mds", null, MetadataContent = "section='R' field='R1b'")]
15  public string ClaimAdjusterName {get; set;}
```

Line 10 declares a field targeting "ANY_TARGET" (target name is not set), notice the use of lower-case .ctor parameter references.
The subsequent declarations define new targets "edi" and "mds" each deriving (borrowing) all of the metadata from the "null" target
which references line 10. Consequently, `Field` entry for "mds" would contain all metadata declared on line 10 with additional overrides
declared on line 14. Notice the use of upper-case property accessors (e.g. `MetadataContent=...`) as the target derivation .ctor does not 
take any other parameters.

The derivation process works only on the SAME field in the SAME document. If you need to borrow metadata from another document then look at the 
 cloning and prototyping section.

#### Multilevel derivation chain

You can create chain inheritance patterns, for example this is used for multi-level document validation:
```csharp
  [Field]
  [Field("LEVEL1", null, Required = true)]
  [Field("LEVEL2", "LEVEL1", MinLength = 10, MaxLength = 200)]
  [Field("LEVEL3", "LEVEL2", MaxLength = 30)]
  public string MyField { get; set;}
```
Notice that every subsequent level borrows all attributes from the prior one, which in turn borrows it from its prior one etc...
Now we can perform validation targeting specific level like so:
```csharp
  var errors = doc.Validate("LEVEL3"); 
```

The **cyclical references** are not allowed *(detected and thrown at runtime)*.


#### Structural merges
Derivation becomes even more appreciated when one needs to override/merge/mix-in complex structured values. This is used for `ValueList` and 
`MetadataContent` properties. Lets look at the example:
```csharp
 [Field(valueList: "a|apl: apple, b: borland, m: microsoft")]
 [Field("newAge","*", ValueList = "a: #del#, i: ibm")]// only inherited [apl, b,m] remain in the list, [i] is added => [apl, b,m,i]
 public string Hardcoded{ get; set;}
```
The second field declaration erases "a" key *(but keeps its alias "apl")* and adds "i" for "ibm" value. The keys are deleted using "#del#" reserved value.
As a side note, notice that the second line references the first one by `ANY_TARGET` moniker '*' (you could pass null instead).
Any existing keys get replaced by newer once (overridden).

The similar process applies to **structural merges of custom metadata**, only custom metadata merges can be controlled with `_override` parameters
as they are implemented using standard [Azos Configuration Patterns](/src/Azos/Conf). For example:

```csharp
 [Field(metadata: "a=1 b=2")]
 [Field("ANOTHER", null, MetadataContent = "a=-21 subsection{ x=123 }")]
 public bool? Myflag{ get; set;}
```
The resulting metadata for "ANOTHER" target would look like this:
```
  meta
  {
    a=-21 //notice that a was overridden in the second attribute
    b=2   //be inherited from the first attribute
    subsection
    { 
      x=123
    }
  }
```
Refer to [Azos Configuration Documentation](/src/Azos/Conf) for more details.

See [Target Inherit Unit Tests](/src/testing/Azos.Tests.Nub/DataAccess/FieldAttrTargetInheritTests.cs) for more details.


## Cloning and Prototyping Patterns

Many application have variations of similar models. An example of that would be user data document which contains name, email, 
and password hash fields, however a user registration form contains all of those fields but not the ones for password - instead of password hash and salt, 
the form contains 2 password fields for entry during registration.

The first pattern is called field clone. It **borrows ALL field attributes from one data document into another**, the field property names have to be the same,
for example:
```csharp
  //in DocumentA class:
  [Field(...lots of metadata..)]
  [Field(...lots of metadata..)]
  public string MyData { get; set; }

  //in DocumentB class:
  [Field(typeof(DocumentA))] //<--- would clone everything (all field attributes) from DocumentA.MyData
  public string MyData { get; set; }

```
See [Attribute Cloning Unit Tests](/src/testing/Azos.Tests.Nub/DataAccess/FieldAttrCloningTests.cs) for more details.

The second pattern is more advanced. You can specify clone-from type and prototype field name and then partially override values:
```csharp
  //in DocumentA class:
  [Field(...lots of metadata..)]
  [Field(...lots of metadata..)]
  public string MyData { get; set; }

  //in DocumentB class:
  [Field(typeof(DocumentA), "MyData", maxLength: 30)] //<--- would clone from DocumentA.MyData/ANY_TARGET 
  [Field(typeof(DocumentA), "targetX:MyData", required: false)] //<--- would clone from DocumentA.MyData/targetX 
  public string MyData2 { get; set; }
```
You can also specify prototype target name using "protoTargetName:protoFieldName"

See [Attribute Prototyping Unit Tests](/src/testing/Azos.Tests.Nub/DataAccess/FieldAttrPrototyping.cs) for more details.


## Custom Metadata
Business applications often need to store business-specific metadata attributes which can be as simple as a list of named variables **or** as complex as a 
hierarchical structure with variables at different levels. 

Azos stores custom information in `MetadataContent` string property of `Field` attribute, and provides a `Metadata` accessor which is a root configuration 
object of `IConfigSectionNode`, consequently metadata is a configuration content.

Here are a few examples:
```csharp
public class SegmentAbc : ClaimSegment
{
  . . . 
  [Field(targetName: EDI, //this is for EDI processor
         backendName: "9Z", //field is called '9Z' in EDI data stream
         metadata: @"fmt='counter' mode='strict' pub{ legacy-lbl='Code Request Overrides'}")] 
  public List<CodeRequestItem> Codes{ get; set; } 
  . . . 
}  
```
The metadata content is inlined and you can get it in code like so:
```
  var schema = Schema.GetForTypedDoc<SegmentAbc>();
  var defCodes = schema["Codes"];
  var edi = defCodes[EDI];
  
  Conout.Writeline(edi.Metadata.Navigate("$fmt").ValueAsString()); // counter
  Conout.Writeline(edi.Metadata.Navigate("$mode").ValueAsString()); // strict
  Conout.Writeline(edi.Metadata.Navigate("pub/$legacy-lbl").ValueAsString()); // Code Request Overrides
```
In practice the above snippet was taken from government-regulated data message which gets serialized as EDI or XML, the EDI target tells the system how to call the field in the EDI stream and how to handle partial matches.

By convention, the metadata residing under `'pub'` is harvested and returned by the **ApiDocumentation Generator**, this is done on purpose not to leak/disclose sensitive data. The name of the public section may be changed at the ApiGenerator class. 



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

## Value Lists
Value lists represent a set (as the name implies) of applicable field values.
Value lists have finite number of typically hard-coded (by declaration) choices, such as codes, enums etc.
Value lists are typically used as a source of drop-down choices in UI controls (views).
The value list gets consulted with by `doc.Validate()`: a field value may not be present in an
acceptable `valueList` set - this is a validation error.

You should not use value lists for indefinite number of values such as values obtained from database
of unknown length - these cases need to be validated using custom logic (via module dep injection).
Value lists contain a well-defined small set of values, typically less than 20 values.

> We have used this system with 100+ hard-coded 3-char codes for government-defined forms just fine *(e.g. NCPDP Claim Data)*

There are two ways of specifying value lists for fields: **declarative** and **imperative**.

**Important:** both ways of obtaining field value lists support multi-targeting - that is: you can return different values depending on 
a target of applicability *(see multi targeting above)*

**Declaratively**, you specify acceptable value lists using `[Field]` attribute:
```csharp
  [Field(valueList: "a|apl: apple, b: banana")]
  public string Fruit....
```

The list is a string having its items delimited by either `","` or `";"`. The `key:values` are delimited by `":"`. Extra spaces is trimmed.
You can provide alternate keys using `"|"` sub delimiter - notice the use of `"|"` pipe in the above key specification which is
analogous to re-declaring the entry with the same description more than once using different keys. 
This is very useful for hard-coded form codes like: `1|01: Normal benefit coverage span` *(e.g. in EDI standard)*

**Imperatively**, you can get value list from logic/modules which typically get the values from database.
To do this, you can override data `Doc` method:

```csharp
 [Inject] ILookupLogic m_Lookups;
 ...
 public override JsonDataMap GetDynamicFieldValueList(Schema.FieldDef fdef, string targetName, string isoLang)
 {
  if (fdef.Name=nameof(MyField)) 
    return m_Lookups.GetMyFieldValueList(targetName, isoLang);
  else
    return base.GetDynamicFieldValueList(fdef, targetName, isoLang);
 }
```














See also:
- [Data Access Overview](readme.md)
- [Metadata](metadata.md)
- [Data Validation with Domains](domains.md)
- [Data Modeling](modeling.md)

Back to [Documentation Index](/src/documentation-index.md)
