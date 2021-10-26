///*<FILE_LICENSE>
// * Azos (A to Z Application Operating System) Framework
// * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
// * See the LICENSE file in the project root for more information.
//</FILE_LICENSE>*/

//using System;

//using Azos;
//using Azos.Data;
//using Azos.Data.Access;
//using Azos.Data.Access.MySql;
//using Azos.Data.Business;
//using Azos.Platform;

//using MySqlConnector;

//namespace Azos.MySql.ConfForest.Queries.Tree
//{
//  public sealed class GetNodeInfoOfVersion : MySqlCrudQueryHandler<Query>
//  {
//    public GetNodeInfoOfVersion(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

//    protected override void DoBuildCommandAndParameters(MySqlCrudQueryExecutionContext context, MySqlCommand cmd, Query qry)
//    {
//      var etp = qry.GetParameterValueAs<Atom>("etp");
//      context.SetState(etp);

//      cmd.Parameters.AddWithValue("etp", G8CorpEntityType.MapToDb(etp));
//      cmd.Parameters.AddWithValue("gv", qry.GetParameterValueAs<GDID>("gv"));
//      cmd.CommandText = GetType().GetText("GetNodeInfoOfVersion.sql");
//    }

//    protected override Doc DoPopulateDoc(MySqlCrudQueryExecutionContext context, Type tDoc, Schema schema, Schema.FieldDef[] toLoad, MySqlDataReader reader)
//    {
//      var etp = context.GetState<Atom>();
//      var result = CorporateNodeInfo.MakeOfType(etp, reader.AsGdidField("G_PARENT"));

//      result.Gdid = reader.AsGdidField("GDID");
//      result.Caption = reader.AsStringField("CAPTION");
//      result.Mnemonic = reader.AsStringField("MNEMONIC");
//      result.StartUtc = reader.AsDateTimeField("START_UTC").Value;
//      result.Properties = G8ConfigScript.MapToConfigRoot(reader.AsStringField("PROPERTIES"));
//      result.LevelConfig = G8ConfigScript.MapToConfigRoot(reader.AsStringField("CONFIG"));
//      result.DataVersion = new VersionInfo
//      {
//        G_Version = reader.AsGdidField("G_VERSION"),
//        State = G8DataState.Map(reader.AsStringField("VERSION_STATE")),
//        Actor = reader.AsEntityIdField("VERSION_ACTOR").Value,
//        Origin = reader.AsAtomField("VERSION_ORIGIN").Value,
//        Utc = reader.AsDateTimeField("VERSION_UTC").Value
//      };

//      return result;
//    }
//  }
//}
