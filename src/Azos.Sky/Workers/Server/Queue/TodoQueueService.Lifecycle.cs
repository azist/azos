using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Log;


namespace Azos.Sky.Workers.Server.Queue { public sealed partial class TodoQueueService{

      /// <summary>
      /// Writes to log on behalf of worker service
      /// </summary>
      public Guid Log(MessageType type,
                      Todo todo,
                      string from,
                      string message,
                      Exception error = null,
                      Guid? relatedMessageID = null,
                      string parameters = null)
      {
        if (type < LogLevel) return Guid.Empty;

        return Log(type, "{0}.{1}".Args(todo.GetType().FullName, from), message, error, relatedMessageID, parameters);
      }

      protected override void DoConfigure(IConfigSectionNode node)
      {
        base.DoConfigure(node);

        if (node == null) return;

        DisposeAndNull(ref m_QueueStore);
        var queueStoreNode = node[CONFIG_QUEUE_STORE_SECTION];
        if (queueStoreNode.Exists)
          m_QueueStore = FactoryUtils.Make<TodoQueueStore>(queueStoreNode, args: new object[] { this, queueStoreNode });

        m_Queues.Clear();
        foreach (var queueNode in node.Children.Where(n => n.IsSameName(CONFIG_QUEUE_SECTION)))
          m_Queues.Register(new TodoQueue(this, queueNode));
      }

      protected override void DoStart()
      {
        m_Skip = 0;
        m_CorrelationLocker.Clear();

        if (m_QueueStore == null)
          throw new WorkersException("{0} does not have queue store injected".Args(GetType().Name));

        if (m_Queues.Count == 0)
          throw new WorkersException("{0} does not have any queues injected".Args(GetType().Name));

        if (m_QueueStore is IService) ((IService)m_QueueStore).Start();
        base.DoStart();
      }

      protected override void DoSignalStop()
      {
        if (m_QueueStore is IService) ((IService)m_QueueStore).SignalStop();
        base.DoSignalStop();
      }

      protected override void DoWaitForCompleteStop()
      {
        if (m_QueueStore is IService) ((IService)m_QueueStore).WaitForCompleteStop();
        base.DoWaitForCompleteStop();
        m_CorrelationLocker.Clear();
        m_Duplicates.Clear();
      }

      private int m_Skip;
      protected override void DoThreadSpin(DateTime utcNow)
      {
        try
        {
          if (InstrumentationEnabled) Interlocked.Increment(ref m_stat_QueueThreadSpins);

          var cpuCount = Environment.ProcessorCount;
          var maxWorkers = cpuCount / 2;
          if (maxWorkers < 2) maxWorkers = 2;

          if (m_Skip >= m_Queues.Count) m_Skip = 0;
          var working = m_Queues.Count( q => !q.CanBeAcquired(utcNow));
          foreach(var queue in m_Queues.Skip(m_Skip))
          {
            if (working>=maxWorkers) break;

            m_Skip++;
            if (!queue.TryAcquire(utcNow)) continue;

            working++;
            try
            {
              Task.Factory.StartNew( q => processOneQueue(q, utcNow), queue);
            }
            catch
            {
              queue.Release();
              throw;
            }
          }//foreach
        }
        catch (Exception error)
        {
          Log(MessageType.CatastrophicError, "DoThreadSpin()", error.ToMessageWithType(), error);
        }
      }


    protected override void DoDumpStats(IInstrumentation instr, DateTime utcNow)
    {
      instr.Record( new Instrumentation.EnqueueCalls( Interlocked.Exchange(ref m_stat_EnqueueCalls, 0) ) );
      m_stat_EnqueueTodoCount.SnapshotAllLongsInto<Instrumentation.EnqueueTodoCount>(0);
      instr.Record( new Instrumentation.QueueThreadSpins( Interlocked.Exchange(ref m_stat_QueueThreadSpins, 0) ) );
      m_stat_ProcessOneQueueCount.SnapshotAllLongsInto<Instrumentation.ProcessOneQueueCount>(0, instr);
      m_stat_MergedTodoCount.SnapshotAllLongsInto<Instrumentation.MergedTodoCount>(0, instr);
      m_stat_FetchedTodoCount.SnapshotAllLongsInto<Instrumentation.FetchedTodoCount>(0, instr);
      m_stat_ProcessedTodoCount.SnapshotAllLongsInto<Instrumentation.ProcessedTodoCount>(0, instr);
      m_stat_PutTodoCount.SnapshotAllLongsInto<Instrumentation.PutTodoCount>(0, instr);
      m_stat_UpdateTodoCount.SnapshotAllLongsInto<Instrumentation.UpdateTodoCount>(0, instr);
      m_stat_CompletedTodoCount.SnapshotAllLongsInto<Instrumentation.CompletedTodoCount>(0, instr);
      m_stat_CompletedOkTodoCount.SnapshotAllLongsInto<Instrumentation.CompletedOkTodoCount>(0, instr);
      m_stat_CompletedErrorTodoCount.SnapshotAllLongsInto<Instrumentation.CompletedErrorTodoCount>(0, instr);
      m_stat_QueueOperationErrorCount.SnapshotAllLongsInto<Instrumentation.QueueOperationErrorCount>(0, instr);
      m_stat_TodoDuplicationCount.SnapshotAllLongsInto<Instrumentation.TodoDuplicationCount>(0, instr);
    }

    protected override void DoResetStats(DateTime utcNow)
    {
      Interlocked.Exchange(ref m_stat_EnqueueCalls, 0);
      m_stat_EnqueueTodoCount.Clear();
      Interlocked.Exchange(ref m_stat_QueueThreadSpins, 0);
      m_stat_ProcessOneQueueCount     .Clear();
      m_stat_MergedTodoCount          .Clear();
      m_stat_FetchedTodoCount         .Clear();
      m_stat_ProcessedTodoCount       .Clear();
      m_stat_PutTodoCount             .Clear();
      m_stat_UpdateTodoCount          .Clear();
      m_stat_CompletedTodoCount       .Clear();
      m_stat_CompletedOkTodoCount     .Clear();
      m_stat_CompletedErrorTodoCount  .Clear();
      m_stat_QueueOperationErrorCount .Clear();
      m_stat_TodoDuplicationCount     .Clear();
    }


  }
}
