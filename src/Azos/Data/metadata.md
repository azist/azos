# Data Schema Metadata
This section described accessing/working with Data in Azos.
See also [Data Access Overview](readme.md) [Data Validation with Domains](domains.md)

## Overview
`Azos.Data` namespace implements data `Schema` class which provides definitions for data documents. This is 
used as a metadata (data about data) of **data documents** which are used by many sub-systems of Azos 
framework such as (not a complete list):
* "Database Rows" - work with RDBMS systems (e.g. Oracle, Ms Sql Server, My Sql etc.)
* Documents - work with NoSql hierarchical data a la Mongo Db, Erlang Mnesia, Json
* Service Payload - consume and expose data from/to web services/REST APIs
* JSON serialization - data documents are JSON-serializable
* BSON serialization - data documents are BSON-serializable (binary JSON)
* Arow serialization - high performance version-aware binary serialization
* Various version-tolerant system contracts such as:
  * Worker Todos
  * Processes
  * Protocol Frames

**COMPARISON** 
The following projects/frameworks use "schema" concept to decorate CLR type members:
* Libraries like Protobuf.Net or Thrift use special schema definitions to "shape" data
* .Net DataContractSerializer uses its own annotations
* .Net Data Annotations provide metadata for data validation

Azos data documents (based on schema) **provide UNIVERSAL data-definition layer** which is used for all of the aforementioned tasks.
*There is no need to use additional annotation libraries with Azos*. In cases when built-in field attributes
are not sufficient (e.g. when one needs to store custom metadata per field), you can use custom config vector per field
which allows for any number/structure of custom config vectors attached to very field.



