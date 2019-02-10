/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Conf;
using Azos.Security;
using Azos.IO.FileSystem;
using Azos.Log;

using Azos.Sky;
using Azos.Sky.Metabase;

namespace Azos.Apps
{
  /// <summary>
  /// Gets the boot configuration for app chassis.
  /// Reads the local process config to determine where the metabase is and what file system to use to connect to it.
  /// Once metabase connection is established get all information from there identifying the host by sky/Host/$name.
  /// If the name of the host is not set in config, then take it from SKY_HOST_NAME environment var. If that name is blank then
  ///  take host name from: DEFAULT_WORLD_GLOBAL_ZONE_PATH+LOCAL_COMPUTER_NAME (NetBIOSName)
  /// </summary>
  public sealed class BootConfLoader : DisposableObject
  {
    #region CONSTS
    public const string ENV_VAR_METABASE_FS_ROOT     = "SKY_METABASE_FS_ROOT";
    public const string ENV_VAR_METABASE_FS_TYPE     = "SKY_METABASE_FS_TYPE";
    public const string ENV_VAR_METABASE_FS_CSTRING  = "SKY_METABASE_FS_CSTRING";

    public const string ENV_VAR_HOST_NAME  = "SKY_HOST_NAME";
    public const string ENV_VAR_SKY_BOOT_CONF_FILE = "SKY_BOOT_CONF_FILE";

    /// <summary>
    /// Used to append to local machine name if ENV_VAR_HOST_NAME is not set, then this value is concatenated with local machine name
    /// </summary>
    public const string DEFAULT_HOST_ZONE_PATH  = SysConsts.DEFAULT_WORLD_GLOBAL_ZONE_PATH;

    public const string CONFIG_SKY_SECTION      = "sky";
    public const string CONFIG_HOST_SECTION     = "host";
    public const string CONFIG_METABASE_SECTION = "metabase";
    public const string CONFIG_FS_SECTION       = "file-system";
    public const string CONFIG_SESSION_CONNECT_PARAMS_SECTION = "session-connect-params";
    public const string CONFIG_APPLICATION_NAME_ATTR   = "app-name";


    /// <summary>
    /// Switch for Sky options
    /// </summary>
    public const string CMD_ARG_SKY_SWITCH = "sky";

    /// <summary>
    /// Used to specify application name from command line
    /// </summary>
    public const string CMD_ARG_APP_NAME = "app-name";

    /// <summary>
    /// Specifies host name in metabase, i.e. "/USA/East/Cle/A/I/wmed0001"
    /// </summary>
    public const string CONFIG_HOST_NAME_ATTR   = "name";

    /// <summary>
    /// Root of file system
    /// </summary>
    public const string CONFIG_ROOT_ATTR       = "root";


    public const string LOG_FROM_BOOTLOADER = "AppBootLoader";


   #endregion

    internal BootConfLoader(ISkyApplication application,
                            IApplication bootApplication,
                            SystemApplicationType appType,
                            string [] cmdArgs,
                            ConfigSectionNode rootConfig)
    {
      Platform.Abstraction.PlatformAbstractionLayer.SetProcessInvariantCulture();
      SystemVarResolver.Bind();//todo: Refactor to use ~App. app var resolver instead
      Configuration.ProcesswideConfigNodeProviderType = typeof(MetabankFileConfigNodeProvider);

      m_SystemApplicationType = appType;

      m_Application = application.NonNull(nameof(application));



      //1. Init boot app container
      m_BootApplication = bootApplication;

      if (m_BootApplication==null)
      {
        m_BootApplication = new AzosApplication(allowNesting: true, cmdArgs: cmdArgs, rootConfig: rootConfig);
        m_IsBootApplicationOwner = true;
      }

      writeLog(MessageType.Trace, "Entering Sky app bootloader...");

      //2. what host is this?
      m_HostName = determineHostName(null);

      //Sets cluster host name in all log messages
      if (m_HostName.IsNotNullOrWhiteSpace())
        Message.DefaultHostName = m_HostName;

      //3. Mount Metabank
      m_Metabase = mountMetabank();

      writeLog(MessageType.Trace, "...exiting Sky app bootloader");
    }

