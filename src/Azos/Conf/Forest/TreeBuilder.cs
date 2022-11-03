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
  /// Defines action types for nodes of trees built by <see cref="TreeBuilder"/>
  /// </summary>
  public enum NodeActionType
  {
    /// <summary>
    /// The tree level must exist asof specified date.
    /// If not present then the exception is thrown and processing aborted
    /// </summary>
    Require = 0,

    /// <summary>
    /// Will try to fetch this level. If not found then the whole sub-tree will be skipped
    /// </summary>
    Conditional,

    /// <summary>
    /// Will insert or update (upsert) a node asof date
    /// </summary>
    Save,

    /// <summary>
    /// Tombstones a node as of date
    /// </summary>
    Delete
  }


  /// <summary>
  /// Facilitates construction of tree hierarchy from a config script
  /// </summary>
  public class TreeBuilder
  {
    public const string CONFIG_ACTION_ATTR = "action";
    public const string CONFIG_ASOF_ATTR   = "asof";
    public const string CONFIG_PROPS_SECT  = "props";
    public const string CONFIG_CONF_SECT   = "conf";


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
      var nid = new EntityId(idForest, idTree, Constraints.SCH_PATH, "/");
      await DoLevelAsync(nid, cfgTree, "/", GDID.ZERO);
    }

    protected virtual async Task DoLevelAsync(EntityId idNode,IConfigSectionNode cfgNode, string pathSeg, GDID gParent)
    {
      var action = cfgNode.Of(CONFIG_ACTION_ATTR).ValueAsEnum(NodeActionType.Require);

      var asof = m_Logic.DefaultAndAlignOnPolicyBoundary(cfgNode.Of(CONFIG_ASOF_ATTR)
                                                                .Value
                                                                .AsNullableDateTime(styles: CoreConsts.UTC_TIMESTAMP_STYLES),
                                                         idNode);

      var nodeInfo = await GetExistingNodeAsync(idNode, asof).ConfigureAwait(false);

      TreeNode node;

      if (nodeInfo == null)
      {
        if (action == NodeActionType.Conditional) return;//do nothing

        node = new TreeNode();
        node.FormMode = FormMode.Insert;
        node.Forest = idNode.System;
        node.Tree = idNode.Type;
        node.Gdid = GDID.ZERO;
        node.G_Parent = gParent;
        node.PathSegment = pathSeg;
      }
      else
      {
        node = nodeInfo.CloneIntoPersistedModel();
        node.FormMode = FormMode.Update;
      }

      var nProps = cfgNode[CONFIG_PROPS_SECT].NonEmpty("{0}/{1}".Args(cfgNode.RootPath, CONFIG_PROPS_SECT));
      node.Properties = new ConfigVector(nProps);

      var nConf = cfgNode[CONFIG_CONF_SECT].NonEmpty("{0}/{1}".Args(cfgNode.RootPath, CONFIG_CONF_SECT));
      node.Config = new ConfigVector(nConf);

      node.StartUtc = asof;

      await m_Logic.SaveNodeAsync(node).ConfigureAwait(false);
    }

    protected virtual async Task<TreeNodeInfo> GetExistingNodeAsync(EntityId idNode, DateTime? asOfUtc)
    {
       var node = await m_Logic.GetNodeInfoAsync(idNode, asOfUtc, CacheParams.NoCache);//the freshest
       return node;
    }
  }
}
