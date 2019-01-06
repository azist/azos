/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
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