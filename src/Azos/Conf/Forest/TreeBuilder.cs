/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Data.Access;

namespace Azos.Conf.Forest
{
  /// <summary>
  /// Defines action types for nodes of trres built by <see cref="TreeBuilder"/>
  /// </summary>
  public enum NodeActionType
  {
    Require = 0,
    Conditional,
    Insert,
    Update,
    Save,//Upsert
    Delete,
  }


  /// <summary>
  /// Facilitates construction of tree hierarchy from a config script
  /// </summary>
  public class TreeBuilder
  {
    public const string CONFIG_ACTION_ATTR = "action";
    public const string CONFIG_ASOF_ATTR   = "asof";
    public const string CONFIG_PROPS_ATTR  = "props";
    public const string CONFIG_CONF_ATTR   = "config";


    public TreeBuilder(IForestSetupLogic logic)
    {
      m_Logic = logic.NonDisposed(nameof(logic));
    }

    protected readonly IForestSetupLogic m_Logic;

    public async Task BuildAsync(IConfigSectionNode cfgTree, Atom idForest, Atom idTree)
    {
      cfgTree.NonEmpty(nameof(cfgTree));
      (!idForest.IsZero && idForest.IsValid).IsTrue("Valid forest id");
      (!idTree.IsZero && idTree.IsValid).IsTrue("Valid tree id");
      var path = "/";
      await DoLevelAsync(idForest, idTree, cfgTree, path);
    }

    protected virtual async Task DoLevelAsync(Atom idForest, Atom idTree, IConfigSectionNode cfgNode, string path)
    {
      var action = cfgNode.Of(CONFIG_ACTION_ATTR).ValueAsEnum(NodeActionType.Require);
      var asof = cfgNode.Of(CONFIG_ASOF_ATTR).Value.AsNullableDateTime(styles: CoreConsts.UTC_TIMESTAMP_STYLES);
      var nid = new EntityId(idForest, idTree, Constraints.SCH_PATH, path);
      var node = await GetExistingNodeAsync(nid, asof);

      //chekc required etc...
    }

    protected virtual async Task<TreeNodeInfo> GetExistingNodeAsync(EntityId idNode, DateTime? asOfUtc)
    {
       var node = await m_Logic.GetNodeInfoAsync(idNode, asOfUtc, CacheParams.NoCache);//the freshest
       return node;
    }
  }
}
