# Domain Data Validation
Back to [Documentation Index](/src/documentation-index.md)

See also:
- [Data Access Overview](readme.md) 
- [Data Schema / Metadata](metadata.md)
- [Data Modeling](modeling.md)


## Overview
The described approach uses [**relational schema modeling** ](modeling.md) described [here](modeling.cs).
Relational schema compiler understands **user-defined data type definition objects**, aka. **"domains"**.
Domains inherit form base class [`RDBMSDomain`](./Modeling/DataTypes/RDBMSDomain.cs) and dictate the
 limits applied and how DDL gets generated.

## Example
Example domain describing an enumerated type:
```CSharp
  public abstract class MyEnum : RDBMSDomain
  {
    public DBCharType Type;

    public int Size;

    public string[] Values;

    protected MyiEnum(DBCharType type, string values)
    {
      Type = type;
      var vlist = values.Split('|');
      Size = vlist.Max(v => v.Trim().Length);
      if (Size < 1) Size = 1;
      Values = vlist;
    }

    public override string GetTypeName(RDBMSCompiler compiler)
    {
      return Type == DBCharType.Varchar ? 
         "VARCHAR({0})".Args(Size) :
         "CHAR({0})".Args(Size);
    }

    public override string GetColumnCheckScript(RDBMSCompiler compiler,
                                                RDBMSEntity column, 
                                                Compiler.Outputs outputs)
    {
      var enumLine = string.Join(", ", Values.Select(v => compiler.EscapeString(v.Trim())));
      return compiler.TransformKeywordCase("check ({0} in ({1}))")
                  .Args(
                        compiler.GetQuotedIdentifierName(RDBMSEntityType.Column, 
                                                         column.TransformedName),
                        enumLine
                       );
    }
  }

//... inherit different enum-like types from MyEnum ...

  public class MyCommentType : MyEnum
  {
    public const int MAX_LEN = 8;

    public const string GENERAL = "GEN";
    public const string REVIEW = "REV";
    public const string QNA = "QNA";

    public const string VALUE_LIST = "GEN: General, REV: Review, QNA: Q&A";

    public static string MapDescription(string value)
    {
      if (value.EqualsOrdIgnoreCase(GENERAL)) return "General";
      if (value.EqualsOrdIgnoreCase(REVIEW)) return "Review";
      if (value.EqualsOrdIgnoreCase(QNA)) return "Q&A";
      return "Unknown";
    }

    public MyiCommentDimension() : base(DBCharType.Char, "GEN|REV|QNA")
    {
    }
  }
```

We have gathered all data consistency rules for the domain (data type) in one place.
Now all places in code and the database use the same source of master definition. For example,
this is how a field is declared in the data doc now:
```CSharp
  // Value list is taken right from the DOMAIN definition
  [Field(valueList: Domains.MyCommentType.VALUE_LIST)]
  public string CommentType { get; set;}
```


We can now use this domain directly in the RSC scripts like so:
```
  table=Comment
  {
    comment="Holds user comments"
    ...
    column=CommentType { type=MyCommentType required=true  comment="Comment type" }
  ......
```

## Compiler Configuration and Scripting

To enable this behavior we need to supply `rsc` compiler with the assembly declaring
the custom domain definitions so custom domain types can be discovered at compiler runtime:

```batch
  set DOMAINS="MyApp.Data.Domains.*, MyApp; Azos.Data.Modeling.DataTypes.*, Azos"
  rsc my-customer.rschema /o out-name-prefix="cust." domain-search-paths=%DOMAINS%
  rsc my-vendor.rschema /o out-name-prefix="vnd." domain-search-paths=%DOMAINS%
```

You can use the following template script for complex Mdb model setup:
```batch
@echo off

SET PROJECT_HOME=%YOUR_PROJECT_NAME_HOME%
SET LAST=%PROJECT_HOME:~-1%
IF %LAST% NEQ \ (SET PROJECT_HOME=%PROJECT_HOME%\)

set MYAPP_HOME=%PROJECT_HOME%MYAPP\
set AZOS_HOME=%PROJECT_HOME%AZOS\
set RSC_EXE="%MYAPP_HOME%out\Debug\run-netf\rsc.exe"
set DOMAINS="MyApp.Data.Domains.*, MyApp; Azos.Sky.Social.Graph.Server.Data.Schema.*, Azos.Sky.Social; Azos.Data.Model.DataTypes.*, Azos"

echo Building CENTRAL Scripts ---------------------------------------------------
%RSC_EXE% "%MYAPP_HOME%src\db\Mdb\my-central-00-all.rschema" /o out-name-prefix=my.central. domain-search-paths=%DOMAINS%
if errorlevel 1 goto ERROR

echo Building USER Scripts ------------------------------------------------------
%RSC_EXE% "%MYAPP_HOME%src\db\Mdb\my-user-00-all.rschema"    /o out-name-prefix=my.user. domain-search-paths=%DOMAINS%
if errorlevel 1 goto ERROR

echo Done!
goto :FINISH

:ERROR
echo Error happened!
:FINISH
pause
```

## Domains and RDBMS Domains

[`RDBMSDomain`](./Modeling/DataTypes/RDBMSDomain.cs) inherits from [`Domain`](./Modeling/DataTypes/Domain.cs) as it provides additional rdbms-specific services:
```CSharp
//
// Summary:
//     Represents a domain - named type with optional constraints/checks for permitted
//     values
public abstract class RDBMSDomain : Domain
{
  protected RDBMSDomain();

  //
  // Summary:
  //     Returns script for auto-generated values, may also emit compiler-specific object
  //     like sequence or generator
  public virtual string GetColumnAutoGeneratedScript(RDBMSCompiler compiler, RDBMSEntity column, Compiler.Outputs outputs);
  //
  // Summary:
  //     Returns script for check constraint on column level
  public virtual string GetColumnCheckScript(RDBMSCompiler compiler, RDBMSEntity column, Compiler.Outputs outputs);
  //
  // Summary:
  //     Returns script for default values, may also emit compiler-specific object like
  //     sequence or generator or insert rows in some other table
  public virtual string GetColumnDefaultScript(RDBMSCompiler compiler, RDBMSEntity column, Compiler.Outputs outputs);
  //
  // Summary:
  //     Returns true to indicate that column of this type is always required
  //
  // Parameters:
  //   compiler:
  //     The context that the result depends on
  public virtual bool? GetColumnRequirement(RDBMSCompiler compiler);
  //
  // Summary:
  //     Returns the name of the resulting type that this domain maps to
  //
  // Parameters:
  //   compiler:
  //     The context that the result depends on
  //
  // Returns:
  //     Target type name, i.e. BIGINT, DECIMAL(8,2) etc...
  public abstract string GetTypeName(RDBMSCompiler compiler);
  //
  // Summary:
  //     Changes column name, i.e. adds prefix
  public virtual void TransformColumnName(RDBMSCompiler compiler, RDBMSEntity column);
}
```



See also:
- [Data Access Overview](readme.md)
- [Data Schema / Metadata](metadata.md)
- [Data Modeling](modeling.md)

Back to [Documentation Index](/src/documentation-index.md)
