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
  /// Glue trampoline for TodoQueueService
  /// </summary>
  public sealed class TodoQueueServer : ITodoQueue
  {
    [Inject] IApplication m_App;

    public int Enqueue(TodoFrame[] todos)
      => m_App.NonNull(nameof(m_App))
              .Singletons
              .Get<TodoQueueService>()
              .NonNull(nameof(TodoQueueService))
              .Enqueue(todos);
  }
}
