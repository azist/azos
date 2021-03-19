/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Data;
using Azos.Log;
using Azos.IO.FileSystem;
using Azos.Security;
using Azos.Web;

namespace Azos.Wave.Cms.Default
{
  /// <summary>
  /// Provides implementation of CmsSource based on a Virtual File System (VFS) accessor.
  /// VFS represents any kind of file system which gets injected via configuration.
  /// If VFS is not configured then LocalFileSystem is used to access content from disk/UNC share
  /// </summary>
  /// <remarks>
  /// File system structure:
  /// <code>
  /// fsRoot/
  ///        {portal}/             -  directory
  ///          portal.config       -  portal cfg/supported languages list
  ///          {namespace}/        -  directory
  ///            content-block-id[_lang].ext  - file
  /// Example:
  ///
  ///  c:\home\cms/
  ///             app1/          - portal directory name
  ///                portal.config       -  portal cfg/supported languages list
  ///                emails/     - namespace withing portal
  ///                  logo.png  - content block
  ///                  hello.txt      - content with generic language
  ///                  hello_esp.txt  - content with ESP iso lang
  /// </code>
  /// </remarks>
  public sealed class FileSystemCmsSource : ApplicationComponent<ICmsFacade>, ICmsSource
  {
    public const string CONFIG_FILE_SYSTEM_SECTION = "file-system";
    public const string CONFIG_ROOT_PATH_ATTR = "root-path";
    public const string CONFIG_SESSION_CONNECT_PARAMS_SECTION = "session-connect-params";

    public string PORTAL_CONFIG_FILE = "portal.laconf";

    public FileSystemCmsSource(ICmsFacade facade) : base(facade)
    {
    }


    private FileSystem m_FS;
    private FileSystemSessionConnectParams m_FSConnectParams;
    private string m_FSRootPath;

    public override string ComponentLogTopic => CoreConsts.CMS_TOPIC;

    public void Configure(IConfigSectionNode node)
    {
      if (node == null || !node.Exists) return;

      //making FileSystem instance along with connect parameters
      var fsNode = node[CONFIG_FILE_SYSTEM_SECTION];

      DisposeAndNull(ref m_FS);//dispose existing

      //make new virtual FS instance
      m_FS = FactoryUtils.MakeAndConfigureDirectedComponent<FileSystem>(this, fsNode, typeof(Azos.IO.FileSystem.Local.LocalFileSystem));

      var paramsNode = fsNode[CONFIG_SESSION_CONNECT_PARAMS_SECTION];
      if (paramsNode.Exists)
        m_FSConnectParams = FileSystemSessionConnectParams.Make<FileSystemSessionConnectParams>(paramsNode);
      else
        m_FSConnectParams = new FileSystemSessionConnectParams() { User = User.Fake };

      m_FSRootPath = fsNode.AttrByName(CONFIG_ROOT_PATH_ATTR)
                           .ValueAsString()
                           .NonBlank(CONFIG_ROOT_PATH_ATTR);

      WriteLog(MessageType.Trace,
               nameof(FileSystemCmsSource),
               $"Configured FS: '{m_FS.GetType().FullName}' type, using '{m_FSConnectParams.GetType().Name}' connect parameters. Root path: '{m_FSRootPath}'");
    }

    /// <summary>
    /// Implements <see cref="ICmsSource.FetchAllLangDataAsync"/>
    /// </summary>
    public async Task<Dictionary<string, IEnumerable<LangInfo>>> FetchAllLangDataAsync()
    {
      ensureOperationalState();
      var result = new Dictionary<string, IEnumerable<LangInfo>>();
      using (var session = m_FS.StartSession(m_FSConnectParams))
      {
        var rootDir = await session.GetItemAsync(m_FSRootPath).ConfigureAwait(false) as FileSystemDirectory;
        if (rootDir==null)
          throw new CmsException($"Error in configuration of `{nameof(FileSystemCmsSource)}`. The root fs path `{m_FSRootPath}` does not land at the existing directory");

        foreach (var portalDirName in await rootDir.GetSubDirectoryNamesAsync().ConfigureAwait(false))
        {
          var dir = await rootDir.GetItemAsync(portalDirName).ConfigureAwait(false) as FileSystemDirectory;
          if (dir == null) continue;

          //portal directory found, try to read config file
          var cfile = await dir.GetFileAsync(PORTAL_CONFIG_FILE).ConfigureAwait(false);
          if (cfile==null)
          {
            result.Add(dir.Name, new[] { ComponentDirector.DefaultGlobalLanguage });
            continue;
          }

          var cfgContent = await cfile.ReadAllTextAsync().ConfigureAwait(false);
          try
          {
            var cfg = cfgContent.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
            result.Add(dir.Name, LangInfo.MakeManyFromConfig(cfg));
          }
          catch(Exception error)
          {
            throw new CmsException($"Error in configuration file `{cfile.Path}`: {error.ToMessageWithType()}", error);
          }
        }
      }

      return result;
    }

