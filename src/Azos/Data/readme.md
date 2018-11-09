# Azos Data Access
This section described accessing/working with Data in Azos.

## Overview
Azos takes a hybrid approach to data access. It is not a strict ORM or strict CRUD, rather a 
combination of different techniques that are most beneficial for business applications. The ORM
part may be better described as **just OM (Object Mapping)** without the [R]elational part - this is because 
it is necessary to support **non-relational/RDBMS** data sources (e.g. services, NoSql, BigData etc.) as easily
as the traditional ones.

Azos data modeling introduced with **schema/data documents** provides foundational metadata for serialization
(including version-tolerant)(JSON/BSON/Arow), inter-process communication with Glue and Sky net (Todo, Processes) and
general per-document/per-field metadata for [business domain validation rules / class invariants](https://en.wikipedia.org/wiki/Class_invariant).

Azos data access was designed with the following data-store types in mind:

* **Distributed/Api** sources (i.e. various web services/API sources)
* **BigData** (e.g. Hadoop, Hive/HQL etc.)
* **Relational** (Sql: MsSQL, MySQL, ORACLE etc.)
* **NoSql**: Document and others (MongoDB, Riak, tuple spaces etc.)
* **Unstructured** data accessed via custom APIs(parse CSV files etc.)
* **Non-homogeneous** data: all/some of the aforementioned sources may be needed in the same system

The data access is facilitated via the [`Azos.Data.Access.IDataStore`](Access/IDataStore.cs) interface which is just a 
marker interface for the application container (accessible via  a [`Azos.App.DataStore`](../App.cs#L152) shortcut).

Every system may select a combination of the following strategies that fit the particular case the best:

* Calling 3rd party services (e.g. via REST/SOAP/RPC) - pulling data via some API calls
* Read/Write some data via app-specific APIs (classes/props/methods) - similar to ORM
* Work with data via CRUD facade (i.e. DataStore.Insert(new Doc{......}) - similar to the [**Active Record**](https://en.wikipedia.org/wiki/Active_record_pattern) pattern/Entity framework
* Work with higher-level facade to any of the aforementioned ways

## Building Blocks: Docs, Schema, FieldDefs

Any POCO (Plain CLR) class instance may be used to access data, as data stores are just interfaces, 
for example:  `MyCar car = MyApp.Data.GetCarsByDriver("Frank-123");`, as the function in the preceding 
example may return a domain object `MyCar`.

Azos.Data namespace provides a very convenient base for business logic/domain models
 building blocks, which are also typically used in a CRUD scenarios:

* `Azos.Data.Schema` - defines "schema" - a field structure with constraints and other metadata for **data documents**
* `Azos.Data.Doc`/`DynamicDoc`/`TypedDoc` - represent data vectors aka "**data documents**". Documents are either flat (a la traditional RDBMS rows), or contain hierarchical data (like Json objects)
* `Azos.Data.RowsetBase`/`Rowset`/`Table` - a set of data documents of the same schema (like a RDBMS table)
* `Azos.Data.Access.Query` - a [command object](https://en.wikipedia.org/wiki/Command_pattern) sent into the provider for execution

### Schema
[Schema](Schema.cs) defines the structure of data documents, it consists of FieldDef instances that define attributes 
for every field. Fields may be of complex types (e.g. a field of type `List<PatientDataDoc>`). So Schema basically shapes the data
contained in Docs. See [Schema Metadata](metadata.md) for more details.

### Data Documents
A data document "[Doc](Doc.cs)" is a string of data, it consists of fields where every field is defined by a FieldDef from Schema.
Think of data doc as a classes directly or indirectly inheriting from `Doc`, where properties decorated with `[Field]` attribute get read/written 
from/to actual data store. 
**Documents can nest** - they **can contain fields of complex types**, thus forming hierarchical data structures. *You have to keep in mind though,
that for the best backend portability it is better to use linear row-like "data strings" (on-level rows with primitive types only).*

A `Schema` is a property of a `Doc`. `FieldDef` is a property of a field within a document. There are two types of data documents:

* Dynamic Data Documents
* Typed Data Documents

Dynamic documents are instances of [`DynamicDoc`](DynamicDoc.cs) class, they keep data internally in an `object[]`.
Typed rows are instances of sub-types of a [`TypedDoc`](TypedDoc.cs). The fields of typed doc must be explicitly 
declared in code and tagged with a [`[Field]`](Attributes.cs) attribute which defines field's detailed FieldDef.

This design is very flexible, as both doc types stem from `Doc` abstract class, which has the following key
features:
```CSharp
    Doc person = new DynamicDoc(Schema.GetForTypedDoc(PersonDoc));
    person[0] = 123;
    Aver.AreEqual(123, person["id"]);
    person["name"] = "Frank Drebin";
    var error = person.Validate(); //ensure metadata invariants
    Aver.IsNull(error);
    ...
    var person2 = new PersonDoc();//no schema needs to be passed as it is a typed doc
    person.CopyTo(person2);//copies all fields
    ...
```

Docs may override `Validate()` method to **ensure cross-field constraint adherence**.
See [Schema Metadata](metadata.md)

### Rowset
Rowsets are collections of data documents of the same schema - this is used for accessing table-like data consisting of rows - hence the name.
There are two types both inheriting from [`RowsetBase`](RowsetBase.cs):

* [Rowset](Rowset.cs)
* [Table](Table.cs)

The difference between the two is the presence of the primary key in the `Azos.Data.Table`
which allows for **efficient in-memory merges/findKey()** calls, consequently table is not for dynamic sorting.
It is a pk-organized list of rows of the same schema. The findKey() is done via a binary search.

[`Azos.Data.Rowset`](Rowset.cs) does not have this limit - it **allows to sort the data**, however the 
findkey() calls do linear search (which is slow akin to LINQ's `Where(predicate)`).

An interesting feature of rowsets is their ability to mix Dynamic and Typed documents instances in one list
as long as their schemas are the same (referentiality).

Rowsets can track changes, if `RowsetBase.LogChanges=true`, then RowChange enumerable can be obtained 
via `Rowset.Changes` property. The concept is somewhat similar to .NET's DataSet, BUT there is a 
**key difference** in the approach: Azos Data Access is for accessing **any data, not only relational**.

### Virtual Query

Queries are [command objects](https://en.wikipedia.org/wiki/Command_pattern) that group parameters under some name. 
The queries are polymorphic (virtual), that is: the backend provider (DataStore-implementor) is responsible for 
*query-to-actual-handler* resolution. Basically this treats data store requests as NAMED service calls (commands), thus decoupling
the logical query (as a data need request) from the actual implementation.

There are two types of [query handlers](Access/CRUDQueryHandler.cs):

* Script QueryHandler
* Code Query Handler

This design allows for an infinite flexibility, as script queries **may be written in backend-specific 
scripting technology**, i.e.:
```CSharp
  var qry = Query("GetUserById"){ new Query.Param("UID", 1234)};
    
  //for My SQL, will get resolved into    
   SELECT T1.* FROM TBL_USER T1 WHERE T1.ID = ?UID
    
  //For MongoDB
    #pragma
    modify=user
    
    {"_id": "$$UID"}}
    
  //For Erlang MFA(module function arg)
    nfx_test:exec_qry(get_user_bid, Uid:long())
```
For script queries the backend implementation choses the embedded script based on technology-specific suffix, this way
a script assembly may multi-target different data backends (such as Oracle or MySql) from the same application.

In cases where a certain backend can not execute a given request, a handler can be implemented in CLR code. For example,
suppose we use `CONNECT BY` in ORACLE version of the script, then we need to support a less-capable data store which
does not support `CONNECT BY` directly, but can emulate the same result with multiple DB calls. A code-based handler
can execute those calls into the data store returning the same logical result as ORACLE's `CONNECT BY` would.

See [Azos.Tests.Integration](/src/testing/Azos.Tests.Integration/CRUD) for more use-cases.


### CRUD Data Store

[`Azos.Data.Access.Intfs.cs`](Access/Intfs.cs) contains the definitions of `ICRUDOPerations` which stipulate the contract 
for working in a CRUD style:

```CSharp
  /// <summary>
  /// Describes an entity that performs single (not in transaction/batch)CRUD operations
  /// </summary>
  public interface ICRUDOperations
  {
    /// <summary>
    /// Returns true when backend supports true asynchronous operations, such as the ones that do
    /// not create extra threads/empty tasks
    /// </summary>
    bool SupportsTrueAsynchrony { get;}

    Schema        GetSchema(Query query);
    Task<Schema>  GetSchemaAsync(Query query);

    List<RowsetBase>        Load(params Query[] queries);
    Task<List<RowsetBase>>  LoadAsync(params Query[] queries);

    RowsetBase        LoadOneRowset(Query query);
    Task<RowsetBase>  LoadOneRowsetAsync(Query query);

    Doc        LoadOneRow(Query query);
    Task<Doc>  LoadOneRowAsync(Query query);

    Cursor        OpenCursor(Query query);
    Task<Cursor>  OpenCursorAsync(Query query);

    int        Save(params RowsetBase[] rowsets);
    Task<int>  SaveAsync(params RowsetBase[] rowsets);

    int        ExecuteWithoutFetch(params Query[] queries);
    Task<int>  ExecuteWithoutFetchAsync(params Query[] queries);

    int        Insert(Doc row);
    Task<int>  InsertAsync(Doc row);

    int        Upsert(Doc row);
    Task<int>  UpsertAsync(Doc row);

    int        Update(Doc row, IDataStoreKey key = null);
    Task<int>  UpdateAsync(Doc row, IDataStoreKey key = null);

    int        Delete(Doc row, IDataStoreKey key = null);
    Task<int>  DeleteAsync(Doc row, IDataStoreKey key = null);
  }
```

This way of working with data backend is similar to the [**"Active Record"**](https://en.wikipedia.org/wiki/Active_record_pattern) pattern.

An example use case:

```CSharp    
  var person = new PersonDoc
  {
    ID = MyApp.Data.IDGenerator.GetNext(typeof(PersonRow)),
    Name = "Jon Lord",
    IsCertified = true
  };
  
  MyApp.Data.Upsert(person);
```
    
Or a typical case of use with Azos.Wave.Mvc Web Api:

```CSharp
  [Action("person", 1, "match{ methods='GET' accept-json='true'}"]
  public object GetPerson(string id)
  {
      return MyApp.Data.LoadOneRow(Queries.PersonById(id));
  }
      
  [Action("person", 1, "match{ methods='POST' accept-json='true'}"]
  public object PostPerson(Person person)
  {
    var err = person.Validate();
    if (err!=null)
        return new {OK=false, Err = err.Message};//Or throw HttpStatus code exception
            
    MyApp.Data.Upsert(person);
    return new {OK=true};
  }
```

As illustrated above, the Azos.Wave framework performs **data document injection into the Mvc actions** - a form
of automatic data binding, which is very convenient in CRUD applications.

See also:
- [Schema Metadata](metadata.md)


