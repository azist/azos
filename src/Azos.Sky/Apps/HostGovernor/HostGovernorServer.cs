/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps.Injection;

namespace Azos.Sky.Apps.HostGovernor
{
  /// <summary>
  /// Implements contracts trampoline that uses a singleton instance of HostGovernorService
  /// </summary>
  public class HostGovernorServer
    : Sky.Contracts.IHostGovernor,
      Sky.Contracts.IPinger
  {

    [Inject] IApplication m_App;

    public HostGovernorService Service => m_App.NonNull(nameof(m_App))
                                               .Singletons
                                               .Get<HostGovernorService>() ?? throw new AHGOVException(StringConsts.AHGOV_INSTANCE_NOT_ALLOCATED_ERROR);

    public Sky.Contracts.HostInfo GetHostInfo() => Service.GetHostInfo();
    public void Ping() => Service.Ping();
  }
}
