/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.IO;

using Azos.Graphics;
using Azos.Media.PDF.DocumentModel;

namespace Azos.Media.PDF.Elements
{
  /// <summary>
  /// PDF image element
  /// </summary>
  public class ImageElement : PdfElement, IPdfXObject
  {
    private const string XREFERENCE_FORMAT = "/I{0}";
    private const string FULL_IMAGE_REFERENCE = "/I{0} {0} 0 R";

    public ImageElement(string filePath)
    {
      loadImage(filePath);
    }

    public ImageElement(string filePath, float width, float height)
    {
      loadImage(filePath);

      Width = width;
      Height = height;
    }

    /// <summary>
    /// Image unique object Id
    /// </summary>
    public int XObjectId { get; set; }

    /// <summary>
    /// PDF displayed image width
    /// </summary>
    public float Width { get; private set; }

    /// <summary>
    /// PDF displayed image height
    /// </summary>
    public float Height { get; private set; }

    /// <summary>
    /// Image bytes
    /// </summary>
    public byte[] Content { get; private set; }

    /// <summary>
    /// Image's own widht
    /// </summary>
    public float OwnWidth { get; private set; }

    /// <summary>
    /// Image's own height
    /// </summary>
    public float OwnHeight { get; private set; }

    /// <summary>
    /// Returns image's bits per pixel
    /// </summary>
    public int BitsPerPixel { get; private set; }

    /// <summary>
    /// Returns PDF x-object indirect reference
    /// </summary>
    public string GetXReference()
    {
      return XREFERENCE_FORMAT.Args(XObjectId);
    }

    public string GetCoupledReference()
    {
      return FULL_IMAGE_REFERENCE.Args(XObjectId);
    }

    /// <summary>
    /// Writes element into file stream
    /// </summary>
    /// <param name="writer">PDF writer</param>
    /// <returns>Written bytes count</returns>
    public override void Write(PdfWriter writer)
    {
      writer.Write(this);
    }

    private void loadImage(string filePath)
    {
      using (var image = Image.FromFile(filePath))
        using (var stream = new MemoryStream())
        {
          var imgf = JpegImageFormat.Standard;
          image.Save(stream, imgf);
          Content = stream.ToArray();
          Width = image.Width;
          OwnWidth = image.Width;
          Height = image.Height;
          OwnHeight = image.Height;
          BitsPerPixel = 24;//todo - review, this was jpeg.BPP - but what is the point of jpeg.bpp?
        }
    }
  }
}
