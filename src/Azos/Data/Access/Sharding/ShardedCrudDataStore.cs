/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Data.Access.Sharding
{
  /// <summary>
  /// Implements IDataContext(IDataStore) with sharding. The default implementation uses weighted rendezvous hashing technique
  /// </summary>
  /// <remarks>
  /// See https://en.wikipedia.org/wiki/Rendezvous_hashing
  /// </remarks>
  public class ShardedCrudDataStore : DaemonWithInstrumentation<IApplicationComponent>, IShardedCrudDataStoreImplementation
  {
    public const string CONFIG_STORE_SECTION = "store";
    public const string CONFIG_SHARDSET_SECTION = "shard-set";

    public ShardedCrudDataStore(IApplication app) : base(app) {  }
    public ShardedCrudDataStore(IApplicationComponent director) : base(director) { }

    protected override void Destructor()
    {
      cleanup();
      base.Destructor();
    }

    private bool m_InstrumentationEnabled;
    private ICrudDataStoreImplementation m_PhysicalStore;
    private OrderedRegistry<ShardSet> m_ShardSets = new OrderedRegistry<ShardSet>();

    /// <summary>
    /// Underlying physical data store servicing requests
    /// </summary>
    public ICrudDataStore PhysicalStore => m_PhysicalStore.NonNull("configured `./store`");


    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_DATA)]
    public StoreLogLevel DataLogLevel { get; set; }

    public string TargetName => PhysicalStore.TargetName;

    public int DefaultTimeoutMs
    {
      get => (m_PhysicalStore?.DefaultTimeoutMs) ?? 0;
      set { }
    }

    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;

    /// <summary>
    /// Implements IInstrumentable
    /// </summary>
    [Config(Default = false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled
    {
      get { return m_InstrumentationEnabled; }
      set { m_InstrumentationEnabled = value; }
    }

    public ShardSet CurrentShardSet => m_ShardSets[0];

    public IOrderedRegistry<ShardSet> ShardSets => throw new NotImplementedException();


    public virtual void TestConnection() { }

    CrudOperationCallContext IShardedCrudDataStoreImplementation.MakeCallContext(IShard shard) => DoMakeCallContext(shard);
    IShard IShardedCrudDataStoreImplementation.MakeShard(ShardSet set, IConfigSectionNode conf) => DoMakeShard(set, conf);
    IShard IShardedCrudDataStoreImplementation.GetShardFor(ShardSet set, ShardKey key) => DoGetShardFor(set, key);


    protected virtual CrudOperationCallContext DoMakeCallContext(IShard shard)
    {
      return new CrudOperationCallContext()
      {
        ConnectString = shard.RouteConnectString,
        DatabaseName = shard.RouteDatabaseName
      };
    }

    protected virtual IShard DoMakeShard(ShardSet set, IConfigSectionNode conf)
     => new Shard(set, conf);

    protected virtual IShard DoGetShardFor(ShardSet set, ShardKey key)
     => set.RendezvouzRoute(key);


    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      cleanup();

      if (node==null || !node.Exists) return;

      var nstore = node[CONFIG_STORE_SECTION];
      m_PhysicalStore = FactoryUtils.MakeAndConfigureDirectedComponent<ICrudDataStoreImplementation>(this, nstore);

      foreach (var nset in node.ChildrenNamed(CONFIG_SHARDSET_SECTION))
      {
        var set = FactoryUtils.MakeDirectedComponent<ShardSet>(this, nset, typeof(ShardSet), new[]{nset});

        if (!m_ShardSets.Register(set))
          throw new DataAccessException(StringConsts.DATA_SHARDING_DUPLICATE_SECTION_CONFIG_ERROR.Args(CONFIG_SHARDSET_SECTION, set.Name));
      }

      if (m_ShardSets.Count != m_ShardSets.DistinctBy(s => s.Order).Count())
      {
        throw new DataAccessException(StringConsts.DATA_SHARDING_DUPLICATE_SHARDSET_ORDER_CONFIG_ERROR);
      }
    }


    protected override void DoStart()
    {
      m_PhysicalStore.NonNull(nameof(m_PhysicalStore));
      if (m_PhysicalStore is Daemon d) d.Start();
      base.DoStart();
    }

    protected override void DoSignalStop()
    {
      base.DoSignalStop();
      if (m_PhysicalStore is Daemon d) d.SignalStop();
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();
      if (m_PhysicalStore is Daemon d) d.WaitForCompleteStop();
    }

    protected override void DoAcceptManagerVisit(object manager, DateTime managerNow)
    {
      base.DoAcceptManagerVisit(manager, managerNow);

      if (m_PhysicalStore is Daemon d)
      {
        this.DontLeak(() => d.AcceptManagerVisit(this, managerNow));
      }
    }

    private void cleanup()
    {
      this.DontLeak(() =>  DisposeAndNull(ref m_PhysicalStore));

      m_ShardSets.ForEach(s => this.DontLeak(() => DisposeAndNull(ref m_PhysicalStore)));
      m_ShardSets.Clear();
    }


  }
}
