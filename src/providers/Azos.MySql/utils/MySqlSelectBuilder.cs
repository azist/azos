/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using MySqlConnector;

using Azos;
using Azos.Data.AST;
using Azos.Data.Business;

namespace Azos.Data.Access.MySql
{
  /// <summary>
  /// Provides shorthand extensions for building dynamic SQL for MySql
  /// </summary>
  public static class MySqlBuilderHelpers
  {
    /// <summary>
    /// Build Sql command and parameters from the shorthand fluent syntax
    /// </summary>
    /// <remarks>
    /// //Example query with multiple blocks
    /// cmd.BuildSelect(builder =>
    /// {
    ///    builder.Limit = 100;
    ///    builder.Select("T1.*").From("dbo", "TBL_USER", "T1");
    ///
    ///    builder.WhereBlockBegin(WhereClauseType.Or)
    ///           .AndWhere("T1.FIRSTNAME = ?FN1", new MySqlParameter("FN1", "Adam"))
    ///           .AndWhere("T1.FIRSTNAME != ?FN2", new MySqlParameter("FN2", "Amber"))
    ///           .WhereBlockEnd();
    ///
    ///    builder.WhereBlockBegin(WhereClauseType.Or)
    ///           .AndWhere("T1.LASTNAME = ?LN1", new MySqlParameter("LN1", "Adamson"))
    ///           .AndWhere("T1.LASTNAME != ?LN2", new MySqlParameter("LN2", "Amberman"))
    ///           .WhereBlockEnd();
    ///
    ///    builder.OrderByAsc("T1.LASTNAME")
    ///           .OrderByDesc("T1.FIRSTNAME");
    ///    builder.GroupBy("T1.FIRSTNAME");
    ///    builder.Having("T1.SCORE > ?SCORE", new MySqlParameter("SCORE", 12_000));
    ///});
    /// </remarks>
    public static MySqlCommand BuildSelect(this MySqlCrudQueryExecutionContext ctx,
                                           MySqlCommand command,
                                           Action<MySqlSelectBuilder> body)
    {
      command.NonNull(nameof(command));
      body.NonNull(nameof(body));
      using (var builder = MySqlSelectBuilder.For(ctx.DataStore.SchemaName.NonBlank("schema-name")))
      {
        body(builder);
        builder.Build(command);
      }
      return command;
    }

    /// <summary>
    /// Build Sql command and parameters from the shorthand fluent syntax
    /// </summary>
    /// <remarks>
    /// //Example query with multiple blocks
    /// cmd.BuildSelect(builder =>
    /// {
    ///    builder.Limit = 100;
    ///    builder.Select("T1.*").From("dbo", "TBL_USER", "T1");
    ///
    ///    builder.WhereBlockBegin(WhereClauseType.Or)
    ///           .AndWhere("T1.FIRSTNAME = ?FN1", new MySqlParameter("FN1", "Adam"))
    ///           .AndWhere("T1.FIRSTNAME != ?FN2", new MySqlParameter("FN2", "Amber"))
    ///           .WhereBlockEnd();
    ///
    ///    builder.WhereBlockBegin(WhereClauseType.Or)
    ///           .AndWhere("T1.LASTNAME = ?LN1", new MySqlParameter("LN1", "Adamson"))
    ///           .AndWhere("T1.LASTNAME != ?LN2", new MySqlParameter("LN2", "Amberman"))
    ///           .WhereBlockEnd();
    ///
    ///    builder.OrderByAsc("T1.LASTNAME")
    ///           .OrderByDesc("T1.FIRSTNAME");
    ///    builder.GroupBy("T1.FIRSTNAME");
    ///    builder.Having("T1.SCORE > ?SCORE", new MySqlParameter("SCORE", 12_000));
    ///});
    /// </remarks>
    public static MySqlCommand BuildSelect(this MySqlCrudQueryExecutionContext ctx,
                                           MySqlCommand command,
                                           IBusinessFilterModel filter,
                                           Action<MySqlSelectBuilder> body)
    {
      command.NonNull(nameof(command));
      body.NonNull(nameof(body));
      using (var builder = MySqlSelectBuilder.For(ctx.DataStore.SchemaName.NonBlank("schema-name")))
      {
        builder.Limit = filter.NonNull(nameof(filter)).PagingCount;
        body(builder);
        builder.Build(command);
      }
      return command;
    }
  }


  /// <summary>
  /// Facilitates the creation of dynamic SELECT statements for MySql
  /// </summary>
  public sealed class MySqlSelectBuilder : IDisposable
  {
    //the linear lookup structures work faster while having < 6..8 elements
    private static MySqlSelectBuilder s_Cache1;
    private static MySqlSelectBuilder s_Cache2;
    private static MySqlSelectBuilder s_Cache3;
    private static MySqlSelectBuilder s_Cache4;
    private static MySqlSelectBuilder s_Cache5;

