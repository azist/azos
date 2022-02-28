/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Data.Access.MySql;
using Azos.Data.Business;

using MySqlConnector;


namespace Azos.AuthKit.Server.MySql.Queries.Admin
{
  public sealed class GetUserList : MySqlCrudQueryHandler<UserListFilter>
  {
    public GetUserList(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

    protected override void DoBuildCommandAndParameters(MySqlCrudQueryExecutionContext context, MySqlCommand cmd, UserListFilter qParams)
    {
      context.BuildSelect(cmd, qParams, builder =>
      {
        builder.Limit = 100; // qParams.PagingCount.LimitPagingCount()

        builder.Select("T1.GDID");

        builder.AndWhere("T1.REALM = @realm", new MySqlParameter("realm", qParams.Realm));

        if (!qParams.Gdid.IsZero)
        {
          builder.AndWhere("T1.GDID = @g_user", new MySqlParameter("g_user", qParams.Gdid));
        }

        if (qParams.LoginId.IsNotNullOrWhiteSpace())
        {
          builder.FromClause("tbl_user T1 INNER JOIN tbl_login T2 ON T1.GDID = T2.G_USER");
          builder.AndWhere("T2.ID = @login_id", new MySqlParameter("login_id", qParams.LoginId));
        }
        else
        {
          builder.From("tbl_user", "T1");
        }

        if (qParams.Guid.HasValue)
        {
          builder.AndWhere("T1.GUID = @guid", new MySqlParameter("guid", qParams.Guid));
        }

        if (qParams.Level.HasValue)
        {
          var level = Constraints.MapUserStatus(qParams.Level.Value);
          builder.AndWhere("T1.LEVEL = @level", new MySqlParameter("level", level));
        }

        if (qParams.Name.IsNotNullOrWhiteSpace())
        {
          builder.AndWhere("T1.NAME = @name", new MySqlParameter("name", qParams.Name));
        }

        if (qParams.OrgUnit.IsNotNullOrWhiteSpace())
        {
          builder.AndWhere("T1.ORG_UNIT = @org_unit", new MySqlParameter("org_unit", qParams.OrgUnit));
        }

        //TODO: add AsOfUtc, Active, Locked, TagFilter properties to filter logic

      });

      ////////////Console.WriteLine(cmd.CommandText);

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
