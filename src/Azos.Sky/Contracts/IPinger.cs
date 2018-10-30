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