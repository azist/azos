///*<FILE_LICENSE>
// * Azos (A to Z Application Operating System) Framework
// * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
// * See the LICENSE file in the project root for more information.
//</FILE_LICENSE>*/

//using System;
//using System.Threading.Tasks;
//using Azos;
//using Azos.Data;
//using Azos.Data.Access;
//using Azos.Data.Access.MySql;
//using Azos.Platform;
//using Azos.Serialization.JSON;

//using MySqlConnector;

//namespace Azos.MySql.ConfForest.Queries.Tree
//{
//  public sealed class GetChildNodeList : MySqlCrudQueryHandler<Query>
//  {
//    public GetChildNodeList(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

//    protected override void DoBuildCommandAndParameters(MySqlCrudQueryExecutionContext context, MySqlCommand cmd, Query qry)
//    {
//      var entityType = qry.GetParameterValueAs<Atom>("etp");
//      context.SetState(entityType);

//      cmd.Parameters.AddWithValue("etp", G8.Data.Domains.G8CorpEntityType.MapToDb(entityType));
//      cmd.Parameters.AddWithValue("gparent", qry.GetParameterValueAs<GDID>("gparent"));
//      cmd.Parameters.AddWithValue("asof", qry.GetParameterValueAs<DateTime>("asof"));

//      cmd.CommandText = GetType().GetText("GetChildNodeList.sql");
//    }

//    protected override Doc DoPopulateDoc(MySqlCrudQueryExecutionContext context, Type tDoc, Schema schema, Schema.FieldDef[] toLoad, MySqlDataReader reader)
//    {
//      var json = (reader.AsStringField("DATA").JsonToDataObject() as JsonDataMap).NonNull("DATA!null");
//      var entityType = context.GetState<Atom>();
//      var isDeleted = Data.Domains.G8DataState.Map(json["version_state"].AsString()) == Azos.Data.Business.VersionInfo.DataState.Deleted;

//      if (isDeleted) return null;

//      return new ListItem
//      {
//        Id = EntityIds.Corporate.OfType(entityType,  json["gdid"].AsGDID()),
//        G_Version = json["g_ver"].AsGDID(),
//        Mnemonic  = json["mnemonic"].AsString(),
//        Caption   = json["caption"].AsString(),
//        StartUtc  = json["start_utc"].AsDateTime(CoreConsts.UTC_TIMESTAMP_STYLES)
//      };
//    }
//  }
//}
