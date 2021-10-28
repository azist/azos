///*<FILE_LICENSE>
// * Azos (A to Z Application Operating System) Framework
// * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
// * See the LICENSE file in the project root for more information.
//</FILE_LICENSE>*/

//using System;

//using Azos;
//using Azos.Conf.Forest.Server;
//using Azos.Data;
//using Azos.Data.Access;
//using Azos.Data.Access.MySql;
//using Azos.Data.Business;
//using Azos.Platform;

//using MySqlConnector;

//namespace Azos.MySql.ConfForest.Queries.Tree
//{
//  public sealed class GetNodeInfo : MySqlCrudQueryHandler<Query>
//  {
//    public GetNodeInfo(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

//    protected override void DoBuildCommandAndParameters(MySqlCrudQueryExecutionContext context, MySqlCommand cmd, Query qry)
//    {
//      var gom = qry.GetParameterValueAs<GdidOrPath>("gom");
//      context.SetState(gom);

//      cmd.Parameters.AddWithValue("asof", qry.GetParameterValueAs<DateTime>("asof"));
//      cmd.Parameters.AddWithValue("etp", Data.Domains.G8CorpEntityType.MapToDb(gom.Type));

//      if (gom.Gdid.IsZero)
//      {
//        cmd.Parameters.AddWithValue("mnemonic", gom.Mnemonic);
//        cmd.CommandText = GetType().GetText("GetNodeInfoByMnemonic.sql");
//      }
//      else
//      {
//        cmd.Parameters.AddWithValue("gdid", gom.Gdid);
//        cmd.CommandText = GetType().GetText("GetNodeInfoByGdid.sql");
//      }
//    }

//    protected override Doc DoPopulateDoc(MySqlCrudQueryExecutionContext context, Type tDoc, Schema schema, Schema.FieldDef[] toLoad, MySqlDataReader reader)
//    {
//      var verState = G8DataState.Map(reader.AsStringField("VERSION_STATE"));
//      if (!VersionInfo.IsExistingStateOf(verState)) return null; //deleted, skip this doc

//      var gom = context.GetState<GdidOrPath>();
//      var result = CorporateNodeInfo.MakeOfType(gom.Type, reader.AsGdidField("G_PARENT"));

//      result.Gdid = reader.AsGdidField("GDID");
//      result.Caption = reader.AsStringField("CAPTION");
//      result.Mnemonic = reader.AsStringField("MNEMONIC");
//      result.StartUtc = reader.AsDateTimeField("START_UTC").Value;
//      result.Properties = G8ConfigScript.MapToConfigRoot(reader.AsStringField("PROPERTIES"));
//      result.LevelConfig = G8ConfigScript.MapToConfigRoot(reader.AsStringField("CONFIG"));
//      result.DataVersion = new VersionInfo
//      {
//        G_Version = reader.AsGdidField("G_VERSION"),
//        State = verState,
//        Actor = reader.AsEntityIdField("VERSION_ACTOR").Value,
//        Origin = reader.AsAtomField("VERSION_ORIGIN").Value,
//        Utc = reader.AsDateTimeField("VERSION_UTC").Value
//      };

//      return result;
//    }
//  }
//}
