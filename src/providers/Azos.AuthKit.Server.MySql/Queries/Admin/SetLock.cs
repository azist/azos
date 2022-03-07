/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Threading.Tasks;

using MySqlConnector;

using Azos;
using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Access.MySql;
using Azos.Data.Business;
using Azos.Platform;
using Azos.Time;
using Azos.Wave;


namespace Azos.AuthKit.Server.MySql.Queries.Admin
{
  public sealed class SetLock : MySqlCrudQueryHandler<LockStatus>
  {
    public SetLock(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

    protected override async Task<Doc> DoExecuteProcedureParameterizedQueryAsync(MySqlCrudQueryExecutionContext ctx, Query query, LockStatus lockStatus)
    {
      var result = new EntityChangeInfo
      {
        Id = lockStatus.TargetEntity,
        Version = ctx.MakeVersionInfo(lockStatus.TargetEntity.Address.AsGDID(), lockStatus.FormMode)
      };

      if (lockStatus.TargetEntity.Type == Constraints.ETP_USER)
      {
        await ctx.ExecuteCompoundCommand(CommandTimeoutSec, System.Data.IsolationLevel.ReadCommitted,
          cmd => setUserLock(cmd, lockStatus)
        ).ConfigureAwait(false);
      }
      else
      {
        await ctx.ExecuteCompoundCommand(CommandTimeoutSec, System.Data.IsolationLevel.ReadCommitted,
          cmd => setLoginLock(cmd, lockStatus)
        ).ConfigureAwait(false);
      }

      return result;
    }

    private void setUserLock(MySqlCommand cmd, LockStatus lockStatus)
    {
      cmd.CommandText = GetType().GetText("SetUserLock.sql");

      DateRange validSpan = lockStatus.LockSpanUtc.Value;

      cmd.Parameters.AddWithValue("gdid", lockStatus.TargetEntity.Address.AsGDID());
      cmd.Parameters.AddWithValue("start_utc", validSpan.Start);
      cmd.Parameters.AddWithValue("end_utc", validSpan.End);
      cmd.Parameters.AddWithValue("actor", lockStatus.LockActor);
      cmd.Parameters.AddWithValue("note", lockStatus.LockNote);
    }

    private void setLoginLock(MySqlCommand cmd, LockStatus lockStatus)
    {
      cmd.CommandText = GetType().GetText("SetLoginLock.sql");

      DateRange validSpan = lockStatus.LockSpanUtc.Value;

      cmd.Parameters.AddWithValue("gdid", lockStatus.TargetEntity.Address.AsGDID());
      cmd.Parameters.AddWithValue("start_utc", validSpan.Start);
      cmd.Parameters.AddWithValue("end_utc", validSpan.End);
      cmd.Parameters.AddWithValue("actor", lockStatus.LockActor);
      cmd.Parameters.AddWithValue("note", lockStatus.LockNote);
    }

  }
}
