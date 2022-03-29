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
using Azos.Security;
using Azos.Security.Authkit;
using MySqlConnector;


namespace Azos.AuthKit.Server.MySql.Queries.Admin
{
  public sealed class GetLogins : MySqlCrudQueryHandler<GDID>
  {
    public static readonly Permission SEC_USER_MGMT = new UserManagementPermission();
    public GetLogins(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

    protected override void DoBuildCommandAndParameters(MySqlCrudQueryExecutionContext context, MySqlCommand cmd, GDID gUser)
    {
      var alevel = context.GetApp().GetAccessLevel(SEC_USER_MGMT);
      var disclosePasswords = alevel.Level >= AccessLevel.VIEW_CHANGE_DELETE;

      context.SetState(disclosePasswords);
      cmd.Parameters.AddWithValue("g_user", gUser);
      cmd.Parameters.AddWithValue("realm", Ambient.CurrentCallSession.GetAtomDataContextName());

      cmd.CommandText = GetType().GetText("GetLogins.sql");
    }


    protected override Doc DoPopulateDoc(MySqlCrudQueryExecutionContext context, Type tDoc, Schema schema, Schema.FieldDef[] toLoad, MySqlDataReader reader)
    {
      var disclosePasswords = context.GetState<bool>();

      var verState = VersionInfo.MapCanonicalState(reader.AsStringField("VERSION_STATE"));
      if (!VersionInfo.IsExistingStateOf(verState)) return null; //deleted, skip this doc

      var result = new LoginInfo()
      {
        Gdid = reader.AsGdidField("GDID"),
        Realm = reader.AsAtomField("REALM").Value,
        G_User = reader.AsGdidField("G_USER"),
        LevelDemotion = Constraints.MapUserStatus(reader.AsString("LEVEL_DOWN")).Value,
        LoginId = reader.AsStringField("ID"),
        LoginType = reader.AsAtomField("TID").Value,
        Provider = reader.AsAtomField("PROVIDER").Value,
        Password = disclosePasswords ? reader.AsStringField("PWD") : "*****",
        ProviderData = reader.AsStringField("PROVIDER_DATA"),
        ValidSpanUtc = new Time.DateRange(reader.AsDateTimeField("START_UTC"), reader.AsDateTimeField("END_UTC")),
        Props = reader.AsStringField("PROPS"),
        Rights = reader.AsStringField("RIGHTS"),
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
        LockActor = reader.AsEntityIdField("LOCK_ACTOR"),
        LockNote = reader.AsStringField("LOCK_NOTE")
      };

      return result;
    }
  }
}
