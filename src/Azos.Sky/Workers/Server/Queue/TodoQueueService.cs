/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Collections;

using Azos.Sky.Contracts;

namespace Azos.Sky.Workers.Server.Queue
{
  /// <summary>
  /// Service that enqueues todos
  /// </summary>
  public sealed partial class TodoQueueService : AgentServiceBase, ITodoQueue, ITodoHost
  {
    #region CONSTS
    public const string CONFIG_QUEUE_SECTION = "queue";
    public const string CONFIG_QUEUE_STORE_SECTION = "queue-store";
    public const string CONFIG_TYPE_RESOLVER_SECTION = "type-resolver";

    public const int FULL_BATCH_SIZE = 1024;

    public const string ALL ="*";
    #endregion

    #region .ctor
    public TodoQueueService(IApplication app) : base(app) => ctor();
    public TodoQueueService(IApplicationComponent director) : base(director) => ctor();

    private void ctor()
    {
      m_Queues = new Registry<TodoQueue>();
      m_Duplicates = new CappedSet<Data.GDID>(this);

      if (!App.Singletons.GetOrCreate(() => this).created)
        throw new WorkersException("{0} is already allocated".Args(typeof(TodoQueueService).FullName));
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_QueueStore);
      App.Singletons.Remove<TodoQueueService>();
    }
    #endregion

    #region Fields
    private TodoQueueStore       m_QueueStore;
    private Registry<TodoQueue>  m_Queues;
    private CappedSet<Data.GDID> m_Duplicates;

    private int               m_stat_EnqueueCalls;
    private NamedInterlocked  m_stat_EnqueueTodoCount       = new NamedInterlocked();
    private int               m_stat_QueueThreadSpins;
    private NamedInterlocked  m_stat_ProcessOneQueueCount   = new NamedInterlocked();

    private NamedInterlocked  m_stat_MergedTodoCount        = new NamedInterlocked();
    private NamedInterlocked  m_stat_FetchedTodoCount       = new NamedInterlocked();
    private NamedInterlocked  m_stat_ProcessedTodoCount     = new NamedInterlocked();


    private NamedInterlocked  m_stat_PutTodoCount            = new NamedInterlocked();
    private NamedInterlocked  m_stat_UpdateTodoCount         = new NamedInterlocked();
    private NamedInterlocked  m_stat_CompletedTodoCount      = new NamedInterlocked();
    private NamedInterlocked  m_stat_CompletedOkTodoCount    = new NamedInterlocked();
    private NamedInterlocked  m_stat_CompletedErrorTodoCount = new NamedInterlocked();

    private NamedInterlocked  m_stat_QueueOperationErrorCount    = new NamedInterlocked();

    private NamedInterlocked  m_stat_TodoDuplicationCount  = new NamedInterlocked();
    #endregion

    #region Properties
    public override string ComponentLogTopic => SysConsts.LOG_TOPIC_TODO;
    public override int ThreadGranularityMs { get { return 25; } }
    #endregion

    #region Public

    void ITodoHost.LocalEnqueue(Todo todo)
    {
      this.Enqueue(new[] { todo });
    }

    void ITodoHost.LocalEnqueue(IEnumerable<Todo> todos)
    {
      this.Enqueue(todos);
    }

    Task ITodoHost.LocalEnqueueAsync(IEnumerable<Todo> todos)
    {
      CheckDaemonActive();
      return Task.Factory.StartNew(() => this.Enqueue(todos) );
    }

    /// <summary>
    /// Routes todos to appropriate queue and enqueues for processing
    /// </summary>
    public int Enqueue(IEnumerable<Todo> todos)
    {
      if (todos==null || !todos.Any()) return FULL_BATCH_SIZE;

      todos.ForEach( todo => check(todo));

      return this.Enqueue( todos.Select( t => new TodoFrame(t) ).ToArray() );
    }

    /// <summary>
    /// Routes todos to appropriate queue and enqueues for processing
    /// </summary>
    public int Enqueue(TodoFrame[] todos)
    {
      CheckDaemonActive();
      if (todos == null || todos.Length == 0)
        return FULL_BATCH_SIZE;

      TodoQueueAttribute attr = null;
      foreach (var todo in todos)
      {
        if (!todo.Assigned)
          continue;

        var todoAttr = GuidTypeAttribute.GetGuidTypeAttribute<Todo, TodoQueueAttribute>(todo.Type, App.AsSky().ProcessManager.TodoTypeResolver);

        if (attr==null) attr = todoAttr;

        if (attr.QueueName != todoAttr.QueueName)
          throw new WorkersException(StringConsts.TODO_QUEUE_ENQUEUE_DIFFERENT_ERROR);
      }

      var queue = m_Queues[attr.QueueName];
      if (queue == null)
        throw new WorkersException(StringConsts.TODO_QUEUE_NOT_FOUND_ERROR.Args(attr.QueueName));


      if (InstrumentationEnabled)
      {
        Interlocked.Increment(ref m_stat_EnqueueCalls);
        m_stat_EnqueueTodoCount.IncrementLong(ALL);
        m_stat_EnqueueTodoCount.IncrementLong(queue.Name);
      }

      enqueue(queue, todos);

      return m_QueueStore.GetInboundCapacity(queue);
    }

