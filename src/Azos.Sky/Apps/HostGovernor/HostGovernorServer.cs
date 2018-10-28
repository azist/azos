
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
