/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azos;
using Azos.Apps;
using Azos.Data;
using Azos.Data.Business;

namespace Azos.Conf.Forest.Server
{
  /*
  Datastore hub is hosted by the logic
  Data context name is: `[forest]::[tree]`

  */
  public sealed class CorporateHierarchyLogic : ModuleBase, IForestLogic, IForestSetupLogic
  {
    private const string CACHE_TBL_GENERIC = "CorporateHierarchyLogic.GENERIC";
    private const string CACHE_TBL_HIERARCHY_NODE = "CorporateHierarchyLogic.HIERARCHY_NODE";
    private const string CONFIG_DATA_SECTION = "data";

    public CorporateHierarchyLogic(IApplication app) : base(app) { }
    public CorporateHierarchyLogic(IModule parent) : base(parent) { }

    public override bool IsHardcodedModule => false;

    public override string ComponentLogTopic => CoreConsts.CONF_TOPIC;

    public bool IsServerImplementation => true;

    //allocated here
    IForestDataSource m_Data;
//todo Abstract this away
//    [InjectModule] IEventProducer m_Events;


    private void purgeHierarchyCacheTables()
    {
      m_Data.Cache.Tables[CACHE_TBL_GENERIC]?.Purge();
      m_Data.Cache.Tables[CACHE_TBL_HIERARCHY_NODE]?.Purge();
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


    #region IForestLogic

    public Task<IEnumerable<Atom>> GetTreeListAsync(Atom idForest) => Task.FromResult(m_Data.TryGetAllForestTrees(idForest));

    public Task<IEnumerable<TreeNodeHeader>> GetChildNodeListAsync(EntityId idParent, DateTime? asOfUtc = null, ICacheParams cache = null)
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<VersionInfo>> GetNodeVersionListAsync(EntityId id)
    {
      throw new NotImplementedException();
    }

    public Task<TreeNodeInfo> GetNodeInfoVersionAsync(EntityId idVersion)
    {
      throw new NotImplementedException();
    }


    public Task<TreeNodeInfo> GetNodeInfoAsync(EntityId id, DateTime? asOfUtc = null, ICacheParams cache = null)
    {
      throw new NotImplementedException();
    }
    #endregion


    #region IForestSetyupLogic
    public Task<ValidState> ValidateNodeAsync(TreeNode node, ValidState state)
    {
      throw new NotImplementedException();
    }

    public Task<ChangeResult> SaveNodeAsync(TreeNode node)
    {
      throw new NotImplementedException();
    }

    public Task<ChangeResult> DeleteNodeAsync(EntityId id, DateTime? startUtc = null)
    {
      throw new NotImplementedException();
    }
    #endregion
  }
}
