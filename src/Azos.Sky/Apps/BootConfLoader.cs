/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Apps;
using Azos.Conf;
using Azos.Security;
using Azos.IO.FileSystem;
using Azos.Log;

using Azos.Sky.Metabase;

namespace Azos.Sky.Apps
{
    /// <summary>
    /// Gets the boot configuration for app container.
    /// Reads the local process config to determine where the metabase is and what file system to use to connect to it.
    /// Once metabase connection is established get all information form there identifying the host by sky/Host/$name.
    /// If the name of the host is not set in config, then take it from SKY_HOST_NAME environment var. If that name is blank then
    ///  take host name from: DEFAULT_WORLD_GLOBAL_ZONE_PATH+LOCAL_COMPUTER_NAME (NetBIOSName)
    /// </summary>
    public static class BootConfLoader
    {
        #region CONSTS
          public const string ENV_VAR_METABASE_FS_ROOT     = "SKY_METABASE_FS_ROOT";
          public const string ENV_VAR_METABASE_FS_TYPE     = "SKY_METABASE_FS_TYPE";
          public const string ENV_VAR_METABASE_FS_CSTRING  = "SKY_METABASE_FS_CSTRING";

          public const string ENV_VAR_HOST_NAME  = "SKY_HOST_NAME";

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

        private static SystemApplicationType s_SystemApplicationType;
        private static bool s_Loaded;
        private static Exception s_LoadException;
        private static string s_HostName;
        private static string s_DynamicHostNameSuffix;
        private static Metabank s_Metabase;
        private static string s_ParentZoneGovernorPrimaryHostName;

        /// <summary>
        /// Internal hack to compensate for c# inability to call .ctor within .ctor body
        /// </summary>
        internal static string[] SetSystemApplicationType(SystemApplicationType appType, string[] args)
        {
          s_SystemApplicationType = appType;
          return args;
        }

        /// <summary>
        /// Application container system type
        /// </summary>
        public static SystemApplicationType SystemApplicationType { get { return s_SystemApplicationType; } }

        /// <summary>
        /// Returns true after configuration has loaded
        /// </summary>
        public static bool Loaded { get{ return s_Loaded;} }

        /// <summary>
        /// Returns exception (if any) has occurred during application config loading process
        /// </summary>
        public static Exception LoadException { get{ return s_LoadException;} }


        /// <summary>
        /// Host name as determined at boot
        /// </summary>
        public static string HostName { get { return s_HostName ?? string.Empty;} }


        /// <summary>
        /// For dynamic hosts, host name suffix as determined at boot. It is the last part of HostName for dynamic hosts
        /// including the separation character
        /// </summary>
        public static string DynamicHostNameSuffix { get { return s_DynamicHostNameSuffix ?? string.Empty;} }


        /// <summary>
        /// True when metabase section host declares this host as dynamic and HostName ends with DynamicHostNameSuffix
        /// </summary>
        public static bool DynamicHost {get { return s_DynamicHostNameSuffix.IsNotNullOrWhiteSpace();}}


        /// <summary>
        /// Returns primary zone governor parent host as determined at boot or null if this is the top-level host
        /// </summary>
        public static string ParentZoneGovernorPrimaryHostName { get { return s_ParentZoneGovernorPrimaryHostName;}}



        /// <summary>
        /// Metabase as determined at boot or null in case of failure
        /// </summary>
        public static Metabank Metabase { get { return s_Metabase;} }



             private class TestDisposer : IDisposable {    public void Dispose(){ BootConfLoader.Unload();}       }


        internal static void SetDomainInvariantCulture()
        {
          System.Globalization.CultureInfo.DefaultThreadCurrentCulture =
            System.Globalization.CultureInfo.InvariantCulture;
        }

        public static IDisposable LoadForTest(SystemApplicationType appType, Metabank mbase, string host, string dynamicHostNameSuffix = null)
        {
          SetDomainInvariantCulture();
          s_Loaded = true;
          s_SystemApplicationType = appType;
          s_Metabase = mbase;
          s_HostName = host;

          if (dynamicHostNameSuffix.IsNotNullOrWhiteSpace())
           s_DynamicHostNameSuffix = Metabank.HOST_DYNAMIC_SUFFIX_SEPARATOR + dynamicHostNameSuffix;
          else
          {
             var sh = mbase.CatalogReg.NavigateHost(host);
             if (sh.Dynamic)
               s_DynamicHostNameSuffix = thisMachineDynamicNameSuffix();
          }

          if (s_DynamicHostNameSuffix.IsNotNullOrWhiteSpace())
            s_HostName = s_HostName + s_DynamicHostNameSuffix;

          SystemVarResolver.Bind();

          Configuration.ProcesswideConfigNodeProviderType = typeof(Metabase.MetabankFileConfigNodeProvider);

          return new TestDisposer();
        }

        private static string thisMachineDynamicNameSuffix()
        {
          //no spaces between
          return "{0}{1}".Args(Metabank.HOST_DYNAMIC_SUFFIX_SEPARATOR, Platform.Computer.UniqueNetworkSignature.Trim());//trim() as a safeguard
        }


