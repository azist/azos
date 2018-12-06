using System.Threading.Tasks;

using Azos.Apps;

namespace Azos.Sky.Locking
{
  /// <summary>
  /// Used for testing, facade for calling locking APIs from client code into server that is hosted in the same process
  /// </summary>
  public sealed class LocalTestingLockManager : LockManagerBase
  {
    public LocalTestingLockManager(IApplication app) : base(app) { }
    public LocalTestingLockManager(IApplicationComponent director) : base(director) { }

    private Server.LockServerService m_Server;


    protected override void DoStart()
    {
      m_Server = new Server.LockServerService(this);
      m_Server.Start();
    }

    protected override void DoWaitForCompleteStop()
    {
      DisposableObject.DisposeAndNull(ref m_Server);
    }

    protected override LockTransactionResult DoExecuteLockTransaction(LockSession session, LockTransaction transaction)
    {
      return m_Server.ExecuteLockTransaction(session.Data, transaction);
    }

    protected override Task<LockTransactionResult> DoExecuteLockTransactionAsync(LockSession session, LockTransaction transaction)
    {
      return TaskUtils.AsCompletedTask( () => m_Server.ExecuteLockTransaction(session.Data, transaction) );
    }

    protected override bool DoEndLockSession(LockSession session)
    {
      return m_Server.EndLockSession(session.ID);
    }

    protected override Task<bool> DoEndLockSessionAsync(LockSession session)
    {
      return TaskUtils.AsCompletedTask( () => m_Server.EndLockSession(session.ID ) );
    }
  }

}
