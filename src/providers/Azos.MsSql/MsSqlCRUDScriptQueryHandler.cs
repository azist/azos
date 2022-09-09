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

using Microsoft.Data.SqlClient;

namespace Azos.Data.Access.MsSql
{
  /// <summary>
  /// Executes Oracle CRUD script-based queries
  /// </summary>
  public sealed class MsSqlCRUDScriptQueryHandler : CRUDQueryHandler<MsSqlCRUDDataStoreBase>
  {
      #region .ctor
          public MsSqlCRUDScriptQueryHandler(MsSqlCRUDDataStoreBase store, QuerySource source) : base(store, source) { }
      #endregion

      #region ICRUDQueryHandler
          public override Schema GetSchema(ICrudQueryExecutionContext context, Query query)
          {
              var ctx = (MsSqlCRUDQueryExecutionContext)context;
              var target = ctx.DataStore.TargetName;

              using (var cmd = ctx.Connection.CreateCommand())
              {
                  cmd.CommandText =  Source.StatementSource;


                  PopulateParameters(cmd, query);



                  cmd.Transaction = ctx.Transaction;

                  SqlDataReader reader = null;

                  try
                  {
                      reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly);
                      GeneratorUtils.LogCommand(ctx.DataStore, "queryhandler-ok", cmd, null);
                  }
                  catch(Exception error)
                  {
                      GeneratorUtils.LogCommand(ctx.DataStore, "queryhandler-error", cmd, error);
                      throw;
                  }


                  using (reader)
                  {
                    Schema.FieldDef[] toLoad;
                    return GetSchemaForQuery(target, query, reader, Source, out toLoad);
                  }//using reader
              }//using command
          }


          public override Task<Schema> GetSchemaAsync(ICrudQueryExecutionContext context, Query query)
          {
            return TaskUtils.AsCompletedTask( () => this.GetSchema(context, query));
          }


          public override RowsetBase Execute(ICrudQueryExecutionContext context, Query query, bool oneDoc = false)
          {
              var ctx = (MsSqlCRUDQueryExecutionContext)context;
              var target = ctx.DataStore.TargetName;

              using (var cmd = ctx.Connection.CreateCommand())
              {
                  cmd.CommandText =  Source.StatementSource;

                  PopulateParameters(cmd, query);

                  cmd.Transaction = ctx.Transaction;

                  SqlDataReader reader = null;
                  try
                  {
                      reader = oneDoc ? cmd.ExecuteReader(CommandBehavior.SingleRow) : cmd.ExecuteReader();
                      GeneratorUtils.LogCommand(ctx.DataStore, "queryhandler-ok", cmd, null);
                  }
                  catch(Exception error)
                  {
                      GeneratorUtils.LogCommand(ctx.DataStore, "queryhandler-error", cmd, error);
                      throw;
                  }

                  using (reader)
                    return PopulateRowset(ctx, reader, target, query, Source, oneDoc);
              }//using command
          }

          public override Task<RowsetBase> ExecuteAsync(ICrudQueryExecutionContext context, Query query, bool oneDoc = false)
          {
            return TaskUtils.AsCompletedTask( () => this.Execute(context, query, oneDoc));
          }


