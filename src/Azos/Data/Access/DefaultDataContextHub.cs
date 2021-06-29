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

namespace Azos.Data.Access
{
  /// <summary>
  /// Implements IDataContextHubImplementation
  /// </summary>
  public sealed class DefaultDataContextHub : DaemonWithInstrumentation<IApplicationComponent>, IDataContextHubImplementation
  {
    public const string CONFIG_CONTEXT_SECTION = "context";

    public const string NOT_CONFIGURED_ERROR = "The class was not configured. Call .Config(section)";

    public DefaultDataContextHub(IApplication app) : base(app) {  }

    protected override void Destructor()
    {
      cleanup();
      base.Destructor();
    }

    private bool m_InstrumentationEnabled;
    private Registry<IDataContextImplementation> m_Contexts = new Registry<IDataContextImplementation>();


    public IRegistry<IDataContext> Contexts => m_Contexts.NonNull(NOT_CONFIGURED_ERROR);

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

      m_Contexts = new Registry<IDataContextImplementation>();
      foreach (var ndb in node.ChildrenNamed(CONFIG_CONTEXT_SECTION))
      {
        var context = FactoryUtils.MakeAndConfigureDirectedComponent<IDataContextImplementation>(
                                        this,
                                        ndb);

        if (!m_Contexts.Register(context))
          throw new DataAccessException($"{nameof(DefaultDataContextHub)} config contains duplicate named section: ./context[name='{context.Name}']");
      }
    }

    protected override void DoStart()
    {
      base.DoStart();
      m_Contexts.OfType<Daemon>().ForEach(c => this.DontLeak(() => c.Start()));
    }

    protected override void DoSignalStop()
    {
      base.DoSignalStop();
      m_Contexts.OfType<Daemon>().ForEach(c => this.DontLeak(() => c.SignalStop()));
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();
      m_Contexts.OfType<Daemon>().ForEach(c => this.DontLeak(() => c.WaitForCompleteStop()));
    }

    protected override void DoAcceptManagerVisit(object manager, DateTime managerNow)
    {
      base.DoAcceptManagerVisit(manager, managerNow);
      m_Contexts.OfType<Daemon>().ForEach(d => this.DontLeak(() => d.AcceptManagerVisit(this, managerNow)));
    }

    private void cleanup()
    {
      var all = m_Contexts.ToArray();
      m_Contexts.Clear();
      all.ForEach(c => this.DontLeak(() => c.Dispose()));
    }
  }
}
