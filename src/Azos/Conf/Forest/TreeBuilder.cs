/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azos.Apps.Injection;
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
    /// The tree level must exist asof the specified date.
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

    public const string CONFIG_NODE_PREFIX_SECT = "node::";


    public TreeBuilder(IApplication app)
    {
      app.InjectInto(this);
      m_App = app;
    }

    protected readonly IApplication m_App;
    [Inject] protected IForestSetupLogic m_Logic;

    public async Task BuildAsync(IConfigSectionNode cfgTree, Atom idForest, Atom idTree)
    {
      cfgTree.NonEmpty(nameof(cfgTree));
      (!idForest.IsZero && idForest.IsValid).IsTrue("Valid forest id");
      (!idTree.IsZero && idTree.IsValid).IsTrue("Valid tree id");
      var nid = new EntityId(idForest, idTree, Constraints.SCH_PATH, "/");
      var node = await DoLevelAsync(nid, cfgTree, "/", GDID.ZERO).ConfigureAwait(false);

      if (node != null)
      {
        await DoLevelChildrenAsync(cfgTree, node).ConfigureAwait(false);
      }
    }

    protected async virtual Task DoLevelChildrenAsync(IConfigSectionNode cfgNode, TreeNodeInfo nodeInfo)
    {
      var tplChildren = cfgNode.Children
                               .Where(c => c.Name.StartsWith(CONFIG_NODE_PREFIX_SECT) && c.Name.Length > CONFIG_NODE_PREFIX_SECT.Length)
                               .Select(c => (cfg: c, seg: c.Name.Substring(CONFIG_NODE_PREFIX_SECT.Length).Trim()))
                               .Where(tpl => tpl.seg.IsNotNullOrWhiteSpace());

      foreach(var tplChild in tplChildren)
      {
        var idChildNode = new EntityId(nodeInfo.Forest, nodeInfo.Tree, Constraints.SCH_PATH, TreePath.Join(nodeInfo.FullPath, tplChild.seg));

        var levelInfo = await DoLevelAsync(idChildNode, tplChild.cfg, tplChild.seg, nodeInfo.G_Parent).ConfigureAwait(false);
        await DoLevelChildrenAsync(tplChild.cfg, levelInfo).ConfigureAwait(false);
      }
    }

    protected virtual async Task<TreeNodeInfo> DoLevelAsync(EntityId idNode, IConfigSectionNode cfgNode, string pathSeg, GDID gParent)
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
        if (action == NodeActionType.Require) throw new ConfigException("Required node `{0}` is not found".Args(idNode));
        if (action == NodeActionType.Conditional) return null;//do nothing
        if (action == NodeActionType.Delete) return null;//do nothing

        if (gParent.IsZero) throw new ConfigException("Root insert is prohibited");

        node = new TreeNode();
        node.FormMode = FormMode.Insert;
        node.Gdid = GDID.ZERO;
        node.Forest = idNode.System;
        node.Tree = idNode.Type;
        node.G_Parent = gParent;
      }
      else
      {
        if (action == NodeActionType.Require || action == NodeActionType.Conditional) return nodeInfo;
        if (action == NodeActionType.Delete)
        {
          await m_Logic.DeleteNodeAsync(idNode, asof);
          return nodeInfo;
        }

        node = nodeInfo.CloneIntoPersistedModel();
        node.FormMode = FormMode.Update;
      }

      var nProps = cfgNode[CONFIG_PROPS_SECT].NonEmpty("{0}/{1}".Args(cfgNode.RootPath, CONFIG_PROPS_SECT));
      var nConf  = cfgNode[CONFIG_CONF_SECT] .NonEmpty("{0}/{1}".Args(cfgNode.RootPath, CONFIG_CONF_SECT));

      node.Properties = new ConfigVector(nProps);
      node.Config = new ConfigVector(nConf);
      node.StartUtc = asof;
      node.PathSegment = pathSeg;

      var saved = await node.SaveAsync(m_App).ConfigureAwait(false);

      //We now need to re-fetch the full info by its G_VERSION - exactly that data copy
      var treeChange = TreeNodeChangeInfo.FromChangeAs<TreeNodeChangeInfo>(saved.Result);
      var verId = new EntityId(treeChange.Id.System, treeChange.Id.Type, Constraints.SCH_GVER, treeChange.Version.G_Version.ToHexString());

      nodeInfo = await m_Logic.GetNodeInfoVersionAsync(verId).ConfigureAwait(false);
      return nodeInfo;
    }

    protected virtual async Task<TreeNodeInfo> GetExistingNodeAsync(EntityId idNode, DateTime? asOfUtc)
    {
      var node = await m_Logic.GetNodeInfoAsync(idNode, asOfUtc, CacheParams.NoCache);//the freshest
      return node;
    }
  }
}
