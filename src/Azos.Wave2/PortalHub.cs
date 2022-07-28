/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

using Azos.Apps;
using Azos.Conf;
using Azos.Collections;
using Azos.IO.FileSystem;

namespace Azos.Wave
{
  /// <summary>PortalHub accessor extensions of application </summary>
  public static class PortalHubExtensions
  {
    /// <summary>
    /// Provides shortcut for PortalHub module access of application context
    /// </summary>
    public static PortalHub GetPortalHub(this IApplication app) => app.NonNull(nameof(app)).ModuleRoot.Get<PortalHub>(PortalHub.APP_MODULE_NAME);
  }

  /// <summary>
  /// Portal hub module provides a root registry of portals per application -
  /// it establishes a context for portal inter-operation (i.e. so one portal may locate another one by name)
  ///  when some settings need to be inherited/cloned.
  /// </summary>
  /// <remarks>
  /// PortalHub is a module which boots (created and configured) by application chassis along
  /// with its all portal-children, emitting config errors at app boot time.
  /// To test PortalHub - configure a test app container with PortalHub module injected at the app root
  /// </remarks>
  public sealed class PortalHub : ModuleBase
  {
    /// <summary>
    /// An app-wide unique name for PortalHub module instance. There can be only one PortalHub
    /// instance allocated in the app context
    /// </summary>
    public const string APP_MODULE_NAME = "@@@PortalHub";
    public const string CONFIG_PORTAL_SECTION = "portal";
    public const string CONFIG_CONTENT_FS_SECTION = "content-file-system";
    public const string CONFIG_FS_CONNECT_PARAMS_SECTION = "connect-params";
    public const string CONFIG_FS_ROOT_PATH_ATTR = "root-path";
    public const string CONFIG_CMS_BANK_SECTION = "cms-bank";


    internal PortalHub(IModule parentAppRootModule) : base(parentAppRootModule)
    {
      //check that there is no other instance of PortalHub allocated by configuration mistake
      //in the child module (this is a safeguard and will never happen)
      var singleton = App.Singletons.GetOrCreate<PortalHub>(() => this);
      if (!singleton.created)
        throw new WaveException(StringConsts.CONFIG_PORTAL_HUB_DUPLICATE_INSTANCE_ERROR);

      m_Portals = new Registry<Portal>(false);
    }

    protected override void Destructor()
    {
      base.Destructor();

      App.Singletons.Remove<PortalHub>();

      m_Portals.ForEach( p => p.Dispose() );

      DisposableObject.DisposeAndNull(ref m_ContentFS);
    }

    private FileSystemSessionConnectParams m_ContentFSConnect;
    private FileSystem m_ContentFS;
    private string m_ContentFSRootPath;

    internal Registry<Portal> m_Portals;


    public sealed override string Name => APP_MODULE_NAME;//No other module may be called this name

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.WEB_TOPIC;

    /// <summary>
    /// Registry of all portals in the hub
    /// </summary>
    public IRegistry<Portal> Portals => m_Portals;

    /// <summary>
    /// Returns file system that serves static content for portals
    /// </summary>
    public IFileSystem ContentFileSystem => m_ContentFS;

    public FileSystemSessionConnectParams ContentFileSystemConnectParams => m_ContentFSConnect;

    /// <summary>
    /// Returns root path for content file system
    /// </summary>
    public string ContentFileSystemRootPath => m_ContentFSRootPath ?? string.Empty;

    /// <summary>
    /// Returns first portal which is not Offline and marked as default
    /// </summary>
    public Portal DefaultOnline => m_Portals.FirstOrDefault(p => !p.Offline && p.Default);


    protected override void DoConfigure(IConfigSectionNode node)
    {
      if (node==null || !node.Exists)
        throw new WaveException(StringConsts.CONFIG_PORTAL_HUB_NODE_ERROR);

      foreach(var cn in node.Children.Where(cn=>cn.IsSameName(CONFIG_PORTAL_SECTION)))
      {
        var portal = FactoryUtils.MakeDirectedComponent<Portal>(this, cn, extraArgs: new [] { cn });
        if (!m_Portals.Register( portal))
          throw new WaveException(StringConsts.PORTAL_HUB_MODULE_ALREADY_CONTAINS_PORTAL_ERROR.Args(portal.Name));
      }


      //Make File System
      var fsNode =  node[CONFIG_CONTENT_FS_SECTION];

      m_ContentFS = FactoryUtils.MakeAndConfigureDirectedComponent<FileSystem>(this, fsNode, typeof(IO.FileSystem.Local.LocalFileSystem));
      var fsPNode = fsNode[CONFIG_FS_CONNECT_PARAMS_SECTION];

      if (fsPNode.Exists)
      {
        m_ContentFSConnect = FileSystemSessionConnectParams.Make<FileSystemSessionConnectParams>(fsPNode);
      }
      else
      {
        m_ContentFSConnect = new FileSystemSessionConnectParams(){ User = Azos.Security.User.Fake};
      }

      m_ContentFSRootPath = fsNode.AttrByName(CONFIG_FS_ROOT_PATH_ATTR).Value;
      if (m_ContentFSRootPath.IsNullOrWhiteSpace())
       throw new WaveException(StringConsts.CONFIG_PORTAL_HUB_FS_ROOT_PATH_ERROR.Args(CONFIG_CONTENT_FS_SECTION, CONFIG_FS_ROOT_PATH_ATTR));

    }


    /// <summary>
    /// Generates file version path segment suitable for usage in file name.
    /// This method is slow as it does byte-by-byte file signature calculation
    /// </summary>
    public string GenerateContentFileVersionSegment(string filePath)
    {
       if (m_ContentFS==null) return null;
       if (m_ContentFSConnect==null) return null;
       if (filePath.IsNullOrWhiteSpace()) return null;

       using(var session = m_ContentFS.StartSession(m_ContentFSConnect))
       {
         var buf = new byte[8*1024];
         var fName = m_ContentFS.CombinePaths(m_ContentFSRootPath, filePath);
         var fsFile = session[fName] as FileSystemFile;
         if (fsFile==null) return null;
         long sz = 0;

         var csum = new Azos.IO.ErrorHandling.Adler32();
         using(var stream = fsFile.FileStream)
         while(true)
         {
           var got = stream.Read(buf, 0, buf.Length);
           if (got<=0) break;
           sz += got;
           csum.Add(buf, 0, got);
         }


         var data = (ulong)sz << 32 | (ulong)csum.Value;//high order 32 bits store file size, low order store Adler32 hash
         return data.ToString("X").ToLowerInvariant();
       }
    }
  }//hub
}
