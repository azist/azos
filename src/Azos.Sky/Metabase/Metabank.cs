/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Azos;
using Azos.Apps;
using Azos.Conf;
using Azos.Collections;
using Azos.Data.Access.Cache;
using Azos.Log;
using Azos.IO.FileSystem;
using Azos.IO.FileSystem.Local;
using Azos.Instrumentation;

namespace Azos.Sky.Metabase
{
  /// <summary>
  /// Provides interface to METABASE - a metadata bank for the Sky.
  /// Metabase is organized as a hierarchical directory structure with various files such as:
  /// * configuration files that describe structure, policies, settings, hosts etc.
  /// * configs files for particular components (i.e.: application, region, zone, db bank etc.)
  /// * binary packages - resources necessary to distribute and run code across Sky (assemblies, images, static data files etc.)
  /// Metabase is usually stored in a version-controlled system, such as SVN or GIT so the whole distribution
  ///  is versioned so any change can be rolled-back. This class is thread safe for using its instance methods
  /// </summary>
  /// <remarks>
  ///  Metabase is a special kind of database that contains meta information about the whole system.
  ///  The information is logically organized into catalogs that are further broken down into sections.
  ///  Catalog implementers contain various instances of Section classes that represent pieces of metabase whole data that
  ///  can lazily load from the source file system. Contrary to monolithic application configurations, that load
  ///  at once from a single source (i.e. disk file), metadata class allows to wrap configuration and load segments of configuration
  ///   on a as-needed basis.
  ///
  ///  The following table describes the root metabase structure:
  ///  <code>
  ///   /app - application catalog, contains Application and Roles
  ///       /applications
  ///           /app1
  ///           ..
  ///           /appX
  ///       /roles
  ///           /role1
  ///           ..
  ///           /roleX
  ///   /bin - contains binary packages that get installed on appropriate hosts
  ///       /package-name1.platform.os.version
  ///       ..
  ///       /package-nameX.platform.os.version
  ///   /reg - regional catalog, organizes hierarchies of regions, NOC(facilities), zones and hosts
  ///       /region1
  ///           /sub-region1
  ///                /noc1
  ///                    /zone1
  ///                        /subzone1
  ///                            /host1
  ///                            ..
  ///                        ..
  ///                    ..
  ///               ..
  ///          ..
  ///       ..
  ///       /regionX
  ///
  ///  </code>
  /// </remarks>
  public sealed partial class Metabank : ApplicationComponent, IInstrumentable
  {
    #region .ctor

    /// <summary>
    /// Opens metabase from the specified file system instance at the specified metabase root path
    /// </summary>
    /// <param name="fileSystem">An instance of a file system that stores the metabase files</param>
    /// <param name="fsSessionParams">File system connection params</param>
    /// <param name="rootPath">A path to the directory within the file system that contains metabase root data</param>
    public Metabank(IFileSystem fileSystem, FileSystemSessionConnectParams fsSessionParams, string rootPath) : base(fileSystem.NonNull(nameof(fileSystem)).App)
    {
      if (fileSystem is LocalFileSystem && fsSessionParams==null)
        fsSessionParams = new FileSystemSessionConnectParams();

      if (fileSystem==null || fsSessionParams==null)
        throw new MetabaseException(StringConsts.ARGUMENT_ERROR+"Metabank.ctor(fileSystem|fsSessionParams==null)");

      using(var session = ctorFS(fileSystem, fsSessionParams, rootPath))
      {
          m_CommonLevelConfig = getConfigFromFile(session, CONFIG_COMMON_FILE).Root;

          m_RootConfig        = getConfigFromFile(session, CONFIG_SECTION_LEVEL_FILE).Root;
          includeCommonConfig(m_RootConfig);
          m_RootConfig.ResetModified();

          m_RootAppConfig  = getConfigFromFile(session, CONFIG_SECTION_LEVEL_ANY_APP_FILE).Root;
          m_PlatformConfig = getConfigFromFile(session, CONFIG_PLATFORMS_FILE).Root;
          m_NetworkConfig  = getConfigFromFile(session, CONFIG_NETWORKS_FILE).Root;
          m_ContractConfig  = getConfigFromFile(session, CONFIG_CONTRACTS_FILE).Root;
      }
      m_Catalogs = new Registry<Catalog>();
      var cacheStore = new CacheStore(this, "AC.Metabank");
      //No app available - nowhere to configure: //cacheStore.Configure(null);
      /*
      cacheStore.TableOptions.Register( new TableOptions(APP_CATALOG, 37, 3) );
      cacheStore.TableOptions.Register( new TableOptions(BIN_CATALOG, 37, 7) );
      cacheStore.TableOptions.Register( new TableOptions(REG_CATALOG, 37, 17) );
      superseded by the cacheStore.DefaultTableOptions below:
        */
      cacheStore.DefaultTableOptions = new TableOptions("*", 37, 17);
      //reg catalog needs more space
      cacheStore.TableOptions.Register( new TableOptions(REG_CATALOG, 571, 37) );

      cacheStore.InstrumentationEnabled = false;

      m_Cache = new ComplexKeyHashingStrategy(cacheStore);

      new AppCatalog( this );
      new BinCatalog( this );
      new SecCatalog( this );
      new RegCatalog( this );

      ConfigAttribute.Apply(this, m_RootConfig);
    }

