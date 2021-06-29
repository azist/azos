/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using MySqlConnector;

namespace Azos.Data.Access.MySql
{
  /// <summary>
  /// Executes MySql CRUD queries
  /// </summary>
  public abstract class MySqlCrudQueryHandler : InstrumentedCrudQueryHandler<MySqlCrudDataStoreBase, MySqlCrudQueryExecutionContext>
  {
    #region .ctor
    public MySqlCrudQueryHandler(MySqlCrudDataStoreBase store, string name) : base(store, name) { }
    public MySqlCrudQueryHandler(MySqlCrudDataStoreBase store, QuerySource source) : base(store, source) { }
    #endregion

    #region Helpers
    /// <summary>
    /// Reads data from reader into rowset. the reader is NOT disposed
    /// </summary>
    protected virtual async Task<Rowset> DoPopulateRowsetAsync(MySqlCrudQueryExecutionContext context, MySqlDataReader reader, string target, Query query, QuerySource qSource, bool oneDoc)
    {
      Schema schema = GetSchemaForQuery(target, query, reader, qSource, out var toLoad);
      var store= context.DataStore;

      var result = new Rowset(schema);

      while( await reader.ReadAsync().ConfigureAwait(false) )
      {
        var doc = DoPopulateDoc(context, query.ResultDocType, schema, toLoad, reader);

        if (doc == null) continue;

        result.Add( doc );

        if (oneDoc) break;

        if (result.Count > MaxDocCount)
          throw new MySqlDataAccessException($"Query {GetType().FullName} exceeded the maximum allowed row count of {MaxDocCount}. Limit the number of rows returned using WHERE or LIMIT");
      }

      return result;
    }

    /// <summary>
    /// Reads data from reader into rowset. the reader is NOT disposed
    /// </summary>
    protected virtual Doc DoPopulateDoc(MySqlCrudQueryExecutionContext context, Type tDoc, Schema schema, Schema.FieldDef[] toLoad, MySqlDataReader reader)
    {
      var store= context.DataStore;
      var doc = Doc.MakeDoc(schema, tDoc);

      for (int i = 0; i < reader.FieldCount; i++)
      {
        var fdef = toLoad[i];
        if (fdef==null) continue;

        var val = reader.GetValue(i);

        if (val==null || val is DBNull)
        {
          doc[fdef.Order] = null;
          continue;
        }

        if (fdef.NonNullableType == typeof(bool))
        {
          if (store.StringBool)
          {
            var bval = (val is bool) ? (bool)val : val.ToString().EqualsIgnoreCase(store.StringForTrue);
            doc[fdef.Order] = bval;
          }
          else
            doc[fdef.Order] = val.AsNullableBool();
        }
        else if (fdef.NonNullableType == typeof(DateTime))
        {
          var dtVal = val.AsNullableDateTime();
          doc[fdef.Order] = dtVal.HasValue ? DateTime.SpecifyKind(dtVal.Value, store.DateTimeKind) : (DateTime?)null;
        }
        else
          doc[fdef.Order] = val;
      }

      return doc;
    }

    /// <summary>
    /// Populates MySqlCommand with parameters from CRUD Query object
    /// Note: this code was purposely made provider specific because other providers may treat some nuances differently
    /// </summary>
    protected void PopulateParameters(MySqlCommand cmd, Query query)
    {
        foreach(var par in query.Where(p => p.HasValue))
        cmd.Parameters.AddWithValue(par.Name, par.Value);

        if (query.StoreKey!=null)
        {
        var where = GeneratorUtils.KeyToWhere(query.StoreKey, cmd.Parameters);
        cmd.CommandText += "\n WHERE \n {0}".Args( where );
        }

        CrudGenerator.ConvertParameters(Store, cmd.Parameters);
    }

    /// <summary>
    /// Gets CRUD schema from MySqlReader per particular QuerySource.
    /// If source is null then all columns from reader are copied.
    /// Note: this code was purposely made provider specific because other providers may treat some nuances differently
    /// </summary>
    protected static Schema GetSchemaFromReader(string name, QuerySource source, MySqlDataReader reader)
    {
        var table = name;
        var fdefs = new List<Schema.FieldDef>();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            var fname = reader.GetName(i);
            var ftype = reader.GetFieldType(i);

            var def = new Schema.FieldDef(fname, ftype, source!=null ? ( source.HasPragma ? source.ColumnDefs[fname] : null) : null);
            fdefs.Add( def );
        }

        if (source!=null)
        if (source.HasPragma && source.ModifyTarget.IsNotNullOrWhiteSpace()) table = source.ModifyTarget;

        if (table.IsNullOrWhiteSpace()) table = Guid.NewGuid().ToString();

        return new Schema(table, source!=null ? source.ReadOnly : true,  fdefs);
    }

