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


namespace Azos.Sky.Fabric.Server
{
  /// <summary>
  /// Abstraction of fiber persisted store - where the system stores the state of fibers
  /// </summary>
  public sealed class FiberProcessorLogic : ModuleBase//, IFiberManagerLogic
  {
    public FiberProcessorLogic(IApplication application) : base(application)
    {
    }

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.FABRIC_TOPIC;

    private Atom m_ProcessorId;
    private AtomRegistry<RunspaceMapping> m_Runspaces;


    /// <summary>
    /// Processor Id. Must be immutable for lifetime of shard
    /// </summary>
    public Atom ProcessorId => m_ProcessorId;

    /// <summary>
    /// Returns runspaces which this processor recognizes
    /// </summary>
    IAtomRegistry<RunspaceMapping> Runspaces => m_Runspaces;


    private int m_PendingCount;//semaphore
    private AutoResetEvent m_PendingEvent;


    //instead need to base scheduling on semaphore+CPU usage
    //load: number of pending fibers + CPU usage on machine
    private bool scheduleQuantum()
    {
      const int QUANTUM_SIZE = 100;//<=== MOVE to property instead
      const int QUANTUM_SIZE_MAX = QUANTUM_SIZE * 1000;

      var work = new List<ShardMapping>(m_Runspaces.Sum(rs => rs.Shards.Sum(sh => QUANTUM_SIZE)));
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
      scheduleShardQuantum(workQueue);

      return workQueue.Count > 0;
    }

    private void scheduleShardQuantum(IEnumerable<ShardMapping> workQueue)
    {
      foreach(var shard in workQueue)
      {
        //throttle
        while(App.Active)
        {
          var pendingNow = Thread.VolatileRead(ref m_PendingCount);
          //read CPU consumption here and throttle down proportionally to CPU usage
          var cpu = Platform.Computer.CurrentProcessorUsagePct;
          if (pendingNow < 100) break;
          m_PendingEvent.WaitOne(250);
        }

        var _ = Task.Run(async () => {
          var memory = await shard.CheckOutNextPendingAsync(shard.Runspace.Name, ProcessorId).ConfigureAwait(false);
          if (memory == null) return;//no pending work

          Interlocked.Increment(ref m_PendingCount);
          try
          {
            await processFiberQuantum(memory).ConfigureAwait(false);//<===================== FIBER SLICE gets called

            var delta = memory.MakeDeltaSnapshot();
            await shard.CheckInAsync(delta);
          }
          finally
          {
            Interlocked.Decrement(ref m_PendingCount);
            m_PendingEvent.Set();
          }
        });
      }//foreach
    }

    private async Task processFiberQuantum(FiberMemory memory)
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

  }
}
