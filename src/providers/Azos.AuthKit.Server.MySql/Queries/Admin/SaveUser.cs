/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using MySqlConnector;

using Azos;
using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Access.MySql;
using Azos.Data.Business;
using Azos.Platform;
using Azos.Time;
using Azos.Web;


namespace Azos.AuthKit.Server.MySql.Queries.Admin
{
  public sealed class SaveUser : MySqlCrudQueryHandler<UserEntity>
  {
    public SaveUser(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

    protected override async Task<Doc> DoExecuteProcedureParameterizedQueryAsync(MySqlCrudQueryExecutionContext ctx, Query query, UserEntity user)
    {
      var result = new EntityChangeInfo
      {
        Id = user.Id,
        Version = ctx.MakeVersionInfo(user.Gdid, user.FormMode)
      };

      if (user.FormMode == FormMode.Insert)
      {
        await ctx.ExecuteCompoundCommand(CommandTimeoutSec, System.Data.IsolationLevel.ReadCommitted,
          cmd => saveUser(true, cmd, result.Version, user)
        ).ConfigureAwait(false);
      }
      else
      {
        var affected = await ctx.ExecuteCompoundCommand(CommandTimeoutSec, System.Data.IsolationLevel.ReadCommitted,
          cmd => saveUser(false, cmd, result.Version, user)
        ).ConfigureAwait(false);

        if (affected < 1)
        {
          throw new HTTPStatusException(404,
           "User Entity not found", "Can not find `{0}`(Gdid=`{1}`) entity in expected realm to update".Args(user.GetType().Name, user.Gdid));
        }
      }
      return result;
    }

    private void saveUser(bool isInsert, MySqlCommand cmd, VersionInfo version, UserEntity user)
    {
      cmd.CommandText = GetType().GetText(isInsert ? "SaveUserInsert.sql" : "SaveUserUpdate.sql");

      DateRange validSpan = user.ValidSpanUtc.Value;

      cmd.MapVersionToSqlParameters(version);

      cmd.Parameters.AddWithValue("guid", Guid.NewGuid().ToNetworkByteOrder());
      cmd.Parameters.AddWithValue("realm", user.Realm);
      cmd.Parameters.AddWithValue("name", user.Name);
      cmd.Parameters.AddWithValue("level", Constraints.MapUserStatus(user.Level));
      cmd.Parameters.AddWithValue("description", user.Description);
      cmd.Parameters.AddWithValue("start_utc", validSpan.Start);
      cmd.Parameters.AddWithValue("end_utc", validSpan.End);
      cmd.Parameters.AddWithValue("org_unit", user.OrgUnit.HasValue ? user.OrgUnit.Value.AsString() : null); // should we pass null here?
      cmd.Parameters.AddWithValue("props", user.Props.Content);
      cmd.Parameters.AddWithValue("rights", user.Rights?.Content);
      cmd.Parameters.AddWithValue("note", user.Note);
    }

  }
}
