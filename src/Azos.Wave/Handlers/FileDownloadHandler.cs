/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;

using Azos.Web;
using Azos.Conf;
using Azos.Data;
using Azos.IO.FileSystem;

namespace Azos.Wave.Handlers
{
  /// <summary>
  /// Downloads local files or files from portal content file system.
  /// Be careful with this handler as the incorrect root setup may allow users to download system-internal files
  /// </summary>
  public class FileDownloadHandler : WorkHandler
  {
     public const string CONFIG_CACHE_CONTROL_SECTION = "cache-control";

     public const string VAR_FILE_PATH  = "filePath";
     public const string VAR_ATTACHMENT = "attachment";
     public const string VAR_CHUNKED    = "chunked";

     public const string INVALID_ROOT   = @"Invalid-Root-Path:\";

     protected FileDownloadHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match) : base(dispatcher, name, order, match)
     {
     }


     protected FileDownloadHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode)
     {
        ConfigAttribute.Apply(this, confNode);
        if (confNode != null && confNode.Exists)
          m_CacheControl = ConfigAttribute.Apply(new CacheControl(), confNode[CONFIG_CACHE_CONTROL_SECTION]);
     }


     [Config]
     private string m_RootPath;

     [Config(Default=true)]
     private bool m_Throw = true;

     [Config]
     private bool m_UsePortalHub;

     [Config]
     private string m_VersionSegmentPrefix;

     private CacheControl m_CacheControl = CacheControl.PublicMaxAgeSec();

     /// <summary>
     /// Specifies local root path
     /// </summary>
     public string RootPath
     {
        get {return m_RootPath.IsNullOrWhiteSpace() ? INVALID_ROOT : m_RootPath;}
        set {m_RootPath = value;}
     }

     /// <summary>
     /// Specifies whether the handler generates simple 404 text or throws
     /// </summary>
     public bool Throw
     {
        get {return m_Throw;}
        set {m_Throw = value;}
     }

     /// <summary>
     /// When true, downloads files from PortalHub.ContentFileSystem for selected portal
     /// </summary>
     public bool UsePortalHub
     {
       get { return m_UsePortalHub;}
       set { m_UsePortalHub = value;}
     }

     public CacheControl CacheControl
     {
       get { return m_CacheControl; }
       set { m_CacheControl = value; }
     }

     /// <summary>
     /// When set indicates the case-insensitive prefix of a path segment that should be ignored by the file system.
     /// Version prefixes are used for attaching a surrogate path "folder" that makes resource differ based on their content.
     /// For example when prefix is "@",  path '/static/img/@767868768768/picture.png' resolves to actual '/static/img/picture.png'
     /// </summary>
     public string VersionSegmentPrefix
     {
       get { return m_VersionSegmentPrefix;}
       set { m_VersionSegmentPrefix = value;}
     }



     //Cut the surrogate out of path, i.e. '/static/img/@@767868768768/picture.png' -> '/static/img/picture.png'
     internal static string CutVersionSegment(string path, string pfxVer)
     {
       if (path.IsNotNullOrWhiteSpace() && pfxVer.IsNotNullOrWhiteSpace())
       {
         var i = -1;
         var eatTail = 0;
         if (path.Trim().IndexOf(pfxVer, StringComparison.OrdinalIgnoreCase)==0)
         {
          i=0;
          eatTail = 1;
         }
         else
          i = path.IndexOf('/'+pfxVer, StringComparison.OrdinalIgnoreCase);

         if (i<0) i = path.IndexOf('\\'+pfxVer, StringComparison.OrdinalIgnoreCase);

         if (i>=0 && i<path.Length-2)
         {
           var j = path.IndexOf('/', i+1);
           if (j<0) j = path.IndexOf('\\', i+1);
           if (j>i && j<path.Length-1)
           path = path.Substring(0, i) + path.Substring(j+eatTail);
         }
       }

       return path;
     }


     protected override void DoHandleWork(WorkContext work)
     {
         var fp         = work.MatchedVars[VAR_FILE_PATH].AsString("none");
         var attachment = work.MatchedVars[VAR_ATTACHMENT].AsBool(true);
         var chunked    = work.MatchedVars[VAR_CHUNKED].AsBool(true);

         //Sanitize
         fp = fp.Replace("..", string.Empty)
                .Replace(":/", string.Empty)
                .Replace(@"\\", @"\");

         //Cut the surrogate out of path, i.e. '/static/img/@@767868768768/picture.png' -> '/static/img/picture.png'
         fp = CutVersionSegment(fp, m_VersionSegmentPrefix);


         string fileName = null;
         IFileSystem fs = null;
         FileSystemSession fsSession = null;
         FileSystemFile fsFile = null;
         bool exists = false;

         if (m_UsePortalHub)
         {
            var hub = App.GetPortalHub();
            fs = hub.ContentFileSystem;
            fileName = m_RootPath!=null ? fs.CombinePaths(hub.ContentFileSystemRootPath, m_RootPath, fp)
                                        : fs.CombinePaths(hub.ContentFileSystemRootPath, fp);
            fsSession = fs.StartSession(hub.ContentFileSystemConnectParams);
            fsFile = fsSession[fileName] as FileSystemFile;
            exists = fsFile!=null;
         }
         else
         {
            fileName = Path.Combine(RootPath, fp);
            exists = File.Exists(fileName);
         }

         try
         {
             if (!exists)
             {
               var text = StringConsts.FILE_DL_HANDLER_NOT_FOUND_INFO.Args(fileName);
               if (m_Throw)
                throw new HTTPStatusException(WebConsts.STATUS_404, WebConsts.STATUS_404_DESCRIPTION, text);

               work.Response.ContentType = ContentType.TEXT;
               work.Response.Write( text );
               work.Response.StatusCode = WebConsts.STATUS_404;
               work.Response.StatusDescription = WebConsts.STATUS_404_DESCRIPTION;
               return;
             }

             if (!work.Response.WasWrittenTo)
               work.Response.Buffered = !chunked;

             work.Response.SetCacheControlHeaders(CacheControl);

             if (fsFile==null)
               work.Response.WriteFile(fileName, attachment: attachment);
             else
             {
               var ext = Path.GetExtension(fsFile.Name);
               work.Response.ContentType = App.GetContentTypeMappings().MapFileExtension(ext).ContentType;
               work.Response.WriteStream(fsFile.FileStream, attachmentName: attachment ? Path.GetFileName(fileName) : null);
             }
         }
         finally
         {
           DisposableObject.DisposeAndNull(ref fsSession);
         }
     }
  }
}
