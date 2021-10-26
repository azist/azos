///*<FILE_LICENSE>
// * Azos (A to Z Application Operating System) Framework
// * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
// * See the LICENSE file in the project root for more information.
//</FILE_LICENSE>*/

//using System;

//using Azos;
//using Azos.Data;
//using Azos.Data.Access.MySql;
//using Azos.Platform;
//using Azos.Serialization.JSON;

//using MySqlConnector;

//namespace Azos.MySql.ConfForest.Queries.Tree
//{
//  public sealed class GetEnterpriseNodeList : MySqlCrudQueryHandler<DateTime>
//  {
//    public GetEnterpriseNodeList(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

//    protected override void DoBuildCommandAndParameters(MySqlCrudQueryExecutionContext context, MySqlCommand cmd, DateTime asof)
//    {
//      ////Sql builder is not neede here as it is easier to write query by hand as SQL
//      //context.BuildSelect(cmd, builder =>
//      //{
//      //  builder.Select("T1.COLUMN")
//      //         .Select("T2.COLUMN")
//      //         .From("tbl_hnode", "T1")
//      //         .AndWhere("T1.START_UTC <= @asof", new MySqlParameter("asof", asof));

//      //  builder.OrderByDesc("T1.COL1");
//      //});
//      cmd.CommandText = GetType().GetText("GetEnterpriseNodeList.sql");
//      cmd.Parameters.AddWithValue("asof", asof);
//    }

//    protected override Doc DoPopulateDoc(MySqlCrudQueryExecutionContext context, Type tDoc, Schema schema, Schema.FieldDef[] toLoad, MySqlDataReader reader)
//    {
//      var raw = reader.AsStringField("DATA");
//      if (raw.IsNullOrWhiteSpace()) return null;
//      var json = (raw.JsonToDataObject() as JsonDataMap).NonNull("DATA!null");

//      var isDeleted = Data.Domains.G8DataState.Map(json["version_state"].AsString()) == Azos.Data.Business.VersionInfo.DataState.Deleted;
//      if (isDeleted) return null;

//      return new ListItem
//      {
//        Id = EntityIds.Corporate.OfEnterprise(json["gdid"].AsGDID()),
//        G_Version = json["g_ver"].AsGDID(),
//        Mnemonic  = json["mnemonic"].AsString(),
//        Caption   = json["caption"].AsString(),
//        StartUtc  = json["start_utc"].AsDateTime(CoreConsts.UTC_TIMESTAMP_STYLES)
//      };
//    }
//  }
//}
