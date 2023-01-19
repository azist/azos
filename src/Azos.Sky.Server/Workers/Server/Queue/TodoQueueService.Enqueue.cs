/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using Azos.Log;

namespace Azos.Sky.Workers.Server.Queue { partial class TodoQueueService{



  private void enqueue(TodoQueue queue, TodoFrame[] todos)
  {
    var tx = m_QueueStore.BeginTransaction(queue);
    try
    {
      enqueueCore(queue, todos, tx);
      m_QueueStore.CommitTransaction(queue, tx);
    }
    catch(Exception error)
    {
      m_QueueStore.RollbackTransaction(queue, tx);

      var from = "enqueue('{0}')".Args(queue.Name);
      WriteLog(MessageType.CatastrophicError, from, error.ToMessageWithType(), error);
      if (InstrumentationEnabled)
      {
         m_stat_QueueOperationErrorCount.IncrementLong(ALL);
         m_stat_QueueOperationErrorCount.IncrementLong(from);
      }

      throw new WorkersException(ServerStringConsts.TODO_ENQUEUE_TX_BODY_ERROR.Args(queue.Name, error.ToMessageWithType()), error);
    }
  }

  private void enqueueCore(TodoQueue queue, TodoFrame[] todos, object tx)
  {
    foreach(var todo in todos)
    {
      if (!todo.Assigned)
        continue;

      //20171114 If the todo was already submitted, then do nothing
      var isdup = checkDups(queue, todo);
      if (isdup)
      {
        if (InstrumentationEnabled)
        {
           var from = "enqueue-dup('{0}')".Args(queue.Name);
           m_stat_TodoDuplicationCount.IncrementLong(ALL);
           m_stat_TodoDuplicationCount.IncrementLong(from);
        }
        continue;
      }


      if (todo.CorrelationKey==null)//regular todos just add
      {
        put(queue, todo, tx, true);
        continue;
      }

      //Correlated -------------------------------------------------------------
      var utcNow = App.TimeSource.UTCNow;//warning locking depends on this accurate date
      var utcCorrelateSD = lockCorrelatedEnqueue(todo.CorrelationKey, utcNow);
      try
      {
        var existing = m_QueueStore.FetchLatestCorrelated(queue, todo.CorrelationKey, utcCorrelateSD);
        if (!existing.Assigned) //no existing with same correlation, just add
        {
          put(queue, todo, tx, true);
          continue;
        }

        var todoExisting = existing.Materialize( App.ProcessManager.TodoTypeResolver ) as CorrelatedTodo;
        var todoAnother  = todo.Materialize( App.ProcessManager.TodoTypeResolver ) as CorrelatedTodo;
        if (todoExisting==null || todoAnother==null)//safeguard
        {
          put(queue, todo, tx, true);
          continue;
        }

        CorrelatedTodo.MergeResult mergeResult;
        try
        {
          mergeResult = todoExisting.Merge( this, utcNow, todoAnother );
        }
        catch(Exception error)
        {
          throw new WorkersException(ServerStringConsts.TODO_CORRELATED_MERGE_ERROR.Args(todoExisting, todoAnother, error.ToMessageWithType()), error);
        }

        if (InstrumentationEnabled)
        {
          m_stat_MergedTodoCount.IncrementLong(ALL);
          m_stat_MergedTodoCount.IncrementLong(queue.Name);
          m_stat_MergedTodoCount.IncrementLong(queue.Name+":"+mergeResult);
        }

        if (mergeResult == CorrelatedTodo.MergeResult.Merged)
        {
          update(queue, todoExisting, sysOnly: false, leak: true, transaction: tx);
          continue;
        } else if (mergeResult == CorrelatedTodo.MergeResult.IgnoreAnother)
        {
          continue;//do nothing, just drop another as-if never existed
        }

        //Else, merge == CorrelatedTodo.MergeResult.None
        put(queue, todo, tx, true);
      }
      finally
      {
        releaseCorrelatedEnqueue(todo.CorrelationKey);
      }
    }//foreach
  }


  private bool checkDups(TodoQueue queue, TodoFrame todo)
  {
    var mode = queue.DuplicationHandling;

    if (mode==TodoQueue.DuplicationHandlingMode.NotDetected) return false;//never a duplicate

    var result = checkDupsInMemory(todo);

    if (result || mode==TodoQueue.DuplicationHandlingMode.HostFastDetection) return result;

    result = checkDupsInQueueStore(queue, todo);

    if (result || mode==TodoQueue.DuplicationHandlingMode.HostAccurateDetection) return result;

    result = checkDupsInHostset(queue, todo);

    return result;
  }

  private bool checkDupsInMemory(TodoFrame todo)
  {
    return !m_Duplicates.Put(todo.ID);
  }

  private bool checkDupsInQueueStore(TodoQueue queue, TodoFrame todo)
  {
    return false; // TODO implement
  }

  private bool checkDupsInHostset(TodoQueue queue, TodoFrame todo)
  {
    return false; // TODO implement
  }


}}
