/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Log;
using Azos.Instrumentation;
using Azos.Data.Idgen;
using Azos.Apps.Injection;
using Azos.Data;

namespace Azos.Sky.Fabric.Server
{
  /// <summary>
  /// Abstraction of fiber persisted store - where the system stores the state of fibers
  /// </summary>
  public sealed partial class FiberProcessorDaemon : DaemonWithInstrumentation<IApplicationComponent>, IFiberManager
  {
    public const int VERSION_YYYYMMDD = 20230101;

    public const string CONFIG_IMAGE_RESOLVER_SECTION = "image-resolver";


    public const int QUANTUM_SIZE_DEFAULT = 32;
    public const int QUANTUM_SIZE_MIN = 1;
    public const int QUANTUM_SIZE_MAX = 250;

    public const int SHUTDOWN_TIMEOUT_SEC_DEFAULT = 25;
    public const int SHUTDOWN_TIMEOUT_SEC_MIN = 0;
    public const int SHUTDOWN_TIMEOUT_SEC_MAX = 10 * 60;

    private static readonly int MAX_TASKS = Environment.ProcessorCount * 8;

    private sealed class daemonRuntime : ApplicationComponent<FiberProcessorDaemon>, IFiberRuntime
    {
      public daemonRuntime(FiberProcessorDaemon director) : base(director){ }
      public override string ComponentLogTopic => CoreConsts.FABRIC_TOPIC;

      public override MessageType ComponentEffectiveLogLevel => ComponentDirector.ComponentEffectiveLogLevel;
      public override string ComponentLogFromPrefix => $"@{ComponentSID}:FabProcNode(`{ComponentDirector.ProcessorId}`).";

      public int   Version => VERSION_YYYYMMDD;
      public float SystemLoadCoefficient => ComponentDirector.SystemLoadCoefficient;
      public bool  IsRunning => ComponentDirector.Running;
      public bool  IsDebugging => false;
    }


    public FiberProcessorDaemon(IApplicationComponent director) : base(director)
    {
      m_Runtime = new daemonRuntime(this);

    }

    protected override void Destructor()
    {
      cleanupRunspaces();
      DisposeAndNull(ref m_Runtime);
      base.Destructor();
    }

    [Inject] ISystemLoadMonitor<SysLoadSample> m_SysLoadMonitor;
    [Inject] IGdidProviderModule m_Gdid;

    private daemonRuntime m_Runtime;
    private GuidTypeResolver<Fiber, FiberImageAttribute> m_ImageTypeResolver;
    private Thread m_Thread;
    private AutoResetEvent m_PendingEvent;
    private AutoResetEvent m_IdleEvent;

    private Atom m_ProcessorId;
    private AtomRegistry<RunspaceMapping> m_Runspaces;
    private int m_PendingCount;//semaphore

    private int m_QuantumSize = QUANTUM_SIZE_DEFAULT;
    private int m_ShutdownTimeoutSec = SHUTDOWN_TIMEOUT_SEC_DEFAULT;
    private EntityId m_DefaultSecurityAccount;


    public override string ComponentLogTopic => CoreConsts.FABRIC_TOPIC;

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_FABRIC, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled { get; set; }