          public override Cursor OpenCursor(ICrudQueryExecutionContext context, Query query)
          {
            var ctx = (MsSqlCRUDQueryExecutionContext)context;
            var target = ctx.DataStore.TargetName;

            Schema.FieldDef[] toLoad;
            Schema schema = null;
            SqlDataReader reader = null;
            var cmd = ctx.Connection.CreateCommand();
            try
            {

              cmd.CommandText =  Source.StatementSource;

              PopulateParameters(cmd, query);

              cmd.Transaction = ctx.Transaction;

              try
              {
                  reader = cmd.ExecuteReader();
                  GeneratorUtils.LogCommand(ctx.DataStore, "queryhandler-ok", cmd, null);
              }
              catch(Exception error)
              {
                  GeneratorUtils.LogCommand(ctx.DataStore, "queryhandler-error", cmd, error);
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

            var enumerable = execEnumerable(ctx, cmd, reader, schema, toLoad, query);
            return new MsSqlCursor( ctx, cmd, reader, enumerable );
          }

                    private IEnumerable<Doc> execEnumerable(MsSqlCRUDQueryExecutionContext ctx, SqlCommand cmd, SqlDataReader reader, Schema schema, Schema.FieldDef[] toLoad, Query query)
                    {
                      using(cmd)
                        using(reader)
                        while(reader.Read())
                        {
                          var row = PopulateDoc(ctx, query.ResultDocType, schema, toLoad, reader);
                          yield return row;
                        }
                    }

          public override Task<Cursor> OpenCursorAsync(ICrudQueryExecutionContext context, Query query)
          {
            return TaskUtils.AsCompletedTask( () => this.OpenCursor(context, query));
          }


          public override Doc ExecuteProcedure(ICrudQueryExecutionContext context, Query query)
          {
              var ctx = (MsSqlCRUDQueryExecutionContext)context;

              using (var cmd = ctx.Connection.CreateCommand())
              {

                  cmd.CommandText =  Source.StatementSource;

                  PopulateParameters(cmd, query);

                  cmd.Transaction = ctx.Transaction;

                  try
                  {
                      var affected = cmd.ExecuteNonQuery();
                      GeneratorUtils.LogCommand(ctx.DataStore, "queryhandler-ok", cmd, null);
                      return new RowsAffectedDoc(affected);
                  }
                  catch(Exception error)
                  {
                      GeneratorUtils.LogCommand(ctx.DataStore, "queryhandler-error", cmd, error);
                      throw;
                  }
              }//using command
          }

          public override Task<Doc> ExecuteProcedureAsync(ICrudQueryExecutionContext context, Query query)
          {
              return TaskUtils.AsCompletedTask( () => this.ExecuteProcedure(context, query));
          }

      #endregion

      #region Static Helpers

          /// <summary>
          /// Reads data from reader into rowset. the reader is NOT disposed
          /// </summary>
          public static Rowset PopulateRowset(MsSqlCRUDQueryExecutionContext context, SqlDataReader reader, string target, Query query, QuerySource qSource, bool oneDoc)
          {
            Schema.FieldDef[] toLoad;
            Schema schema = GetSchemaForQuery(target, query, reader, qSource, out toLoad);
            var store= context.DataStore;

            var result = new Rowset(schema);
            while(reader.Read())
            {
              var row = PopulateDoc(context, query.ResultDocType, schema, toLoad, reader);

              result.Add( row );
              if (oneDoc) break;
            }

            return result;
          }

          /// <summary>
          /// Reads data from reader into rowset. the reader is NOT disposed
          /// </summary>
          public static Doc PopulateDoc(MsSqlCRUDQueryExecutionContext context, Type tDoc, Schema schema, Schema.FieldDef[] toLoad, SqlDataReader reader)
          {
            var store= context.DataStore;
            var row = Doc.MakeDoc(schema, tDoc);

            for (int i = 0; i < reader.FieldCount; i++)
            {
              var fdef = toLoad[i];
              if (fdef==null) continue;

              var val = reader.GetValue(i);

              if (val==null || val is DBNull)
              {
                row[fdef.Order] = null;
                continue;
              }

              if (fdef.NonNullableType == typeof(bool))
              {
                if (store.StringBool)
                {
                  var bval = (val is bool) ? (bool)val : val.ToString().EqualsIgnoreCase(store.StringForTrue);
                  row[fdef.Order] = bval;
                }
                else
                  row[fdef.Order] = val.AsNullableBool();
              }
              else if (fdef.NonNullableType == typeof(DateTime))
              {
                var dtVal = val.AsNullableDateTime();
                row[fdef.Order] = dtVal.HasValue ? DateTime.SpecifyKind(dtVal.Value, store.DateTimeKind) : (DateTime?)null;
              }
              else
                row[fdef.Order] = val;
            }

            return row;
          }


          /// <summary>
          /// Populates SqlCommand with parameters from CRUD Query object
          /// Note: this code was purposely made provider specific because other providers may treat some nuances differently
          /// </summary>
          public void PopulateParameters(SqlCommand cmd, Query query)
          {
              foreach(var par in query.Where(p => p.HasValue))
                cmd.Parameters.AddWithValue(par.Name, par.Value);

              if (query.StoreKey!=null)
              {
              var where = GeneratorUtils.KeyToWhere(query.StoreKey, cmd.Parameters);
              cmd.CommandText += "\n WHERE \n {0}".Args( where );
              }

              CRUDGenerator.ConvertParameters(Store, cmd.Parameters);
          }

          /// <summary>
          /// Gets CRUD schema from OracleReader per particular QuerySource.
          /// If source is null then all columns from reader are copied.
          /// Note: this code was purposely made provider specific because other providers may treat some nuances differently
          /// </summary>
          public static Schema GetSchemaFromReader(string name, QuerySource source, SqlDataReader reader)
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
          public static Schema GetSchemaForQuery(string target, Query query, SqlDataReader reader, QuerySource qSource, out Schema.FieldDef[] toLoad)
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

}