    /// <summary>
    /// Provides a clean instance of the builder for the specified schema.
    /// You should dispose this instance once done using .Dispose() call which returns
    /// it to the internal cache pool. Use the 'using(var builder = MySelectBuilder.For("schema"){ }' pattern
    /// </summary>
    public static MySqlSelectBuilder For(string schemaName)
    {
      var instance =
        Interlocked.Exchange(ref s_Cache1, null) ??
        Interlocked.Exchange(ref s_Cache2, null) ??
        Interlocked.Exchange(ref s_Cache3, null) ??
        Interlocked.Exchange(ref s_Cache4, null) ??
        Interlocked.Exchange(ref s_Cache5, null);

      if (instance == null) instance = new MySqlSelectBuilder();
      instance.m_SchemaName = schemaName;

      return instance;
    }

#pragma warning disable CA1063 // Implement IDisposable Correctly
    void IDisposable.Dispose()
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
      Limit = 0;
      m_SchemaName = null;
      m_Statement.Clear();// StringBuilder uses internal pool to relive allocation pressure
      m_Select.Clear();
      m_From.Clear();
      m_Where.Clear();
      m_OrderBy.Clear();
      m_WhereLevels.Clear();
      m_CurrentWhereNodes = 0;
      m_GroupBy.Clear();
      m_Having.Clear();
      m_Parameters.Clear();//List: but does not trim excess so internal buffer gets cached

      Thread.MemoryBarrier();

      //push to cache
      if (null == Interlocked.CompareExchange(ref s_Cache1, this, null)) return;
      if (null == Interlocked.CompareExchange(ref s_Cache2, this, null)) return;
      if (null == Interlocked.CompareExchange(ref s_Cache3, this, null)) return;
      if (null == Interlocked.CompareExchange(ref s_Cache4, this, null)) return;
      Interlocked.CompareExchange(ref s_Cache5, this, null);
    }


    private MySqlSelectBuilder() { }//callers should use .For(schema) accessor

    private string m_SchemaName;
    private StringBuilder m_Statement = new StringBuilder();
    private StringBuilder m_Select    = new StringBuilder();
    private StringBuilder m_From      = new StringBuilder();
    private StringBuilder m_Where     = new StringBuilder();
    private StringBuilder m_OrderBy   = new StringBuilder();
    private StringBuilder m_GroupBy   = new StringBuilder();
    private StringBuilder m_Having    = new StringBuilder();
    private List<MySqlParameter> m_Parameters = new List<MySqlParameter>();

    private Stack<int> m_WhereLevels = new Stack<int>();
    private int m_CurrentWhereNodes;



    public string SchemaName => m_SchemaName;

    public int Limit { get; set; }


    public MySqlSelectBuilder Select(string dbExpression, string alias = null)
    {
      dbExpression = dbExpression.NonBlank(nameof(dbExpression)).Trim();

      var hasContent = m_Select.Length > 0;

      if (hasContent) m_Select.Append(", \n");

      m_Select.Append(quote(dbExpression));

      if (alias.IsNotNullOrWhiteSpace())
        m_Select.Append($" AS \"{alias}\"");

      return this;
    }


    public MySqlSelectBuilder From(string table, string alias = null)
    {
      table = table.NonBlank(nameof(table)).Trim();

      var hasContent = m_From.Length > 0;

      if (hasContent) m_From.Append(", \n");

      var owner = m_SchemaName;

      if (owner.IsNotNullOrWhiteSpace())
      {
        m_From.Append(quote(owner));
        m_From.Append('.');
      }

      m_From.Append(quote(table));
      m_From.Append(" ");

      if (alias.IsNotNullOrWhiteSpace())
        m_From.Append(quote(alias));

      return this;
    }

    public MySqlSelectBuilder FromClause(string clause)
    {
      clause = clause.NonBlank(nameof(clause)).Trim();

      clause = clause.Args(m_SchemaName);
      var owner = m_SchemaName;

      m_From.Append(" ");
      m_From.Append(clause);
      m_From.Append(" ");

      return this;
    }


    public MySqlSelectBuilder AndWhere(string dbExpression, params MySqlParameter[] parameters)
     => WhereClause(WhereClauseType.And, dbExpression, parameters);

    public MySqlSelectBuilder OrWhere(string dbExpression, params MySqlParameter[] parameters)
     => WhereClause(WhereClauseType.Or, dbExpression, parameters);

    public MySqlSelectBuilder WhereExpressionBlock(WhereClauseType clause, SqlXlatContext xlat)
     => WhereBlockBegin(clause, xlat.Parameters.Cast<MySqlParameter>().ToArray())
            .OrWhere(xlat.SQL.ToString())
            .WhereBlockEnd();

