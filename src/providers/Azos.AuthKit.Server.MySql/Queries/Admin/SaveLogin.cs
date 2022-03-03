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
  public sealed class SaveLogin : MySqlCrudQueryHandler<LoginEntity>
  {
    public SaveLogin(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

    protected override async Task<Doc> DoExecuteProcedureParameterizedQueryAsync(MySqlCrudQueryExecutionContext ctx, Query query, LoginEntity login)
    {
      var result = new EntityChangeInfo
      {
        Id = login.Id,
        Version = ctx.MakeVersionInfo(login.Gdid, login.FormMode)
      };

      if (login.FormMode == FormMode.Insert)
      {
        await ctx.ExecuteCompoundCommand(CommandTimeoutSec, System.Data.IsolationLevel.ReadCommitted,
          cmd => saveLogin(true, cmd, result.Version, login)
        ).ConfigureAwait(false);
      }
      else
      {
        var affected = await ctx.ExecuteCompoundCommand(CommandTimeoutSec, System.Data.IsolationLevel.ReadCommitted,
          cmd => saveLogin(false, cmd, result.Version, login)
        ).ConfigureAwait(false);

        if (affected < 1)
        {
          throw new HTTPStatusException(404,
           "Login Entity not found", "Can not find `{0}`(Gdid=`{1}`) entity in expected realm to update".Args(login.GetType().Name, login.Gdid));
        }
      }
      return result;
    }

    private void saveLogin(bool isInsert, MySqlCommand cmd, VersionInfo version, LoginEntity login)
    {
      cmd.CommandText = GetType().GetText(isInsert ? "SaveLoginInsert.sql" : "SaveLoginUpdate.sql");

      DateRange validSpan = login.ValidSpanUtc.Value;

      cmd.MapVersionToSqlParameters(version);

      cmd.Parameters.AddWithValue("gdid", login.Gdid);
      cmd.Parameters.AddWithValue("realm", login.Realm);
      cmd.Parameters.AddWithValue("g_user", login.G_User);
      cmd.Parameters.AddWithValue("level_down", Constraints.MapUserStatus(login.LevelDemotion));
      cmd.Parameters.AddWithValue("login_id", login.LoginId);
      cmd.Parameters.AddWithValue("tid", login.LoginType);
      cmd.Parameters.AddWithValue("provider", login.Provider);
      cmd.Parameters.AddWithValue("pwd", login.Password);
      cmd.Parameters.AddWithValue("provider_data", login.ProviderData);
      cmd.Parameters.AddWithValue("start_utc", validSpan.Start);
      cmd.Parameters.AddWithValue("end_utc", validSpan.End);
      cmd.Parameters.AddWithValue("props", login.Props.Content);
      cmd.Parameters.AddWithValue("rights", login.Rights.Content);
    }

  }
}
