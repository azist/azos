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
using Azos.Apps.Injection;
using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Business;
using Azos.Pile;
using Azos.Platform;
using Azos.Serialization.JSON;
using Azos.Sky.EventHub;
using Azos.Text;

namespace Azos.Conf.Forest.Server
{
  public sealed class CorporateHierarchyLogic : ModuleBase, IForestLogic, IForestSetupLogic
  {
    private const string CACHE_TBL_GENERIC = "CorporateHierarchyLogic.GENERIC";
    private const string CACHE_TBL_HIERARCHY_NODE = "CorporateHierarchyLogic.HIERARCHY_NODE";

    public CorporateHierarchyLogic(IApplication app) : base(app) { }
    public CorporateHierarchyLogic(IModule parent) : base(parent) { }

    public override bool IsHardcodedModule => false;

    public override string ComponentLogTopic => CoreConsts.CONF_TOPIC;

    public bool IsServerImplementation => true;

    [Inject] IDataStoreHub m_DataHub;
    [Inject] ICacheModule m_Cache;
//todo Abstract this away
//    [InjectModule] IEventProducer m_Events;


    private void purgeHierarchyCacheTables()
    {
      m_Cache.Cache.Tables[CACHE_TBL_GENERIC]?.Purge();
      m_Cache.Cache.Tables[CACHE_TBL_HIERARCHY_NODE]?.Purge();
    }

    protected override bool DoApplicationAfterInit()
    {
      return base.DoApplicationAfterInit();
    }


    #region IForestLogic

    public Task<IEnumerable<VersionInfo>> GetNodeVersionListAsync(EntityId id)
    {
      throw new NotImplementedException();
    }

    public Task<TreeNodeInfo> GetNodeInfoVersionAsync(EntityId idVersion)
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<Atom>> GetTreeListAsync(Atom idForest)
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<TreeNodeHeader>> GetChildNodeListAsync(EntityId idParent, DateTime? asOfUtc = null, ICacheParams cache = null)
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
