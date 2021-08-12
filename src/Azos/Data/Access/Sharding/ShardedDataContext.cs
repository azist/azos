/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Data.Access.Sharding
{
  /// <summary>
  /// Implements IDataContext(IDataStore) with sharding
  /// </summary>
  public class ShardedDataContext : DaemonWithInstrumentation<IApplicationComponent>, IShardedCrudDataStoreImplementation
  {
    public const string CONFIG_STORE_SECTION = "store";

    public ShardedDataContext(IApplication app) : base(app) {  }
    public ShardedDataContext(IApplicationComponent director) : base(director) { }

    protected override void Destructor()
    {
      cleanup();
      base.Destructor();
    }

    private bool m_InstrumentationEnabled;
    private ICrudDataStoreImplementation m_PhysicalStore;

    /// <summary>
    /// Underlying physical data store servicing requests
    /// </summary>
    public ICrudDataStore PhysicalStore => m_PhysicalStore.NonNull("configured `./store`");


    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_DATA)]
    public StoreLogLevel DataLogLevel { get; set; }

    public string TargetName => "*";
    public int DefaultTimeoutMs { get => 0; set {} }

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

    public IEnumerable<IShard> CurrentSet => throw new NotImplementedException();


    public void TestConnection()
    {

    }

    public CrudOperations GetOperationsFor(ShardKey key)
    {
      throw new NotImplementedException();
    }

    CrudOperationCallContext IShardedCrudDataStoreImplementation.MakeCallContext(IShard shard)
    {
      throw new NotImplementedException();
    }


    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      if (node == null) return;

      cleanup();

      //m_Contexts = new Registry<IDataContextImplementation>();
      //foreach (var ndb in node.ChildrenNamed(CONFIG_CONTEXT_SECTION))
      //{
      //  var context = FactoryUtils.MakeAndConfigureDirectedComponent<IDataContextImplementation>(
      //                                  this,
      //                                  ndb);

      //  if (!m_Contexts.Register(context))
      //    throw new DataAccessException($"{nameof(DefaultDataContextHub)} config contains duplicate named section: ./context[name='{context.Name}']");
      //}
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
    }


  }
}
