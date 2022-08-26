/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Data.Access.MySql;
using Azos.Data.Business;
using Azos.Platform;

using MySqlConnector;

namespace Azos.MySql.ConfForest.Queries.Tree
{
  public sealed class GetNodeVersionList : MySqlCrudQueryHandler<GDID>
  {
    public GetNodeVersionList(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

    protected override void DoBuildCommandAndParameters(MySqlCrudQueryExecutionContext context, MySqlCommand cmd, GDID gNode)
    {
      cmd.Parameters.AddWithValue("gnode", gNode);
      cmd.CommandText = GetType().GetText("GetNodeVersionList.sql");
    }

    protected override Doc DoPopulateDoc(MySqlCrudQueryExecutionContext context, Type tDoc, Schema schema, Schema.FieldDef[] toLoad, MySqlDataReader reader)
    {
      return new VersionInfo()
      {
        G_Version = reader.AsGdidField("GDID"),
        State = VersionInfo.MapCanonicalState(reader.AsStringField("VERSION_STATE")),
        Actor = reader.AsEntityIdField("VERSION_ACTOR").Value,
        Origin = reader.AsAtomField("VERSION_ORIGIN").Value,
        Utc = reader.AsDateTimeField("VERSION_UTC").Value
      };
    }
  }
}