    public MySqlSelectBuilder WhereBlockBegin(WhereClauseType clause, params MySqlParameter[] parameters)
    {
      var hasContent = m_CurrentWhereNodes > 0;
      m_WhereLevels.Push(m_CurrentWhereNodes);
      m_CurrentWhereNodes = 0;

      if (hasContent) m_Where.Append($" {(clause == WhereClauseType.And ? "AND" : "OR")} \n");

      m_Where.Append("( ");

      parameters.ForEach(p => m_Parameters.Add(p));

      return this;
    }

    public MySqlSelectBuilder WhereBlockEnd()
    {
      if (m_WhereLevels.Count == 0)
        throw new MySqlDataAccessException("Begin/End query builder blocks are mismatched");

      m_CurrentWhereNodes = m_WhereLevels.Pop();
      m_CurrentWhereNodes++;

      m_Where.AppendLine(" )");
      return this;
    }

    public MySqlSelectBuilder WhereClause(WhereClauseType clause, string dbExpression, params MySqlParameter[] parameters)
    {
      dbExpression = dbExpression.NonBlank(nameof(dbExpression)).Trim();

      var hasContent = m_CurrentWhereNodes > 0;
      m_CurrentWhereNodes++;

      if (hasContent) m_Where.Append($" {(clause == WhereClauseType.And ? "AND" : "OR")} ");

      m_Where.Append(quote(dbExpression));

      parameters.ForEach(p => m_Parameters.Add(p));
      return this;
    }


    public MySqlSelectBuilder OrderByAsc(string dbExpression)
    => OrderByClause(OrderClauseType.Asc, dbExpression);

    public MySqlSelectBuilder OrderByDesc(string dbExpression)
     => OrderByClause(OrderClauseType.Desc, dbExpression);


    public MySqlSelectBuilder OrderByClause(OrderClauseType clause, string dbExpression)
    {
      dbExpression = dbExpression.NonBlank(nameof(dbExpression)).Trim();

      var hasContent = m_OrderBy.Length > 0;

      if (hasContent) m_OrderBy.Append(", \n");

      m_OrderBy.Append(quote(dbExpression));

      m_OrderBy.Append($" {clause.ToString().ToUpperInvariant()}");

      return this;
    }

    public MySqlSelectBuilder GroupBy(string dbExpression)
    {
      dbExpression = dbExpression.NonBlank(nameof(dbExpression)).Trim();

      var hasContent = m_GroupBy.Length > 0;

      if (hasContent) m_GroupBy.Append(", ");

      m_GroupBy.Append(quote(dbExpression));

      return this;
    }

    public MySqlSelectBuilder Having(string dbExpression, params MySqlParameter[] parameters)
    {
      dbExpression = dbExpression.NonBlank(nameof(dbExpression)).Trim();

      var hasContent = m_Having.Length > 0;

      if (hasContent) m_Having.Append(", ");

      m_Having.Append(quote(dbExpression));

      parameters.ForEach(p => m_Parameters.Add(p));

      return this;
    }


    /// <summary>
    /// Builds the SQL statement and parameters right into the Sql command object
    /// </summary>
    public MySqlSelectBuilder Build(MySqlCommand command)
    {
      command.NonNull(nameof(command));
      var built = Build();
      command.CommandText = built.command;
      built.pars.ForEach(p => command.Parameters.Add(p));
      return this;
    }

    /// <summary>
    /// Builds the SQL statement
    /// </summary>
    public (string command, IEnumerable<MySqlParameter> pars) Build()
    {
      if (m_Select.Length == 0) throw new MySqlDataAccessException("No columns to select");
      if (m_From.Length == 0) throw new MySqlDataAccessException("No tables in 'from'");

      m_Statement.AppendLine("SELECT");
      m_Statement.AppendLine(m_Select.ToString());

      m_Statement.AppendLine("FROM");
      m_Statement.AppendLine(m_From.ToString());

      if (m_Where.Length > 0)
      {
        m_Statement.AppendLine("WHERE");
        m_Statement.AppendLine(m_Where.ToString());
      }

      if (m_GroupBy.Length > 0)
      {
        m_Statement.AppendLine("GROUP BY");
        m_Statement.AppendLine(m_GroupBy.ToString());
      }

      if (m_Having.Length > 0)
      {
        m_Statement.AppendLine("HAVING");
        m_Statement.AppendLine(m_Having.ToString());
      }

      if (m_OrderBy.Length > 0)
      {
        m_Statement.AppendLine("ORDER BY");
        m_Statement.AppendLine(m_OrderBy.ToString());
      }

      if (Limit > 0) m_Statement.AppendLine("LIMIT {0}".Args(Limit));

      return (m_Statement.ToString(), m_Parameters);
    }


    private string quote(string expression)
    => needsQuotes(expression) ? $"`{expression}`" : expression;

    private bool needsQuotes(string expression)
    => !(
         expression.StartsWith("`") ||  // "a"
         expression.EndsWith("`") ||  // t."b"
         expression.Contains("(") ||  // (select...)  SUM(x)...
         expression.Contains(".")        // t1.a
        );

  }
}

