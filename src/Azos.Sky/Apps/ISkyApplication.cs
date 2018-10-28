/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Apps;
using Azos.Data.Access;

using Azos.Sky.Identification;

namespace Azos.Sky.Apps
{

  /// <summary>
  /// Denotes system application/process types that this app container has, i.e.:  HostGovernor, WebServer, etc.
  /// </summary>
  public enum SystemApplicationType
  {
    Unspecified = 0,
    HostGovernor,
    ZoneGovernor,
    WebServer,
    GDIDAuthority,
    ServiceHost,
    ProcessHost,
    SecurityAuthority,
    TestRig,
    Tool
  }


  /// <summary>
  /// Defines a contract for applications
  /// </summary>
  public interface ISkyApplication : IApplication
  {
    /// <summary>
    /// Returns the name that uniquely identifies this application in the metabase. Every process/executable must provide its unique application name in metabase
    /// </summary>
    string MetabaseApplicationName { get; }

    /// <summary>
    /// References application configuration root used to boot this application instance
    /// </summary>
    IConfigSectionNode BootConfigRoot { get; }

    /// <summary>
    /// Denotes system application/process type that this app container has, i.e.:  HostGovernor, WebServer, etc.
    /// </summary>
    SystemApplicationType SystemApplicationType { get; }

    /// <summary>
    /// References distributed lock manager
    /// </summary>
    Locking.ILockManager LockManager { get; }

    /// <summary>
    /// References distributed GDID provider
    /// </summary>
    IGDIDProvider GDIDProvider { get; }

    /// <summary>
    /// References distributed process manager
    /// </summary>
    Workers.IProcessManager ProcessManager { get; }

    /// <summary>
    /// References dynamic host manager
    /// </summary>
    Dynamic.IHostManager DynamicHostManager { get; }
  }
}
