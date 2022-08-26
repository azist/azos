/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Log;

using System.Threading.Tasks;

namespace Azos.Sky.Workers.Server.Queue{ partial class TodoQueueService{

      private void processOneQueue(object queue, DateTime utcNow)
      {
        var q = (TodoQueue)queue;
        try
        {
          if (InstrumentationEnabled)
          {
            m_stat_ProcessOneQueueCount.IncrementLong(ALL);
            m_stat_ProcessOneQueueCount.IncrementLong(q.Name);
          }

          processOneQueueCore(q, utcNow);
        }
        catch (Exception error)
        {
          var from = "processOneQueueCore('{0}')".Args(q.Name);
          WriteLog(MessageType.CatastrophicError, from, error.ToMessageWithType(), error);
          if (InstrumentationEnabled)
          {
            m_stat_QueueOperationErrorCount.IncrementLong(ALL);
            m_stat_QueueOperationErrorCount.IncrementLong(from);
          }
        }
        finally
        {
          q.Release();
        }
      }

      private void processOneQueueCore(TodoQueue queue, DateTime utcNow)
      {
        var fetched = m_QueueStore.Fetch(queue, utcNow);
        try
        {
          var batch = fetched.Where( todo =>
          {
            if (todo.CorrelationKey==null) return true;
            return tryLockCorrelatedProcessing(todo.CorrelationKey, todo.StartDate);
          }).ToList();//must materialize not to double lock

          if (InstrumentationEnabled)
          {
            var fetchCount = fetched.Count();
            m_stat_FetchedTodoCount.AddLong(ALL, fetchCount);
            m_stat_FetchedTodoCount.AddLong(queue.Name, fetchCount);
          }

          processOneQueueBatch(queue, batch, utcNow);


          if (InstrumentationEnabled)
          {
            m_stat_ProcessedTodoCount.AddLong(ALL, batch.Count);
            m_stat_ProcessedTodoCount.AddLong(queue.Name, batch.Count);
          }
        }
        finally
        {
          fetched.ForEach( todo =>
          {
            if (todo.CorrelationKey==null) return;
            releaseCorrelatedProcessing(todo.CorrelationKey);
          });
        }
      }

      private void processOneQueueBatch(TodoQueue queue, IEnumerable<TodoFrame> batch, DateTime utcNow)
      {
        if (queue.Mode == TodoQueue.ExecuteMode.Sequential)
        {
          batch.OrderBy(t => t.StartDate).ForEach(todo => executeOne(queue, todo, utcNow));
        }
        else if (queue.Mode == TodoQueue.ExecuteMode.Parallel)
        {
          Parallel.ForEach(batch.OrderBy(t => t.StartDate), todo => executeOne(queue, todo, utcNow));
        }
        else//ParallelByKey
        {
          var tasks = new List<Task>();

          var parallelTodos = batch.Where(t => t.ParallelKey == null).ToArray();
          if (parallelTodos.Length > 0)
            tasks.Add(Task.Factory.StartNew(ts => Parallel.ForEach(((IEnumerable<TodoFrame>)ts).OrderBy(t => t.StartDate), todo => executeOne(queue, todo, utcNow)), parallelTodos));

          List<TodoFrame> todos = null;
          string parallelKey = null;
          foreach (var todo in batch.Where(t => t.ParallelKey != null).OrderBy(t => t.ParallelKey))
          {
            if (parallelKey != todo.ParallelKey)
            {
              if (todos != null)
                tasks.Add(Task.Factory.StartNew(ts => ((IEnumerable<TodoFrame>)ts).OrderBy(t => t.StartDate).ForEach(t => executeOne(queue, t, utcNow)), todos));
              todos = new List<TodoFrame>();
              parallelKey = todo.ParallelKey;
            }
            todos.Add(todo);
          }

          if (todos != null)
            tasks.Add(Task.Factory.StartNew(ts => ((IEnumerable<TodoFrame>)ts).OrderBy(t => t.StartDate).ForEach(t => executeOne(queue, t, utcNow)), todos));

          Task.WaitAll(tasks.ToArray());
        }
      }


