/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Data.Access.Rpc;
using Azos.Security.Data;
using Azos.Serialization.JSON;

namespace Azos.Data.Access.MsSql
{
  /// <summary>
  /// Provides base for `IRpcHandler` implementations for Microsoft SQL Server
  /// </summary>
  public abstract class MsSqlRpcHandler : ModuleBase, IRpcHandler
  {
    /// <summary>
    /// Absolute maximum number of rows to fetch at once
    /// </summary>
    public const int DEFAULT_FETCH_LIMIT = 1000;


    /// <summary>
    /// Creates a root module without a parent
    /// </summary>
    protected MsSqlRpcHandler(IApplication application) : base(application) { }

    /// <summary>
    /// Creates a module under a parent module, such as HubModule
    /// </summary>
    protected MsSqlRpcHandler(IModule parent) : base(parent) { }

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => MsSqlConsts.MSSQL_TOPIC;


    [Config]
    public string ConnectString { get; set; }

    #region Protected
    protected static readonly DataRpcPermission PERM_READ = new DataRpcPermission(DataRpcAccessLevel.Read);
    protected static readonly DataRpcPermission PERM_TRANSACT = new DataRpcPermission(DataRpcAccessLevel.Transact);

    protected override void DoConfigureInScope(IConfigSectionNode node)
    {
      using(new Security.SecurityFlowScope(Security.TheSafe.SAFE_ACCESS_FLAG))
      {
        base.DoConfigureInScope(node);
      }
    }

    protected override bool DoApplicationAfterInit()
    {
      return base.DoApplicationAfterInit();
    }

    /// <summary>
    /// Override to map incoming request based on normalized <see cref="ISession.DataContextName"/> into a connect string.
    /// The default base implementation just allocates a connection using <see cref="ConnectString" /> property
    /// </summary>
    /// <param name="headers">Optional headers supplied with request (may be null)</param>
    /// <returns>New SqlConnection instance</returns>
    protected virtual SqlConnection GetSqlConnection(JsonDataMap headers)
    {
      var result = new SqlConnection(ConnectString);
      result.Open();
      return result;
    }

    /// <summary>
    /// Must override to validate the read request
    /// </summary>
    protected abstract ValidState DoValidateReadRequest(ValidState state, ReadRequest request);

    /// <summary>
    /// Must override to validate the transact request
    /// </summary>
    protected abstract ValidState DoValidateTransactRequest(ValidState state, TransactRequest request);

    #endregion

    #region IRpcHandler
    public ValidState ValidateReadRequest(ValidState state, ReadRequest request)
    {
      request.NonNull(nameof(request));
      return DoValidateReadRequest(state, request);
    }

    public ValidState ValidateTransactRequest(ValidState state, TransactRequest request)
    {
      request.NonNull(nameof(request));
      return DoValidateTransactRequest(state, request);
    }



    public async Task<JsonDataMap> ReadAsync(ReadRequest request)
    {
      App.Authorize(PERM_READ);

      var command = request.Command.NonNull(nameof(request.Command));
      command.Text.NonBlank(nameof(command.Text));
      var isSql = isSqlStatementType(command);

      try
      {
        Rowset result;
        using(var connection = GetSqlConnection(command.Headers))
        {
          result = await readAsync(connection, command, isSql).ConfigureAwait(false);
        }
        return result.WriteAsJsonIntoMap(metadata: true);
      }
      catch(Exception error)
      {
        throw new DocValidationException(nameof(ReadRequest.Command), "Error executing read request: \n{0}".Args(error.ToMessageWithType()), error);
      }
    }

    public async Task<ChangeResult> TransactAsync(TransactRequest request)
    {
      App.Authorize(PERM_TRANSACT);

      var commands = request.Commands.NonNull(nameof(request.Commands));

      try
      {
        var result = new List<ChangeResult>();

        var time = Time.Timeter.StartNew();
        using (var connection = GetSqlConnection(request.RequestHeaders))
        {
          using(var tx = connection.BeginTransaction())
          try
          {
            foreach (var command in commands)
            {
              var isSql = isSqlStatementType(command);
              var oneChange = await txAsync(connection, tx, command, isSql).ConfigureAwait(false);
              result.Add(oneChange);
            }
            tx.Commit();
          }
          catch
          {
            tx.Rollback();
            throw;
          }
        }
        time.Stop();

        return new ChangeResult(ChangeResult.ChangeType.Processed, result.Count, $"Processed in {time.ElapsedMs:n0} ms", result, 200);
      }
      catch (Exception error)
      {
        throw new DocValidationException(nameof(ReadRequest.Command), "Error executing transact request: \n{0}".Args(error.ToMessageWithType()), error);
      }
    }
    #endregion

