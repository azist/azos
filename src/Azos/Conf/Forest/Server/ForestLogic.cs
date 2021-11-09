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
using Azos.Serialization.JSON;
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
      idForest.HasRequiredValue(nameof(idForest));
      App.Authorize(new TreePermission(TreeAccessLevel.Read));
      var result = await m_Data.NonNull(nameof(m_Data)).TryGetAllForestTreesAsync(idForest).ConfigureAwait(false);
      return result;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TreeNodeHeader>> GetChildNodeListAsync(EntityId idParent, DateTime? asOfUtc = null, ICacheParams cache = null)
    {
      //permission checks are performed in the child traverse loop down below
      var gop = GdidOrPath.OfGNodeOrPath(idParent);
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
    public async Task<TreeNodeInfo> GetNodeInfoAsync(EntityId id, DateTime? asOfUtc = null, ICacheParams cache = null)
    {
      //permission checks are performed in the child traverse loop down below
      if (cache == null) cache = CacheParams.DefaultCache;
      var asof = DefaultAndAlignOnPolicyBoundary(asOfUtc, id);
      var gop = GdidOrPath.OfGNodeOrPath(id);

      TreeNodeInfo result;

      if (gop.PathAddress != null)
      {
        result = await getNodeByTreePath(gop.Tree, gop.PathAddress, asof, cache).ConfigureAwait(false);
      }
      else
      {
        var graph = new HashSet<GDID>();
        result = await getNodeByGdid(graph, gop.Tree, gop.GdidAddress, asof, cache).ConfigureAwait(false);
      }

      return  result;
    }
    #endregion

    #region IForestSetupLogic
    /// <inheritdoc/>
    public Task<ValidState> ValidateNodeAsync(TreeNode node, ValidState state)
    {
#warning Implement ValidateNodeAsync method logic
    //todo: prevent recursive definitions etc...
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<VersionInfo>> GetNodeVersionListAsync(EntityId id)
    {
      var gop = GdidOrPath.OfGNode(id);

      //Working with versions is a part of tree setup functionality
      App.Authorize(new TreePermission(TreeAccessLevel.Setup, id));

      var qry = new Query<VersionInfo>("Tree.GetNodeVersionList")
      {
        new Query.Param("gnode", gop.GdidAddress)
      };

      var result = await m_Data.TreeLoadEnumerableAsync(gop.Tree, qry);
      return result;
    }

    /// <inheritdoc/>
    public async Task<TreeNodeInfo> GetNodeInfoVersionAsync(EntityId idVersion)
    {
      var gop = GdidOrPath.OfGVersion(idVersion);

      //Working with versions is a part of tree setup functionality
      App.Authorize(new TreePermission(TreeAccessLevel.Setup, idVersion));

      var qry = new Query<TreeNodeInfo>("Tree.GetNodeInfoVersionByGdid")
      {
        new Query.Param("gop", gop)
      };
      var result = await m_Data.TreeLoadDocAsync(gop.Tree, qry);
      return result;
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

      //purge cache for that tree
      purgeCacheTables(new TreePtr(node.Forest, node.Tree));
#warning Add shim for EVENT NOTIFICATION system

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

      //3. - Delegate saving to existing SaveNode()
      var change = await this.SaveNodeAsync(tombstone).ConfigureAwait(false);

      return new ChangeResult(ChangeResult.ChangeType.Deleted, change.AffectedCount, "Deleted", change.Data);
    }
    #endregion

    #region pvt

    //cache 2 atom concatenation as string
    private static FiniteSetLookup<TreePtr, string> s_CacheTableName =
      new FiniteSetLookup<TreePtr, string>((t) => "{0}::{1}".Args(t.IdForest, t.IdTree));

    private void purgeCacheTables(TreePtr tree)
    {
      var tname = s_CacheTableName[tree];
      var tbl = m_Data.Cache.Tables[tname];
      if (tbl == null) return;
      tbl.Purge();
    }

    private async Task<TreeNodeInfo> getNodeByTreePath(TreePtr tree, TreePath path, DateTime asOfUtc, ICacheParams caching)
    {
      var tblCache = s_CacheTableName[tree];
      var keyCache = nameof(getNodeByTreePath) + path + asOfUtc.Ticks;

      var result = await m_Data.Cache.FetchThroughAsync(
        keyCache, tblCache, caching,
        async key => {

          TreeNodeInfo nodeParent = null;
          TreeNodeInfo node = null;
          for(var i = -1; i < path.Count; i++) //going from LEFT to RIGHT
          {
            var segment = i < 0 ? Constraints.VERY_ROOT_PATH_SEGMENT : path[i];
            node = await getNodeByPathSegment(tree, nodeParent == null ? GDID.ZERO : nodeParent.Gdid, segment, asOfUtc, caching).ConfigureAwait(false);
            if (node == null) return null;// deleted


            //Config chain inheritance pattern
            if(nodeParent == null)
            {
              node.EffectiveConfig = new ConfigVector(node.LevelConfig.Content);//Copy
              node.FullPath = Constraints.VERY_ROOT_PATH_SEGMENT;
            }
            else
            {
              var confHere = node.LevelConfig.Node.NonEmpty(nameof(node.LevelConfig));
              var confParent = nodeParent.EffectiveConfig.Node.NonEmpty(nameof(nodeParent.EffectiveConfig));
              var confResult = new MemoryConfiguration() { Application = this.App };
              confResult.CreateFromNode(confParent);//inherit
              confResult.Root.OverrideBy(confHere); //override
              node.EffectiveConfig = new ConfigVector(confResult.Root);
              node.FullPath = TreePath.Join(nodeParent.FullPath, node.PathSegment);
            }
            nodeParent = node;
            App.Authorize(new TreePermission(TreeAccessLevel.Read, node.FullPathId));
          }
          return node;

      }).ConfigureAwait(false);

      return result;
    }

    private async Task<TreeNodeInfo> getNodeByPathSegment(TreePtr tree, GDID gParent, string pathSegment, DateTime asOfUtc, ICacheParams caching)
    {
      if (gParent.IsZero) gParent = Constraints.G_VERY_ROOT_NODE;

      var tblCache = s_CacheTableName[tree];
      var keyCache = nameof(getNodeByPathSegment) + gParent.ToHexString() + (pathSegment ?? string.Empty) + asOfUtc.Ticks;

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


    private async Task<TreeNodeInfo> getNodeByGdid(HashSet<GDID> graph, TreePtr tree, GDID gNode, DateTime asOfUtc, ICacheParams caching)
    {
      var tblCache = s_CacheTableName[tree];
      var keyCache = nameof(getNodeByGdid) + gNode.ToHexString() + asOfUtc.Ticks;
      var result = await m_Data.Cache.FetchThroughAsync(
        keyCache, tblCache, caching,
        async key =>
        {
          if (!graph.Add(gNode))
          {
            //circular reference
            var err = new ConfigException("Circular reference in config tree = `{0}`, gnode = `{1}`, asof = `{2}`".Args(tree, gNode, asOfUtc));
            WriteLogFromHere(Log.MessageType.CatastrophicError,
                             err.Message,
                             err,
                             pars: new {tree = tree.ToString(), gnode = gNode, asof = asOfUtc}.ToJson());
            throw err;
          }

          //1 - fetch THIS level - rightmost part of the tree
          var qry = new Query<TreeNodeInfo>("Tree.GetNodeInfoByGdid")
          {
            new Query.Param("tree", tree),
            new Query.Param("gdid", gNode),
            new Query.Param("asof", asOfUtc)
          };

          var node = await m_Data.TreeLoadDocAsync(tree, qry);
          if (node == null) return null;

          //2 - if IAM ROOT, there is no parent for root
          if (node.Gdid == Constraints.G_VERY_ROOT_NODE)
          {
            node.EffectiveConfig = new ConfigVector(node.LevelConfig.Content);//Copy
            node.FullPath = Constraints.VERY_ROOT_PATH_SEGMENT;
            return node;
          }

          //3 - Fetch parent of THIS
          TreeNodeInfo nodeParent = await getNodeByGdid(graph, tree, node.G_Parent, asOfUtc, caching).ConfigureAwait(false);
          if (nodeParent == null) return null;

          //4 - calculate effective config
          var cfgNode = node.LevelConfig.Node.NonEmpty(nameof(node.LevelConfig));
          var cfgParent = nodeParent.EffectiveConfig.Node.NonEmpty(nameof(nodeParent.EffectiveConfig));

          var confResult = new MemoryConfiguration() { Application = this.App };
          confResult.CreateFromNode(cfgParent);//inherit
          confResult.Root.OverrideBy(cfgNode); //override
          node.EffectiveConfig.Node = confResult.Root;

          node.FullPath = TreePath.Join(nodeParent.FullPath, node.PathSegment);

          //the security check is done post factum AFTER tree node full path is known
          return node;
        }
      ).ConfigureAwait(false);

      App.Authorize(new TreePermission(TreeAccessLevel.Read,  result.FullPathId));
      return result;
    }


    private async Task<IEnumerable<TreeNodeHeader>> getChildNodeListByTreePath(TreePtr tree, TreePath pathAddress, DateTime asOfUtc, ICacheParams caching)
    {
      var nodeParent = await getNodeByTreePath(tree, pathAddress, asOfUtc, caching).ConfigureAwait(false);
      if (nodeParent == null) return null;

      var result = await getChildNodeListByGdid(tree, nodeParent.Gdid, asOfUtc, caching);
      return result;
    }

    private async Task<IEnumerable<TreeNodeHeader>> getChildNodeListByGdid(TreePtr tree, GDID gdidAddress, DateTime asOfUtc, ICacheParams caching)
    {
      var tblCache = s_CacheTableName[tree];
      var keyCache = nameof(getChildNodeListByGdid) + gdidAddress.ToHexString()+ asOfUtc.Ticks;

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
  }
}
