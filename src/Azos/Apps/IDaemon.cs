/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;

namespace Azos.Apps
{
  /// <summary>
  /// Stipulates service control phase statuses like: Active/Inactive etc...
  /// </summary>
  public enum DaemonStatus { Inactive = 0, Starting, AbortingStart, Active, Stopping }

  /// <summary>
  /// Defines abstraction for an entity that is controlled by Start/Stop commands and has a status
  /// </summary>
  public interface IDaemonView : IApplicationComponent, Collections.INamed, IComponentDescription
  {
    /// <summary>
    /// Current service status
    /// </summary>
    DaemonStatus Status { get; }

    /// <summary>
    /// Returns true when service is active or about to become active.
    /// Check in service implementation loops/threads/tasks
    /// </summary>
    bool Running { get; }
  }

  /// <summary>
  /// Defines abstraction for an entity that is controlled by Start/Stop commands and has a status
  /// </summary>
  public interface IDaemon : IDaemonView, IConfigurable, IDisposable
  {
    /// <summary>
    /// Blocking call that starts the service instance
    /// </summary>
    void Start();

    /// <summary>
    /// Non-blocking call that initiates the stopping of the service
    /// </summary>
    void SignalStop();

    /// <summary>
    /// Non-blocking call that returns true when the service instance has completely stopped after SignalStop()
    /// </summary>
    bool CheckForCompleteStop();

    /// <summary>
    /// Blocks execution of current thread until this service has completely stopped
    /// </summary>
    void WaitForCompleteStop();
  }
}
