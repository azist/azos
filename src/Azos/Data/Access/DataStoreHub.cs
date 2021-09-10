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

namespace Azos.Data.Access
{
  /// <summary>
  /// Provides base implementation of IDataStoreHubImplementation
  /// </summary>
  public class DataStoreHub : DaemonWithInstrumentation<IApplicationComponent>, IDataStoreHubImplementation
  {
    public const string CONFIG_STORE_SECTION = "store";

    public DataStoreHub(IApplication app) : base(app) {  }

    protected override void Destructor()
    {
      cleanup();
      base.Destructor();
    }

    private bool m_InstrumentationEnabled;
    private Registry<IDataStoreImplementation> m_Stores = new Registry<IDataStoreImplementation>();


    public IRegistry<IDataStore> DataStores => m_Stores;

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



    public void TestConnection() { }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      if (node == null) return;

      cleanup();

      foreach (var ndb in node.ChildrenNamed(CONFIG_STORE_SECTION))
      {
        var store = FactoryUtils.MakeAndConfigureDirectedComponent<IDataStoreImplementation>(
                                        this,
                                        ndb);

        if (!m_Stores.Register(store))
          throw new DataAccessException($"{nameof(DataStoreHub)} config duplicate named section: ./context[name='{store.Name}']");
      }
    }

    protected override void DoStart()
    {
      base.DoStart();
      m_Stores.OfType<Daemon>().ForEach(c => this.DontLeak(() => c.Start()));
    }

    protected override void DoSignalStop()
    {
      base.DoSignalStop();
      m_Stores.OfType<Daemon>().ForEach(c => this.DontLeak(() => c.SignalStop()));
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();
      m_Stores.OfType<Daemon>().ForEach(c => this.DontLeak(() => c.WaitForCompleteStop()));
    }

    protected override void DoAcceptManagerVisit(object manager, DateTime managerNow)
    {
      base.DoAcceptManagerVisit(manager, managerNow);
      m_Stores.OfType<Daemon>().ForEach(d => this.DontLeak(() => d.AcceptManagerVisit(this, managerNow)));
    }

    private void cleanup()
    {
      var all = m_Stores.ToArray();
      m_Stores.Clear();
      all.ForEach(s => this.DontLeak(() => s.Dispose()));
    }
  }
}
