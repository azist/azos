/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Represents MVC action result that downloads binary content
  /// </summary>
  public struct BinaryContent : IActionResult
  {
    public BinaryContent(byte[] data, string contentType = null, string attachmentName = null, int bufferSize = Response.DEFAULT_DOWNLOAD_BUFFER_SIZE)
    {
      Data = data.NonNull(nameof(data));
      ContentType = contentType;
      AttachmentName = attachmentName;
      BufferSize = bufferSize;
    }


    /// <summary>
    /// Binary data to send
    /// </summary>
    public readonly byte[] Data;

    /// <summary>
    /// Content-type header for the binary data
    /// </summary>
    public readonly string ContentType;

    /// <summary>
    /// Download buffer size. Leave unchanged in most cases
    /// </summary>
    public readonly int BufferSize;

    /// <summary>
    /// When true, asks user to save as attachment
    /// </summary>
    public readonly string AttachmentName;


    public void Execute(Controller controller, WorkContext work)
    {
      var ctp = ContentType;
      if (ctp.IsNullOrWhiteSpace())
       ctp = Azos.Web.ContentType.BINARY;

      work.Response.ContentType = ctp;

      using (var ms = new System.IO.MemoryStream(Data))
        work.Response.WriteStream(ms, BufferSize,  AttachmentName);
    }

  }
}
