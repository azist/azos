/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Sky.Apps.HostGovernor
{
  /// <summary>
  /// Implements contracts trampoline that uses a singleton instance of HostGovernorService
  /// </summary>
  public class HostGovernorServer
    : Sky.Contracts.IHostGovernor,
      Sky.Contracts.IPinger
  {
    public Sky.Contracts.HostInfo GetHostInfo()
    {
      return HostGovernorService.Instance.GetHostInfo();
    }

    public void Ping()
    {
      HostGovernorService.Instance.Ping();
    }
  }
}