        private static void writeLog(this AzosApplication app, Azos.Log.MessageType type, string text)
        {
          app.Log.Write( new Message{
                                    Type = type,
                                    From = LOG_FROM_BOOTLOADER,
                                    Text = text
                         });

        }


        /// <summary>
        /// Loads initial application container configuration (app container may re-read it in future using metabase) per supplied local one also connecting the metabase
        /// </summary>
        public static Configuration Load(string[] cmdArgs, Configuration bootConfig)
        {
            if (s_Loaded)
                throw new SkyException(StringConsts.APP_LOADER_ALREADY_LOADED_ERROR);

            SetDomainInvariantCulture();

            SystemVarResolver.Bind();

            Configuration.ProcesswideConfigNodeProviderType = typeof(Metabase.MetabankFileConfigNodeProvider);

            try
            {
                Configuration result = null;

                //init Boot app container
                using(var bootApp = new AzosApplication(cmdArgs, bootConfig.Root))
                {
                   bootApp.writeLog(MessageType.Info, "Entering Sky app bootloader...");

                   determineHostName(bootApp);

                   Message.DefaultHostName = s_HostName;

                   mountMetabank(bootApp);

                   Metabank.SectionHost zoneGov;
                   bool isDynamicHost;
                   result = getEffectiveAppConfigAndZoneGovernor(bootApp, out zoneGov, out isDynamicHost);

                   if (zoneGov!=null) s_ParentZoneGovernorPrimaryHostName = zoneGov.RegionPath;

                   if (isDynamicHost)
                   {
                     bootApp.writeLog(MessageType.Info, "The meatabase host '{0}' is dynamic".Args(s_HostName));
                     s_DynamicHostNameSuffix = thisMachineDynamicNameSuffix();
                     bootApp.writeLog(MessageType.Info, "Obtained actual host dynamic suffix: '{0}' ".Args(s_DynamicHostNameSuffix));
                     s_HostName = s_HostName + s_DynamicHostNameSuffix;//no spaces between
                     bootApp.writeLog(MessageType.Info, "The actual dynamic instance host name is: '{0}'".Args(s_HostName));

                     Message.DefaultHostName = s_HostName;
                   }


                   bootApp.writeLog(MessageType.Info, "...exiting Sky app bootloader");
                }

                return result;
            }
            catch(Exception error)
            {
                s_LoadException = error;
                throw new SkyException(StringConsts.APP_LOADER_ERROR + error.ToMessageWithType(), error);
            }
            finally
            {
                s_Loaded = true;
            }
        }

        public static void ProcessAllExistingIncludes(ConfigSectionNode node, string includePragma, string level)
        {
          const int CONST_MAX_INCLUDE_DEPTH = 7;
          try
          {
            for (int count = 0; node.ProcessIncludePragmas(true, includePragma); count++)
              if (count >= CONST_MAX_INCLUDE_DEPTH)
                throw new ConfigException(StringConsts.CONFIGURATION_INCLUDE_PRAGMA_DEPTH_ERROR.Args(CONST_MAX_INCLUDE_DEPTH));
          }
          catch (Exception error)
          {
            throw new ConfigException(StringConsts.CONFIGURATION_INCLUDE_PRAGMA_ERROR.Args(level, error.ToMessageWithType()), error);
          }
        }


        internal static void Unload()
        {
          if (!s_Loaded) return;
          s_Loaded = false;
          s_SystemApplicationType = SystemApplicationType.Unspecified;
          s_HostName = null;
          s_DynamicHostNameSuffix = null;
          s_ParentZoneGovernorPrimaryHostName = null;
          try
          {
            if (s_Metabase!=null)
            {
              var mb = s_Metabase;
              var fs = mb.FileSystem;
              s_Metabase = null;

              try
              {
                mb.Dispose();
              }
              finally
              {
                if (fs!=null)
                {
                  fs.Dispose();
                }
              }//finally
            }
          }
          catch
          {
            //nowhere to log anymore as all loggers have stopped
          }
        }

        #region .pvt .impl

          private static void determineHostName(AzosApplication bootApp)
          {
              var hNode = bootApp.ConfigRoot[CONFIG_SKY_SECTION][CONFIG_HOST_SECTION];

              s_HostName = hNode.AttrByName(CONFIG_HOST_NAME_ATTR).Value;

              if (s_HostName.IsNullOrWhiteSpace())
              {
                  bootApp.writeLog(MessageType.Warning, "Host name was not specified in config, trying to take from machine env var {0}".Args(ENV_VAR_HOST_NAME));
                  s_HostName = System.Environment.GetEnvironmentVariable(ENV_VAR_HOST_NAME);
              }
              if (s_HostName.IsNullOrWhiteSpace())
              {
                  bootApp.writeLog(MessageType.Warning, "Host name was not specified in neither config nor env var, taking from local computer name");
                  s_HostName = "{0}/{1}".Args(DEFAULT_HOST_ZONE_PATH, System.Environment.MachineName);
              }

              bootApp.writeLog(MessageType.Info, "Host name: " + s_HostName);
          }


