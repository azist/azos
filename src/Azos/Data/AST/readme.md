# AST - Abstract Syntax Trees

In the context of `Azos.Data` library, abstract syntax trees represent ad-hoc expressions which are typically passed-in as a part of filter objects to APIs for advanced data queries. 
The concept is somewhat similar to GraphQL.

> **Warning:** Ad-hoc querying ability is an advanced feature which should be carefully designed in your application 
> and data source as opening your underlying data store (such as SQL) for *any kind of ad-hoc filtering may be a
> dangerous* and unneeded practice. The fields/columns to search on may not all be indexed. Special care must be taken to 
> ensure that clients can not bring servers down with complex queries. You should limit search-able column/field lists using Xlat 
> IndentifierLookup and limit supported tree operator types

An AST expression tree is built of series of nodes, all JSON-serializable. The nodes implement visitor design pattern which 
accepts the translation context `XlatContext`. Translators take a tree and transform it to another
form, such as SQL for RDBMS or a different query language (e.g. Mongo Db query graph or even external ws calls).

The shape of the tree, its operators and values depend on actual [`Xlat`](xlat.cs) implementation used to translate
the AST for a concrete backend. It is possible to re-interpret the same tree for different targets, such as SQL, 
NoSQL, Graph and other data store types.

The [`SqlBaseXlat`](SqlBaseXlat.cs) provides base implementation for SQL-related transforms.

You can override `UnaryOperators`, `BinaryOperators` to customize what operations are supported.
`IdentifierFilter` is an injectable `Func<IdentifierExpression, bool>` predicate that you can use to accept/reject
certain identifiers which get submitted in an AST, consequently an **AST translation is secure**.

An example of forming an AST tree by code:
```csharp
  var ast = new BinaryExpression
  {
    LeftOperand = new IdentifierExpression { Identifier = "Name" },
    Operator = "=",
    RightOperand = new ValueExpression { Value = "Smith" }
  };

  var xlat = new MySqlXlat();
  var ctx = xlat.TranslateInContext(ast);
  ctx.SQL ...
  ctx.Parameters ...
```

## Advanced Search API

Consider the following filter payload submitted to customer list API as an example:
```json
POST /customer/list  application/json
{
  "pagingCount": 75,
  "AdvancedFilter": {
     "Operator": "and",
     "LeftOperand": {
      "Operator": "=",
      "LeftOperand": { "Identifier": "FNAME"},
      "RightOperand": { "Value": "Joseph"}
    },
    "RightOperand": {
      "Operator": "=",
      "LeftOperand": { "Identifier": "LNAME"},
      "RightOperand": { "Value": "Appleman"}
    }
  }
}
```

A client POSTs a filter model (e.g. via MVC controller, GRPC call etc.) that contains a list of acceptable fields and an
advanced filter root which is an `Expression`:
```csharp
  //declares fields allowed in advanced filter operations
  public static readonly StringMap ADVANCED_SEARCH_FIELDS = new StringMap
  {
    {"FNAME",      "First Name"},
    {"LNAME",      "Last Name"},
    {"ADDR1",      "Address 1"},
    {"ADDR2",      "Address 2"},
    //more fields...
  };

  public static readonly string[] ADVANCED_SEARCH_FIELD_NAMES = 
    ADVANCED_SEARCH_FIELDS.Keys.ToArray();

  /// <summary>
  /// An optional complex expression tree; this filter is overlaid on top of other filter 
  /// fields if they are supplied
  /// </summary>
  [Field]
  public Azos.Data.AST.Expression AdvancedFilter { get; set; }
```

In data query handler, we use SQL translator (Oracle in this example):
```CSharp
  . . . 
  //declare Expression translator to Oracle SQL
  private static OracleXlat s_Xlat = new Azos.Data.AST.OracleXlat // from Azos.Oracle
  {
    IdentifierFilter = (id) => id.Identifier.IsOneOf(MemberListFilter.ADVANCED_SEARCH_FIELD_NAMES)
  };

  private bool tryFilterAdvanced(Expression expression, OraSelectBuilder builder)
  {
    if (expression == null) return false;

    //translate expression to SQL
    var ctx = s_Xlat.TranslateInContext(expression);

    //add expression as an extra Where block
    builder.WhereExpressionBlock(WhereClauseType.And, ctx);
    return true;
  }
```

Using a helper SQL builder method (Oracle is used just for example):
```csharp
public OraSelectBuilder WhereExpressionBlock(WhereClauseType clause, SqlXlatContext xlat)
  => WhereBlockBegin(clause, xlat.Parameters.Cast<OracleParameter>().ToArray())
            .OrWhere(xlat.SQL.ToString())
            .WhereBlockEnd();
```

