/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

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
  public sealed class GetById : MySqlCrudQueryHandler<Query>
  {
    public GetById(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

    [Inject] IIdpHandlerLogic m_Logic;

    protected override void DoBuildCommandAndParameters(MySqlCrudQueryExecutionContext context, MySqlCommand cmd, Query qry)
    {
      var id = qry.GetParameterValueAs<EntityId>("id");
      context.SetState(id);
      cmd.Parameters.AddWithValue("realm",    qry.GetParameterValueAs<Atom>("realm"));
      cmd.Parameters.AddWithValue("id",       id.Address);
      cmd.Parameters.AddWithValue("tid",      id.Type);
      cmd.Parameters.AddWithValue("provider", id.System);

      cmd.CommandText = GetType().GetText("GetById.sql");
    }

    protected override Doc DoPopulateDoc(MySqlCrudQueryExecutionContext context, Type tDoc, Schema schema, Schema.FieldDef[] toLoad, MySqlDataReader reader)
    {
      var verState = VersionInfo.MapCanonicalState(reader.AsStringField("VERSION_STATE"));
      if (!VersionInfo.IsExistingStateOf(verState)) return null; //deleted, skip this doc

      var id = context.GetState<EntityId>();

      var realm = reader.AsAtomField("REALM").Value;
      var gUser = reader.AsGdidField("GDID");
      var sysToken = m_Logic.MakeSystemTokenData(realm, gUser);
      var name = reader.AsStringField("NAME");

      var level = Constraints.MapUserStatus(reader.AsString("LEVEL")) ?? Security.UserStatus.Invalid;
      var levelDown = Constraints.MapUserStatus(reader.AsString("LEVEL_DOWN"));
      if (levelDown.HasValue && levelDown.Value < level) level = levelDown.Value;

      var result = new MinIdpUserData
      {
        SysId = gUser.ToHexString(),
        Realm = realm,
        SysTokenData = sysToken,
        Status = level,

        CreateUtc = reader.AsDateTimeField("CREATE_UTC", DateTime.MinValue).Value,
        StartUtc = reader.AsDateTimeField("START_UTC", DateTime.MinValue).Value,
        EndUtc = reader.AsDateTimeField("END_UTC", DateTime.MaxValue).Value,
        LoginId = id,
        LoginPassword = reader.AsStringField("PWD"),
        LoginStartUtc = reader.AsDateTimeField("LOGIN_START_UTC"),
        LoginEndUtc = reader.AsDateTimeField("LOGIN_START_UTC"),
        ScreenName = name,
        Name = name,
        Description = reader.AsStringField("DESCRIPTION"),
        Note = reader.AsStringField("NOTE"),

        Role = null, // TODO: extract role from PROPS column data
        Rights = reader.AsStringField("RIGHTS"),
        Props = null
      };


      return result;
    }
  }
}
