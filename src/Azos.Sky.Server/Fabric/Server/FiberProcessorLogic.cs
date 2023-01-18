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

namespace Azos.Sky.Fabric.Server
{
  /// <summary>
  /// Abstraction of fiber persisted store - where the system stores the state of fibers
  /// </summary>
  public sealed class FiberProcessorDaemon : Daemon, IFiberManager
  {
    public FiberProcessorDaemon(IApplicationComponent director) : base(director) { }

    protected override void Destructor()
    {
      base.Destructor();
    }

    private Thread m_Thread;
    private AutoResetEvent m_PendingEvent;
    private AutoResetEvent m_IdleEvent;

    private Atom m_ProcessorId;
    private AtomRegistry<RunspaceMapping> m_Runspaces;
    private int m_PendingCount;//semaphore



    public override string ComponentLogTopic => CoreConsts.FABRIC_TOPIC;

    /// <summary>
    /// Processor Id. Must be immutable for lifetime of shard
    /// </summary>
    public Atom ProcessorId => m_ProcessorId;

    /// <summary>
    /// Returns runspaces which this processor recognizes
    /// </summary>
    public IAtomRegistry<RunspaceMapping> Runspaces => m_Runspaces;


    /// <summary>
    /// Returns a number of Fiber quanta being executed by the processor right now
    /// </summary>
    public int PendingCount => Thread.VolatileRead(ref m_PendingCount);


    #region Daemon
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
    }

    protected override void DoStart()
    {
      //todo Check preconditions

      m_PendingEvent = new AutoResetEvent(false);
      m_IdleEvent = new AutoResetEvent(false);
      m_Thread = new Thread(threadSpin);
      m_Thread.Name = "Main " + nameof(FiberProcessorDaemon);
      m_Thread.Start();
    }

    protected override void DoSignalStop()
    {
      m_IdleEvent.Set();
    }

    protected override void DoWaitForCompleteStop()
    {
      m_Thread?.Join();
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
      const int QUANTUM_SIZE = 100;//<=== MOVE to property instead
      const int QUANTUM_SIZE_MAX = QUANTUM_SIZE * 10;

      var work = new List<ShardMapping>(10 * 1024);
      foreach(var runspace in m_Runspaces)
      {
        var rsBatch = ((int)(QUANTUM_SIZE * runspace.ProcessingFactor)).KeepBetween(0, QUANTUM_SIZE_MAX);
        if (rsBatch == 0) continue;

        foreach(var shard in runspace.Shards)
        {
          var shBatch = ((int)(rsBatch * shard.ProcessingFactor)).KeepBetween(0, QUANTUM_SIZE_MAX);
          for(var i=0; i<shBatch; i++)
          {
            work.Add(shard);
          }
        }
      }

      var workQueue = work.RandomShuffle();//ensure fairness for all pieces of work across
      processQuantumQueue(workQueue);
    }

    private static readonly int MAX_TASKS = System.Environment.ProcessorCount * 8;

    private void processQuantumQueue(IEnumerable<ShardMapping> workQueue)
    {
      foreach(var shard in workQueue)
      {
        //dynamic throttle
        while(this.Running)
        {
          var pendingNow = Thread.VolatileRead(ref m_PendingCount);
          var cpu = Platform.Computer.CurrentProcessorUsagePct; //USE EMA filter
          //read CPU consumption here and throttle down proportionally to CPU usage
          var maxTasksNow = cpu < 45 ? MAX_TASKS : cpu < 65 ? MAX_TASKS / 2 : cpu < 85 ? MAX_TASKS / 4 : 1;
          if (pendingNow < maxTasksNow) break;

          //system is busy, wait
          m_PendingEvent.WaitOne(250);
        }

        //Of the thread pool, spawn a worker
        Task.Factory.StartNew(s => processFiberQuantum((ShardMapping)s),
                              shard,
                              TaskCreationOptions.HideScheduler);// FiberTaskScheduler);
      }//foreach
    }

    private async Task processFiberQuantum(ShardMapping shard)
    {
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
      Interlocked.Increment(ref m_PendingCount);
      try
      {
        var memory = await shard.CheckOutNextPendingAsync(shard.Runspace.Name, ProcessorId).ConfigureAwait(false);
        if (memory == null) return;//no pending work

        await processFiberQuantumCore(memory).ConfigureAwait(false);//<===================== FIBER SLICE gets called

        var delta = memory.MakeDeltaSnapshot();

        var saveErrorCount = 0;
        while(this.Running)
        {
          try
          {
            await shard.CheckInAsync(delta).ConfigureAwait(false);
            break;
          }
          catch(Exception saveError)
          {
            if (saveErrorCount++ > 3) throw;

            WriteLog(MessageType.CriticalAlert,
                     "processFiberQuantumUnsafe.save",
                     "Unable to save MemoryDelta {0} times: {1}".Args(saveErrorCount, saveError.ToMessageWithType()),
                     saveError,
                     related: logRel);

            await Task.Delay(1000).ConfigureAwait(false);
          }
        }
      }
      finally
      {
        Interlocked.Decrement(ref m_PendingCount);
        m_PendingEvent.Set();
      }
    }


    private async Task processFiberQuantumCore(FiberMemory memory)
    {
      Fiber fiber = null;//Allocate dyn from proccess image id
   //   fiber.__processor__ctor(runtime, pars, state);

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
    }
    #endregion

    #region IFiberManager
    public IEnumerable<Atom> GetRunspaces()
    {
      throw new NotImplementedException();
    }

    public FiberId AllocateFiberId(Atom runspace)
    {
      throw new NotImplementedException();
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