      private void executeOne(TodoQueue queue, TodoFrame todoFrame, DateTime utcNow)//must not leak
      {
        try
        {
          if (!Running) return;

          Todo todo;
          try
          {
            todo = todoFrame.Materialize( App.ProcessManager.TodoTypeResolver );
            App.DependencyInjector.InjectInto( todo );
          }
          catch(Exception me)
          {
            var from = "executeOne('{0}').Materialize".Args(queue.Name);
            WriteLog(MessageType.Critical, from, "Frame materialization: "+me.ToMessageWithType(), me);
            if (InstrumentationEnabled)
            {
              m_stat_QueueOperationErrorCount.IncrementLong(ALL);
              m_stat_QueueOperationErrorCount.IncrementLong(from);
            }
            throw;
          }

          var wasState = todo.SysState;
          while (todo.SysState != Todo.ExecuteState.Complete)
          {
            var nextState = todo.Execute(this, utcNow);

            if (nextState == Todo.ExecuteState.ReexecuteUpdatedAfterError)
            {
              todo.SysTries++;
              var ms = todo.RetryAfterErrorInMs(utcNow);
              if (ms > 0) todo.SysStartDate = todo.SysStartDate.AddMilliseconds(ms);
              update(queue, todo, false);
              return;
            }

            if (nextState == Todo.ExecuteState.ReexecuteAfterError)
            {
              todo.SysTries++;
              var ms = todo.RetryAfterErrorInMs(utcNow);
              if (ms > 0) todo.SysStartDate = todo.SysStartDate.AddMilliseconds(ms);
              if (ms >= 0 || todo.SysState != wasState) update(queue, todo, true);
              return;
            }

            if (nextState == Todo.ExecuteState.ReexecuteUpdated)
            {
              update(queue, todo, false);
              return;
            }

            if (nextState == Todo.ExecuteState.ReexecuteSysUpdated)
            {
              update(queue, todo, true);
              return;
            }

            if (nextState == Todo.ExecuteState.Reexecute)
            {
              if (todo.SysState != wasState) update(queue, todo, true);
              return;
            }

            todo.SysTries = 0;
            todo.SysState = nextState;
          }

          complete(queue, todoFrame, null);
        }
        catch (Exception error)
        {
          complete(queue, todoFrame, error);
        }
      }

      private void complete(TodoQueue queue, TodoFrame todo, Exception error, object transaction = null)
      {
        try
        {
          m_QueueStore.Complete(queue, todo, error, transaction);

          if (error!=null)
          {
            WriteLog(MessageType.Error, "complete('{0}')".Args(queue.Name), "Completed with error: " + error.ToMessageWithType(), error);
          }

          if (InstrumentationEnabled)
          {
            m_stat_CompletedTodoCount.IncrementLong(ALL);
            m_stat_CompletedTodoCount.IncrementLong(queue.Name);

            if (error!=null)
            {
              m_stat_CompletedErrorTodoCount.IncrementLong(ALL);
              m_stat_CompletedErrorTodoCount.IncrementLong(queue.Name);
            }
            else
            {
              m_stat_CompletedOkTodoCount.IncrementLong(ALL);
              m_stat_CompletedOkTodoCount.IncrementLong(queue.Name);
            }
          }
        }
        catch(Exception e)
        {
          var from = "complete('{0}')".Args(queue.Name);
          WriteLog(MessageType.Critical, from, "{0} Leaked: {1}".Args(todo, e.ToMessageWithType()), e);
          if (InstrumentationEnabled)
          {
            m_stat_QueueOperationErrorCount.IncrementLong(ALL);
            m_stat_QueueOperationErrorCount.IncrementLong(from);
          }
        }
      }

      private void update(TodoQueue queue, Todo todo, bool sysOnly, object transaction = null, bool leak = false)
      {
        try
        {
          m_QueueStore.Update(queue, todo, sysOnly, transaction);
          if (InstrumentationEnabled)
          {
            m_stat_UpdateTodoCount.IncrementLong(ALL);
            m_stat_UpdateTodoCount.IncrementLong(queue.Name);
            if (sysOnly)
              m_stat_UpdateTodoCount.IncrementLong(queue.Name+"-sys");
          }
        }
        catch(Exception e)
        {
          var from = "update('{0}')".Args(queue.Name);
          WriteLog(MessageType.Critical, from, "{0} Leaked: {1}".Args(todo, e.ToMessageWithType()), e);
          if (InstrumentationEnabled)
          {
            m_stat_QueueOperationErrorCount.IncrementLong(ALL);
            m_stat_QueueOperationErrorCount.IncrementLong(from);
          }
          if (leak) throw;
        }
      }

      private void put(TodoQueue queue, TodoFrame todo, object transaction = null, bool leak = false)
      {
        try
        {
          m_QueueStore.Put(queue, todo, transaction);
          if (InstrumentationEnabled)
          {
            m_stat_PutTodoCount.IncrementLong(ALL);
            m_stat_PutTodoCount.IncrementLong(queue.Name);
          }
        }
        catch(Exception e)
        {
          var from = "put('{0}')".Args(queue.Name);
          WriteLog(MessageType.Critical, from, "{0} Leaked: {1}".Args(todo, e.ToMessageWithType()), e);
          if (InstrumentationEnabled)
          {
            m_stat_QueueOperationErrorCount.IncrementLong(ALL);
            m_stat_QueueOperationErrorCount.IncrementLong(from);
          }
          if (leak) throw;
        }
      }


}}
