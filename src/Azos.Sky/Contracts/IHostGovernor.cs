/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Glue;

namespace Azos.Sky.Contracts
{
  /// <summary>
  /// Returns information about the host
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface IHostGovernor : ISkyService
  {
    HostInfo GetHostInfo();
  }


  /// <summary>
  /// Contract for client of IHostGovernor svc
  /// </summary>
  public interface IHostGovernorClient : ISkyServiceClient, IHostGovernor
  {
    CallSlot Async_GetHostInfo();
  }
}