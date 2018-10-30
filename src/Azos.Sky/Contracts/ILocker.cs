using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Glue;
using Azos.Log;

using Azos.Sky.Locking;
using Azos.Sky.Locking.Server;

namespace Azos.Sky.Contracts
{

    /// <summary>
    /// Implemented by ZoneGovernors, provide distributed lock manager services
    /// </summary>
    [Glued]
    [LifeCycle(ServerInstanceMode.Singleton)]
    public interface ILocker : ISkyService
    {
       LockTransactionResult ExecuteLockTransaction(LockSessionData session, LockTransaction transaction);
       bool EndLockSession(LockSessionID sessionID);
    }


    /// <summary>
    /// Contract for client of ILocker svc
    /// </summary>
    public interface ILockerClient : ISkyServiceClient, ILocker
    {
       CallSlot Async_ExecuteLockTransaction(LockSessionData session, LockTransaction transaction);
       CallSlot Async_EndLockSession(LockSessionID sessionID);
    }


}