    /// <summary>
    /// Gets schema from reader taking Query.ResultDocType in consideration
    /// </summary>
    protected static Schema GetSchemaForQuery(string target, Query query, MySqlDataReader reader, QuerySource qSource, out Schema.FieldDef[] toLoad)
    {
      Schema schema;
      var rtp = query.ResultDocType;

      if (rtp != null && typeof(TypedDoc).IsAssignableFrom(rtp))
        schema = Schema.GetForTypedDoc(query.ResultDocType);
      else
        schema = GetSchemaFromReader(query.Name, qSource, reader);

      //determine what fields to load
      toLoad = new Schema.FieldDef[reader.FieldCount];
      for (int i = 0; i < reader.FieldCount; i++)
      {
        var name = reader.GetName(i);
        var fdef = schema[name];
        // 2017-09-08 EIbr + DKh fixed binding by backend name bug
        //var fdef = schema.GetFieldDefByBackendName(target, name);

        //todo A gde GetBackendNameFor target?
        if (fdef==null) continue;

        var attr =  fdef[target];
        if (attr!=null)
        {
          if (attr.StoreFlag!=StoreFlag.LoadAndStore && attr.StoreFlag!=StoreFlag.OnlyLoad) continue;
        }
        toLoad[i] = fdef;
      }

      return schema;
    }

   #endregion
  }

  /// <summary>
  /// Facilitates creation of code-based queries with typed first parameter, such as a complex
  /// filter object.
  /// </summary>
  /// <typeparam name="TQueryParameters">Type of query first parameter, such as a complex filter object</typeparam>
  public abstract class MySqlCrudQueryHandler<TQueryParameters> : MySqlCrudQueryHandler
  {
    public MySqlCrudQueryHandler(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

    /// <summary>
    /// Casts first query parameter to TQueryParameters
    /// </summary>
    protected TQueryParameters CastParameters(Query query)
    {
      query.NonNull(nameof(query));
      if (query.Count < 1)
        throw new MySqlDataAccessException("Query '{0}' requires at least one parameter of type '{1}' but was supplied none".Args(
                                             GetType().Name,
                                             typeof(TQueryParameters).Name));
      try
      {
        return (TQueryParameters)query[0].Value;
      }
      catch (Exception error)
      {
        throw new MySqlDataAccessException("Query '{0}' was supplied an invalid parameter type. '{1}' is expected: {2}".Args(
                                           GetType().Name,
                                           typeof(TQueryParameters).Name,
                                           error.ToMessageWithType()), error);
      }
    }//cast parameters

    public sealed async override Task<RowsetBase> ExecuteAsync(MySqlCrudQueryExecutionContext context, Query query, bool oneRow = false)
    {
      var qParams = CastParameters(query);
      return await DoExecuteParameterizedQueryAsync(context, query, qParams);
    }

    public sealed async override Task<int> ExecuteWithoutFetchAsync(MySqlCrudQueryExecutionContext context, Query query)
    {
      var qParams = CastParameters(query);
      return await DoExecuteWithoutFetchParameterizedQueryAsync(context, query, qParams);
    }


    /// <summary>
    /// Provides default implementation by invoking DoBuildCommandAndParameters and then executing a command.
    /// </summary>
    protected async virtual Task<int> DoExecuteWithoutFetchParameterizedQueryAsync(MySqlCrudQueryExecutionContext ctx, Query query, TQueryParameters queryParameters)
    {
      using (var cmd = ctx.Connection.CreateCommand())
      {
        cmd.CommandTimeout = CommandTimeoutSec;

        DoBuildCommandAndParameters(ctx, cmd, queryParameters);

        ctx.ConvertParameters(cmd.Parameters);

        cmd.Transaction = ctx.Transaction;

        try
        {
          var affected = await cmd.ExecuteNonQueryAsync();
          GeneratorUtils.LogCommand(ctx.DataStore, "DoExecuteWithoutFetchQuery-ok", cmd, null);
          return affected;
        }
        catch (Exception error)
        {
          GeneratorUtils.LogCommand(ctx.DataStore, "DoExecuteWithoutFetchQuery-error", cmd, error);
          throw;
        }
      }//using command
    }

    /// <summary>
    /// Provides default implementation by invoking DoBuildCommandAndParameters and then executing a command then mapping data into a rowset
    /// </summary>
    protected async virtual Task<RowsetBase> DoExecuteParameterizedQueryAsync(MySqlCrudQueryExecutionContext ctx, Query query, TQueryParameters queryParameters)
    {
      using (var cmd = ctx.Connection.CreateCommand())
      {
        cmd.CommandTimeout = CommandTimeoutSec;

        DoBuildCommandAndParameters(ctx, cmd, queryParameters);

        ctx.ConvertParameters(cmd.Parameters);

        cmd.Transaction = ctx.Transaction;

        MySqlDataReader reader = null;
        try
        {
          reader = await cmd.ExecuteReaderAsync();
          GeneratorUtils.LogCommand(ctx.DataStore, "DoExecuteFilteredQuery-ok", cmd, null);
        }
        catch (Exception error)
        {
          GeneratorUtils.LogCommand(ctx.DataStore, "DoExecuteFilteredQuery-error", cmd, error);
          throw;
        }

        using (reader)
        {
          return await DoPopulateRowsetAsync(ctx, reader, ctx.DataStore.TargetName, query, null, false);
        }
      }//using command
    }

    /// <summary>
    /// Override to perform custom command building including parameters per TQueryParameters
    /// </summary>
    /// <param name="context">MySql execution context (connection, transaction, type converter etc.)</param>
    /// <param name="cmd">New MySqlCommand instance to build text and parameters into</param>
    /// <param name="queryParameters">Non-null parameters, typically a FilterModel object</param>
    /// <remarks>
    /// This method is synchronous CPU-bound work
    /// </remarks>
    protected virtual void DoBuildCommandAndParameters(MySqlCrudQueryExecutionContext context, MySqlCommand cmd, TQueryParameters queryParameters) { }
  }
}
