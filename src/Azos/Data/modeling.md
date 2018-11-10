# Azos Data Modeling
Back to [Documentation Index](/src/documentation-index.md)

Azos provides a built-in abstraction for declaring data models - **a relational schema** configuration
language. 

The modeling data types are defined in the [Azos.Data.Modeling](./Modeling) namespace.

The idea is to script the model using hierarchical config file, nodes representing tables with child columns
 and constraints. The RSC tool (described below) takes these files and generates DDL (Data Definition Language)
for the particular backend technology taking care of nuances such as auto-inc keys and other syntactic differences.
It is also possible to generate code (e.g. CSharp classes) for data documents directly from the schema definitions.

<img src="/doc/img/rsc-1.png">


This rest of this article describes the relational schema compiler tool.

---

The purpose of **Relational Schema (RS)** is to abstract away the declarative (such as DDL) database 
design nuances. The syntaxes for the majority of RDBMS solutions (i.e. compare Microsoft SQL and 
ORACLE) are somewhat similar yet, different. For example: all of the modern databases support a 
concept of auto-incremented columns/fields (IDs/Counters) which are usually employed as non-intelligent
primary keys; yet, the mechanics of their declaration is different (i.e. IDENTITY columns in MsSQL,
sequences in ORACLE etc.). 

The **Relational Schema Compiler (RSC)** tool takes a universal definition of database schema expressed 
as a set of *DSL configuration scripts*, which are easy to create and maintain (e.g. diff via SCM), 
and turns it into a target-specific DDL (Data Definition Language). The targeting is done via an 
injectable compiler using a `-c|-compiler` switch. The default compiler is `MySQLCompiler`
(used when no other compiler type is specified).

The main advantage of using Azos Relational Schema over other tools (i.e. Entity Framework Code First)
is that this approach allows for **multi database targeting within the same application code base**
as Azos allows for model [`Field`](Field.cs) attribute multi-targeting and virtual queries against hybrid data 
stores. Please see the [Data Access](/readme.md) topic. 

The `rsc` is usually included in the build script directly or via Ms Build `exec` step.


```css
Usage:
   rsc schema_file [/h | /? | /help]
              [/c | /compiler  fully_qualified_type_name]
              [/options | /opt | /o
                            [out-path= path]
                            [out-name-prefix= prefix]
                            [name-case-sensitivity= ToUpper|ToLower|AsIs]
                            [domain-search-paths= path[;pathX...]]
                            [separate-indexes= true|false]
                            [separate-fkeys= true|false]
              ]

schema-file - relational schema file


Options:

 /c | /compiler - a fully qualified compiler type name, if omitted MySQLCompiler is used
 /o | /options - specifies compiler options.

    out-path - output path, if omitted then input file path is used
    out-name-prefix - prefix gets appended to every out file name
    name-case-sensitivity - when AsIs, indicates that object names are case sensitive, 
      so they must be kept kept as-is, otherwise applies ToUpper or ToLower transform
    domain-search-paths - RDBMS only option, a ";" delimited list of assembly-qualified 
      namespaces paths with '.*' wildcard that should be searched for domain type names
    separate-indexes - RDBMS only option, write create index in a separate output
    separate-fkeys - RDBMS only option, write all foreign keys in a separate output using
      ALTER TABLE ADD CONSTRAINT...

Examples:

  rsc "c:\db\Doctor.rschema"
      -options
          out-name-prefix=MyProject
          domain-search-paths="MyProject.DataTypes;MyWeb.Domains" 
Compiles "doctors" schema using MySQLCompiler and prepends all output file names with "MyProject". 
Also specifies namespaces used for domain type lookup
```

## Relational Schema Language

The language is based on Laconic configuration format and abides by all configuration framework rules.
Refer to Laconic Configuration documentation.
The configuration tree has the following general structure:

