using System.Threading.Tasks;

using Azos.Apps;

namespace Azos.Sky.Locking
{
  /// <summary>
  /// Facade for calling locking APIs from client code
  /// </summary>
  public sealed class LockManager : LockManagerBase
  {
    public LockManager(IApplication app) : base(app) { }
    public LockManager(IApplicationComponent director) : base(director) { }


    protected override LockTransactionResult DoExecuteLockTransaction(LockSession session, LockTransaction transaction)
    {
      return App.GetServiceClientHub().CallWithRetry<Contracts.ILockerClient, LockTransactionResult>
      (
        (locker) => locker.ExecuteLockTransaction(session.Data, transaction),
        session.ServerHosts
      );
    }

    protected override Task<LockTransactionResult> DoExecuteLockTransactionAsync(LockSession session, LockTransaction transaction)
    {
      return App.GetServiceClientHub().CallWithRetryAsync<Contracts.ILockerClient, LockTransactionResult>
      (
        (locker) => locker.Async_ExecuteLockTransaction(session.Data, transaction).AsTaskReturning<LockTransactionResult>(),
        session.ServerHosts
      );
    }

    protected override bool DoEndLockSession(LockSession session)
    {
      return App.GetServiceClientHub().CallWithRetry<Contracts.ILockerClient, bool>
      (
        (locker) => locker.EndLockSession(session.Data.ID),
        session.ServerHosts
      );
    }

    protected override Task<bool> DoEndLockSessionAsync(LockSession session)
    {
      return App.GetServiceClientHub().CallWithRetryAsync<Contracts.ILockerClient, bool>
      (
        (locker) => locker.Async_EndLockSession(session.Data.ID).AsTaskReturning<bool>(),
        session.ServerHosts
      );
    }
  }

}