    #endregion

    #region .pvt
    private void check(Todo todo)
    {
      if (todo == null) throw new WorkersException(StringConsts.ARGUMENT_ERROR + "Todo.ValidateAndPrepareForEnqueue(todo==null)");

      todo.ValidateAndPrepareForEnqueue(null);
    }

    private class _lck{ public Thread TEnqueue; public DateTime Date; public int IsEnqueue; public int IsProc; }
    private Dictionary<string, _lck> m_CorrelationLocker = new Dictionary<string, _lck>(StringComparer.Ordinal);


/*
 * Locking works as follows:
 * ---------------------------
 * All locking is done per CorrelationKey (different CorrelationKey do not inter-lock at all)
 * Within the same key:
 *
 *   Any existing Enqueue lock blocks another Enqueue until existing is released
 *   Any existing Enqueue lock returns FALSE for another Process until all Enqueue released
 *
 *   Any existing Processing lock yields next date to Enqueue (+1 sec)
 *   Any Exisiting Processing lock shifts next Date if another Processing lock is further advanced in time
 *
 *
 *   Enqueue lock is reentrant for the same thread.
 *   Processing lock is reentrant regardless of thread ownership
 *
 */



      /// <summary>
      /// Returns the point in time AFTER which the enqueue operation may fetch correlated todos
      /// </summary>
      private DateTime lockCorrelatedEnqueue(string key, DateTime sd)
      {
        if (key==null) key = string.Empty;

        var ct = Thread.CurrentThread;

        uint spinCount = 0;
        while(true)
        {
          lock(m_CorrelationLocker)
          {
            _lck lck;
            if (!m_CorrelationLocker.TryGetValue(key, out lck))
            {
               lck = new _lck { TEnqueue = ct, Date = sd, IsEnqueue = 1, IsProc = 0 };
               m_CorrelationLocker.Add(key, lck);
               return sd;
            }

            if (lck.IsEnqueue==0)
            {
              lck.IsEnqueue = 1;
              lck.TEnqueue = ct;
              return lck.Date.AddSeconds(1);
            }

            if (lck.TEnqueue==ct)//if already acquired by this thread
            {
              lck.IsEnqueue++;
              if (sd<lck.Date) lck.Date = sd;
              return lck.Date;
            }
          }
          if (spinCount<100)
            Thread.SpinWait(500);
          else
            Thread.Yield();

          unchecked {  spinCount++; }
        }
      }

      /// <summary>
      /// Tries to lock the correlated todo instance with the specified scheduled date, true if locked, false, otherwise.
      /// Takes lock IF not enqueue, false otherwise
      /// </summary>
      private bool tryLockCorrelatedProcessing(string key, DateTime sd)
      {
        if (key==null) key = string.Empty;

        lock(m_CorrelationLocker)
        {
          _lck lck;
          if (!m_CorrelationLocker.TryGetValue(key, out lck))
          {
              lck = new _lck { Date = sd, IsEnqueue = 0, IsProc = 1 };
              m_CorrelationLocker.Add(key, lck);
              return true;
          }

          if (lck.IsEnqueue==0)
          {
            lck.IsProc++;
            if (sd>lck.Date) lck.Date = sd;
            return true;
          }

          return false;//lock is enqueue type
        }
      }

      private bool releaseCorrelatedEnqueue(string key)
      {
        if (key==null) key = string.Empty;

        lock(m_CorrelationLocker)
        {
          _lck lck;
          if (!m_CorrelationLocker.TryGetValue(key, out lck)) return false;

          if (lck.IsEnqueue==0) return false;

          if (Thread.CurrentThread!=lck.TEnqueue) return false;//not locked by this thread

          lck.IsEnqueue--;

          if (lck.IsEnqueue>0) return true;

          lck.TEnqueue = null;//release thread

          if (lck.IsProc==0)
            m_CorrelationLocker.Remove(key);

          return true;
        }
      }

      private bool releaseCorrelatedProcessing(string key)
      {
        if (key==null) key = string.Empty;

        lock(m_CorrelationLocker)
        {
          _lck lck;
          if (!m_CorrelationLocker.TryGetValue(key, out lck)) return false;

          if (lck.IsProc==0) return false;

          lck.IsProc--;

          if (lck.IsProc>0) return true;

          if (lck.IsEnqueue==0)
            m_CorrelationLocker.Remove(key);

          return true;
        }
      }

    #endregion

  }
}
