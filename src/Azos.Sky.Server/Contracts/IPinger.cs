/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Glue;

namespace Azos.Sky.Contracts
{
  /// <summary>
  /// Used to see if the host responds at all
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface IPinger : ISkyService
  {
    void Ping();
  }


  /// <summary>
  /// Contract for client of IPinger svc
  /// </summary>
  public interface IPingerClient : ISkyServiceClient, IPinger
  {
    CallSlot Async_Ping();
  }
}