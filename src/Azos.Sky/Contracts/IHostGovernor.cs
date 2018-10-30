
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