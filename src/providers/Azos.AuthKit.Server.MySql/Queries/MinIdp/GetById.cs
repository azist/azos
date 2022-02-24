/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azos.Apps.Injection;
using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Access.MySql;
using Azos.Data.Business;
using Azos.Platform;
using Azos.Security.MinIdp;

using MySqlConnector;


namespace Azos.AuthKit.Server.MySql.Queries.MinIdp
{
  public sealed class GetById : MySqlCrudQueryHandler<AuthContext>
  {
    public GetById(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

    protected override void DoBuildCommandAndParameters(MySqlCrudQueryExecutionContext context, MySqlCommand cmd, AuthContext actx)
    {
      context.SetState(actx);
      cmd.Parameters.AddWithValue("realm",    actx.Realm);
      cmd.Parameters.AddWithValue("id",       actx.LoginId.Address);
      cmd.Parameters.AddWithValue("tid",      actx.LoginId.Type);
      cmd.Parameters.AddWithValue("provider", actx.LoginId.System);

      cmd.CommandText = GetType().GetText("GetById.sql");
    }

    protected override Task<RowsetBase> DoExecuteParameterizedQueryAsync(MySqlCrudQueryExecutionContext ctx, Query query, AuthContext queryParameters)
    {
      return base.DoExecuteParameterizedQueryAsync(ctx, query, queryParameters);
    }

    //we use this because we need a reader, but we read into INPUT parameter, hence we return affected dummy
    protected override Doc DoPopulateDoc(MySqlCrudQueryExecutionContext context, Type tDoc, Schema schema, Schema.FieldDef[] toLoad, MySqlDataReader reader)
    {
      var ctx = context.GetState<AuthContext>();

      var verState = VersionInfo.MapCanonicalState(reader.AsStringField("VERSION_STATE"));
      if (!VersionInfo.IsExistingStateOf(verState)) return null; //deleted, skip this doc

      verState = VersionInfo.MapCanonicalState(reader.AsStringField("LOGIN_VERSION_STATE"));
      if (!VersionInfo.IsExistingStateOf(verState)) return null; //deleted, skip this doc

      ctx.HasResult = true;//FOUND!!!!!!!!!!!!
      ctx.G_User = reader.AsGdidField("GDID");
      ctx.SysId = ctx.G_User.ToHexString();
      ctx.Name = reader.AsStringField("NAME");
      ctx.ScreenName = ctx.Name;
      ctx.Description = reader.AsStringField("DESCRIPTION");
      ctx.Note = reader.AsStringField("NOTE");
      ctx.Rights = reader.AsStringField("RIGHTS");
      ctx.Props = reader.AsStringField("PROPS");


      var level = Constraints.MapUserStatus(reader.AsString("LEVEL")) ?? Security.UserStatus.Invalid;
      var levelDown = Constraints.MapUserStatus(reader.AsString("LEVEL_DOWN"));
      if (levelDown.HasValue && levelDown.Value < level) level = levelDown.Value;//the LEAST wins

      ctx.Status = level;
      ctx.CreateUtc = reader.AsDateTimeField("CREATE_UTC").Value;
      ctx.StartUtc = reader.AsDateTimeField("START_UTC").Value;
      ctx.EndUtc = reader.AsDateTimeField("END_UTC").Value;

      ctx.LoginPassword = reader.AsStringField("PWD");
      ctx.LoginStartUtc = reader.AsDateTimeField("LOGIN_START_UTC").Value;
      ctx.LoginEndUtc = reader.AsDateTimeField("LOGIN_END_UTC").Value;

      ctx.LoginProps = reader.AsStringField("LOGIN_PROPS");
      ctx.LoginRights = reader.AsStringField("LOGIN_RIGHTS");


      //var result = new MinIdpUserData
      //{
      //  SysId = gUser.ToHexString(),
      //  Realm = realm,
      //  SysTokenData = sysToken,
      //  Status = level,

      //  CreateUtc = reader.AsDateTimeField("CREATE_UTC", DateTime.MinValue).Value,
      //  StartUtc = reader.AsDateTimeField("START_UTC", DateTime.MinValue).Value,
      //  EndUtc = reader.AsDateTimeField("END_UTC", DateTime.MaxValue).Value,
      //  LoginId = id,
      //  LoginPassword = reader.AsStringField("PWD"),
      //  LoginStartUtc = reader.AsDateTimeField("LOGIN_START_UTC"),
      //  LoginEndUtc = reader.AsDateTimeField("LOGIN_START_UTC"),
      //  ScreenName = name,
      //  Name = name,
      //  Description = reader.AsStringField("DESCRIPTION"),
      //  Note = reader.AsStringField("NOTE"),

      //  Role = null, // TODO: extract role from PROPS column data
      //  Rights = reader.AsStringField("RIGHTS"),
      //  Props = reader.AsStringField("PROPS")
      //};


      return new RowsAffectedDoc(1);//we use this because we need a reader, but we read into INPUT parameter, hence we return affected dummy
    }
  }
}
