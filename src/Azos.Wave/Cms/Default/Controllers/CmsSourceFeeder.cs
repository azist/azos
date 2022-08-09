/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Wave.Mvc;

namespace Azos.Wave.Cms.Default.Controllers
{
  [ApiControllerDoc(
    BaseUri = "/content-source",
    Authentication = "pub",
    Description = "Provides internal content feed for ICmsSource web implementation")]
  public sealed class CmsSourceFeeder : Controller
  {
    public const string CTP_COMPRESSED = "azos/wave-cms-compressed";
    public const string CTP_UNCOMPRESSED = "azos/wave-cms";

    [InjectModule] ICmsFacade m_Cms;

    [ApiEndpointDoc(
      Methods = new []{"GET: gets for all portals all languages"},
      Description = "Fetches all languages for all portals",
      ResponseContent = "JSON {OK: true, data_dictionary({portal: [lang-info]}))}")]
    [ActionOnGet(Name = "languages")]
    public object Languages()
    {
      var allPortals = m_Cms.GetAllPortalsAsync().GetAwaiter().GetResult();
      var data = allPortals.ToDictionary(
                  p => p,
                  p => m_Cms.GetAllSupportedLanguagesAsync(p).GetAwaiter().GetResult()
                            .Select(one => new {iso = one.ISO, name = one.Name})
                            .ToArray());
      return new {OK = true, data};
    }

    [ApiEndpointDoc(
      Methods = new []{"GET: gets `Content` binary body"},
      Description = "Fetches all languages for all portals",
      ResponseContent = "Binary content representation",
      RequestQueryParameters = new []{"portal: Portal id",
                                      "ns: Namespace id within a portal",
                                      "block: content Block id within portal namespace",
                                      "isoLang: ISO language code",
                                      "nocache: true to bypass cache and fetch from upstream, false is default",
                                      "buffered: pass true to use double buffering with `Content-Length` header, otherwise(default) chunked transfer is used"})]
    [ActionOnGet(Name = "feed")]
    public async Task Feed(string portal, string ns, string block, Atom isolang, bool nocache = false, bool buffered = false)
    {
      ContentId id = default(ContentId);
      try
      {
        id = new ContentId(portal, ns, block);
      }
      catch(Exception error)
      {
        throw HTTPStatusException.BadRequest_400(error.Message);
      }

      var content = m_Cms.GetContentAsync(id, isolang, nocache.NoOrDefaultCache())
                         .GetAwaiter().GetResult();

      if (content == null)
      {
        throw HTTPStatusException.NotFound_404(id.ToString());
      }

      var compressionThreshold = App.ConfigRoot[SysConsts.CONFIG_WAVE_SECTION]
                                    .Of("cms-source-feeder-compressor-threshold-bytes").ValueAsInt(8000);

      var compress = (content.StringContent != null && content.StringContent.Length > compressionThreshold) ||
                     (content.BinaryContent != null && content.BinaryContent.Length > compressionThreshold);

      WorkContext.Response.Buffered = buffered;
      WorkContext.Response.StatusCode = 200;
      WorkContext.Response.StatusDescription = "Found CMS content";
      WorkContext.Response.ContentType = compress ? CTP_COMPRESSED : CTP_UNCOMPRESSED;
      var httpStream = await WorkContext.Response.GetDirectOutputStreamForWritingAsync().ConfigureAwait(false);
      if (compress)
      {
        using(var zipStream = new GZipStream(httpStream, CompressionLevel.Optimal))
        {
          content.WriteToStream(zipStream);
        }
      }
      else
      {
        content.WriteToStream(httpStream);
      }
    }
  }
}