          private static void mountMetabank(AzosApplication bootApp)
          {
            var mNode = bootApp.ConfigRoot[CONFIG_SKY_SECTION][CONFIG_METABASE_SECTION];

            ensureMetabaseAppName(bootApp, mNode);


            FileSystemSessionConnectParams fsSessionConnectParams;
            var fs = getFileSystem(bootApp, mNode, out fsSessionConnectParams);

            var fsRoot = mNode[CONFIG_FS_SECTION].AttrByName(CONFIG_ROOT_ATTR).Value;
            if (fsRoot.IsNullOrWhiteSpace())
            {
              bootApp.writeLog(MessageType.Info,
                               "Metabase fs root is null in config, trying to take from machine env var {0}".Args(ENV_VAR_METABASE_FS_ROOT));
              fsRoot = System.Environment.GetEnvironmentVariable(ENV_VAR_METABASE_FS_ROOT);
            }


            bootApp.writeLog(MessageType.Info, "Metabase FS root: " + fsRoot);


            bootApp.writeLog(MessageType.Info, "Mounting metabank...");
            try
            {
              s_Metabase = new Metabank(fs, fsSessionConnectParams, fsRoot );
            }
            catch(Exception error)
            {
              bootApp.writeLog(MessageType.CatastrophicError, error.ToMessageWithType());
              throw error;
            }
            bootApp.writeLog(MessageType.Info, "...Metabank mounted");
          }


          private static Configuration getEffectiveAppConfigAndZoneGovernor(AzosApplication bootApp,
                                                                            out Metabank.SectionHost zoneGovernorSection,
                                                                            out bool isDynamicHost)
          {
            Configuration result = null;

            bootApp.writeLog(MessageType.Info, "Getting effective app config for '{0}'...".Args(SkySystem.MetabaseApplicationName));
            try
            {
              var host = s_Metabase.CatalogReg.NavigateHost(s_HostName);

              result =  host.GetEffectiveAppConfig(SkySystem.MetabaseApplicationName).Configuration;

              zoneGovernorSection = host.ParentZoneGovernorPrimaryHost();//Looking in the same NOC only
              isDynamicHost = host.Dynamic;
            }
            catch(Exception error)
            {
              bootApp.writeLog(MessageType.CatastrophicError, error.ToMessageWithType());
              throw error;
            }
            bootApp.writeLog(MessageType.Info, "...config obtained");

            return result;
          }


          private static void ensureMetabaseAppName(AzosApplication bootApp, IConfigSectionNode mNode)
          {
            if (SkySystem.MetabaseApplicationName==null)
            {
              var appName = bootApp.CommandArgs[CMD_ARG_SKY_SWITCH].AttrByName(CMD_ARG_APP_NAME).Value;
              if (appName.IsNotNullOrWhiteSpace())
                bootApp.writeLog(MessageType.Info,
                                 "Metabase application name was not defined in code, but is injected from cmd arg '-{0} {1}=<app name>'".Args(CMD_ARG_SKY_SWITCH, CMD_ARG_APP_NAME));
              else
              {
                bootApp.writeLog(MessageType.Warning,
                                 "Metabase application name was not defined in code or cmd switch, reading from '{0}/${1}'".Args(CONFIG_METABASE_SECTION, CONFIG_APPLICATION_NAME_ATTR));
                appName = mNode.AttrByName(CONFIG_APPLICATION_NAME_ATTR).Value;
              }

              try
              {
                SkySystem.MetabaseApplicationName = appName;
              }
              catch(Exception error)
              {
                bootApp.writeLog(MessageType.CatastrophicError, error.ToMessageWithType());
                throw error;
              }
              bootApp.writeLog(MessageType.Info,
                               "Metabase application name from config set to: "+SkySystem.MetabaseApplicationName);
            }
            else
            {
              bootApp.writeLog(MessageType.Info,
                               "Metabase application name defined in code: "+SkySystem.MetabaseApplicationName);

              if (mNode.AttrByName(CONFIG_APPLICATION_NAME_ATTR).Exists)
                bootApp.writeLog(MessageType.Warning,
                                 "Metabase application name defined in code but the boot config also defines the name which was ignored");
            }
          }


          private static IFileSystem getFileSystem(AzosApplication bootApp,
                                                   IConfigSectionNode mNode,
                                                   out FileSystemSessionConnectParams cParams)
          {
            IFileSystem result = null;

            bootApp.writeLog(MessageType.Info, "Making metabase FS instance...");

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

            bootApp.writeLog(MessageType.Info, "...Metabase FS FileSystemSessionConnectParams instance of '{0}' made".Args(cParams.GetType().FullName));
            bootApp.writeLog(MessageType.Info, "...Metabase FS instance of '{0}' made".Args(result.GetType().FullName));

            return result;
          }


        #endregion

    }
}
