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
  /// Executes MySql CRUD script-based queries
  /// </summary>
  public sealed class MySqlCrudScriptQueryHandler : MySqlCrudQueryHandler
  {
    #region .ctor
    public MySqlCrudScriptQueryHandler(MySqlCrudDataStoreBase store, QuerySource source) : base(store, source) { }
    #endregion

    #region ICRUDQueryHandler
    public override Schema GetSchema(ICrudQueryExecutionContext context, Query query)
      => GetSchemaAsync(context, query).GetAwaiter().GetResult();

    public override async Task<Schema> GetQuerySchemaAsync(MySqlCrudQueryExecutionContext context, Query query)
    {
      var target = context.DataStore.TargetName;

      using (var cmd = context.Connection.CreateCommand())
      {
        cmd.CommandText =  Source.StatementSource;

        PopulateParameters(cmd, query);

        cmd.Transaction = context.Transaction;

        MySqlDataReader reader = null;

        try
        {
            reader = await cmd.ExecuteReaderAsync(CommandBehavior.SchemaOnly).ConfigureAwait(false);
            GeneratorUtils.LogCommand(context.DataStore, "queryhandler-ok", cmd, null);
        }
        catch(Exception error)
        {
            GeneratorUtils.LogCommand(context.DataStore, "queryhandler-error", cmd, error);
            throw;
        }


        using (reader)
        {
          Schema.FieldDef[] toLoad;
          return GetSchemaForQuery(target, query, reader, Source, out toLoad);
        }//using reader
      }//using command
    }

    public override async Task<RowsetBase> ExecuteAsync(MySqlCrudQueryExecutionContext context, Query query, bool oneDoc = false)
    {
      var target = context.DataStore.TargetName;

      using (var cmd = context.Connection.CreateCommand())
      {

        cmd.CommandText =  Source.StatementSource;

        PopulateParameters(cmd, query);

        cmd.Transaction = context.Transaction;

        MySqlDataReader reader = null;

        try
        {
            reader = oneDoc ? await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow).ConfigureAwait(false) :
                              await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            GeneratorUtils.LogCommand(context.DataStore, "queryhandler-ok", cmd, null);
        }
        catch(Exception error)
        {
            GeneratorUtils.LogCommand(context.DataStore, "queryhandler-error", cmd, error);
            throw;
        }

        using (reader)
        {
          return await DoPopulateRowsetAsync(context, reader, target, query, Source, oneDoc);
        }
      }//using command
    }

    public override async Task<Cursor> OpenCursorAsync(MySqlCrudQueryExecutionContext context, Query query)
    {
      var target = context.DataStore.TargetName;

      Schema.FieldDef[] toLoad;
      Schema schema = null;
      MySqlDataReader reader = null;
      var cmd = context.Connection.CreateCommand();
      try
      {

        cmd.CommandText =  Source.StatementSource;

        PopulateParameters(cmd, query);

        cmd.Transaction = context.Transaction;

        try
        {
            reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            GeneratorUtils.LogCommand(context.DataStore, "queryhandler-ok", cmd, null);
        }
        catch(Exception error)
        {
            GeneratorUtils.LogCommand(context.DataStore, "queryhandler-error", cmd, error);
            throw;
        }


        schema = GetSchemaForQuery(target, query, reader, Source, out toLoad);
      }
      catch
      {
        if (reader!=null) reader.Dispose();
        cmd.Dispose();
        throw;
      }

      var enumerable = execEnumerable(context, cmd, reader, schema, toLoad, query);

      return new MySqlCursor(context, cmd, reader, enumerable );
    }

    private IEnumerable<Doc> execEnumerable(MySqlCrudQueryExecutionContext ctx, MySqlCommand cmd, MySqlDataReader reader, Schema schema, Schema.FieldDef[] toLoad, Query query)
    {
      using(cmd)
      {
        using(reader)
        {
          while (reader.Read())
          {
            var row = DoPopulateDoc(ctx, query.ResultDocType, schema, toLoad, reader);
            yield return row;
          }
        }
      }
    }


    public override async Task<int> ExecuteWithoutFetchAsync(MySqlCrudQueryExecutionContext context, Query query)
    {
      using (var cmd = context.Connection.CreateCommand())
      {

        cmd.CommandText =  Source.StatementSource;

        PopulateParameters(cmd, query);

        cmd.Transaction = context.Transaction;

        try
        {
          var affected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
          GeneratorUtils.LogCommand(context.DataStore, "queryhandler-ok", cmd, null);
          return affected;
        }
        catch(Exception error)
        {
          GeneratorUtils.LogCommand(context.DataStore, "queryhandler-error", cmd, error);
          throw;
        }
      }//using command
    }

    #endregion
  }
}