    //tests and sets FS connection params
    private FileSystemSession ctorFS(IFileSystem fileSystem, FileSystemSessionConnectParams fsSessionParams, string rootPath)
    {
      FileSystemSession session = null;
      //Test FS connection
      try
      {
        session = fileSystem.StartSession(fsSessionParams);
        if (fsSessionParams.Version==null)
            fsSessionParams.Version = session.LatestVersion;
      }
      catch(Exception error)
      {
        throw new MetabaseException(StringConsts.METABASE_FS_CONNECTION_ERROR.Args(fileSystem.GetType().FullName,
                                                                                    fileSystem.Name,
                                                                                    fsSessionParams.ToString(),
                                                                                    error.ToMessageWithType()
                                                                                    ), error);
      }

      m_FS = fileSystem;
      m_FSSessionConnectParams = fsSessionParams;
      m_FSRootPath = rootPath ?? string.Empty;

      return session;
    }

    protected override void Destructor()
    {
      m_Active = false;

      if (m_FSSessionCacheThread!=null)
      {
        m_FSSessionCacheThreadWaiter.Set();
        m_FSSessionCacheThread.Join();
        m_FSSessionCacheThreadWaiter.Close();

        m_FSSessionCacheThread = null;
        m_FSSessionCacheThreadWaiter = null;
      }

      if (m_Cache!=null)
        if (m_Cache.Store!=null)
          m_Cache.Store.Dispose();

      base.Destructor();
    }

    #endregion

    #region Fields

    //Invariant culture ignore case comparer
    private static readonly StringComparer INVSTRCMP = StringComparer.InvariantCultureIgnoreCase;

    private bool m_Active = true;
    private IFileSystem m_FS;
    private FileSystemSessionConnectParams m_FSSessionConnectParams;
    private string m_FSRootPath;

    private bool m_InstrumentationEnabled;

    private ComplexKeyHashingStrategy m_Cache;


    private ConfigSectionNode m_CommonLevelConfig;
    private ConfigSectionNode m_RootConfig;
    private ConfigSectionNode m_RootAppConfig;
    private ConfigSectionNode m_PlatformConfig;
    private ConfigSectionNode m_NetworkConfig;
    private ConfigSectionNode m_ContractConfig;

    private Registry<Catalog> m_Catalogs;

    private int m_ResolveDynamicHostNetSvcTimeoutMs = DEFAULT_RESOLVE_DYNAMIC_HOST_NET_SVC_TIMEOUT_MS;

    #endregion

    #region Properties

        public override string ComponentLogTopic => SysConsts.LOG_TOPIC_METABASE;

        public override string ComponentCommonName { get { return "metabase"; }}