    internal BootConfLoader(ISkyApplication application,
                            IApplication bootApplication,
                            SystemApplicationType appType,
                            IFileSystem metabaseFileSystem,
                            FileSystemSessionConnectParams metabaseFileSystemSessionParams,
                            string metabaseFileSystemRootPath,
                            string thisHostName,
                            string[] cmdArgs,
                            ConfigSectionNode rootConfig,
                            string dynamicHostNameSuffix = null)
    {
      Platform.Abstraction.PlatformAbstractionLayer.SetProcessInvariantCulture();
      SystemVarResolver.Bind();//todo: Refactor to use ~App. app var resolver instead
      Configuration.ProcesswideConfigNodeProviderType = typeof(MetabankFileConfigNodeProvider);

      m_SystemApplicationType = appType;

      m_Application = application.NonNull(nameof(application));

      //1. Init boot app container
      m_BootApplication = bootApplication;

      if (m_BootApplication == null)
      {
        m_BootApplication = new AzosApplication(allowNesting: true, cmdArgs: cmdArgs, rootConfig: rootConfig);
        m_IsBootApplicationOwner = true;
      }

      writeLog(MessageType.Trace, "Entering Sky app bootloader...");

      //2. what host is this?
      m_HostName = determineHostName(thisHostName);

      //Sets cluster host name in all log messages
      if (m_HostName.IsNotNullOrWhiteSpace())
        Message.DefaultHostName = m_HostName;

      //3. Mount metabank
      var mNode = m_BootApplication.ConfigRoot[CONFIG_SKY_SECTION][CONFIG_METABASE_SECTION];
      ensureMetabaseAppName(mNode);

      m_Metabase = new Metabank(m_BootApplication, m_Application, metabaseFileSystem, metabaseFileSystemSessionParams, metabaseFileSystemRootPath);
      m_IsMetabaseFSOwner = false;//externally supplied

      writeLog(MessageType.Trace, "...exiting Sky app bootloader");
    }

    protected override void Destructor()
    {
      base.Destructor();

      if (m_IsBootApplicationOwner && m_BootApplication is IDisposable da) da.Dispose();
      m_BootApplication = null;

      m_SystemApplicationType = SystemApplicationType.Unspecified;
      m_HostName = null;
      m_DynamicHostNameSuffix = null;
      m_ParentZoneGovernorPrimaryHostName = null;
      try
      {
        if (m_Metabase != null)
        {
          var mb = m_Metabase;
          var fs = mb.FileSystem;
          m_Metabase = null;

          try
          {
            mb.Dispose();
          }
          finally
          {
            if (m_IsMetabaseFSOwner && fs != null)
            {
              fs.Dispose();
            }
          }//finally
        }
      }
      catch
      {
        //nowhere to log anymore as all loggers have stopped
#warning  COnsider OS event log
      }
    }

    private ISkyApplication m_Application;
    private IApplication m_BootApplication;
    private bool m_IsBootApplicationOwner;
    private SystemApplicationType m_SystemApplicationType;
    private Exception m_LoadException;
    private string m_HostName;
    private string m_DynamicHostNameSuffix;
    private Metabank m_Metabase;
    private bool m_IsMetabaseFSOwner;
    private string m_ParentZoneGovernorPrimaryHostName;
    private Configuration m_ApplicationConfiguration;


    /// <summary>
    /// References Sky application which booted using this instance
    /// </summary>
    public ISkyApplication Application => m_Application;

    /// <summary>
    /// References app chassis under which this one booted
    /// </summary>
    public IApplication BootApplication => m_BootApplication;

    /// <summary>
    /// Application container system type
    /// </summary>
    public SystemApplicationType SystemApplicationType => m_SystemApplicationType;

    /// <summary>
    /// Returns effective sky application configuration computed from the metabase
    /// </summary>
    public Configuration ApplicationConfiguration
    {
      get
      {
        if (m_ApplicationConfiguration!=null) return m_ApplicationConfiguration;

        writeLog(MessageType.Trace, "obtaining app config...");

        Metabank.SectionHost zoneGov;
        bool isDynamicHost;
        m_ApplicationConfiguration = getEffectiveAppConfigAndZoneGovernor(out zoneGov, out isDynamicHost);

        if (zoneGov != null) m_ParentZoneGovernorPrimaryHostName = zoneGov.RegionPath;

        if (isDynamicHost)
        {
          writeLog(MessageType.Trace, "The metabase host '{0}' is dynamic".Args(m_HostName));
          m_DynamicHostNameSuffix = thisMachineDynamicNameSuffix();
          writeLog(MessageType.Trace, "Obtained actual host dynamic suffix: '{0}' ".Args(m_DynamicHostNameSuffix));
          m_HostName = m_HostName + m_DynamicHostNameSuffix;//no spaces between
          writeLog(MessageType.Trace, "The actual dynamic instance host name is: '{0}'".Args(m_HostName));

          Message.DefaultHostName = m_HostName;
        }

        writeLog(MessageType.Trace, "...app config obtained");

        return m_ApplicationConfiguration;
      }
    }

