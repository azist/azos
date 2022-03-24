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

    protected override void DoBuildCommandAndParameters(MySqlCrudQueryExecutionContext context, MySqlCommand cmd, UserListFilter filter)
    {
      context.BuildSelect(cmd, filter, builder =>
      {
        builder.Limit = 100; // qParams.PagingCount.LimitPagingCount()

        builder.Select("T1.GDID")
               .Select("T1.REALM")
               .Select("T1.GUID")
               .Select("T1.NAME")
               .Select("T1.LEVEL")
               .Select("T1.DESCRIPTION")
               .Select("T1.START_UTC")
               .Select("T1.END_UTC")
               .Select("T1.ORG_UNIT")
               .Select("T1.PROPS")
               .Select("T1.RIGHTS")
               .Select("T1.NOTE")
               .Select("T1.CREATE_ACTOR")
               .Select("T1.CREATE_ORIGIN")
               .Select("T1.CREATE_UTC")
               .Select("T1.VERSION_ACTOR")
               .Select("T1.VERSION_ORIGIN")
               .Select("T1.VERSION_UTC")
               .Select("T1.VERSION_STATE")
               .Select("T1.LOCK_START_UTC")
               .Select("T1.LOCK_END_UTC")
               .Select("T1.LOCK_ACTOR")
               .Select("T1.LOCK_NOTE")
               ;

        builder.AndWhere("T1.REALM = @realm", new MySqlParameter("realm", filter.Realm));

        if (!filter.Gdid.IsZero)
        {
          builder.AndWhere("T1.GDID = @g_user", new MySqlParameter("g_user", filter.Gdid));
        }

        //FROM=======================================
        if (filter.LoginId.IsNotNullOrWhiteSpace())
        {
          builder.FromClause("tbl_user T1 INNER JOIN tbl_login T2 ON T1.GDID = T2.G_USER");
          builder.AndWhere("((T2.REALM = @realm) AND (T2.ID = @login_id))", new MySqlParameter("login_id", filter.LoginId));
        }
        else
        {
          builder.From("tbl_user", "T1");
        }
        //===========================================

        if (filter.Guid.HasValue)
        {
          builder.AndWhere("T1.GUID = @guid", new MySqlParameter("guid", filter.Guid));
        }

        if (filter.Level.HasValue)
        {
          var level = Constraints.MapUserStatus(filter.Level.Value);
          builder.AndWhere("T1.LEVEL = @level", new MySqlParameter("level", level));
        }

        if (filter.Name.IsNotNullOrWhiteSpace())
        {
          builder.AndWhere("T1.NAME = @name", new MySqlParameter("name", filter.Name));
        }

        if (filter.OrgUnit.IsNotNullOrWhiteSpace())
        {
          builder.AndWhere("T1.ORG_UNIT = @org_unit", new MySqlParameter("org_unit", filter.OrgUnit));
        }

        var asof = filter.AsOfUtc ?? context.GetUtcNow();
        cmd.Parameters.Add(new MySqlParameter("asof", asof));

        if (filter.Active.HasValue)
        {
          if (filter.Active.Value)
            builder.AndWhere("((T1.START_UTC <= @asof) AND (@asof < T1.END_UTC))");
          else
            builder.AndWhere("((@asof < T1.START_UTC) OR (@asof > T1.END_UTC))");
        }

        if (filter.Locked.HasValue)
        {
          if (filter.Locked.Value)
            builder.AndWhere("((T1.LOCK_START_UTC <= @asof) AND (@asof < T1.LOCK_END_UTC))");
          else
            builder.AndWhere("((T1.LOCK_START_UTC IS NULL) OR (@asof < T1.LOCK_START_UTC)) OR ((T1.LOCK_END_UTC IS NULL) OR (@asof > T1.LOCK_END_UTC))");
        }

        //TODO:  TagFilter properties to filter logic
        builder.OrderByAsc("T1.NAME");
      });//BuildSelect
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
        Level = Constraints.MapUserStatus(reader.AsStringField("LEVEL")).Value,
        Description = reader.AsStringField("DESCRIPTION"),
        ValidSpanUtc = new Time.DateRange(reader.AsDateTimeField("START_UTC"), reader.AsDateTimeField("END_UTC")),
        OrgUnit = reader.AsEntityIdField("ORG_UNIT"),
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
        LockActor = reader.AsEntityIdField("LOCK_ACTOR"),
        LockNote = reader.AsStringField("LOCK_NOTE")
      };

      return result;
    }
  }
}