        /// <summary>
        /// Returns false as soon as destruction starts
        /// </summary>
        public bool Active{get{return m_Active;}}

        /// <summary>
        /// Returns file system that this bank is read from
        /// </summary>
        public IFileSystem FileSystem { get{ return m_FS;} }

        /// <summary>
        /// Returns parameters for file system session establishment
        /// </summary>
        public FileSystemSessionConnectParams FileSystemSessionConnectParams { get{ return m_FSSessionConnectParams;}}

        /// <summary>
        /// Returns root path of the metabase data in the file system
        /// </summary>
        public string FileSystemRootPath { get{ return m_FSRootPath;} }

        /// <summary>
        /// Controls instrumentation availability
        /// </summary>
        [Config]
        [ExternalParameter(SysConsts.EXT_PARAM_GROUP_METABASE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
        public bool InstrumentationEnabled
        {
          get { return m_InstrumentationEnabled; }
          set
          {
            m_Cache.Store.InstrumentationEnabled = value;
            m_InstrumentationEnabled = value;
          }
        }

        [Config(Default = DEFAULT_RESOLVE_DYNAMIC_HOST_NET_SVC_TIMEOUT_MS)]
        [ExternalParameter(SysConsts.EXT_PARAM_GROUP_METABASE)]
        public int ResolveDynamicHostNetSvcTimeoutMs
        {
          get { return m_ResolveDynamicHostNetSvcTimeoutMs;}
          set
          {
            m_ResolveDynamicHostNetSvcTimeoutMs = value < MIN_RESOLVE_DYNAMIC_HOST_NET_SVC_TIMEOUT_MS ?
              DEFAULT_RESOLVE_DYNAMIC_HOST_NET_SVC_TIMEOUT_MS : value;
          }
        }


        /// <summary>
        /// Returns common config for all levels, the one that gets included in all level configs of metabase
        /// </summary>
        public IConfigSectionNode CommonLevelConfig { get{ return m_CommonLevelConfig;}}


        /// <summary>
        /// Returns root-level config for the whole meatabase
        /// </summary>
        public IConfigSectionNode RootConfig { get{ return m_RootConfig;}}

        /// <summary>
        /// Returns root-level config for all applications that this meatabase defines
        /// </summary>
        public IConfigSectionNode RootAppConfig { get{ return m_RootAppConfig;}}


        /// <summary>
        /// Returns all catalogs that this metabank has initialized
        /// </summary>
        public IRegistry<Catalog> Catalogs { get{ return m_Catalogs;}}



        /// <summary>
        /// Returns application catalog instance
        /// </summary>
        public AppCatalog CatalogApp { get{ return (AppCatalog)m_Catalogs[APP_CATALOG];}}

        /// <summary>
        /// Returns bin catalog instance
        /// </summary>
        public BinCatalog CatalogBin { get{ return (BinCatalog)m_Catalogs[BIN_CATALOG];}}


        /// <summary>
        /// Returns region catalog instance
        /// </summary>
        public RegCatalog CatalogReg { get{ return (RegCatalog)m_Catalogs[REG_CATALOG];}}

        /// <summary>
        /// Returns security catalog instance
        /// </summary>
        public SecCatalog CatalogSec { get{ return (SecCatalog)m_Catalogs[SEC_CATALOG];}}

        /// <summary>
        /// Returns the list of platform nodes declared in platforms file
        /// </summary>
        public IEnumerable<IConfigSectionNode> PlatformConfNodes
        {
          get{ return m_PlatformConfig.Children.Where(cn=>cn.IsSameName(CONFIG_PLATFORM_SECTION));}
        }


        /// <summary>
        /// Returns the list of platform names declared in platforms file
        /// </summary>
        public IEnumerable<string> PlatformNames
        {
          get { return PlatformConfNodes.Select(n => n.AttrByName(CONFIG_NAME_ATTR).Value); }
        }

        /// <summary>
        /// Returns the list of operating system nodes declared in platforms file
        /// </summary>
        public IEnumerable<IConfigSectionNode> OSConfNodes
        {
          get
          {
            foreach(var pnode in PlatformConfNodes)
              foreach(var osNode in pnode.Children.Where(cn=>cn.IsSameName(CONFIG_OS_SECTION)))
                yield return osNode;
          }
        }

        /// <summary>
        /// Returns the list of operating system names declared in platforms file
        /// </summary>
        public IEnumerable<string> OSNames
        {
          get { return OSConfNodes.Select(n => n.AttrByName(CONFIG_NAME_ATTR).Value); }
        }



        /// <summary>
        /// Returns the list of network nodes declared in networks file
        /// </summary>
        public IEnumerable<IConfigSectionNode> NetworkConfNodes
        {
          get{ return m_NetworkConfig.Children.Where(cn=>cn.IsSameName(CONFIG_NETWORK_SECTION));}
        }

        /// <summary>
        /// Returns the list of network names declared in networks file
        /// </summary>
        public IEnumerable<string> NetworkNames
        {
          get { return NetworkConfNodes.Select(n => n.AttrByName(CONFIG_NAME_ATTR).Value); }
        }

        /// <summary>
        /// Returns metabase root section for service client hub from contracts.acmb
        /// </summary>
        public IConfigSectionNode ServiceClientHubConfNode
        {
          get { return m_ContractConfig[Contracts.ServiceClientHub.CONFIG_SERVICE_CLIENT_HUB_SECTION];}
        }


        /// <summary>
        /// Returns enumeration of Global Distributed ID generation Authorities declared in root metabase level
        /// </summary>
        public IEnumerable<Identification.GdidGenerator.AuthorityHost> GDIDAuthorities
        {
          get
          {
            return Identification.GdidGenerator.AuthorityHost.FromConfNode(m_RootConfig[CONFIG_GDID_SECTION]);
          }
        }


        /// <summary>
        /// Helper method to dump the status of metabase cache
        /// </summary>
        public void DumpCacheStatus(StringBuilder to)
        {
          if (m_Cache==null) return;
          var cache = m_Cache.Store;
          if (cache==null) return;

          to.AppendLine("Table definitions for cache store: " + cache.Name);
          to.AppendLine("Table                      Buckets   RPPage Locks   Empt.Size");
          to.AppendLine("---------------------------------------------------------------------------");
          foreach(var tbl in cache.Tables)
            to.AppendLine(
                        "{0,-26} {1,-9} {2,-7} {3,-6} {4,-10}".Args(tbl.Name, tbl.BucketCount, tbl.RecPerPage, tbl.LockCount, tbl.BucketCount*IntPtr.Size)
                        );
          to.AppendLine();
          to.AppendLine("Table current status for cache store: " + cache.Name);
          to.AppendLine("Table                       Count    Pages   Hits   Misses");
          to.AppendLine("---------------------------------------------------------------------------");
          foreach(var tbl in cache.Tables)
            to.AppendLine(
                        "{0,-26} {1,-9} {2,-7} {3,-6} {4,-10}".Args(tbl.Name, tbl.Count, tbl.PageCount, tbl.StatComplexHitCount, tbl.StatComplexMissCount)
                        );

        }


          /// <summary>
          /// Returns named parameters that can be used to control this component
          /// </summary>
          public IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get { return ExternalParameterAttribute.GetParameters(this); } }

          /// <summary>
          /// Returns named parameters that can be used to control this component
          /// </summary>
          public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
          {
            return ExternalParameterAttribute.GetParameters(this, groups);
          }


    #endregion


    #region Public

        /// <summary>
        /// Validates metabase by checking all of it contents for consistency - may take time for large metabases.
        /// This method is not expected to be called by business applications, only by tools
        /// </summary>
        /// <remarks>
        /// As of Jul 2015, Validation is purposely convoluted as procedural code (validate methods)
        /// instead of doing injectable rules etc. This is done for Practical simplicity of the design
        /// </remarks>
        /// <param name="output">
        /// A list where output such as erros and warnings is redirected.
        /// May use Collections.EventedList for receiving notifications upon list addition
        /// </param>
        public void Validate(IList<MetabaseValidationMsg> output)
        {
          var context = new ValidationContext(output);
          validate(context);
        }

        private void validate(ValidationContext ctx)
        {
          var output = ctx.Output;

          var fromHost =  Apps.BootConfLoader.HostName;

          if (fromHost.IsNullOrWhiteSpace())
          {
            output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_VALIDATION_WRONG_HOST_ERROR.Args("null|empty")) );
            return;
          }

