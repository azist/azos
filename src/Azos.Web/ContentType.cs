/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Web
{
    /// <summary>
    /// Declares web-related/mime content types
    /// </summary>
    public static class ContentType
    {
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

       public const string JSON = "application/json";


       public const string TTF  = "application/font-sfnt";
       public const string EOT  = "application/vnd.ms-fontobject";
       public const string OTF  = "application/font-sfnt";
       public const string WOFF = "application/font-woff";




       public const string FORM_URL_ENCODED = "application/x-www-form-urlencoded";

       public const string FORM_MULTIPART_ENCODED = "multipart/form-data";
       public const string FORM_MULTIPART_ENCODED_BOUNDARY = "multipart/form-data; boundary={0}";


       public static string ExtensionToContentType(string ext, string dflt = ContentType.HTML)
       {
         //todo Make this list configurable and use dictionary of default values instead of switch/case which is slower than dict lookup
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

                case "ico": return ContentType.ICO;

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
