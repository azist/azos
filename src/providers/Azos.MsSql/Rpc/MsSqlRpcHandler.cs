/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
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
    /// Override to validate the read request
    /// </summary>
    protected abstract ValidState DoValidateReadRequest(ValidState state, ReadRequest request);

    /// <summary>
    /// Override to validate the transact request
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
      //App.Authorize();
      SqlConnection connection = null;//getConnection for context()

      var command = request.Command.NonNull(nameof(request.Command));
      var isSql = true;

      if (command.Headers != null)
      {
        var tp = command.Headers[StandardHeaders.Sql.COMMAND_TYPE].AsString();
        if (tp.EqualsOrdIgnoreCase(StandardHeaders.Sql.COMMAND_TYPE_VALUE_TEXT)) isSql = true;
        else if (tp.EqualsOrdIgnoreCase(StandardHeaders.Sql.COMMAND_TYPE_VALUE_PROC)) isSql = false;
        else if (tp.IsNotNullOrWhiteSpace()) throw new DocValidationException(nameof(ReadRequest.Command), "Invalid header `{0}` = `{1}`".Args(StandardHeaders.Sql.COMMAND_TYPE, tp));
      }

      Rowset rowset = null;
      //if (isSql)
      //  rowset = await executeReadSqlAsync(connection, command).ConfigureAwait(false);
      //else
      //  rowset = await executeReadStoredProcAsync(connection, command).ConfigureAwait(false);

      return rowset;
    }

    public Task<IEnumerable<ChangeResult>> TransactAsync(TransactRequest request)
    {
      throw new NotImplementedException();
    }
    #endregion

  }
}
