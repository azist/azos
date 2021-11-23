///*<FILE_LICENSE>
// * Azos (A to Z Application Operating System) Framework
// * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
// * See the LICENSE file in the project root for more information.
//</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Access.MySql;

namespace Azos.MySql.ConfForest.Queries.Tree
{
  public sealed class Purge : MySqlCrudQueryHandler<Query>
  {
    public Purge(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

    protected override async Task<Doc> DoExecuteProcedureParameterizedQueryAsync(MySqlCrudQueryExecutionContext ctx, Query query, Query _)
    {
      await ctx.ExecuteCompoundCommand(CommandTimeoutSec, System.Data.IsolationLevel.ReadUncommitted,
        cmd => cmd.CommandText = "SET FOREIGN_KEY_CHECKS = 0",
        cmd => cmd.CommandText = "TRUNCATE tbl_nodelog",
        cmd => cmd.CommandText = "TRUNCATE tbl_node",
        cmd => cmd.CommandText = "SET FOREIGN_KEY_CHECKS = 1",
        /* create SEED ROOT NODE data */
        cmd => cmd.CommandText = "insert into `tbl_node` (`GDID`, `CREATE_UTC`) values (0x000000000000000000000001, utc_timestamp())",
        cmd => cmd.CommandText = @"
        insert into `tbl_nodelog`
        (
          `GDID`,
          `VERSION_UTC`,
          `VERSION_ORIGIN`,
          `VERSION_ACTOR`,
          `VERSION_STATE`,
          `G_NODE`,
          `G_PARENT`,
          `PATH_SEGMENT`,
          `START_UTC`,
          `PROPERTIES`,
          `CONFIG`
        )
        values
        (
          0x000000000000000000000001, -- GDID
          utc_timestamp(),  -- VERSION_UTC
          7567731,          -- VERSION_ORIGIN - 0x737973 = `sys`
          'usrn@idp::root', -- VERSION_ACTOR
          'C',-- VERSION_STATE - 'C' = created
          0x000000000000000000000001, -- G_NODE
          0x000000000000000000000001, -- G_PARENT
          '/', -- PATH_SEGMENT
          '1000-01-01 00:00:00', -- START_UTC
          '{""r"": {}}', -- PROPERTIES
          '{""r"": {}}'-- CONFIG
        )"
      ).ConfigureAwait(false);

      return new RowsAffectedDoc(0) { ProviderResult = "Purged" };
    }

  }
}