    /// <summary>
    /// Host name as determined at boot
    /// </summary>
    public string HostName => m_HostName ?? string.Empty;


    /// <summary>
    /// For dynamic hosts, host name suffix as determined at boot. It is the last part of HostName for dynamic hosts
    /// including the separation character
    /// </summary>
    public string DynamicHostNameSuffix => m_DynamicHostNameSuffix ?? string.Empty;


    /// <summary>
    /// True when metabase section host declares this host as dynamic and HostName ends with DynamicHostNameSuffix
    /// </summary>
    public bool IsDynamicHost => m_DynamicHostNameSuffix.IsNotNullOrWhiteSpace();


    /// <summary>
    /// Returns primary zone governor parent host as determined at boot or null if this is the top-level host
    /// </summary>
    public string ParentZoneGovernorPrimaryHostName => m_ParentZoneGovernorPrimaryHostName;

    /// <summary>
    /// Metabase as determined at boot or null in case of failure
    /// </summary>
    public Metabank Metabase => m_Metabase;




    private string thisMachineDynamicNameSuffix()
    {
      //no spaces between
      return "{0}{1}".Args(Metabank.HOST_DYNAMIC_SUFFIX_SEPARATOR, Platform.Computer.UniqueNetworkSignature.Trim());//trim() as a safeguard
    }


#warning MOVE to Azos, use include provider based on NOPApplication for FS access
    public static void ProcessAllExistingIncludes(ConfigSectionNode node, string includePragma, string level)
    {
      const int CONST_MAX_INCLUDE_DEPTH = 7;
      try
      {
        for (int count = 0; node.ProcessIncludePragmas(true, includePragma); count++)
          if (count >= CONST_MAX_INCLUDE_DEPTH)
            throw new ConfigException(Sky.StringConsts.CONFIGURATION_INCLUDE_PRAGMA_DEPTH_ERROR.Args(CONST_MAX_INCLUDE_DEPTH));
      }
      catch (Exception error)
      {
        throw new ConfigException(StringConsts.CONFIGURATION_INCLUDE_PRAGMA_ERROR.Args(level, error.ToMessageWithType()), error);
      }
    }

    #region .pvt .impl

    private string determineHostName( string hostName)
    {
      if (hostName.IsNotNullOrWhiteSpace()) return hostName;

      var hNode = m_BootApplication.ConfigRoot[CONFIG_SKY_SECTION][CONFIG_HOST_SECTION];

      var result =  hNode.AttrByName(CONFIG_HOST_NAME_ATTR).Value;

      if (result.IsNullOrWhiteSpace())
      {
          writeLog( MessageType.Warning, "Host name was not specified in config, trying to take from machine env var {0}".Args(ENV_VAR_HOST_NAME));
          result = Environment.GetEnvironmentVariable(ENV_VAR_HOST_NAME);
      }
      if (result.IsNullOrWhiteSpace())
      {
          writeLog( MessageType.Warning, "Host name was not specified in neither config nor env var, taking from local computer name");
          result = "{0}/{1}".Args(DEFAULT_HOST_ZONE_PATH, Environment.MachineName);
      }

      writeLog( MessageType.Info, "Host name: " + result);

      return result;
    }


    private Metabank mountMetabank()
    {
      var mNode = m_BootApplication.ConfigRoot[CONFIG_SKY_SECTION][CONFIG_METABASE_SECTION];

      ensureMetabaseAppName( mNode);

      FileSystemSessionConnectParams fsSessionConnectParams;
      var fs = getFileSystem(mNode, out fsSessionConnectParams);
      m_IsMetabaseFSOwner = true;

      var fsRoot = mNode[CONFIG_FS_SECTION].AttrByName(CONFIG_ROOT_ATTR).Value;
      if (fsRoot.IsNullOrWhiteSpace())
      {
        writeLog( MessageType.Info,
                          "Metabase fs root is null in config, trying to take from machine env var {0}".Args(ENV_VAR_METABASE_FS_ROOT));
        fsRoot = Environment.GetEnvironmentVariable(ENV_VAR_METABASE_FS_ROOT);
      }


      writeLog( MessageType.Info, "Metabase FS root: " + fsRoot);


      writeLog( MessageType.Info, "Mounting metabank...");
      try
      {
        var result = new Metabank(m_BootApplication, m_Application, fs, fsSessionConnectParams, fsRoot );
        writeLog( MessageType.Info, "...Metabank mounted");
        return result;
      }
      catch(Exception error)
      {
        writeLog( MessageType.CatastrophicError, error.ToMessageWithType());
        throw error;
      }
    }


