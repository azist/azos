/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using Azos.Glue;
using Azos.Sky.Workers;

namespace Azos.Sky.Contracts
{

  /// <summary>
  /// Sends todos to the queue on the remote host
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface ITodoQueue : ISkyService
  {
    /// <summary>
    /// Enqueues todos in the order. Returns the maximum size of the next call to Enqueue()
    /// which reflects the ability of the remote queue to enqueue more work.
    /// This may be used for dynamic flow control.
    /// Calling this method with empty or null array just returns the status
    /// </summary>
    int Enqueue(TodoFrame[] todos);
  }


  /// <summary>
  /// Contract for client of ITodoQueue svc
  /// </summary>
  public interface ITodoQueueClient : ISkyServiceClient, ITodoQueue
  {
    CallSlot Async_Enqueue(TodoFrame[] todos);
  }
}