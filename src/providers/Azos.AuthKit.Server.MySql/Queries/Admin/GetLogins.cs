/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Data.Access.MySql;
using Azos.Data.Business;
using Azos.Platform;
using Azos.Security.Authkit;
using MySqlConnector;


namespace Azos.AuthKit.Server.MySql.Queries.Admin
{
  public sealed class GetLogins : MySqlCrudQueryHandler<GDID>
  {
    public GetLogins(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

    protected override void DoBuildCommandAndParameters(MySqlCrudQueryExecutionContext context, MySqlCommand cmd, GDID gUser)
    {
      //new UserManagementPermission(UserManagementAccessLevel.View);

      //context.GetApp().SecurityManager.Authorize(Ambient.CurrentCallUser, )

      //context.SetState(actx);
      cmd.Parameters.AddWithValue("g_user", gUser);

      cmd.CommandText = GetType().GetText("GetLogins.sql");

    }



    protected override Doc DoPopulateDoc(MySqlCrudQueryExecutionContext context, Type tDoc, Schema schema, Schema.FieldDef[] toLoad, MySqlDataReader reader)
    {
      var verState = VersionInfo.MapCanonicalState(reader.AsStringField("VERSION_STATE"));
      if (!VersionInfo.IsExistingStateOf(verState)) return null; //deleted, skip this doc

      var result = new UserInfo()
      {
        Gdid = reader.AsGdidField("GDID"),
        Realm = reader.AsAtomField("REALM").Value,
        Guid = reader.AsGuidField("GUID").Value,
        Name = reader.AsStringField("NAME"),
        Level = Constraints.MapUserStatus(reader.AsString("LEVEL")).Value,
        Description = reader.AsStringField("DESCRIPTION"),
        ValidSpanUtc = new Time.DateRange(reader.AsDateTimeField("START_UTC"), reader.AsDateTimeField("END_UTC")),
        OrgUnit = reader.AsEntityIdField("ORG_UNIT").Value,
        Props = reader.AsStringField("PROPS"),
        Rights = reader.AsStringField("RIGHTS"),
        Note = reader.AsStringField("NOTE"),
        CreateVersion = new VersionInfo
        {
          G_Version = reader.AsGdidField("GDID"),
          State = VersionInfo.DataState.Created,
          Actor = reader.AsEntityIdField("CREATE_ACTOR").Value,
          Origin = reader.AsAtomField("CREATE_ORIGIN").Value,
          Utc = reader.AsDateTimeField("CREATE_UTC").Value
        },
        DataVersion = new VersionInfo
        {
          G_Version = reader.AsGdidField("GDID"),
          State = verState,
          Actor = reader.AsEntityIdField("VERSION_ACTOR").Value,
          Origin = reader.AsAtomField("VERSION_ORIGIN").Value,
          Utc = reader.AsDateTimeField("VERSION_UTC").Value
        },
        LockSpanUtc = new Time.DateRange(reader.AsDateTimeField("LOCK_START_UTC"), reader.AsDateTimeField("LOCK_END_UTC")),
        LockActor = reader.AsEntityIdField("LOCK_ACTOR").Value,
        LockNote = reader.AsStringField("LOCK_NOTE")
      };

      return result;
    }
  }
}