    /// <summary>
    /// Default account is used for execution of fibers when fibers do not impersonate as someone else
    /// </summary>
    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_SECURITY, CoreConsts.EXT_PARAM_GROUP_FABRIC)]
    public EntityId DefaultSecurityAccount
    {
      get => m_DefaultSecurityAccount;
      set => m_DefaultSecurityAccount = value.HasRequiredValue(nameof(DefaultSecurityAccount));
    }


    /// <summary>
    /// Controls processor fiber scheduling mechanism by defining the size of fiber quantum -
    /// a size of work batch which gets fibers from the store shard for a single schedule processing run.
    /// The default value is 32
    /// </summary>
    [Config(Default = QUANTUM_SIZE_DEFAULT), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_QUEUE, CoreConsts.EXT_PARAM_GROUP_FABRIC)]
    public int QuantumSize
    {
      get => m_QuantumSize;
      set
      {
        m_QuantumSize = value.KeepBetween(QUANTUM_SIZE_MIN, QUANTUM_SIZE_MAX);
      }
    }

    /// <summary>
    /// Processor Id. Must be immutable for lifetime of processor
    /// </summary>
    public Atom ProcessorId => m_ProcessorId;

    /// <summary>
    /// Returns runspaces which this processor recognizes
    /// </summary>
    public IAtomRegistry<RunspaceMapping> Runspaces => m_Runspaces;

    public override int ExpectedShutdownDurationMs => ShutdownTimeoutSec * 1000;

    /// <summary>
    /// Specifies how long it takes to shut down the processing daemon, 20 sec is the default
    /// </summary>
    [Config(Default = SHUTDOWN_TIMEOUT_SEC_DEFAULT), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_FABRIC)]
    public int ShutdownTimeoutSec
    {
      get => m_ShutdownTimeoutSec;
      set => m_ShutdownTimeoutSec.KeepBetween(SHUTDOWN_TIMEOUT_SEC_MIN,
                                              SHUTDOWN_TIMEOUT_SEC_MAX);
    }

    /// <summary>
    /// Returns a number of Fiber quanta being executed by the processor right now
    /// </summary>
    public int PendingCount => Thread.VolatileRead(ref m_PendingCount);

    /// <summary>
    /// Returns an averaged current system load coefficient expressed as [0..1].
    /// 1.0 represents full system availability (e.g. low CPU/ram consumption),
    /// 0.0 represents system being maxed-out of resources.
    /// Fibers may sometimes query this number to postpone/reschedule processing of slices
    /// with heavy activity
    /// </summary>
    public float SystemLoadCoefficient
    {
      get
      {
        var sysLoad = m_SysLoadMonitor.DefaultAverage;
        var cpu = sysLoad.CpuLoadPercent * 100;
        var ram = sysLoad.RamLoadPercent * 100;

        var wl = (PendingCount / (float)MAX_TASKS).KeepBetween(0.0f, 1.0f);


        var result = 1.0f;

        if (ram > 90) return 0f;
        else if (ram > 80) result -= 0.90f;
        else if (ram > 70) result -= 0.60f;
        else if (ram > 60) result -= 0.32f;
        else if (ram > 50) result -= 0.05f;

        if (cpu > 95) return 0f;
        else if (cpu > 80) result -= 0.75f;
        else if (cpu > 70) result -= 0.60f;
        else if (cpu > 50) result -= 0.18f;

        if (result > 0 &&  wl > .5f)
        {
          result *= (1f - wl);
        }

        return result.AtMinimum(0.0f);
      }
    }


    #region Daemon
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      if (node == null) return;
      cleanupRunspaces();
      m_Runspaces = new AtomRegistry<RunspaceMapping>();
      foreach (var nrunspace in node.ChildrenNamed(RunspaceMapping.CONFIG_RUNSPACE_SECTION))
      {
        var runspace = FactoryUtils.MakeDirectedComponent<RunspaceMapping>(this, nrunspace, typeof(RunspaceMapping), new[] { nrunspace });
        m_Runspaces.Register(runspace).IsTrue($"Unique runspace name `{runspace.Name}`");
      }

      m_ImageTypeResolver = new GuidTypeResolver<Fiber, FiberImageAttribute>(node[CONFIG_IMAGE_RESOLVER_SECTION]);
    }

    private void cleanupRunspaces()
    {
      m_Runspaces.ForEach(r => r.Dispose());
    }

    protected override void DoStart()
    {
      (m_Runspaces.Count > 0).IsTrue("Configured runspaces");
      m_ImageTypeResolver.NonNull($"Configured `{CONFIG_IMAGE_RESOLVER_SECTION}`")
                         .HasAnyEntries.IsTrue("Image type mappings");

      m_DefaultSecurityAccount.HasRequiredValue(nameof(DefaultSecurityAccount));

      //todo Check preconditions

      m_PendingEvent = new AutoResetEvent(false);
      m_IdleEvent = new AutoResetEvent(false);

      m_Thread = new Thread(threadSpin);
      m_Thread.IsBackground = false;
      m_Thread.Name = "Main " + nameof(FiberProcessorDaemon);
      m_Thread.Start();
    }

    protected override void DoSignalStop()
    {
      m_IdleEvent.Set();
      m_PendingEvent.Set();
    }

    protected override void DoWaitForCompleteStop()
    {
      var tm = Time.Timeter.StartNew();
      var timeout = ShutdownTimeoutSec;
      while (PendingCount > 0 || (timeout > 0 && tm.ElapsedSec > timeout))
      {
        m_PendingEvent.WaitOne(200);
      }
      tm.Stop();

      var pc = PendingCount;
      if (pc > 0)
      {
        WriteLog(MessageType.Critical,
                 "DoWaitForCompleteStop.pending",
                 "There are still {0} tasks unfinished even after waiting over the max timeout of {1} for {2:n1} sec".Args(pc, timeout, tm.ElapsedSec));
      }

      m_Thread.Join();
      DisposeAndNull(ref m_PendingEvent);
      DisposeAndNull(ref m_IdleEvent);
    }
    #endregion
  }
}