## Schema
[Schema](Schema.cs) provides definition of data contained in **data documents** aka. [`Doc`](Doc.cs) instances.
Every field definition in schema is represented by an instance of [`FieldDef`](Schema.cs#L37) class.

Schemas are **named immutable objects**. The following diagram depicts schema as a composite of `FieldDefs` each having
one or more **targeted attributes** which describe metadata. Lets compare "ORACLE" vs "MySQL-Legacy" below:
* Logical field "Name" is called "name" for "ORACLE" and "nm" for "MySQL-Legacy" targets
* See that min/max string sizes are different for these targets
* The field "DOB" is kept as "date_birth" for both targets (see "Target: *")

**Note:** targets may represent not only a particular backend technology but also a different backend
version/structure, as in the example below, older system has slightly different schema

<img src="/doc/img/data-schema.png" height="500px">

 You can create an arbitrary schema like so:
```CSharp
  var schema = new Schema("TEZT",
       new Schema.FieldDef("ID", typeof(int), new []{ new FieldAttribute(required: true, key: true)}),
       new Schema.FieldDef("Description", typeof(string), new []{ new FieldAttribute(required: true)})
);
```

or you can get a schema for any of `TypedDoc` derivatives which "shape" data explicitly - by constitution. An important
distinction between ad-hoc and typed schemas - the typed schema instances are pooled per type, so
if you obtain the schema **for the same type you will get the same instance for all calls**.

```CSharp
  [Serializable]
  public class MyType : TypedDoc
  {
    public MyType() { ... }

    [Field(required: true, key: true)]
    public string ID {get; set;}

    [Field(required: true, key: true)]
    public DateTime StartDate {get; set;}

    [Field(required: true)]
    public string Description {get; set;}
  }

 ...

 //The returned schema object instance is POOLED for each type
 var schema = Schema.GetForTypedDoc<MyType>();

 foreach(var def in schema)
 {
   ...
 }
```

## Field Attribute

The [`FieldAttribute`](Attributes.cs#L159) class is the cornerstone of metadata specification. Public class properties must
be decorated by one or more (in case of multi-targeting) `[Field]` attributes. 

The following table describes `FieldAttribute` properties (that is: metadata for fields in data documents):

 Name |Type| Description
 ---- |----|-----------
 storeFlag |StoreFlag| Specifies whether field is skipped, stored, loaded or stored and loaded
 key |bool| True to indicate that this field is (a part of) primary key
 kind |DataKind| Specifies data kinds such as: `Text, ScreenName, Color, Date, DateTime, DateTimeLocal, EMail, Month, Number, Range, Money, Search, Telephone, Time, Url, Week`
 required|bool| True to indicate that field must be non-null value
 visible|bool| True indicates that field is shown on UI (used for MVVM)
valueList|string| A delimited list of permissible lookup values (lookup dictionary)
dflt|object|Default field value (if any)
min|object|Minimum permissible value or null
max|object|Maximum permissible value or null
minLength|int|Minimum data length, 0 = no limit
maxLength|int|Maximum data length, 0 = no limit
charCase|CharCase|Defaults to `AsIs`. CharCases are: `AsIs, Upper, Lower, Caps` *The first and subsequent chars after space or '.' are capitalized, the rest left intact*, `CapsNorm` - *The first and subsequent chars after space or '.' are capitalized, the rest is lower-cased*
backendName|string|Specifies how this field is called in the backend, e.g. a field/property `DateOfBirth` may be called `dob` in the database
backendType|string|Defines db-specific (per target) type for the field, e.g. "binary image"
description|string|Textual description usually shown as data entry field caption
metadata|string|Laconic configuration vector providing extra ad-hoc metadata for the decorated field
nonUI|bool|Set to True to exclude the field from transportation to client side (such as Web UI); this is used to not send sensitive/internal data to the client
formatRegExp|string|When set, contains regular expression that validates the data
displayFormat|string|When set, provides a mask for .ToString()/display on UI
isArow|bool|True to include the field in Arow serialization

These properties are usually set directly at the document properties declaration site like so:
```CSharp
...
  [Field(required: true,
         kind: DataKind.ScreenName,
         charCase: CharCase.Upper,
         minLength: Domains.MyiScreenName.MIN_LEN,
         maxLength: Domains.MyiScreenName.MAX_LEN,
         description: "Screen Name",
         metadata: @"Placeholder='Screen Name'
                     Hint='The value of this field uniquely identifies you in the system'")]
  public string Screen_Name { get; set; }

  [Field(required: true,
         kind: DataKind.EMail,
         maxLength: Domains.MyiEMail.MAX_LEN,
         description: "Primary EMail",
         metadata: @"Placeholder='Primary EMail'
                     Hint='E Mail that you use most often. We are not sharing your email with anyone!'
                     ControlType='Text'")]
  public string Primary_EMail { get; set; }
...
```
Notable features above: the min/max length checking is isolated into `Domains` namespace (see [Data Validation with Domains](domains.md)),
also **custom metadata is used** to introduce `Placeholder`, `Hint`, and `ControlType` extra attributes which are used by the
client-side view system.

## Form Models
There are many cases when multiple models are needed around the same data source. A good example would be a "UserRow" of data containing 
fields for password hash, however users enter ID and password twice. The password hash gets computed and stored, the password fields are not 
stored at all.

To facilitate UI/consumer-driven modeling (vs backend/domain-driven) Azos provides [`Form`](Form.cs) class that signifies the purpose - **forms
are used as a projection of data documents to provide an alternate view of data** (usually for user entry). Forms are data documents, the difference
is in the intent.

When you declare a form you usually clone many fields from existing data doc model. The copious field declaration would have been prohibitively
  inconvenient to say the least. Azos provides a **prototypical metadata cloning pattern** for solving the issue as illustrated:
```CSharp
//We are in another doc or form, e.g. UserRegistrationForm
...
  [Field(typeof(UserRow))] //Take all metadata from the field with the same name in UserRow class
  public string Screen_Name { get; set; }
  
  [Field(typeof(UserRow))]
  public string Primary_EMail { get; set; }
...
```
The example above is based on the prior one. We have just brought in 2 fields from a different schema - we **cloned all field metadata** so
we do not need to repeat it here again.

If you need to make some changes to metadata in a proto-cloned field, you can do so by specifying those override on the declaration level:
```CSharp
  //We are going to override backendName and required constraint
  //take the rest of definition from UserRow.Lazzzt_Name
  [Field(typeof(UserRow), "Lazzzt_Name",  backendName: "abra", required: false)]
  public string Last_Name { get; set; }
```







See also:
- [Data Access Overview](readme.md)
- [Data Validation with Domains](domains.md)