    private Configuration getEffectiveAppConfigAndZoneGovernor(out Metabank.SectionHost zoneGovernorSection,
                                                               out bool isDynamicHost)
    {
      Configuration result = null;

      writeLog(MessageType.Info, "Getting effective app config for '{0}'...".Args(SkySystem.MetabaseApplicationName));
      try
      {
        var host = m_Metabase.CatalogReg.NavigateHost(m_HostName);

        result =  host.GetEffectiveAppConfig(SkySystem.MetabaseApplicationName).Configuration;

        result.Application = this.m_Application;

        zoneGovernorSection = host.ParentZoneGovernorPrimaryHost();//Looking in the same NOC only
        isDynamicHost = host.Dynamic;
      }
      catch(Exception error)
      {
        writeLog( MessageType.CatastrophicError, error.ToMessageWithType());
        throw error;
      }
      writeLog(MessageType.Info, "...config obtained");

      return result;
    }


    private void ensureMetabaseAppName(IConfigSectionNode mNode)
    {
      if (SkySystem.MetabaseApplicationName==null)
      {
        var appName = m_BootApplication.CommandArgs[CMD_ARG_SKY_SWITCH].AttrByName(CMD_ARG_APP_NAME).Value;
        if (appName.IsNotNullOrWhiteSpace())
          writeLog( MessageType.Info,
                            "Metabase application name was not defined in code, but is injected from cmd arg '-{0} {1}=<app name>'".Args(CMD_ARG_SKY_SWITCH, CMD_ARG_APP_NAME));
        else
        {
          writeLog( MessageType.Warning,
                            "Metabase application name was not defined in code or cmd switch, reading from '{0}/${1}'".Args(CONFIG_METABASE_SECTION, CONFIG_APPLICATION_NAME_ATTR));
          appName = mNode.AttrByName(CONFIG_APPLICATION_NAME_ATTR).Value;
        }

        try
        {
          SkySystem.MetabaseApplicationName = appName;
        }
        catch(Exception error)
        {
          writeLog( MessageType.CatastrophicError, error.ToMessageWithType());
          throw error;
        }
        writeLog( MessageType.Info,
                          "Metabase application name from config set to: "+SkySystem.MetabaseApplicationName);
      }
      else
      {
        writeLog( MessageType.Info,
                          "Metabase application name defined in code: "+SkySystem.MetabaseApplicationName);

        if (mNode.AttrByName(CONFIG_APPLICATION_NAME_ATTR).Exists)
          writeLog( MessageType.Warning,
                            "Metabase application name defined in code but the boot config also defines the name which was ignored");
      }
    }


    private IFileSystem getFileSystem(IConfigSectionNode mNode,
                                      out FileSystemSessionConnectParams cParams)
    {
      IFileSystem result = null;

      writeLog(MessageType.Info, "Making metabase FS instance...");

      var fsNode = mNode[CONFIG_FS_SECTION];

      var fsFallbackTypeName = Environment.GetEnvironmentVariable(ENV_VAR_METABASE_FS_TYPE);
      var fsFallbackType = typeof(IO.FileSystem.Local.LocalFileSystem);

      if (fsFallbackTypeName.IsNotNullOrWhiteSpace())
          fsFallbackType = Type.GetType(fsFallbackTypeName, true);

      result = FactoryUtils.MakeAndConfigure<FileSystem>(fsNode,
                                                        fsFallbackType,
                                                        args: new object[]{CONFIG_METABASE_SECTION, fsNode});

      var paramsNode = fsNode[CONFIG_SESSION_CONNECT_PARAMS_SECTION];
      if (paramsNode.Exists)
      {
        cParams = FileSystemSessionConnectParams.Make<FileSystemSessionConnectParams>(paramsNode);
      }
      else
      {
        var fsFallbackCString = Environment.GetEnvironmentVariable(ENV_VAR_METABASE_FS_CSTRING);
        if (fsFallbackCString.IsNotNullOrWhiteSpace())
          cParams = FileSystemSessionConnectParams.Make<FileSystemSessionConnectParams>(fsFallbackCString);
        else
          cParams = new FileSystemSessionConnectParams(){ User = User.Fake};
      }

      writeLog(MessageType.Info, "...Metabase FS FileSystemSessionConnectParams instance of '{0}' made".Args(cParams.GetType().FullName));
      writeLog(MessageType.Info, "...Metabase FS instance of '{0}' made".Args(result.GetType().FullName));

      return result;
    }

    private void writeLog(MessageType type, string text)
    {
      var app = m_BootApplication;
      if (app==null) return;

      app.Log.Write(new Message
      {
        Type = type,
        From = LOG_FROM_BOOTLOADER,
        Text = text
      });

    }
    #endregion

  }
}
