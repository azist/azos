/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;


namespace Azos.Sky.Locking
{
  /// <summary>
  /// Defines operations of distributed lock manager
  /// </summary>
  public interface ILockManager
  {

    /// <summary>
    /// Creates a session of work with lock server at the specified path level and sharding id
    /// </summary>
    /// <param name="path">The level of the that has to be covered by the lock, the zone governors are looked up at that or higher level</param>
    /// <param name="shardingID">The object used for work partitioning. Keep in mind that ALL logically-connected lock entities must use the same shardingID</param>
    /// <param name="description">Logical description of the session</param>
    /// <param name="maxAgeSec">The maximum session age in seconds, or null for default</param>
    /// <returns>New lock session registered with the manager. The session instance may be later looked-up by LockSessionID</returns>
    LockSession MakeSession(string path, object shardingID, string description = null, int? maxAgeSec = null);

    /// <summary>
    /// Executes a lock transaction in the secified session returning transaction result object even if lock could not be set.
    /// The exception would indicate inability to deliver the transaction request to the server or other system problem
    /// </summary>
    LockTransactionResult ExecuteLockTransaction(LockSession session, LockTransaction transaction);

    /// <summary>
    /// Executes a lock transaction in the secified session returning transaction result object even if lock could not be set.
    /// The exception would indicate inability to deliver the transaction request to the server or other system problem
    /// </summary>
    Task<LockTransactionResult> ExecuteLockTransactionAsync(LockSession session, LockTransaction transaction);

    /// <summary>
    /// Ends the remote lock session returning true if it was found remotely and destroyed
    /// </summary>
    bool EndLockSession(LockSession session);

    /// <summary>
    /// Ends the remote lock session returning true if it was found remotely and destroyed
    /// </summary>
    Task<bool> EndLockSessionAsync(LockSession session);

    /// <summary>
    /// Returns the session by id or null
    /// </summary>
    LockSession this[LockSessionID sid] { get; }
  }

  /// <summary>
  /// Denotes implementations of ILockManager
  /// </summary>
  public interface ILockManagerImplementation : ILockManager, IApplicationComponent, IDisposable, IConfigurable, IInstrumentable
  {

  }

}
