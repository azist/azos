///*<FILE_LICENSE>
// * Azos (A to Z Application Operating System) Framework
// * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
// * See the LICENSE file in the project root for more information.
//</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos;
using Azos.Conf.Forest;
using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Access.MySql;
using Azos.Data.Business;
using Azos.Platform;
using Azos.Serialization.JSON;

using MySqlConnector;

namespace Azos.MySql.ConfForest.Queries.Tree
{
  public sealed class GetChildNodeList : MySqlCrudQueryHandler<Query>
  {
    public GetChildNodeList(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

    protected override void DoBuildCommandAndParameters(MySqlCrudQueryExecutionContext context, MySqlCommand cmd, Query qry)
    {
      var eid = qry.GetParameterValueAs<EntityId>("eid");
      context.SetState(eid);

      cmd.Parameters.AddWithValue("gparent", qry.GetParameterValueAs<GDID>("gparent"));
      cmd.Parameters.AddWithValue("asof", qry.GetParameterValueAs<DateTime>("asof"));

      cmd.CommandText = GetType().GetText("GetChildNodeList.sql");
    }

    protected override Doc DoPopulateDoc(MySqlCrudQueryExecutionContext context, Type tDoc, Schema schema, Schema.FieldDef[] toLoad, MySqlDataReader reader)
    {
      var json = (reader.AsStringField("DATA").JsonToDataObject() as JsonDataMap).NonNull("DATA!null");
      var eid = context.GetState<EntityId>();
      var isDeleted = VersionInfo.MapCanonicalState(json["version_state"].AsString()) == VersionInfo.DataState.Deleted;

      if (isDeleted) return null;

      return new TreeNodeHeader
      {
        Id = eid,
        G_Version = json["g_ver"].AsGDID(),
        PathSegment = json["psegment"].AsString(),
        StartUtc = json["start_utc"].AsDateTime(CoreConsts.UTC_TIMESTAMP_STYLES)
      };
    }
  }
}