```cs
schema
{
  include="file-name"{}
  script-include="file-name"{}
  script-text="verbatim text to include in the output"{}

  table=TABLE_NAME
  {
    short-name=TABLE_SHORT_NAME
    comment="Entity comment text"
    script-comment="Comment text to be placed in script"
    column=COLUMN_NAME
    {
      short-name=COLUMN_SHORT_NAME
      type=DOMAIN_NAME(.CTOR_PARAMS){DOMAIN_CONFIG_TREE}
      required=true|false
      default=value
      primary-key=NAME{OPTIONS}
      reference=NAME{table=REF_TABLE_NAME column=REF_COLUMN_NAME}
    }

    index=INDEX_NAME
    {
        unique=true|false
        column=COLUMN_NAME { order=asc|desc length=int}
    }
  }
}
```

## The Output

The RSC generates the appropriate scripts (i.e. SQL files for RDBMS targets). Here is an example table
`ShipCode` expressed in RS DSL, and resulting DDL generated by RSC tool for My SQL target.

```css
table=ShipCode
{
  comment="Code of product shipping, i.e. 'FRAGILE'"
  
  //Global ID
  _call=/scripts/gdid{}
  
  column=Vendor
  {
    type=$(/$TRequiredGDIDRef)
    comment="Vendor"
    reference{ table="Vendor" column=$(/$PK_COLUMN) }
  }
  
  column=ID { type=$(/$TShortMnemonic) required=true comment="ID of shipping code"}
  _call=/scripts/description{}
  column=Config{type=$(/$TConfigScript) required=true comment=""}
  _call=/scripts/in-use{}
  _call=/scripts/external-data{}
  index=uk{ unique=true column=Vendor{} column=ID{} }
}
```

gets compiled into My SQL DDL below, note the complex constraint naming performed by the RSC,
i.e. column 'Vendor' references table 'Vendor'.pk, get written as "G_VENDOR" BINARY(12) as specified
by the domain. The object casing is controlled by the RSC compiler. It is also possible to emit all 
indexes/constraints in a separate file.

```sql
-- Table tbl_shipcode
create table `tbl_shipcode`
(
 `GDID`           BINARY(12)     not null,
 `G_VENDOR`       BINARY(12)     not null comment 'Vendor',
 `ID`             char(8)        not null comment 'ID of shipping code',
 `DESCRIPTION`    varchar(80)     comment 'Description of this record',
 `CONFIG`         TEXT           not null,
 `IN_USE`         CHAR(1)        not null default 'T',
 `EXTERNAL_DATA`  TEXT            comment 'Attaches arbitrary user-specific external data',
  constraint `pk_tbl_shipcode_primary` primary key (`GDID`),
  constraint `fk_tbl_shipcode_vendor` foreign key (`G_VENDOR`) references `tbl_vendor`(`GDID`)
)
    comment = 'Code of product shipping, i.e. \'FRAGILE\''
;.

delimiter ;.
  create unique index `idx_tbl_shipcode_uk` on `tbl_shipcode`(`G_VENDOR`, `ID`);.
```



## Relational Schema Scripting

The following snippet declares a mixin called "names" which can be invoked from multiple places 
throughout the project:

```CSharp
schema
{
  PK_COLUMN="counter"
  scripts
  {
    script-only=true
    names
    {
        column=first_name  {type=THumanName  required=true}
        column=middle_name {type=THumanName }
        column=last_name   {type=THumanName  required=true}
    }
  }
}
```

Mixing-in the columns by invoking the mixin script `_call=/scripts/names{}` 

```CSharp
schema
{
  include="name-of-script-file"{}

  table=doctor
  {
     short-name="doc"
     comment="Holds information about licensed doctors"
     column=$(/$PK_COLUMN) {type=TCounter  required=true primary-key{} }
     column=NPI {type=TVarchar(24) required=true }
     _call=/scripts/names{}

     index=npi
     {
       unique=true
       column=NPI {}
       comment="Every doctor is uniquely identified by NPI"
     }
  }
}
```

See also:
- [Data Access Overview](readme.md)
- [Data Schema](metadata.md)
- [Data Validation with Domains](domains.md)

Back to [Documentation Index](/src/documentation-index.md)