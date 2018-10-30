using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Glue;

namespace Azos.Sky.Contracts
{

    /// <summary>
    /// Used for various testing
    /// </summary>
    [Glued]
    [LifeCycle(ServerInstanceMode.Singleton)]
    public interface ITester : ISkyService
    {
       object TestEcho(object data);
    }


    /// <summary>
    /// Contract for client of ITester svc
    /// </summary>
    public interface ITesterClient : ISkyServiceClient, ITester
    {
       CallSlot Async_TestEcho(object data);
    }


}