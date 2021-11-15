/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data.Access.Rpc;

namespace Azos.Data.Access.MsSql
{
  /// <summary>
  /// Provides base for `IRpcHandler` implementations for Microsoft SQL Server
  /// </summary>
  public abstract class MsSqlRpcHandler : ModuleBase, IRpcHandler
  {
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

    #region Protected
    protected override bool DoApplicationAfterInit()
    {
      return base.DoApplicationAfterInit();
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

    public async Task<Rowset> ReadAsync(ReadRequest request)
    {
#warning Finish this tomorrow
      //App.Authorize();
      SqlConnection connection = null;//getConnection for context() <-- add as a virtual function to allow only certain context mappings

      var command = request.Command.NonNull(nameof(request.Command));
      command.Text.NonBlank(nameof(command.Text));
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

      var rowset = await readAsync(connection, command, isSql).ConfigureAwait(false);
      return rowset;
    }

    public Task<IEnumerable<ChangeResult>> TransactAsync(TransactRequest request)
    {
      throw new NotImplementedException();
    }
    #endregion

    #region .pvt
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

    private async Task<Rowset> readAsync(SqlConnection connection, Command command, bool isSql)
    {
      Rowset result = null;

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
          }
        }
      }

      return result;
    }
    #endregion
  }
}
