/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Linq;

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
    public const string CONFIG_META_SECTION = "meta";
    public const string CONFIG_NAME_SECTION = "name";

    private static readonly string[] SPLITS = new []{",",";","|"};

    public static readonly IConfigSectionNode DEFAULT_CONFIG= @"
content-type-mappings
{
  map
  {
    extensions='txt'
    content-type='text/plain'
    name{eng{n='Text' d='Plain Text'}}
  }

  map
  {
    extensions='htm,html'
    content-type='text/html'
    name{eng{n='HTML' d='Hypertext Markup Language'}}
  }

  map
  {
    extensions='css'
    content-type='text/css'
    name{eng{n='CSS' d='Cascading Style Sheets'}}
  }

  map
  {
    extensions='xml'
    content-type='text/xml'
    name{eng{n='XML' d='Extensible Markup Language'}}
  }

  map
  {
    extensions='xsl,xslt,xsf,xsd'
    content-type='text/xml'
    name{eng{n='XML Def ' d='XML Related Definitions'}}
  }

  map
  {
    extensions='json'
    content-type='application/json'
    name{eng{n='JSON' d='Java Script Object Notation'}}
  }

  map
  {
    extensions='pdf'
    content-type='application/pdf'
    binary=true
    name{eng{n='PDF' d='PDF Format'}}
  }

  map
  {
    extensions='doc,dot'
    content-type='application/msword'
    binary=true
    name{eng{n='Word' d='MS Word Documents'}}
  }

  map
  {
    extensions='docx'
    content-type='application/vnd.openxmlformats-officedocument.wordprocessingml.document'
    binary=true
    name{eng{n='Word' d='MS Word Documents'}}
  }

  map
  {
    extensions='dotx'
    content-type='application/vnd.openxmlformats-officedocument.wordprocessingml.template'
    binary=true
    name{eng{n='Word' d='MS Word Template'}}
  }

  map
  {
    extensions='xls,xlt'
    content-type='application/vnd.ms-excel'
    binary=true
    name{eng{n='Excel' d='MS Excel'}}
  }

  map
  {
    extensions='xlsx'
    content-type='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    binary=true
    name{eng{n='Excel' d='MS Excel Document'}}
  }

  map
  {
    extensions='xltx'
    content-type='application/vnd.openxmlformats-officedocument.spreadsheetml.template'
    binary=true
    name{eng{n='Excel' d='MS Excel Template'}}
  }

  map
  {
    extensions='bin,dump,data'
    content-type='application/octet-stream'
    binary=true
    name{eng{n='Binary' d='Binary Data'}}
  }

  //Exes are not mapped for security purposes
  //////map
  //////{
  //////  extensions='exe'
  //////  content-type='application/octet-stream'
  //////  binary=true
  //////  name{eng{n='Exe' d='Windows Executable File'}}
  //////}
  

  map
  {
    extensions='jpg,jpe,jpeg,jfif'
    content-type='image/jpeg'
    alt-content-type='image/jpg'
    binary=true image=true
    name{eng{n='Jpeg' d='Jpeg Image File'} rus{n='Jpeg' d='Формат файла изображения JPEG'}}
  }

  map
  {
    extensions='gif'
    content-type='image/git'
    binary=true image=true
    name{eng{n='GIF' d='Graphics Interchange Format'}}
  }

  map
  {
    extensions='png'
    content-type='image/png'
    binary=true image=true
    name{eng{n='PNG' d='Portable Network Graphic'}}
  }

  map
  {
    extensions='bmp'
    content-type='image/bmp'
    binary=true image=true
    name{eng{n='BMP' d='Bitmap Image'}}
  }

  map
  {
    extensions='ico'
    content-type='image/x-icon'
    binary=true image=true
    name{eng{n='ICO' d='Icon Image'}}
  }

  map
  {
    extensions='svg,svgz'
    content-type='image/svg+xml'
    binary=true image=true
    name{eng{n='SVG' d='Scalable Vector Graphics'}}
  }

  map
  {
    extensions='ttf'
    content-type='application/x-font-ttf'
    binary=true
    name{eng{n='TTF' d='True Type Font'}}
  }

  map
  {
    extensions='eot'
    content-type='application/vnd.ms-fontobject'
    binary=true
    name{eng{n='TTF' d='True Type Font'}}
  }

  map
  {
    extensions='woff'
    content-type='application/font-woff'
    binary=true
    name{eng{n='TTF' d='True Type Font'}}
  }

  map
  {
    extensions='otf'
    content-type='application/octet-stream'
    binary=true
    name{eng{n='OTF' d='Open Type Font'}}
  }

  map
  {
    extensions='z'
    content-type='application/x-compress'
    binary=true
    name{eng{n='Z' d='Compressed file'}}
  }

  map
  {
    extensions='zip'
    content-type='application/x-zip-compressed'
    binary=true
    name{eng{n='ZIP' d='Zip Compressed file'}}
  }

  map
  {
    extensions='mp3'
    content-type='audio/mpeg'
    binary=true 
    name{eng{n='MP3' d='MPEG Audio'}}
  }

  map
  {
    extensions='mpg,mpeg,mp2,mpa,mpe,mpv2'
    content-type='video/mpeg'
    binary=true 
    name{eng{n='MPG' d='MPEG Video'}}
  }

  map
  {
    extensions='avi'
    content-type='video/x-msvideo'
    binary=true 
    name{eng{n='AVI' d='AVI COntainer'}}
  }


}".AsLaconicConfig();



    /// <summary>
    /// Describes content type mappings
    /// </summary>
    public sealed class Mappings
    {
      internal Mappings(IApplication app)
      {
        App = app;
        Refresh();
      }

      /// <summary>
      /// References IApplication which these mappings are for
      /// </summary>
      public readonly IApplication App;

      private Dictionary<string, Mapping> m_ExtToMapping;


      /// <summary>
      /// Returns mapping for the specified file extension or null if not found.
      /// The file extension may be supplied with or without the leading dot
      /// </summary>
      public Mapping this[string fileExtension]
      {
        get
        {
          fileExtension = prepExtension(fileExtension);
          if (fileExtension==null) return null;
          if (m_ExtToMapping.TryGetValue(fileExtension, out var mapping)) return mapping;
          return null;
        }
      }

      /// <summary>
      /// Returns all mappings
      /// </summary>
      public IEnumerable<Mapping> All => m_ExtToMapping.Values;

      /// <summary>
      /// Rebuilds mappings from application config. The mappings are under '/web-settings/content-type-mappings' node.
      /// This method is thread safe.
      /// </summary>
      public void Refresh()
      {
        var data = new Dictionary<string, Mapping>();

        //1 - build from default config
        build(data, DEFAULT_CONFIG);

        //2 - build/overwrite from user config
        var rootNode = App.ConfigRoot[WebSettings.CONFIG_WEBSETTINGS_SECTION][CONFIG_CONTENT_TYPE_MAPPINGS_SECTION];
        build(data, rootNode);

        m_ExtToMapping = data;//atomic
      }

      private void build(Dictionary<string, Mapping> data, IConfigSectionNode rootNode)
      {
        foreach (var cnode in rootNode.Children.Where(c => c.IsSameName(CONFIG_MAP_SECTION)))
        {
          var mapping = Mapping.TryBuild(App, cnode);
          if (mapping == null) continue;

          //Build the index
          foreach (var fext in mapping.FileExtensions)
            data[fext] = mapping;//in case of duplication, overwrites with latest value
        }
      }

      internal static string prepExtension(string ext)
      {
        while(true)
        {
          if (ext.IsNullOrWhiteSpace()) return null;
          ext = ext.Trim();
          if (ext[0] == '.')
            ext = ext.Substring(1);
          else
            return ext;
        }
      }
    }//class Mappings

    /// <summary>
    /// Provides content type file information
    /// </summary>
    public sealed class Mapping
    {
      internal static Mapping TryBuild(IApplication app, IConfigSectionNode node)
      {
        var strexts = node.AttrByName(CONFIG_EXTENSIONS_ATTR).Value;
        if (strexts.IsNullOrWhiteSpace())
        {
          app.Log.Write(new Log.Message
          {
            Type = Log.MessageType.Warning,
            Topic = CoreConsts.WEB_TOPIC,
            From = $"{typeof(Mapping).FullName}{nameof(Mapping.TryBuild)}",
            Text = $"{node.RootPath}/${CONFIG_EXTENSIONS_ATTR}.IsNullOrWhiteSpace"
          });
          return null;
        }

        var exts = strexts.Split(SPLITS, System.StringSplitOptions.RemoveEmptyEntries)
                          .Select( e => Mappings.prepExtension(e))
                          .Where( e => e.IsNotNullOrWhiteSpace())
                          .ToArray();

        if (exts.Length==0)
        {
          app.Log.Write(new Log.Message
          {
            Type = Log.MessageType.Warning,
            Topic = CoreConsts.WEB_TOPIC,
            From = $"{typeof(Mapping).FullName}{nameof(Mapping.TryBuild)}",
            Text = $"{node.RootPath}/${CONFIG_EXTENSIONS_ATTR} no extensions"
          });
          return null;
        }

        var ctyp = node.AttrByName(CONFIG_CONTENT_TYPE_ATTR).Value;
        if (ctyp.IsNullOrWhiteSpace())
        {
          app.Log.Write(new Log.Message
          {
            Type = Log.MessageType.Warning,
            Topic = CoreConsts.WEB_TOPIC,
            From = $"{typeof(Mapping).FullName}{nameof(Mapping.TryBuild)}",
            Text = $"{node.RootPath}/${CONFIG_CONTENT_TYPE_ATTR}.IsNullOrWhiteSpace"
          });
          return null;
        }

        var result = new Mapping();
        result.m_Extensions = exts;
        result.m_ContentType = ctyp;
        ConfigAttribute.Apply(result, node);
        result.m_Name = new NLSMap(node[CONFIG_NAME_SECTION]);

        var mnode = node[CONFIG_META_SECTION];
        var cfg = new MemoryConfiguration();
        if (mnode.Exists)
        {
          cfg.CreateFromNode(mnode);
          result.m_Metadata = cfg.Root;
        }
        else
        {
          result.m_Metadata = cfg.EmptySection;
        }

        cfg.SetReadOnly(true);


        return result;
      }

      private string[] m_Extensions;
      private string m_ContentType;
      [Config] private string m_AltContentType;
      [Config] private bool m_Binary;
      [Config] private bool m_Image;
      private NLSMap m_Name;
      private IConfigSectionNode m_Metadata;

      public IEnumerable<string> FileExtensions => m_Extensions;
      public string              ContentType    => m_ContentType;
      public string              AltContentType => m_AltContentType;
      public bool                IsBinary       => m_Binary;
      public bool                IsText         => !m_Binary;
      public bool                IsImage        => m_Image;
      public NLSMap              Name           => m_Name;
      public IConfigSectionNode  Metadata       => m_Metadata;

      public override string ToString() => $"{string.Join(", ", m_Extensions)} -> {m_ContentType}";
    }


    /// <summary>
    /// Returns the singleton instance of content type Mappings object for the application
    /// </summary>
    public static Mappings GetContentTypeMappings(this IApplication app) =>
      app.NonNull(nameof(app)).Singletons.GetOrCreate(() => new Mappings(app)).instance;


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
