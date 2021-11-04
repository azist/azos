///*<FILE_LICENSE>
// * Azos (A to Z Application Operating System) Framework
// * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
// * See the LICENSE file in the project root for more information.
//</FILE_LICENSE>*/

using System;

using Azos;
using Azos.Conf.Forest;
using Azos.Conf.Forest.Server;
using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Access.MySql;
using Azos.Data.Business;
using Azos.Platform;

using MySqlConnector;

namespace Azos.MySql.ConfForest.Queries.Tree
{
  public sealed class GetNodeInfo : MySqlCrudQueryHandler<Query>
  {
    public GetNodeInfo(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

    protected override void DoBuildCommandAndParameters(MySqlCrudQueryExecutionContext context, MySqlCommand cmd, Query qry)
    {
      var tpr = qry.GetParameterValueAs<TreePtr>("tpr");
      context.SetState(tpr);

      cmd.Parameters.AddWithValue("gparent", qry.GetParameterValueAs<GDID>("gparent"));
      cmd.Parameters.AddWithValue("psegment", qry.GetParameterValueAs<String>("psegment"));
      cmd.Parameters.AddWithValue("asof", qry.GetParameterValueAs<DateTime>("asof"));

      cmd.CommandText = GetType().GetText("GetNodeInfo.sql");
    }

    protected override Doc DoPopulateDoc(MySqlCrudQueryExecutionContext context, Type tDoc, Schema schema, Schema.FieldDef[] toLoad, MySqlDataReader reader)
    {
      var verState = VersionInfo.MapCanonicalState(reader.AsStringField("VERSION_STATE"));
      if (!VersionInfo.IsExistingStateOf(verState)) return null; //deleted, skip this doc

      var tpr = context.GetState<TreePtr>();

      var result = new TreeNodeInfo();

      result.Forest = tpr.IdForest;
      result.Tree = tpr.IdTree;
      result.Gdid = reader.AsGdidField("GDID");
      result.G_Parent = reader.AsGdidField("G_PARENT");
      result.PathSegment = reader.AsStringField("PATH_SEGMENT");
      result.FullPath = null;
      result.StartUtc = reader.AsDateTimeField("START_UTC").Value;
      result.Properties = Constraints.MapToConfigRoot(reader.AsStringField("PROPERTIES"));
      result.LevelConfig = Constraints.MapToConfigRoot(reader.AsStringField("CONFIG"));
      result.DataVersion = new VersionInfo
      {
        G_Version = reader.AsGdidField("G_VERSION"),
        State = verState,
        Actor = reader.AsEntityIdField("VERSION_ACTOR").Value,
        Origin = reader.AsAtomField("VERSION_ORIGIN").Value,
        Utc = reader.AsDateTimeField("VERSION_UTC").Value
      };

      return result;
    }
  }
}



// you can pass <Query> or <T> using override virtual CastParameters methods