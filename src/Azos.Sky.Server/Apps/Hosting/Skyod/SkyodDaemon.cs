/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Threading;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Log;
using Azos.Serialization.JSON;

namespace Azos.Apps.Hosting.Skyod
{
  /// <summary>
  /// Provides services for managing subordinate nodes and Governor processes on nodes
  /// </summary>
  public sealed class SkyodDaemon : DaemonWithInstrumentation<IApplicationComponent>
  {
    public const string CONFIG_CHAIN_ATTR = "chain-boot-path";


    public SkyodDaemon(IApplication app) : base(app)
    {
      m_Sets = new Registry<SoftwareSet>();
    }

    protected override void Destructor()
    {
      base.Destructor();
      cleanup();
    }

    private void cleanup()
    {
      DisposeAndNull(ref m_Chain);
      m_Sets.ForEach(c => this.DontLeak(() => c.Dispose()));
      m_Sets.Clear();
    }

    private Registry<SoftwareSet> m_Sets;
    private Thread m_Thread;
    private AutoResetEvent m_Wait;
    private Daemon m_Chain;

    public override string ComponentLogTopic => Sky.SysConsts.LOG_TOPIC_SKYOD;

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled { get; set; }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      cleanup();
      if (node == null) return;

      // 1 Configure chained daemon (if any)
      var chainPath = node.ValOf(CONFIG_CHAIN_ATTR);
      if (chainPath.IsNotNullOrWhiteSpace())
      {
        var nChain = node.NavigateSection(chainPath);
        if (nChain.Exists)
        {
          m_Chain = FactoryUtils.MakeAndConfigureDirectedComponent<Daemon>(this, nChain, typeof(Azos.Wave.WaveServer));
        }
      }

      // 2 Configure software sets
      foreach (var nset in node.ChildrenNamed(SoftwareSet.CONFIG_SOFTWARE_SET_SECTION))
      {
        var set = FactoryUtils.MakeDirectedComponent<SoftwareSet>(this, nset, typeof(SoftwareSet), new[] { nset });
        m_Sets.Register(set).IsTrue("Unique software set name `{0}`".Args(set.Name));
      }

      // 3 configure cluster
    }

    protected override void DoStart()
    {
      base.DoStart();

      (m_Sets.Count > 0).IsTrue("Configured software sets");

      if (m_Chain != null)
      {
        WriteLogFromHere(MessageType.Trace, "Starting: {0}/{1}".Args(m_Chain.ServiceDescription, m_Chain.StatusDescription));
        m_Chain.Start();
        WriteLogFromHere(MessageType.Trace, "Started: {0}/{1}".Args(m_Chain.ServiceDescription, m_Chain.StatusDescription));
      }

      m_Wait = new AutoResetEvent(false);

      m_Thread = new Thread(threadBody);
      m_Thread.IsBackground = false;
      m_Thread.Name = nameof(SkyodDaemon);
      m_Thread.Start();
    }

    protected override void DoSignalStop()
    {
      if (m_Chain != null)
      {
        m_Chain.SignalStop();
      }

      m_Wait.Set();
      base.DoSignalStop();
    }

    protected override void DoWaitForCompleteStop()
    {
      if (m_Thread != null)
      {
        m_Thread.Join();
        m_Thread = null;
      }

      DisposeAndNull(ref m_Wait);

      if (m_Chain != null)
      {
        m_Chain.WaitForCompleteStop();
      }

      base.DoWaitForCompleteStop();
    }


    private void threadBody()
    {
      const int SLICE_MS_LOW = 150;
      const int SLICE_MS_HIGH = 250;

      var rel = Guid.NewGuid();

      while (Running) //<--- LOOP ---
      {
        try
        {
         // doWorkOnce(rel);
        }
        catch (Exception error)
        {
          WriteLogFromHere(MessageType.CatastrophicError, "..scanAllOnce() leaked: " + error.ToMessageWithType(), error, related: rel);
        }

        m_Wait.WaitOne(Ambient.Random.NextScaledRandomInteger(SLICE_MS_LOW, SLICE_MS_HIGH));
      }//while

    }
  }
}
