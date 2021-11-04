/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;
using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Business;
using Azos.Data.Modeling.DataTypes;
using Azos.Platform;
using Azos.Security.ConfigForest;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azos.Conf.Forest.Server
{
  /*
  Datastore hub is hosted by the logic
  Data context name is: `[forest]::[tree]`
  */
  public sealed class ForestLogic : ModuleBase, IForestLogic, IForestSetupLogic
  {
    private const string CONFIG_DATA_SECTION = "data";

    public ForestLogic(IApplication app) : base(app) { }
    public ForestLogic(IModule parent) : base(parent) { }

    //allocated here
    private IForestDataSource m_Data;
#warning Abstract this away - need Event source very much
    //    [InjectModule] IEventProducer m_Events;

    #region Props
    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.CONF_TOPIC;
    public bool IsServerImplementation => true;
    #endregion

    #region Protected
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      var ndata = node.NonEmpty(nameof(node))[CONFIG_DATA_SECTION]
                      .NonEmpty($"section `{CONFIG_DATA_SECTION}`");
      m_Data = FactoryUtils.MakeAndConfigureDirectedComponent<IForestDataSource>(this, ndata, typeof(ForestDataSource));
    }

    protected override bool DoApplicationAfterInit()
    {
      m_Data.NonNull(nameof(Data)).Start();
      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      this.DontLeak(() => m_Data.Dispose() );
      return base.DoApplicationBeforeCleanup();
    }

    #endregion

    #region IForestLogic
    /// <inheritdoc/>
    public DateTime DefaultAndAlignOnPolicyBoundary(DateTime? v, EntityId? id = null)
    {
      if (!v.HasValue) v = App.TimeSource.UTCNow;

#warning Make configurable per tree
      var boundary = 10;

      return v.Value.AlignDailyMinutes(boundary);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Atom>> GetTreeListAsync(Atom idForest)
    {
      App.Authorize(new TreePermission(TreeAccessLevel.Read, EntityId.EMPTY));
      return await m_Data.NonNull(nameof(m_Data)).TryGetAllForestTreesAsync(idForest).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TreeNodeHeader>> GetChildNodeListAsync(EntityId idParent, DateTime? asOfUtc = null, ICacheParams cache = null)
    {
      //permission checks are performed in the child traverse loop down below

      var gop = GdidOrPath.OfGNode(idParent);
      var asof = DefaultAndAlignOnPolicyBoundary(asOfUtc, idParent);
      if (cache == null) cache = CacheParams.DefaultCache;

      IEnumerable<TreeNodeHeader> result;

      if (gop.PathAddress != null)
        result = await getChildNodeListByTreePath(gop.Tree, gop.PathAddress, asof, cache).ConfigureAwait(false);
      else
        result = await getChildNodeListByGdid(gop.Tree, gop.GdidAddress, asof, cache).ConfigureAwait(false);

      return result;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<VersionInfo>> GetNodeVersionListAsync(EntityId id)
    {
      var gop = GdidOrPath.OfGNode(id);

      App.Authorize(new TreePermission(TreeAccessLevel.Setup, id));

      var qry = new Query<VersionInfo>("Tree.GetNodeVersionList")
      {
        new Query.Param("gop", gop)
      };

      var result = await m_Data.TreeLoadEnumerableAsync(gop.Tree, qry);
      return result;
    }

    /// <inheritdoc/>
    public async Task<TreeNodeInfo> GetNodeInfoVersionAsync(EntityId idVersion)
    {
      App.Authorize(new TreePermission(TreeAccessLevel.Setup, idVersion));

      var gop = GdidOrPath.OfGVersion(idVersion);
      var ptr = new TreePtr(idVersion);
      var qry = new Query<TreeNodeInfo>("Tree.GetNodeInfoVersionByGdid")
      {
        new Query.Param("ptr", ptr),
        new Query.Param("gdid", gop.GdidAddress)
      };
      return await m_Data.TreeLoadDocAsync(ptr, qry);
    }

    /// <inheritdoc/>
    public async Task<TreeNodeInfo> GetNodeInfoAsync(EntityId id, DateTime? asOfUtc = null, ICacheParams cache = null)
    {
      if (cache == null) cache = CacheParams.DefaultCache;
      var asof = DefaultAndAlignOnPolicyBoundary(asOfUtc, id);
      var gop = GdidOrPath.OfGNode(id);

      TreeNodeInfo result = null;
      if (gop.PathAddress != null)
        result = await getNodeByTreePath(gop.Tree, gop.PathAddress, asof, cache).ConfigureAwait(false);
      else
        result = await getNodeByGdid(gop.Tree, gop.GdidAddress, asof, cache).ConfigureAwait(false);

      return result;
    }
    #endregion

    #region IForestSetupLogic
    /// <inheritdoc/>
    public Task<ValidState> ValidateNodeAsync(TreeNode node, ValidState state)
    {
#warning Implement ValidateNodeAsync method logic
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async Task<ChangeResult> SaveNodeAsync(TreeNode node)
    {
      node.NonNull(nameof(node));
      var tree = new TreePtr(node.Forest, node.Tree);

      App.Authorize(new TreePermission(TreeAccessLevel.Setup, node.Id));

      var qry = new Query<EntityChangeInfo>("Tree.SaveNode")
      {
        new Query.Param("n", node)
      };

      //1. Update database
      var change = await m_Data.TreeExecuteAsync(tree, qry);

      // TODO: review purge cache logic that was deleted. Is it needed here?

      return new ChangeResult(ChangeResult.ChangeType.Processed, 1, "Saved", change);
    }

    /// <inheritdoc/>
    public async Task<ChangeResult> DeleteNodeAsync(EntityId id, DateTime? startUtc = null)
    {
      var asof = DefaultAndAlignOnPolicyBoundary(startUtc, id);
      App.Authorize(new TreePermission(TreeAccessLevel.Setup, id));

      //1. - Fetch the existing node state
      var existing = await GetNodeInfoAsync(id, asof, CacheParams.NoCache).ConfigureAwait(false);

      if (existing == null) return new ChangeResult("Not Found", 404);

      //2. - Create tombstone
      var tombstone = existing.CloneIntoPersistedModel();
      tombstone.FormMode = FormMode.Delete;
      tombstone.StartUtc = asof;

      //delegate saving to existing SaveNode()
      var change = await this.SaveNodeAsync(tombstone).ConfigureAwait(false);

      return new ChangeResult(ChangeResult.ChangeType.Deleted, change.AffectedCount, "Deleted", change.Data);
    }
    #endregion

    #region pvt

    private void purgeCacheTables()
    {
      m_Data.Cache.PurgeAll();
    }

    //cache 2 atom concatenation as string
    private static FiniteSetLookup<TreePtr, string> s_CacheTableName =
      new FiniteSetLookup<TreePtr, string>((t) => "{0}::{1}".Args(t.IdForest, t.IdTree));

    private async Task<TreeNodeInfo> getNodeByTreePath(TreePtr tree, TreePath path, DateTime asOfUtc, ICacheParams caching)
    {
      TreeNodeInfo nodeParent = null;
      TreeNodeInfo node = null;
      for(var i = -1; i < path.Count; i++)
      {
        var segment = i < 0 ? Constraints.VERY_ROOT_PATH_SEGMENT : path[i];
        node = await getNodeByPathSegment(tree, nodeParent == null ? GDID.ZERO: nodeParent.Gdid, segment, asOfUtc, caching).ConfigureAwait(false);
        if (node == null) return null;// deleted

        App.Authorize(new TreePermission(TreeAccessLevel.Read, node.Id));

        //Config chain inheritance pattern
        if(nodeParent == null)
        {
          node.EffectiveConfig = new ConfigVector(node.LevelConfig.Content);//Copy
        }
        else
        {
          var confHere = node.LevelConfig.Node.NonEmpty(nameof(node.LevelConfig));  // Using LevelConfig is correct ????
          var confParent = nodeParent.EffectiveConfig.Node.NonEmpty(nameof(nodeParent.EffectiveConfig));
          var confResult = new MemoryConfiguration() { Application = this.App };
          confResult.CreateFromNode(confParent);//inherit
          confResult.Root.OverrideBy(confHere); //override
          node.EffectiveConfig = new ConfigVector(confResult.Root);
        }
        nodeParent = node;
      }
      return node;
    }

    private async Task<TreeNodeInfo> getNodeByPathSegment(TreePtr tree, GDID gParent, string pathSegment, DateTime asOfUtc, ICacheParams caching)
    {
      var tblCache = s_CacheTableName[tree];
      var keyCache = asOfUtc.Ticks + gParent.ToHexString() + (pathSegment ?? string.Empty);

      var node = await m_Data.Cache.FetchThroughAsync(
        keyCache, tblCache, caching,
        async key =>
        {
          var qry = new Query<TreeNodeInfo>("Tree.GetNodeInfo")
          {
            new Query.Param("tree", tree),
            new Query.Param("gparent", gParent),
            new Query.Param("psegment", pathSegment),
            new Query.Param("asof", asOfUtc)
          };
          return await m_Data.TreeLoadDocAsync(tree, qry);
        }
      ).ConfigureAwait(false);

      return node;
    }


    private async Task<TreeNodeInfo> getNodeByGdid(TreePtr tree, GDID gNode, DateTime asOfUtc, ICacheParams caching)
    {
      TreeNodeInfo node = await getNodeByGdid_Implementation(tree, gNode, asOfUtc, caching).ConfigureAwait(false);
      if (node == null) return null;
      if (node.G_Parent == GDID.ZERO || node.Gdid == node.G_Parent)
      {
        node.EffectiveConfig = new ConfigVector(node.LevelConfig.Content);//Copy
        return node;
      }

      var gParent = node.G_Parent;
      TreeNodeInfo nodeHere = null;

#warning Circular reference and unneeded complexity. You just need to fetch parent and apply child level over recursive call. See how it is done in Hierarchy
      var nodeList = new List<TreeNodeInfo>();
      while (true)
      {
        nodeHere = await getNodeByGdid_Implementation(tree, gParent, asOfUtc, caching).ConfigureAwait(false);
        if (nodeHere == null) break;
        else
        {
          // Add node then walk up tree by parent
          nodeList.Add(nodeHere);
          if (nodeHere.G_Parent == GDID.ZERO || nodeHere.Gdid == nodeHere.G_Parent) break;
          gParent = nodeHere.G_Parent;
        }
      }

      //Config chain inheritance pattern
      IConfigSectionNode confParent = null;
      IConfigSectionNode confHere = null;

#warning NOT NEEDED. Fetch parent instead (use recursive call instead of a list)
      // Walk down tree applying config
      for (int i = nodeList.Count - 1; i > -1; i--)
      {
        nodeHere = nodeList[i];
        if (confParent == null)
        {
          confParent = nodeHere.LevelConfig.Node.NonEmpty(nameof(nodeHere.LevelConfig));
          continue;
        }

        confHere = nodeHere.LevelConfig.Node.NonEmpty(nameof(nodeHere.LevelConfig));
        var confResult = new MemoryConfiguration() { Application = this.App };
        confResult.CreateFromNode(confParent);//inherit
        confResult.Root.OverrideBy(confHere); //override
        confParent = confResult.Root;
      }
      node.EffectiveConfig = new ConfigVector(confHere);

      return node;
    }

    private async Task<TreeNodeInfo> getNodeByGdid_Implementation(TreePtr tree, GDID gNode, DateTime asOfUtc, ICacheParams caching)
    {
      App.Authorize(new TreePermission(TreeAccessLevel.Read, new EntityId(tree.IdForest, tree.IdTree, Constraints.SCH_GNODE, gNode.ToString())));
      var tblCache = s_CacheTableName[tree] + "::gdid";
      var keyCache = asOfUtc.Ticks + gNode.ToHexString();

      var node = await m_Data.Cache.FetchThroughAsync(
        keyCache, tblCache, caching,
        async key =>
        {
          var qry = new Query<TreeNodeInfo>("Tree.GetNodeInfoByGdid")
          {
            new Query.Param("tree", tree),
            new Query.Param("gdid", gNode),
            new Query.Param("asof", asOfUtc)
          };
          return await m_Data.TreeLoadDocAsync(tree, qry);
        }
      ).ConfigureAwait(false);

      return node;
    }

    #region get child nodes
    private async Task<IEnumerable<TreeNodeHeader>> getChildNodeListByTreePath(TreePtr tree, TreePath pathAddress, DateTime asOfUtc, ICacheParams caching)
    {
      var nodeParent = await getNodeByTreePath(tree, pathAddress, asOfUtc, caching).ConfigureAwait(false);
      return await getChildNodeListByGdid(tree, nodeParent.Gdid, asOfUtc, caching);
    }

    private async Task<IEnumerable<TreeNodeHeader>> getChildNodeListByGdid(TreePtr tree, GDID gdidAddress, DateTime asOfUtc, ICacheParams caching)
    {
      var tblCache = s_CacheTableName[tree] + "::chld";
      var keyCache = asOfUtc.Ticks + gdidAddress.ToHexString();

      var nodes = await m_Data.Cache.FetchThroughAsync(
        keyCache, tblCache, caching,
        async key =>
        {
          var qry = new Query<TreeNodeHeader>("Tree.GetChildNodeList")
          {
            new Query.Param("gparent", gdidAddress),
            new Query.Param("asof", asOfUtc)
          };
          return await m_Data.TreeLoadEnumerableAsync(tree, qry);
        }
      ).ConfigureAwait(false);
      return nodes;
    }
    #endregion

    #endregion
  }
}