          var host = CatalogReg[fromHost] as SectionHost;
          if (host==null)
          {
            output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_VALIDATION_WRONG_HOST_ERROR.Args(fromHost)) );
            output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Info, null, null, StringConsts.METABASE_VALIDATION_WRONG_HOST_INFO.Args(Apps.BootConfLoader.ENV_VAR_HOST_NAME)) );
          }

          validateMetabase(ctx);
        }

        /// <summary>
        /// Joins paths per injected file system
        /// </summary>
        public string JoinPaths(string first, params string[] other)
        {
          return m_FS.CombinePaths(first, other);
        }

        /// <summary>
        /// Returns config node for the specified OS, or throws if this OS is not declared in platforms root file or it is declared more than once
        /// </summary>
        public IConfigSectionNode GetOSConfNode(string osName)
        {
          var list = OSConfNodes.Where(n=>n.IsSameNameAttr(osName)).ToList();

          if (list.Count==0)
            throw new MetabaseException(StringConsts.METABASE_PLATFORMS_OS_NOT_DEFINED_ERROR.Args(osName));

          if (list.Count>1)
            throw new MetabaseException(StringConsts.METABASE_PLATFORMS_OS_DUPLICATION_ERROR.Args(osName));

          return list[0];
        }

        /// <summary>
        /// Gets platform config node per named OS
        /// </summary>
        public IConfigSectionNode GetOSPlatformNode(string osName)
        {
          return GetOSConfNode(osName).Parent;
        }

        /// <summary>
        /// Gets Platform name per named OS
        /// </summary>
        public string GetOSPlatformName(string osName)
        {
          return GetOSPlatformNode(osName).AttrByName(CONFIG_NAME_ATTR).Value;
        }



        /// <summary>
        /// Returns conf node for the network or throws if network with requested name was not found
        /// </summary>
        public IConfigSectionNode GetNetworkConfNode(string netName)
        {
          var net = NetworkConfNodes.FirstOrDefault(n=>n.IsSameNameAttr(netName));
          if (net==null)
            throw new MetabaseException(StringConsts.METABASE_NAMED_NETWORK_NOT_FOUND_ERROR.Args(netName ?? SysConsts.NULL));
          return net;
        }

        /// <summary>
        /// Returns true when the network with the specified name exists in metabase netoworks definition
        /// </summary>
        public bool NetworkExists(string netName)
        {
          var net = NetworkConfNodes.FirstOrDefault(n=>n.IsSameNameAttr(netName));
          return net!=null;
        }

        /// <summary>
        /// Returns description for the network or throws if network with requested name was not found
        /// </summary>
        public string GetNetworkDescription(string netName)
        {
          return GetNetworkConfNode(netName).AttrByName(CONFIG_DESCRIPTION_ATTR).Value;
        }

        /// <summary>
        /// Returns network scope for the network or throws if network with requested name was not found
        /// </summary>
        public NetworkScope GetNetworkScope(string netName)
        {
          return GetNetworkConfNode(netName).AttrByName(CONFIG_SCOPE_ATTR).ValueAsEnum<NetworkScope>(NetworkScope.Any);
        }


        /// <summary>
        /// Returns a list of config nodes for services for the named network
        /// </summary>
        public IEnumerable<IConfigSectionNode> GetNetworkSvcNodes(string netName)
        {
          try
          {
            var netNode = NetworkConfNodes.Single(n=>n.IsSameNameAttr(netName));
            return netNode.Children.Where(cn=>cn.IsSameName(CONFIG_SERVICE_SECTION));
          }
          catch(Exception error)
          {
            throw new MetabaseException(StringConsts.METABASE_NETWORK_CONFIG_ERROR+"GetNetworkSvcNodes({0}): {1}".Args(netName, error.ToMessageWithType(), error));
          }
        }

        /// <summary>
        /// Returns a list of service names for the named network
        /// </summary>
        public IEnumerable<string> GetNetworkSvcNames(string netName)
        {
          return GetNetworkSvcNodes(netName).Select(n=>n.AttrByName(CONFIG_NAME_ATTR).Value);
        }

        /// <summary>
        /// Returns a list of config nodes for groups for the named network
        /// </summary>
        public IEnumerable<IConfigSectionNode> GetNetworkGroupNodes(string netName)
        {
          try
          {
            var netNode = NetworkConfNodes.Single(n=>n.IsSameNameAttr(netName));
            return netNode.Children.Where(cn=>cn.IsSameName(CONFIG_GROUP_SECTION));
          }
          catch(Exception error)
          {
            throw new MetabaseException(StringConsts.METABASE_NETWORK_CONFIG_ERROR+"GetNetworkGroupNodes({0}): {1}".Args(netName, error.ToMessageWithType(), error));
          }
        }

        /// <summary>
        /// Returns a list of group names for the named network
        /// </summary>
        public IEnumerable<string> GetNetworkGroupNames(string netName)
        {
          return GetNetworkGroupNodes(netName).Select(n=>n.AttrByName(CONFIG_NAME_ATTR).Value);
        }




        /// <summary>
        /// Returns a list of config nodes for service bindings for the named service in the named network
        /// </summary>
        public IEnumerable<IConfigSectionNode> GetNetworkSvcBindingNodes(string netName, string svcName)
        {
          try
          {
            var svcNodes = GetNetworkSvcNodes(netName).Single(n=>n.IsSameNameAttr(svcName));
            return svcNodes[CONFIG_BINDINGS_SECTION].Children;
          }
          catch(Exception error)
          {
            throw new MetabaseException(StringConsts.METABASE_NETWORK_CONFIG_ERROR+"GetNetworkSvcBindingNodes({0},{1}): {2}".Args(netName, svcName, error.ToMessageWithType(), error));
          }
        }

        /// <summary>
        /// Returns a list of service binding names for the named service in the named network
        /// </summary>
        public IEnumerable<string> GetNetworkSvcBindingNames(string netName, string svcName)
        {
          return GetNetworkSvcBindingNodes(netName, svcName).Select(n=>n.Name);
        }


        /// <summary>
        /// Returns conf node for the network service or throws if network service with requested name was not found
        /// </summary>
        public IConfigSectionNode GetNetworkSvcConfNode(string netName, string svcName)
        {
          var net = this.GetNetworkSvcNodes(netName).FirstOrDefault(n=>n.IsSameNameAttr(svcName));
          if (net==null)
            throw new MetabaseException(StringConsts.METABASE_NAMED_NETWORK_SVC_NOT_FOUND_ERROR.Args(netName ?? SysConsts.NULL, svcName ?? SysConsts.NULL));
          return net;
        }

        /// <summary>
        /// Returns conf node for the network group or throws if network group with requested name was not found
        /// </summary>
        public IConfigSectionNode GetNetworkGroupConfNode(string netName, string groupName)
        {
          var net = this.GetNetworkGroupNodes(netName).FirstOrDefault(n=>n.IsSameNameAttr(groupName));
          if (net==null)
            throw new MetabaseException(StringConsts.METABASE_NAMED_NETWORK_GRP_NOT_FOUND_ERROR.Args(netName ?? SysConsts.NULL, groupName ?? SysConsts.NULL));
          return net;
        }

        /// <summary>
        /// Returns description for the network service or throws if network with requested name was not found
        /// </summary>
        public string GetNetworkSvcDescription(string netName, string svcName)
        {
          return GetNetworkSvcConfNode(netName, svcName).AttrByName(CONFIG_DESCRIPTION_ATTR).Value;
        }

        /// <summary>
        /// Returns description for the network group or throws if network with requested name was not found
        /// </summary>
        public string GetNetworkGroupDescription(string netName, string groupName)
        {
          return GetNetworkGroupConfNode(netName, groupName).AttrByName(CONFIG_DESCRIPTION_ATTR).Value;
        }


        /// <summary>
        /// Gets external parameter value returning true if parameter was found
        /// </summary>
        public bool ExternalGetParameter(string name, out object value, params string[] groups)
        {
            return ExternalParameterAttribute.GetParameter(this, name, out value, groups);
        }

        /// <summary>
        /// Sets external parameter value returning true if parameter was found and set
        /// </summary>
        public bool ExternalSetParameter(string name, object value, params string[] groups)
        {
          return ExternalParameterAttribute.SetParameter(this, name, value, groups);
        }




    #endregion

    #region .pvt .impl

      ///reads supported configuration file taking file path relative to file system root
      private Configuration getConfigFromFile(FileSystemSession session, string path, bool require = true)
      {
        try
        {
          var fnwe = m_FS.CombinePaths(m_FSRootPath, path);
          foreach(var fmt in Configuration.AllSupportedFormats)
          {
            var fn = "{0}.{1}".Args(fnwe, fmt); // filename.<format>
            var file = session[fn] as FileSystemFile;
            if (file==null) continue;
            using(file)
            {
              var text = file.ReadAllText();
              return Configuration.ProviderLoadFromString(text, fmt);
            }
          }
          if (require)
            throw new MetabaseException(StringConsts.METABASE_FILE_NOT_FOUND_ERROR.Args(path));
          else
          {
            var result = new MemoryConfiguration();

            result.Create(SysConsts.DEFAULT_APP_CONFIG_ROOT);
            result.Root.ResetModified();
            return result;
          }
        }
        catch(Exception error)
        {
          throw new MetabaseException(StringConsts.METABASE_CONFIG_LOAD_ERROR.Args(path ?? SysConsts.UNKNOWN_ENTITY, error.ToMessageWithType()), error);
        }
      }

      ///reads supported configuration file taking file path is absolute
      private Configuration getConfigFromExistingFile(FileSystemSession session, string path)
      {
        try
        {
          var file = session[path] as FileSystemFile;

          if (file==null) throw new MetabaseException(StringConsts.METABASE_FILE_NOT_FOUND_EXACT_ERROR.Args(path));

          using(file)
          {
            var text = file.ReadAllText();
            var fmt = chopNameLeaveExt(path);

            return Configuration.ProviderLoadFromString(text, fmt);
          }
        }
        catch(Exception error)
        {
          throw new MetabaseException(StringConsts.METABASE_CONFIG_LOAD_ERROR.Args(path ?? SysConsts.UNKNOWN_ENTITY, error.ToMessageWithType()), error);
        }
      }


      internal Configuration GetConfigFromExistingFile(string path)
      {
        var session = obtainSession();
        try
        {
          var rootedPath = m_FS.CombinePaths(m_FSRootPath, path);
          return getConfigFromExistingFile(session, rootedPath);
        }
        finally
        {
          Monitor.Exit(session);
        }
      }


      private void cachePut(string entity, string key, object item)
      {
        m_Cache.Put(entity, key, item);
      }

      private object cacheGet(string entity, string key)
      {
        return m_Cache.Get(entity, key);
      }


            private Dictionary<Thread, _fss> m_FSSessionCache = new Dictionary<Thread, _fss>(ReferenceEqualityComparer<Thread>.Instance);
            private Thread m_FSSessionCacheThread;
            private AutoResetEvent m_FSSessionCacheThreadWaiter;

            private class _fss
            {
              public FileSystemSession Session;
              public DateTime LastAccess;
            }

            private FileSystemSession obtainSession()
            {
              EnsureObjectNotDisposed();
              var callerThread = Thread.CurrentThread;
              _fss session;

              lock(m_FSSessionCache)
              {
                if (m_FSSessionCacheThread==null)
                {
                  m_FSSessionCacheThread = new Thread(
                    (object objBank)=>
                    {
                      var bank  = objBank as Metabank;
                      while(bank.Active)
                      {
                        m_FSSessionCacheThreadWaiter.WaitOne(1000);
                        try
                        {
                              lock(m_FSSessionCache)
                              {
                                var now = DateTime.UtcNow;
                                var allKvp = m_FSSessionCache.ToList();
                                foreach(var kvp in allKvp)
                                  if (Monitor.TryEnter(kvp.Value.Session))
                                    try
                                    {
                                      if ((now - kvp.Value.LastAccess).TotalSeconds > 30)//how long to keep fsSession open
                                      {
                                        try{ kvp.Value.Session.Dispose();}
                                        finally{m_FSSessionCache.Remove(kvp.Key);}
                                      }
                                    }
                                    finally
                                    {
                                      Monitor.Exit(kvp.Value.Session);
                                    }
                              }
                        }
                        catch(Exception error)
                        {
                          log(MessageType.Error,
                              "obtainSession(timedLoop)",
                              "Thread '{0}' leaked: {1}".Args(m_FSSessionCacheThread.Name, error.ToMessageWithType()),
                              error);
                        }

                      }//while
                    }
                  );
                  m_FSSessionCacheThread.IsBackground = false;
                  m_FSSessionCacheThread.Name = "Metabank FS Session Cache Thread";
                  m_FSSessionCacheThreadWaiter = new AutoResetEvent(false);
                  m_FSSessionCacheThread.Start(this);
                }//if thread==null

                if (m_FSSessionCache.TryGetValue(callerThread, out session))
                {
                  session.LastAccess = DateTime.UtcNow;
                }
                else
                {
                  session = new _fss{ Session = m_FS.StartSession(m_FSSessionConnectParams), LastAccess = DateTime.UtcNow};
                  m_FSSessionCache.Add(callerThread, session);
                }

                Monitor.Enter(session.Session);
                return session.Session;
              }


            }

      /// <summary>
      /// Internal function that facilitates guarded acceess to the file system. Developers - do not use
      /// </summary>
      internal T fsAccess<T>(string operationName, string path, Func<FileSystemSession, FileSystemDirectory, T> body)
      {
        var session = obtainSession();
        try
        {
          var catPath = m_FS.CombinePaths(m_FSRootPath, path);
          var catDir = session[catPath] as FileSystemDirectory;

          if (catDir==null) throw new MetabaseException(StringConsts.METABASE_PATH_NOT_FOUND_ERROR.Args(operationName, catPath));

          T result;
          try
          {
            result = body(session, catDir);
          }
          finally
          {
            //close all file and directory handles
            var sessionItems = session.Items.ToList();
            foreach(var item in sessionItems)
              item.Dispose();
          }
          return result;
        }
        finally
        {
          Monitor.Exit(session);
        }
      }


      private string chopExt(string name)
      {
        var idx = name.LastIndexOf('.');
        if (idx<=0) return name;
        return name.Substring(0, idx);
      }

      private string chopNameLeaveExt(string name)
      {
        var idx = name.LastIndexOf('.');
        if (idx<=0 || idx==name.Length-1) return string.Empty;
        return name.Substring(idx+1);
      }


      private void includeCommonConfig(ConfigSectionNode levelRoot)
      {
        var placeholder = levelRoot.AddChildNode(Guid.NewGuid().ToString());
        placeholder.Configuration.Include(placeholder, m_CommonLevelConfig);
      }

      private void log(MessageType type, string from, string text, Exception error = null)
      {
        App.Log.Write(
              new Message
              {
                Topic = SysConsts.LOG_TOPIC_METABASE,
                Type = type,
                From = "Metabank."+from,
                Text = text,
                Exception = error
              }
          );
      }


    #endregion

  }

}
