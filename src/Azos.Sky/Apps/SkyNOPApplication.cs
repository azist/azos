using System;

using Azos.Apps;
using Azos.Conf;
using Azos.Data.Access;

using Azos.Dynamic;
using Azos.Workers;

namespace Azos.Sky.Apps
{
  /// <summary>
  /// Represents an application that consists of pure-nop providers, consequently
  ///  this application does not log, does not store data and does not do anything else
  /// still satisfying its contract
  /// </summary>
  public class SkyNOPApplication : NOPApplication, ISkyApplication
  {
    private static SkyNOPApplication s_Instance = new SkyNOPApplication();

    protected SkyNOPApplication() : base() {}

    /// <summary>
    /// Returns a singlelton instance of the AgniNOPApplication
    /// </summary>
    public static new SkyNOPApplication Instance { get { return s_Instance; } }

    public string MetabaseApplicationName { get { return string.Empty; } }

    public IConfigSectionNode BootConfigRoot { get { return m_Configuration.Root; } }

    public bool ConfiguredFromLocalBootConfig { get { return false; } }

    public SystemApplicationType SystemApplicationType { get { return SystemApplicationType.Unspecified; } }

    public Locking.ILockManager LockManager { get { return Locking.NOPLockManager.Instance; } }

    public IGDIDProvider GDIDProvider { get { throw new NotSupportedException("NOPApp.GDIDProvider"); } }

    public IProcessManager ProcessManager { get { throw new NotSupportedException("NOPApp.ProcessManager"); } }

    public IHostManager DynamicHostManager { get { throw new NotSupportedException("NOPApp.HostManager"); } }
  }
}
