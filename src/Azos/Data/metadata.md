# Data Schema and Metadata
This section described accessing/working with Data in Azos.

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
The following projects/frameworks use schema:
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

<img src="/doc/img/data-schema.png">

Schemas are **named immutable objects**. You can create an arbitrary schema like so:
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