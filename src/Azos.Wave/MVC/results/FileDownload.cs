/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Represents MVC action result that downloads a local file
  /// </summary>
  public struct FileDownload : IActionResult
  {
    public FileDownload(string fileName, bool isAttachment = false, int bufferSize = Response.DEFAULT_DOWNLOAD_BUFFER_SIZE)
    {
      LocalFileName = fileName;
      IsAttachment = isAttachment;
      BufferSize = bufferSize;
    }


    /// <summary>
    /// Local file name
    /// </summary>
    public readonly string LocalFileName;

    /// <summary>
    /// Download buffer size. Leave unchanged in most cases
    /// </summary>
    public readonly int    BufferSize;

    /// <summary>
    /// When true, asks user to save as attachment
    /// </summary>
    public readonly bool   IsAttachment;



    public void Execute(Controller controller, WorkContext work)
    {
      work.Response.WriteFile(LocalFileName, BufferSize, IsAttachment);
    }

  }



}
