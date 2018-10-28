/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Data.Access;

using Azos.Sky.Apps;
using Azos.Sky.Identification;
using Azos.Sky.Metabase;

namespace Azos.Sky
{
  /// <summary>
  /// Provides a shortcut access to app-global Sky context
  /// </summary>
  public static class SkySystem
  {
    private static BuildInformation s_CoreBuildInfo;

    /// <summary>
    /// Returns BuildInformation object for the core Sky assembly
    /// </summary>
    public static BuildInformation CoreBuildInfo
    {
      get
      {
        //multithreading: 2nd copy is ok
        if (s_CoreBuildInfo == null)
          s_CoreBuildInfo = new BuildInformation(typeof(SkySystem).Assembly);

        return s_CoreBuildInfo;
      }
    }

    private static string s_MetabaseApplicationName;

    /// <summary>
    /// Every Sky application MUST ASSIGN THIS property at its entry point ONCE. Example: void Main(string[]args){ SkySystem.MetabaseApplicationName = "MyApp1";...
    /// </summary>
    public static string MetabaseApplicationName
    {
      get { return s_MetabaseApplicationName; }
      set
      {
        if (s_MetabaseApplicationName != null || value.IsNullOrWhiteSpace())
          throw new SkyException(StringConsts.METABASE_APP_NAME_ASSIGNMENT_ERROR);
        s_MetabaseApplicationName = value;
      }
    }


    /// <summary>
    /// Returns instance of Sky application container that this SkySystem services
    /// </summary>
    public static ISkyApplication Application
    {
      get { return (App.Instance as ISkyApplication) ?? (ISkyApplication)SkyNOPApplication.Instance; }
    }

    /// <summary>
    /// Denotes system application/process type that this app container has, i.e.:  HostGovernor, WebServer, etc.
    /// </summary>
    public static SystemApplicationType SystemApplicationType { get { return Application.SystemApplicationType; } }

    /// <summary>
    /// References application configuration root used to boot this application instance
    /// </summary>
    public static IConfigSectionNode BootConfigRoot { get { return Application.BootConfigRoot; } }

    /// <summary>
    /// Host name of this machine as determined at boot. This is a shortcut to Sky.AppModel.BootConfLoader.HostName
    /// </summary>
    public static string HostName { get { return BootConfLoader.HostName; } }


    /// <summary>
    /// True if this host is dynamic
    /// </summary>
    public static bool DynamicHost { get { return BootConfLoader.DynamicHost; } }


    /// <summary>
    /// Returns parent zone governor host name or null if this is the top-level host in Sky.
    /// This is a shortcut to Sky.Apps.BootConfLoader.ParentZoneGovernorPrimaryHostName
    /// </summary>
    public static string ParentZoneGovernorPrimaryHostName { get { return BootConfLoader.ParentZoneGovernorPrimaryHostName; } }


    /// <summary>
    /// NOC name for this host as determined at boot
    /// </summary>
    public static string NOCName { get { return NOCMetabaseSection.Name; } }


    /// <summary>
    /// True when metabase is mounted!=null
    /// </summary>
    public static bool IsMetabase { get { return BootConfLoader.Metabase != null; } }


    /// <summary>
    /// Returns metabank instance that interfaces the metabase as determined at application boot.
    /// If metabase is null then exception is thrown. Use IsMetabase to test for null instead
    /// </summary>
    public static Metabank Metabase
    {
      get
      {
        var result = BootConfLoader.Metabase;

        if (result == null)
        {
          var trace = new System.Diagnostics.StackTrace(false);
          throw new SkyException(StringConsts.METABASE_NOT_AVAILABLE_ERROR.Args(trace.ToString()));
        }

        return result;
      }
    }

    /// <summary>
    /// Returns Metabank.SectionHost (metabase's information about this host)
    /// </summary>
    public static Metabank.SectionHost HostMetabaseSection { get { return Metabase.CatalogReg.NavigateHost(HostName); } }

    /// <summary>
    /// Returns Metabank.SectionNOC (metabase's information about the NOC this host is in)
    /// </summary>
    public static Metabank.SectionNOC NOCMetabaseSection { get { return HostMetabaseSection.NOC; } }


    /// <summary>
    /// Returns Sky distributed lock manager
    /// </summary>
    public static Locking.ILockManager LockManager { get { return Application.LockManager; } }

    /// <summary>
    /// References distributed GDID provider
    /// </summary>
    public static IGDIDProvider GDIDProvider { get { return Application.GDIDProvider; } }

    /// <summary>
    /// Returns Sky distributed process manager
    /// </summary>
    public static Workers.IProcessManager ProcessManager { get { return Application.ProcessManager; } }

    /// <summary>
    /// Returns Sky distributed dynamic host manager
    /// </summary>
    public static Dynamic.IHostManager DynamicHostManager { get { return Application.DynamicHostManager; } }
  }
}
