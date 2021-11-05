/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
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

    [ActionOnGet(Name = "feed")]
    public void Feed(string portal, string ns, string block, Atom isolang, bool nocache = false)
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
                                    .Of("cms-source-feeder-compressor-threshold").ValueAsInt(8000);

      var compress = (content.StringContent != null && content.StringContent.Length > compressionThreshold) ||
                     (content.BinaryContent != null && content.BinaryContent.Length > compressionThreshold);

      WorkContext.Response.StatusCode = 200;
      WorkContext.Response.StatusDescription = "Found CMS content";
      WorkContext.Response.ContentType = compress ? CTP_COMPRESSED : CTP_UNCOMPRESSED;
      var http = WorkContext.Response.GetDirectOutputStreamForWriting();
      //todo handle compression
      content.WriteToStream(http);
    }
  }
}
