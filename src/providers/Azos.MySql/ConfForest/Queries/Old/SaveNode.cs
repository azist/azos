///*<FILE_LICENSE>
// * Azos (A to Z Application Operating System) Framework
// * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
// * See the LICENSE file in the project root for more information.
//</FILE_LICENSE>*/

//using System;
//using System.Threading.Tasks;
//using Azos;
//using Azos.Conf.Forest;
//using Azos.Data;
//using Azos.Data.Access;
//using Azos.Data.Access.MySql;
//using Azos.Data.Business;
//using Azos.Platform;

//using MySqlConnector;

//namespace Azos.MySql.ConfForest.Queries.Tree
//{
//  public sealed class SaveNode : MySqlCrudQueryHandler<TreeNode>
//  {
//    public SaveNode(MySqlCrudDataStoreBase store, string name) : base(store, name) { }

//    protected override async Task<Doc> DoExecuteProcedureParameterizedQueryAsync(MySqlCrudQueryExecutionContext ctx, Query query, TreeNode node)
//    {
//      var result = new EntityChangeInfo
//      {
//        Id = node.Id,
//        Version = ctx.MakeVersionInfo(EntityIds.Corporate.ID_NS_CORPORATE,
//                                      EntityIds.Corporate.ID_SEQ_HIERARCHY_NODE_LOG,
//                                      node.FormMode)
//      };

//      if (node.FormMode == FormMode.Insert)
//      {
//        await ctx.ExecuteCompoundCommand(CommandTimeoutSec, System.Data.IsolationLevel.ReadCommitted,
//          cmd => insertNode(cmd, node),
//          cmd => insertNodeLog(true, ctx, cmd, result.Version, node)
//        ).ConfigureAwait(false);
//      }
//      else
//      {
//        var affected = await ctx.ExecuteCompoundCommand(CommandTimeoutSec, System.Data.IsolationLevel.ReadCommitted,
//          cmd => insertNodeLog(false, ctx, cmd, result.Version, node)
//        ).ConfigureAwait(false);

//        if (affected < 1)
//        {
//          throw new HTTPStatusException(404,
//           "Entity not found", "Can not find `{0}`(Gdid=`{1}`) entity of expected type to update".Args(node.GetType().Name, node.Gdid));
//        }
//      }

//      return result;
//    }

//    private void insertNode(MySqlCommand cmd, TreeNode node)
//    {
//      cmd.CommandText = GetType().GetText("SaveNode.sql");
//      cmd.Parameters.AddWithValue("gdid", node.Gdid);
//      cmd.Parameters.AddWithValue("etype", G8CorpEntityType.MapToDb(node.Id.Type));
//    }

//    private void insertNodeLog(bool isInsert, MySqlCrudQueryExecutionContext ctx, MySqlCommand cmd, VersionInfo version, TreeNode node)
//    {
//      cmd.CommandText = GetType().GetText(isInsert ? "SaveNodeLogInsert.sql": "SaveNodeLogUpdate.sql");
//      cmd.MapVersionToSqlParameters(version);

//      cmd.Parameters.AddWithValue("g_node", node.Gdid);
//      if (node is CorporateChildNode cn)
//      {
//        cmd.Parameters.AddWithValue("g_parent", cn.G_Parent);
//      }
//      else
//      {
//        cmd.Parameters.AddWithValue("g_parent", null);
//      }
//      cmd.Parameters.AddWithValue("etype", G8CorpEntityType.MapToDb(node.Id.Type));
//      cmd.Parameters.AddWithValue("mnemonic", node.Mnemonic);
//      cmd.Parameters.AddWithValue("caption", node.Caption);
//      cmd.Parameters.AddWithValue("start_utc", node.StartUtc.Value);
//      cmd.Parameters.AddWithValue("properties", node.Properties.Content);
//      cmd.Parameters.AddWithValue("config", node.Config.Content);
//    }

//  }
//}
