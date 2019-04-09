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
  /// Glue trampoline for TodoQueueService
  /// </summary>
  public sealed class TodoQueueServer : ITodoQueue
  {
#pragma warning disable 649
    [Inject] IApplication m_App;
#pragma warning restore 649

    public int Enqueue(TodoFrame[] todos)
      => m_App.NonNull(nameof(m_App))
              .Singletons
              .Get<TodoQueueService>()
              .NonNull(nameof(TodoQueueService))
              .Enqueue(todos);
  }
}
