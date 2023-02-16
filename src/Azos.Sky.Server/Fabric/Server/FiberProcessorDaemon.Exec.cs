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
using Azos.Log;
using Azos.Platform;
using Azos.Serialization.JSON;
using Azos.Security;
using Azos.Data;

namespace Azos.Sky.Fabric.Server
{
  partial class FiberProcessorDaemon
  {
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
          var shBatch = (int)(rsBatch * shard.ProcessingFactor); //can be zero
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

        if (!this.Running) break;

        //Semaphore throttle
        Interlocked.Increment(ref m_PendingCount);

        //Of the thread pool, spawn a worker
        Task.Factory.StartNew(s => processFiberQuantum((ShardMapping)s),
                              shard,
                              TaskCreationOptions.HideScheduler);// FiberTaskScheduler);

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
      finally
      {
        Interlocked.Decrement(ref m_PendingCount);
        m_PendingEvent.Set();
      }
    }

    private async Task processFiberQuantumUnsafe(ShardMapping shard, Guid logRel)
    {
      FiberMemory memory = null;
      try
      {
        if (!Running) return;//not running anymore

        //Next line can take time (e.g. 1+ sec) to complete
        memory = await shard.CheckOutNextPendingAsync(shard.Runspace.Name, ProcessorId).ConfigureAwait(false);
        if (memory == null || memory.Status != MemoryStatus.LockedForCaller) return;//no pending work
        if (!Running) return;//not running anymore

        //=======================================
        //=======================================
        //=======================================
        var (wasHandled, nextStep, fiberState) = await processFiberQuantumCore(memory).ConfigureAwait(false);//<===================== FIBER SLICE gets called
        if (!wasHandled) return;//get out asap
        //=======================================
        //=======================================
        //=======================================

        var delta = memory.MakeDeltaSnapshot(nextStep, fiberState);//this can throw on invalid state

        var saveErrorCount = 0;
        while(true)
        {
          try
          {
            await shard.CheckInAsync(delta).ConfigureAwait(false);
            memory = null;//check-in successful
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
        #region ---- Undo checked out (if any) and dec throttle ----
        try
        {
          if (memory != null && memory.Status == MemoryStatus.LockedForCaller)
          {
            await shard.UndoCheckoutAsync(memory.Id).ConfigureAwait(false);
          }
        }
        catch(Exception error)
        {
          WriteLog(MessageType.CriticalAlert,
                    "prcQUnsf.fin.UndoCheckout",
                    "Leak: " + error.ToMessageWithType(),
                    error,
                    related: logRel);
        }
        #endregion -------------------------------------------------
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

      //DO NOT throw the error! just log it with maximum criticality
      var error = new FabricProcessorException("Fiber image type id `{0}` is unresolved. Check processor assembly mappings under `../{1}`".Args(
                                                 guid,
                                                 CONFIG_IMAGE_RESOLVER_SECTION));

      WriteLog(MessageType.CatastrophicError,
               nameof(reportUnknownImage),
               error.ToMessageWithType(),
               error,
               pars: new{ imageTypeId = guid }.ToJson() );
    }

    private static readonly FiniteSetLookup<Type, (Type tp, Type ts)> FIBER_STATE_TYPES_MAP_CACHE = new FiniteSetLookup<Type, (Type, Type)>( tfiber =>
    {
      Type tancestor = tfiber;
      while(tancestor != null && tancestor != typeof(object))
      {
        if (tancestor.GetGenericTypeDefinition() == typeof(Fiber<,>))
        {
          var gargs = tancestor.GetGenericArguments();
          var tp = gargs[0].IsOfType<FiberParameters>("TParams : FiberParameters");
          var ts = gargs[1].IsOfType<FiberState>("TState : FiberState");
          return (tp, ts);
        }
        tancestor = tancestor.BaseType;
      }

      throw new FabricFiberDeclarationException("Not able to determine TParams/TState of `{0}` which must descend from Fiber<TParams, TState>"
                                                .Args(tfiber.DisplayNameWithExpandedGenericArgs()));
    });


    private async Task<(bool handled, FiberStep? next, FiberState state)> processFiberQuantumCore(FiberMemory memory)
    {
      //1. - Determine the CLR type
      var tFiber = m_ImageTypeResolver.TryResolve(memory.ImageTypeId);
      if (tFiber == null)//image not found
      {
        reportUnknownImage(memory.ImageTypeId);//send CatastrophicError event
        return (false, null, null);//do nothing
      }

      //2. - Allocate the fiber instance
      Fiber fiber = null;
      try
      {
        //Resolve types of Fiber generic arguments
        var (tParameters, tState) = FIBER_STATE_TYPES_MAP_CACHE[tFiber];

        //Deserialize parameters and state
        var (fiberParameters, fiberState) = memory.UnpackBuffer(tParameters, tState);

        //Make fiber
        fiber = (Fiber)Serialization.SerializationUtils.MakeNewObjectInstance(tFiber);

        //Init inject
        fiber.__processor__ctor(m_Runtime, memory.Id, memory.InstanceGuid, fiberParameters, fiberState);
      }
      catch(Exception allocationError)
      {
        //DO NOT throw the error! just log it with maximum criticality
        var error = new FabricProcessorException("Fiber image type id `{0}` resolved to `{1}` leaked init error: {2}".Args(
                                                    memory.ImageTypeId,
                                                    tFiber.DisplayNameWithExpandedGenericArgs(),
                                                    allocationError.ToMessageWithType()), allocationError);

        WriteLog(MessageType.Critical,
                 nameof(processFiberQuantumCore),
                 error.ToMessageWithType(),
                 error,
                 pars: new { imageTypeId = memory.ImageTypeId, t = tFiber.DisplayNameWithExpandedGenericArgs() }.ToJson());

        return (false, null, null);
      }

      //3. - Impersonate the call flow
      Credentials credentials = mapEntityCredentials(memory.ImpersonateAs ?? m_DefaultSecurityAccount);
      var user = await App.SecurityManager.AuthenticateAsync(credentials).ConfigureAwait(false);

      //inject session for Fiber
      var session = new BaseSession(Guid.NewGuid(), App.Random.NextRandomUnsignedLong);
      session.User = user;
      Azos.Apps.ExecutionContext.__SetThreadLevelSessionContext(session);

      //4. - Invoke Fiber slice
      FiberStep? nextStep = null;
      try
      {
        Permission.AuthorizeAndGuardAction(App.SecurityManager, tFiber);
        nextStep = await fiber.ExecuteSliceAsync() //<===================== FIBER SLICE gets called
                              .ConfigureAwait(false);
      }
      catch(Exception fiberError)
      {
        memory.Crash(fiberError);
      }
      finally
      {
        //reset identity
        Azos.Apps.ExecutionContext.__SetThreadLevelSessionContext(null);
      }

      return (true, nextStep, fiber.State);
    }

    // Should this be moved into a policy in future
    private Credentials mapEntityCredentials(EntityId eid)
    {
      if (eid.Type == Constraints.SEC_CREDENTIALS_BASIC)
      {
        return IDPasswordCredentials.FromBasicAuth(eid.Address);
      }

      return new EntityUriCredentials(eid.Address);
    }
  }
}