    /// <summary>
    /// Implements <see cref="ICmsSource.FetchContentAsync(ContentId, Atom, DateTime, ICacheParams)"/>
    /// </summary>
    public async Task<Content> FetchContentAsync(ContentId id, Atom isoLang, DateTime utcNow, ICacheParams caching)
    {
      ensureOperationalState();
      var appliedIsoLang = ComponentDirector.DefaultGlobalLanguage.ISO; //in future this can be moved to property per portal

      if (isoLang.IsZero) isoLang = appliedIsoLang;

      var dirPath = m_FS.CombinePaths(m_FSRootPath, id.Portal, id.Namespace);

      using (var session = m_FS.StartSession(m_FSConnectParams))
      {
        var dir = await session.GetItemAsync(dirPath).ConfigureAwait(false) as FileSystemDirectory;

        if (dir == null) return null;//directory not found

        //1st try to get language-specific file
        var (fname, fext) = getFileNameWithLangIso(id.Block, isoLang);
        var actualIsoLang = isoLang;

        var file = await dir.GetFileAsync(fname).ConfigureAwait(false);
        //2nd try to get language-agnostic file
        if (file == null)
        {
          fname = id.Block;//without the language
          file = await dir.GetFileAsync(fname).ConfigureAwait(false);
        }
        else
          appliedIsoLang = isoLang;

        if (file == null) return null;//file not found

        var ctp = App.GetContentTypeMappings().MapFileExtension(fext);

        byte[] binContent = null;
        string txtContent = null;

        if (ctp.IsBinary)
        {
          using (var ms = new MemoryStream())
            using (var stream = file.FileStream)
            {
              await stream.CopyToAsync(ms).ConfigureAwait(false);
              binContent = ms.ToArray();
            }
        }
        else
        {
          txtContent = await file.ReadAllTextAsync().ConfigureAwait(false);
        }

        var createUser = m_FS.InstanceCapabilities.SupportsCreationUserNames      ? file.CreationUser.Name     : null;
        var modifyUser = m_FS.InstanceCapabilities.SupportsModificationUserNames  ? file.ModificationUser.Name : null;
        var createDate = m_FS.InstanceCapabilities.SupportsCreationTimestamps     ? file.CreationTimestamp     : null;
        var modifyDate = m_FS.InstanceCapabilities.SupportsModificationTimestamps ? file.ModificationTimestamp : null;

        var result = new Content(id,
                                 ctp.Name,
                                 txtContent,
                                 binContent,
                                 ctp.ContentType,
                                 new LangInfo(appliedIsoLang, appliedIsoLang.Value),
                                 attachmentFileName: id.Block,
                                 createUser: createUser,
                                 modifyUser: modifyUser,
                                 createDate: createDate,
                                 modifyDate: modifyDate);
        return result;
      }
    }


    private void ensureOperationalState()
    {
      EnsureObjectNotDisposed();
      if (m_FS == null)
        throw new CmsException($"CMS component {GetType().Name} is not configured: file system is not set");
    }

    private (string fn, string ext) getFileNameWithLangIso(string fileName, Atom langIso)
    {
      var fn = Path.GetFileNameWithoutExtension(fileName);
      var ext = Path.GetExtension(fileName);//includes the '.'
      return ($"{fn}_{langIso.Value}{ext}", ext);
    }
  }
}
