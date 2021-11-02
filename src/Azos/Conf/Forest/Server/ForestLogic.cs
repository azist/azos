/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Business;
using Azos.Platform;
using Azos.Security.ConfigForest;

namespace Azos.Conf.Forest.Server
{
  /*
  Datastore hub is hosted by the logic
  Data context name is: `[forest]::[tree]`
  */
  public sealed class ForestLogic : ModuleBase, IForestLogic, IForestSetupLogic
  {
    #region const / .ctor / etc.

    private const string CONFIG_DATA_SECTION = "data";

    public ForestLogic(IApplication app) : base(app) { }
    public ForestLogic(IModule parent) : base(parent) { }

    public override bool IsHardcodedModule => false;

    public override string ComponentLogTopic => CoreConsts.CONF_TOPIC;

    public bool IsServerImplementation => true;

    //allocated here
    private IForestDataSource m_Data;
//todo Abstract this away
//    [InjectModule] IEventProducer m_Events;


    private void purgeCacheTables()
    {
      m_Data.Cache.PurgeAll();
    }

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
    public async Task<IEnumerable<Atom>> GetTreeListAsync(Atom idForest) => await m_Data.NonNull(nameof(m_Data)).TryGetAllForestTreesAsync(idForest);

    /// <inheritdoc/>
    public async Task<IEnumerable<TreeNodeHeader>> GetChildNodeListAsync(EntityId idParent, DateTime? asOfUtc = null, ICacheParams cache = null)
    {
      return null;

      if (cache == null) cache = CacheParams.DefaultCache;
      var asof = DefaultAndAlignOnPolicyBoundary(asOfUtc, idParent);
      var gop = GdidOrPath.OfGNode(idParent);

      if (gop.PathAddress != null)
        return await getChildNodeListByTreePath(gop.Tree, gop.PathAddress, asof, cache).ConfigureAwait(false);

      return await getChildNodeListByGdid(gop.Tree, gop.GdidAddress, asof, cache);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<VersionInfo>> GetNodeVersionListAsync(EntityId id)
    {
      var gop = GdidOrPath.OfGNode(id);

      App.Authorize(new TreePermission(TreeAccessLevel.Read, id));

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
      App.Authorize(new TreePermission(TreeAccessLevel.Read, idVersion)); // TODO: This needs reviewed!!!!

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

      if (gop.PathAddress != null)
        return await getNodeByTreePath(gop.Tree, gop.PathAddress, asof, cache).ConfigureAwait(false);

      return await getNodeByGdid(gop.Tree, gop.GdidAddress, asof, cache);
    }
    #endregion

    #region IForestSetupLogic
    /// <inheritdoc/>
    public Task<ValidState> ValidateNodeAsync(TreeNode node, ValidState state)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<ChangeResult> SaveNodeAsync(TreeNode node)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<ChangeResult> DeleteNodeAsync(EntityId id, DateTime? startUtc = null)
    {
      throw new NotImplementedException();
    }
    #endregion

    #region pvt

    //cache 2 atom concatenation as string
    private static FiniteSetLookup<TreePtr, string> s_CacheTableName =
      new FiniteSetLookup<TreePtr, string>((t) => "{0}::{1}".Args(t.IdForest, t.IdTree));

    private async Task<TreeNodeInfo> getNodeByTreePath(TreePtr tree, TreePath path, DateTime asOfUtc, ICacheParams caching)
    {
      // TODO: Need to calculate EffectiveConfig in ForestLogic, see G8 CorporateHierarchyLogic getNodeInfoAsync_Implementation.
      TreeNodeInfo nodeParent = null;
      TreeNodeInfo node = null;
      for(var i = -1; i < path.Count; i++)
      {
        var segment = i < 0 ? Constraints.VERY_ROOT_PATH_SEGMENT : path[i];
        node = await getNodeByPathSegment(tree, nodeParent == null ? GDID.ZERO: nodeParent.Gdid, segment, asOfUtc, caching).ConfigureAwait(false);
        if (node == null) return null;// deleted

        //Config chain inheritance pattern
        var confHere = node.LevelConfig.Node.NonEmpty(nameof(node.LevelConfig));
        if(nodeParent == null)
        {
          node.EffectiveConfig = new ConfigVector(node.LevelConfig.Content);//Copy
        }
        else
        {
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
      // TODO: Need to calculate EffectiveConfig in ForestLogic, see G8 CorporateHierarchyLogic getNodeInfoAsync_Implementation.
      TreeNodeInfo node = null;
      var gParent = GDID.ZERO;

      // TODO:
      node = await getNodeByGdid_Implementation(tree, gNode, asOfUtc, caching).ConfigureAwait(false);

      return node;
    }

    private async Task<TreeNodeInfo> getNodeByGdid_Implementation(TreePtr tree, GDID gNode, DateTime asOfUtc, ICacheParams caching)
    {
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