    #region .pvt
    private bool isSqlStatementType(Command command)
    {
      var isSql = true;

      if (command.Headers != null)
      {
        var tp = command.Headers[StandardHeaders.Sql.COMMAND_TYPE].AsString();

        if (tp.EqualsOrdIgnoreCase(StandardHeaders.Sql.COMMAND_TYPE_VALUE_TEXT)) isSql = true;
        else if (tp.EqualsOrdIgnoreCase(StandardHeaders.Sql.COMMAND_TYPE_VALUE_PROC)) isSql = false;
        else if (tp.IsNotNullOrWhiteSpace())
        {
          throw new DocValidationException(nameof(ReadRequest.Command), "Invalid header `{0}` = `{1}`".Args(StandardHeaders.Sql.COMMAND_TYPE, tp));
        }
      }

      return isSql;
    }

    private void bindParams(SqlCommand cmd, Command command)
    {
      if (command.Parameters == null) return;
      var pName = "?";
      try
      {
        foreach(var par in command.Parameters)
        {
          pName = "?";
          var name = par.Name.NonBlank("parameter name");
          pName = name;
          var val = par.Value;

          if (val != null)
          {
            var tp = Command.Param.TryMap(par.Type);
            if (tp != null)
            {
              val = val.AsString().AsType(tp);
            }
          }
          cmd.Parameters.AddWithValue(name, val);
        }
      }
      catch(Exception error)
      {
        throw new ValidationException("Command parameter `{0}` cast error: {1}".Args(pName, error.ToMessageWithType()), error);
      }
    }

    private Schema inferSchema(SqlDataReader reader, Command command)
    {
      var name = "{0}-{1}".Args(GetType().DisplayNameWithExpandedGenericArgs(), Guid.NewGuid());
      var fdefs = new List<Schema.FieldDef>();

      for (int i = 0; i < reader.FieldCount; i++)
      {
        var fname = reader.GetName(i);
        var ftype = reader.GetFieldType(i);

        var def = new Schema.FieldDef(fname, ftype, attr: null);
        fdefs.Add(def);
      }

      return new Schema(name, readOnly: true, fieldDefs: fdefs);
    }

    private void populateDoc(Doc doc, SqlDataReader reader, Command command)
    {
      for (int i = 0; i < reader.FieldCount; i++)
      {
        var fdef = doc.Schema[i];
        var val = reader.GetValue(i);

        if (val == null || val is DBNull)
        {
          doc[fdef.Order] = null;
          continue;
        }

        doc[fdef.Order] = val;
      }
    }


    protected virtual int GetFetchLimit(Command command, bool isSql) => DEFAULT_FETCH_LIMIT;

    private async Task<Rowset> readAsync(SqlConnection connection, Command command, bool isSql)
    {
      Rowset result = null;

      var limit = GetFetchLimit(command, isSql);

      using(var cmd = connection.CreateCommand())
      {
        cmd.CommandType = isSql ? System.Data.CommandType.Text : System.Data.CommandType.StoredProcedure;
        cmd.CommandText = command.Text;

        bindParams(cmd, command);

        using(var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
        {
          var schema = inferSchema(reader, command);
          result = new Rowset(schema);
          result.LogChanges = false;
          while(await reader.ReadAsync().ConfigureAwait(false))
          {
            var doc = Doc.MakeDoc(result.Schema);
            populateDoc(doc, reader, command);
            result.Add(doc);

            if (result.Count > limit) break;
          }
        }
      }

      return result;
    }

    private async Task<ChangeResult> txAsync(SqlConnection connection, SqlTransaction tx, Command command, bool isSql)
    {
      using (var cmd = connection.CreateCommand())
      {
        cmd.CommandType = isSql ? System.Data.CommandType.Text : System.Data.CommandType.StoredProcedure;
        cmd.CommandText = command.Text;
        cmd.Transaction = tx;

        bindParams(cmd, command);

        var time = Time.Timeter.StartNew();
        var got = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        time.Stop();

        return new ChangeResult(ChangeResult.ChangeType.Processed, got, $"Done in {time.ElapsedMs:n0} ms", null);
      }
    }
    #endregion
  }
}
