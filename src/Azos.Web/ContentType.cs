/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

using Azos.Conf;
using Azos.Collections;
using Azos.Serialization.JSON;

namespace Azos.Web
{
  /// <summary>
  /// Declares web-related/mime content types. Content type mappings get loaded from application CONFIG_CONTENT_TYPES_SECTION
  /// </summary>
  /// <remarks>
  /// See: https://github.com/Microsoft/referencesource/blob/master/System.Web/MimeMapping.cs
  /// Config example:
  /// <code>
  /// app
  /// {
  ///   web-settings
  ///   {
  ///     content-type-mappings
  ///     {
  ///       map
  ///       {
  ///         extensions="jpg;jfif;jpeg"
  ///         content-type="image/jpeg"
  ///         binary=true
  ///         image=true
  ///         name{eng{n='Jpeg' d='Jpeg Image File'} deu{n='Jpeg' d='Jpeg Built File'}}
  ///         meta{} //ad-hoc metadata
  ///       }
  ///
  ///       map
  ///       {
  ///         extensions="htm;html;ht"
  ///         content-type="text/html"
  ///         alt-content-type="html"
  ///         binary=false
  ///         image=false
  ///         name{eng{n='HTML' d='Hypertext Markup Language'}}
  ///         meta{} //ad-hoc metadata
  ///       }
  ///     }
  ///   }
  /// }
  /// </code>
  /// </remarks>
  public static class ContentType
  {
    public const string CONFIG_CONTENT_TYPE_MAPPINGS_SECTION = "content-type-mappings";
    public const string CONFIG_MAP_SECTION = "map";
    public const string CONFIG_EXTENSIONS_ATTR = "extensions";
    public const string CONFIG_CONTENT_TYPE_ATTR = "content-type";
    public const string CONFIG_ALT_CONTENT_TYPE_ATTR = "alt-content-type";

    private static readonly string[] SPLITS = new []{";","|"};

    /// <summary>
    /// Describes content type mappings
    /// </summary>
    public sealed class Mappings
    {
      internal Mappings(IApplication app)
      {
        m_App = app;
        Refresh();
      }

      private readonly IApplication m_App;

      private Dictionary<string, Mapping> m_ExtToMapping;

      /// <summary>
      /// Rebuilds mappings from config
      /// </summary>
      public void Refresh()
      {

      }

    }

    /// <summary>
    /// Provides content type file information
    /// </summary>
    public sealed class Mapping
    {
      internal Mapping(IConfigSectionNode node)
      {
        node.NonNull(nameof(node));

      }

      private string[] m_Extensions;
      private string m_ContentType;
      private string m_AltContentType;
      private bool m_Binary;
      private bool m_Image;
      private NLSMap m_Name;
      public IConfigSectionNode m_Metadata;

      public IEnumerable<string> FileExtensions => m_Extensions;
      public string ContentTypes => m_ContentType;
      public string AltContentType => m_AltContentType;
      public bool IsBinary => m_Binary;
      public bool IsText => !m_Binary;
      public bool IsImage => m_Image;
      public NLSMap Name => m_Name;
      public IConfigSectionNode Metadata => m_Metadata;

    }


    /// <summary>
    /// Returns the singleton instance of content type Mappings object for the application
    /// </summary>
    public static Mappings GetContentTypeMappings(this IApplication app) =>
      app.NonNull(nameof(app)).Singletons.GetOrCreate<Mappings>(() => new Mappings(app)).instance;


    public const string TEXT = "text/plain";
    public const string HTML = "text/html";
    public const string CSS = "text/css";
    public const string JS = "application/x-javascript";

    /// <summary>
    /// Server-Sent Event Stream
    /// </summary>
    public const string SSE = "text/event-stream";


    public const string XML_TEXT = "text/xml";
    public const string XML_APP = "application/xml";

    public const string PDF = "application/pdf";
    public const string BINARY = "application/octet-stream";
    public const string EXE = BINARY;

    public const string GIF = "image/gif";
    public const string JPEG = "image/jpeg";
    public const string PNG = "image/png";
    public const string ICO = "image/x-icon";
    public const string SVG = "image/svg+xml";
    public const string BMP = "image/bmp";

    public const string JSON = "application/json";


    public const string TTF  = "application/font-sfnt";
    public const string EOT  = "application/vnd.ms-fontobject";
    public const string OTF  = "application/font-sfnt";
    public const string WOFF = "application/font-woff";




    public const string FORM_URL_ENCODED = "application/x-www-form-urlencoded";

    public const string FORM_MULTIPART_ENCODED = "multipart/form-data";
    public const string FORM_MULTIPART_ENCODED_BOUNDARY = "multipart/form-data; boundary={0}";

    public static readonly string[] IMAGES = new []{GIF, JPEG, PNG, ICO, SVG, BMP };


    /// <summary>
    /// Returns true if the content type is one of image format types
    /// </summary>
    public static bool IsImage(string ctype)
     => ctype.Default().Trim().IsOneOf(IMAGES);


    public static string ExtensionToContentType(string ext, string dflt = ContentType.HTML)
    {
      //todo Make this list configurable and use dictionary of default values instead of switch/case
      if (ext==null) return dflt;

      ext = ext.ToLowerInvariant().Trim();

      if (ext.StartsWith(".")) ext = ext.Remove(0,1);

      switch(ext)
      {
        case "htm":
        case "html": return ContentType.HTML;

        case "json": return ContentType.JSON;

        case "xml": return ContentType.XML_TEXT;

        case "css": return ContentType.CSS;

        case "js": return ContentType.JS;

        case "gif": return ContentType.GIF;

        case "jpe":
        case "jpg":
        case "jpeg": return ContentType.JPEG;

        case "png": return ContentType.PNG;

        case "bmp": return ContentType.BMP;

        case "ico": return ContentType.ICO;

        case "svg":
        case "svgz": return ContentType.SVG;

        case "pdf": return ContentType.PDF;

        case "exe": return ContentType.EXE;

        case "txt": return ContentType.TEXT;

        case "ttf": return ContentType.TTF;
        case "eot": return ContentType.EOT;
        case "otf": return ContentType.OTF;
        case "woff": return ContentType.WOFF;


        default: return dflt;
      }
    }
  }
}
