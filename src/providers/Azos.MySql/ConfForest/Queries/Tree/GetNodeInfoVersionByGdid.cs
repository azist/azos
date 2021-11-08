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
  public sealed class GetNodeInfoVersionByGdid : MySqlCrudQueryHandler<GdidOrPath>
  {
    public GetNodeInfoVersionByGdid(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

    protected override void DoBuildCommandAndParameters(MySqlCrudQueryExecutionContext context, MySqlCommand cmd, GdidOrPath gop)
    {
      context.SetState(gop);
      cmd.Parameters.AddWithValue("gver", gop.GdidAddress);
      cmd.CommandText = GetType().GetText("GetNodeInfoVersionByGdid.sql");
    }

    protected override Doc DoPopulateDoc(MySqlCrudQueryExecutionContext context, Type tDoc, Schema schema, Schema.FieldDef[] toLoad, MySqlDataReader reader)
    {
      var verState = VersionInfo.MapCanonicalState(reader.AsStringField("VERSION_STATE"));
      var gop = context.GetState<GdidOrPath>();

      var result = new TreeNodeInfo();

      result.Forest   = gop.Id.System;
      result.Tree     = gop.Id.Type;
      result.Gdid     = reader.AsGdidField("GDID");
      result.G_Parent = reader.AsGdidField("G_PARENT");
      result.PathSegment = reader.AsStringField("PATH_SEGMENT");
      result.FullPath    = "[n/a for version specific node]";
      result.StartUtc    = reader.AsDateTimeField("START_UTC").Value;
      result.Properties  = Constraints.MapToConfigRoot(reader.AsStringField("PROPERTIES"));
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