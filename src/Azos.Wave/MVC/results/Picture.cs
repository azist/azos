/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;
using Azos.Graphics;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Represents MVC action result that returns/downloads an image
  /// </summary>
  public struct Picture : IActionResult, IDisposable
  {
    public Picture(Image image, ImageFormat format, string attachmentFileName = null)
    {
      m_Image = image;
      m_Format = format;
      m_AttachmentFileName = attachmentFileName;
    }

    private Image       m_Image;
    private ImageFormat m_Format;
    public  string      m_AttachmentFileName;

    /// <summary>
    /// Picture image
    /// </summary>
    public Image Image => m_Image;

    /// <summary>
    /// Download buffer size. Leave unchanged in most cases
    /// </summary>
    public ImageFormat  Format => m_Format;

    /// <summary>
    /// When non-null asks user to download picture as a named attached file
    /// </summary>
    public string  AttachmentFileName => m_AttachmentFileName;

    public async Task ExecuteAsync(Controller controller, WorkContext work)
    {
      if (m_Image==null) return;

      if (m_AttachmentFileName.IsNotNullOrWhiteSpace())
          work.Response.Headers.Add(WebConsts.HTTP_HDR_CONTENT_DISPOSITION, "attachment; filename={0}".Args(m_AttachmentFileName));

      work.Response.ContentType = Format.WebContentType;
      var stream = await work.Response.GetDirectOutputStreamForWritingAsync().ConfigureAwait(false);
      m_Image.Save(stream, Format);//todo: async
    }

    public void Dispose()
    {
      DisposableObject.DisposeAndNull(ref m_Image);
    }
  }
}
