/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;

using Azos.Glue;

using Azos.Sky.Workers;

namespace Azos.Sky.Contracts
{

  /// <summary>
  /// Controls the spawning and execution of processes.
  /// Dispatches signals
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface IProcessController : ISkyService
  {
    void Spawn(ProcessFrame frame);

    SignalFrame Dispatch(SignalFrame signal);

    ProcessFrame Get(PID pid);

    ProcessDescriptor GetDescriptor(PID pid);

    IEnumerable<ProcessDescriptor> List(int processorID);
  }


  /// <summary>
  /// Contract for client of IProcessController svc
  /// </summary>
  public interface IProcessControllerClient : ISkyServiceClient, IProcessController
  {
    CallSlot Async_Spawn(ProcessFrame frame);
    CallSlot Async_Dispatch(SignalFrame signal);

    CallSlot Async_Get(PID pid);
    CallSlot Async_GetDescriptor(PID pid);
    CallSlot Async_List(int processorID);
  }
}