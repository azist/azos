using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Log;

namespace Azos.Sky.Workers.Server.Queue
{
  /// <summary>
  /// Represents a base for entities that store queue data
  /// </summary>
  public abstract class TodoQueueStore : ApplicationComponent
  {
    protected TodoQueueStore(TodoQueueService director, IConfigSectionNode node) : base(director)
    {
      ConfigAttribute.Apply(this, node);
    }

    public abstract IEnumerable<TodoFrame> Fetch(TodoQueue queue, DateTime utcNow);
    public abstract TodoFrame FetchLatestCorrelated(TodoQueue queue, string correlationKey, DateTime utcStartingFrom);


    /// <summary>
    /// References service that this store is under
    /// </summary>
    public TodoQueueService QueueService { get { return (TodoQueueService)ComponentDirector;} }


    public override string ComponentLogTopic => SysConsts.LOG_TOPIC_TODO;


    public abstract object BeginTransaction(TodoQueue queue);

    public abstract void CommitTransaction(TodoQueue queue, object transaction);

    public abstract void RollbackTransaction(TodoQueue queue, object transaction);

    public abstract void Put(TodoQueue queue, TodoFrame todo, object transaction);

    public abstract void Update(TodoQueue queue, Todo todo, bool sysOnly, object transaction);

    public abstract void Complete(TodoQueue queue, TodoFrame todo, Exception error = null, object transaction = null);

    public abstract int GetInboundCapacity(TodoQueue queue);
  }
}
