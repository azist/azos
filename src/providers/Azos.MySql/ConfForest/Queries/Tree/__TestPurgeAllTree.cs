/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;
using Azos;
using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Access.MySql;


namespace Azos.MySql.ConfForest.Queries.Tree
{
  public sealed class __TestPurgeAllTree : MySqlCrudQueryHandler<Query>
  {
    public __TestPurgeAllTree(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

    protected override async Task<Doc> DoExecuteProcedureParameterizedQueryAsync(MySqlCrudQueryExecutionContext ctx, Query query, Query _)
    {
      if (!ctx.GetApp().IsUnitTest)
      {
        throw new CallGuardException(nameof(__TestPurgeAllTree), "app.$unit-test",
          "This db query handler may ONLY be called by app chassis running in TESTING mode" +
          " as indicated by the `unit-test` app chassis attribute");
      }

      await ctx.ExecuteCompoundCommand(CommandTimeoutSec, System.Data.IsolationLevel.ReadUncommitted,
        cmd => cmd.CommandText = "SET FOREIGN_KEY_CHECKS = 0",
        cmd => cmd.CommandText = "TRUNCATE tbl_hnodelog",
        cmd => cmd.CommandText = "TRUNCATE tbl_hnode",
        cmd => cmd.CommandText = "SET FOREIGN_KEY_CHECKS = 1"
      ).ConfigureAwait(false);


      return new RowsAffectedDoc(0){ ProviderResult = "Purged" };
    }
  }
}
