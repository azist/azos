/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Log;
using Azos.Instrumentation;
using Azos.Data.Idgen;
using Azos.Apps.Injection;
using Azos.Platform;
using Azos.Serialization.JSON;

namespace Azos.Sky.Fabric.Server
{
  /// <summary>
  /// Abstraction of fiber persisted store - where the system stores the state of fibers
  /// </summary>
  public sealed class FiberProcessorDaemon : DaemonWithInstrumentation<IApplicationComponent>, IFiberManager
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



    public override string ComponentLogTopic => CoreConsts.FABRIC_TOPIC;

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_FABRIC, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled { get; set; }

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
        CheckDaemonInactive();
        m_QuantumSize = value.KeepBetween(QUANTUM_SIZE_MIN, QUANTUM_SIZE_MAX);
      }
    }

    /// <summary>
    /// Processor Id. Must be immutable for lifetime of shard
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

    #region .pvt .impl

    private void threadSpin()
    {
      const int MAX_UNHANDLED_ERRORS = 10;

      var rel = Guid.NewGuid();
      var errorCount = 0;
      try
      {
        while(this.Running)
        {
          try
          {
            scheduleQuantum();//safe method which should never leak any errors

            if (errorCount > 0)
            {
              WriteLog(MessageType.CatastrophicError, "threadSpin.inner", "Recovered after {0} errors".Args(errorCount), related: rel);
            }
            errorCount = 0;
          }
          catch(Exception innerError)
          {
            errorCount++;

            if (errorCount > MAX_UNHANDLED_ERRORS) throw;//terminate the process

            WriteLog(MessageType.CatastrophicError, "threadSpin.inner",
                        "Leaked: " + innerError.ToMessageWithType(),
                        innerError,
                        related: rel);

            m_IdleEvent.WaitOne((2000 * errorCount).ChangeByRndPct(0.25f));//progressive back-off delay
            continue;
          }

          if (PendingCount < 1)
          {
            m_IdleEvent.WaitOne(App.Random.NextScaledRandomInteger(250, 1250));
          }
        }//while
      }
      catch(Exception error)
      {
        WriteLog(MessageType.CatastrophicError, "threadSpin.outer",
                         "Processing terminated. Leaked: " + error.ToMessageWithType(),
                         error,
                         related: rel);
      }
    }


    //instead need to base scheduling on semaphore+CPU usage
    //load: number of pending fibers + CPU usage on machine
    private void scheduleQuantum()
    {
      var work = new List<ShardMapping>(10 * 1024);
      foreach(var runspace in m_Runspaces)
      {
        var rsBatch = (int)(QuantumSize * runspace.ProcessingFactor);
        if (rsBatch == 0) continue;

        foreach(var shard in runspace.Shards)
        {
          var shBatch = (int)(rsBatch * shard.ProcessingFactor);
          for(var i=0; i<shBatch; i++)
          {
            work.Add(shard);
          }
        }
      }

      var workQueue = work.RandomShuffle();//ensure fairness for all pieces of work across
      processQuantumQueue(workQueue);
    }



    private void processQuantumQueue(IEnumerable<ShardMapping> workQueue)
    {
      foreach(var shard in workQueue)
      {
        //dynamic throttle
        while(this.Running)
        {
          var pendingNow = Thread.VolatileRead(ref m_PendingCount);
          var sysLoad = m_SysLoadMonitor.DefaultAverage;
          var cpu = sysLoad.CpuLoadPercent;

          //read CPU consumption here and throttle down proportionally to CPU usage
          var maxTasksNow = cpu < 0.45 ? MAX_TASKS : cpu < 0.65 ? MAX_TASKS / 2 : cpu < 0.85 ? MAX_TASKS / 8 : 1;
          if (pendingNow < maxTasksNow) break;

          //system is busy, wait
          m_PendingEvent.WaitOne(250);
        }

        if (this.Running)
        {
          //Of the thread pool, spawn a worker
          Task.Factory.StartNew(s => processFiberQuantum((ShardMapping)s),
                                shard,
                                TaskCreationOptions.HideScheduler);// FiberTaskScheduler);
        }
      }//foreach
    }

    private async Task processFiberQuantum(ShardMapping shard)
    {
      if (!this.Running) return;

      var rel = Guid.NewGuid();
      try
      {
        await processFiberQuantumUnsafe(shard, rel).ConfigureAwait(false);
      }
      catch(Exception error)
      {
        WriteLog(MessageType.CriticalAlert,
                 "processFiberQuantum",
                 "Unexpected leak: " + error.ToMessageWithType(),
                 error,
                 related: rel);
      }
    }

    private async Task processFiberQuantumUnsafe(ShardMapping shard, Guid logRel)
    {
      Interlocked.Increment(ref m_PendingCount); //Semaphore
      try
      {
        var memory = await shard.CheckOutNextPendingAsync(shard.Runspace.Name, ProcessorId).ConfigureAwait(false);
        if (memory == null) return;//no pending work
        if (!Running) return;//not running anymore

        var wasHandled = await processFiberQuantumCore(memory).ConfigureAwait(false);//<===================== FIBER SLICE gets called

        if (!wasHandled)
        {
          await shard.UndoCheckoutAsync(memory.Id).ConfigureAwait(false);
          return;//get out asap
        }

        var delta = memory.MakeDeltaSnapshot();

        var saveErrorCount = 0;
        while(true)
        {
          try
          {
            await shard.CheckInAsync(delta).ConfigureAwait(false);
            break;
          }
          catch(Exception saveError)
          {
            var maxRetries = Running ? 5 : 3;
            if (saveErrorCount++ > maxRetries) throw;

            WriteLog(MessageType.CriticalAlert,
                     "processFiberQuantumUnsafe.save",
                     "Unable to save MemoryDelta {0} times: {1}".Args(saveErrorCount, saveError.ToMessageWithType()),
                     saveError,
                     related: logRel);

            var msPause = Running ? (saveErrorCount * 1000) : 100;
            await Task.Delay(msPause.ChangeByRndPct(0.25f)).ConfigureAwait(false);
          }
        }//while
      }
      finally
      {
        Interlocked.Decrement(ref m_PendingCount);
        m_PendingEvent.Set();
      }
    }

    private DateTime m_UnknownImageGuidsLastReset;
    private FiniteSetLookup<Guid, bool> m_UnknownImageGuids = new FiniteSetLookup<Guid, bool>(_ => true);
    private void reportUnknownImage(Guid guid)
    {
      //Every 30 minutes the system forgets what GUIS it already notified about and it keeps
      //nagging logs as long as execution of fibers with unknown images continues
      var now = DateTime.UtcNow;
      if ((now - m_UnknownImageGuidsLastReset).TotalMinutes > 30)
      {
        m_UnknownImageGuidsLastReset = now;
        m_UnknownImageGuids.Purge();
      }

      var (_, alreadyExisted) = m_UnknownImageGuids.GetWithFlag(guid);
      if (alreadyExisted) return;
      var error = new FabricException("Unknown process id: " + guid);
      WriteLog(MessageType.CatastrophicError, nameof(reportUnknownImage), error.ToMessageWithType(), error, pars: new{ guid }.ToJson() );
    }


    private async Task<bool> processFiberQuantumCore(FiberMemory memory)
    {
      //Determine the type
      var tFiber = m_ImageTypeResolver.TryResolve(memory.ImageTypeId);
      if (tFiber == null)//image not found
      {
        reportUnknownImage(memory.ImageTypeId);//send CatastrophicError event
        return false;//do nothing
      }

      var fiber = (Fiber)Serialization.SerializationUtils.MakeNewObjectInstance(tFiber);
    //  fiber.__processor__ctor(runtime, pars, state);

      //todo:  Impersonate here
      try
      {
        var nextStep = await fiber.ExecuteSliceAsync() //use Timedcall.Run()   KILL LONG running tasks with auto reset timeout
                                  .ConfigureAwait(false);//<===================== FIBER SLICE gets called
      }
      catch(Exception fiberError)
      {
        //crash fiber
        //write to memory state the exception details to crash fiber
      }

      return true;
    }
    #endregion

    #region IFiberManager
    public IEnumerable<Atom> GetRunspaces() => m_Runspaces.Select(one => one.Name).ToArray();

    public FiberId AllocateFiberId(Atom runspace)
    {
      CheckDaemonActive();
      runspace.IsValidNonZero(nameof(runspace));
      var rs = m_Runspaces[runspace];
      rs.NonNull($"Runspace `{runspace}`");
      var shard = rs.GetShardForNewAllocation();

      if (shard == null)
      {
        throw new FabricFiberAllocationException(ServerStringConsts.FABRIC_FIBER_ALLOC_NO_SPACE_ERROR.Args());
      }


      //Runspaces must be in separate databases as their ids are used as sequence names
      var gdid = m_Gdid.Provider.GenerateOneGdid(SysConsts.GDID_NS_FABRIC, runspace.Value);

      var result = new FiberId(rs.Name, shard.Name, gdid);
      return result;
    }

    public Task<FiberInfo> StartFiberAsync(FiberStartArgs args)
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<FiberInfo>> GetFiberListAsync(FiberFilter args)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> GetFiberInfoAsync(FiberId idFiber)
    {
      throw new NotImplementedException();
    }

    public Task<FiberParameters> GetFiberParametersAsync(FiberId idFiber)
    {
      throw new NotImplementedException();
    }

    public Task<FiberResult> GetFiberResultAsync(FiberId idFiber)
    {
      throw new NotImplementedException();
    }

    public Task<FiberState> GetFiberStateAsync(FiberId idFiber)
    {
      throw new NotImplementedException();
    }

    public Task<bool> LoadFiberStateSlotAsync(FiberId idFiber, FiberState.Slot slot)
    {
      throw new NotImplementedException();
    }

    public Task<FiberSignalResponse> SendSignalAsync(FiberSignal signal)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> SetPriorityAsync(FiberId idFiber, float priority, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> PauseAsync(FiberId idFiber, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> SuspendAsync(FiberId idFiber, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> ResumeAsync(FiberId idFiber, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> AbortAsync(FiberId idFiber, string statusDescription)
    {
      throw new NotImplementedException();
    }
    #endregion
  }
}
