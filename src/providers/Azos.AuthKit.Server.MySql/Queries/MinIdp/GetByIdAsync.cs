using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Access.MySql;
using Azos.Data.Business;
using Azos.Data.Modeling.DataTypes;
using Azos.Platform;
using Azos.Security.MinIdp;

using MySqlConnector;

using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.AuthKit.Server.MySql.Queries.MinIdp
{
  public sealed class GetByIdAsync : MySqlCrudQueryHandler<Query>
  {
    public GetByIdAsync(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

    protected override void DoBuildCommandAndParameters(MySqlCrudQueryExecutionContext context, MySqlCommand cmd, Query qry)
    {
      //var tpr = qry.GetParameterValueAs<TreePtr>("tree");
      //context.SetState(tpr);

      cmd.Parameters.AddWithValue("realm", qry.GetParameterValueAs<Atom>("realm"));
      cmd.Parameters.AddWithValue("id", qry.GetParameterValueAs<string>("id"));
      cmd.Parameters.AddWithValue("tid", qry.GetParameterValueAs<Atom>("tid"));
      cmd.Parameters.AddWithValue("provider", qry.GetParameterValueAs<string>("provider"));

      cmd.CommandText = GetType().GetText("GetUserById.sql");
    }

    protected override Doc DoPopulateDoc(MySqlCrudQueryExecutionContext context, Type tDoc, Schema schema, Schema.FieldDef[] toLoad, MySqlDataReader reader)
    {
      var verState = VersionInfo.MapCanonicalState(reader.AsStringField("VERSION_STATE"));
      if (!VersionInfo.IsExistingStateOf(verState)) return null; //deleted, skip this doc

      var result = new MinIdpUserData
      {
        SysId = reader.AsGdidField("GDID").ID.ToString(),
        Realm = reader.AsAtomField("REALM", Atom.ZERO).Value,
        // TODO: Add SysToken Logic
        // TODO: Add Status Logic - LEVEL I|U|A|S and LEVEL_DOWN?
        CreateUtc = reader.AsDateTimeField("CREATE_UTC", DateTime.MinValue).Value,
        StartUtc = reader.AsDateTimeField("START_UTC", DateTime.MinValue).Value,
        EndUtc = reader.AsDateTimeField("END_UTC", DateTime.MaxValue).Value,
        LoginId = reader.AsStringField("ID"),
        LoginPassword = reader.AsStringField("PWD"),
        LoginStartUtc = reader.AsDateTimeField("LOGIN_START_UTC"),
        LoginEndUtc = reader.AsDateTimeField("LOGIN_START_UTC"),
        ScreenName = reader.AsStringField("NAME"), // TODO: review this!
        Name = reader.AsStringField("NAME"),
        Description = reader.AsStringField("DESCRIPTION"),
        Role = null, // TODO: extract role from PROPS column data
        Rights = reader.AsStringField("RIGHTS"),
        Note = reader.AsStringField("NOTE")
      };


      return result;
    }
  }
}
