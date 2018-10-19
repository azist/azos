using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;

using Azos.Graphics;
using AzosImageFormat = Azos.Graphics.ImageFormat;
using Azos.Platform.Abstraction.Graphics;

namespace Azos.Platform.Abstraction.NetFramework.Graphics
{
  /// <summary>
  /// Implements image using .NET framework GDI+ wrapper
  /// </summary>
  public sealed class NetImage : DisposableObject, IPALImage
  {

    internal NetImage(Size size, Size resolution, ImagePixelFormat pixFormat)
    {
      var pf = System.Drawing.Imaging.PixelFormat.Format32bppArgb; // xlator.xlat(pixFormat);  20180108 SPOL: PixelFormat hardcoded for .NET
      m_Bitmap = new Bitmap(size.Width, size.Height, pf);
      m_Bitmap.SetResolution(resolution.Width, resolution.Height);
    }

    public NetImage(System.Drawing.Image img)
    {
      var bmp = img as Bitmap;
      if (bmp==null)
        throw new NetFrameworkPALException(StringConsts.ARGUMENT_ERROR + $"{nameof(NetImage)}.ctor(img!bitmap)");
      m_Bitmap = bmp;

      m_LoadFormat = xlator.xlat( img.RawFormat );
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Bitmap);
    }

    private Bitmap m_Bitmap;
    private Azos.Graphics.ImageFormat m_LoadFormat;

    public Bitmap Bitmap => m_Bitmap;

    public ImagePixelFormat PixelFormat => xlator.xlat(m_Bitmap.PixelFormat);

    public Azos.Graphics.ImageFormat LoadFormat => m_LoadFormat;

    public Color GetPixel(Point p) => m_Bitmap.GetPixel(p.X, p.Y);
    public void SetPixel(Point p, Color color) => m_Bitmap.SetPixel(p.X, p.Y, color);

    public Color GetPixel(PointF p) => m_Bitmap.GetPixel((int)p.X, (int)p.Y);
    public void SetPixel(PointF p, Color color) => m_Bitmap.SetPixel((int)p.X, (int)p.Y, color);

    public int GetXResolution() => (int)m_Bitmap.HorizontalResolution;
    public int GetYResolution() => (int)m_Bitmap.VerticalResolution;
    public void SetResolution(int xDPI, int yDPI) => m_Bitmap.SetResolution(xDPI, yDPI);

    public Size GetSize() => m_Bitmap.Size;


    public void MakeTransparent(Color? dflt)
    {
      if (dflt.HasValue)
        m_Bitmap.MakeTransparent(dflt.Value);
      else
        m_Bitmap.MakeTransparent();
    }

    public IPALCanvas CreateCanvas()
    {
      var ngr = System.Drawing.Graphics.FromImage(m_Bitmap);
      return new NetCanvas(ngr);
    }

    public void Save(string fileName, AzosImageFormat format)
    {
      var (codec, pars) = getEncoder(format);

      using (var copy = makeSaveCopy())
        copy.Save(fileName, codec, pars);
    }

    public void Save(Stream stream, AzosImageFormat format)
    {
      var (codec, pars) = getEncoder(format);

      using (var copy = makeSaveCopy())
        copy.Save(stream, codec, pars);
    }

    public byte[] Save(AzosImageFormat format)
    {
      using(var ms = new MemoryStream())
      {
        this.Save(ms, format);
        return ms.ToArray();
      }
    }

    // This method is needed as a workaround MS GDI+ bug / inefficient behavior:
    // one can not save GDI image that was just loaded from somewhere else
    private Bitmap makeSaveCopy()
    {
      //more info:
      // http://blog.vishalon.net/bitmapsave-a-generic-error-occurred-in-gdi
      // https://social.msdn.microsoft.com/Forums/vstudio/en-US/b15357f1-ad9d-4c80-9ec1-92c786cca4e6/bitmapsave-a-generic-error-occurred-in-gdi?forum=netfxbcl
      return new Bitmap(m_Bitmap);
    }

    private (ImageCodecInfo codec, EncoderParameters pars) getEncoder(AzosImageFormat format)
    {
      ImageCodecInfo codec;
      EncoderParameters pars;

      if (format is BitmapImageFormat)
      {
        codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Bmp.Guid);
        pars = new EncoderParameters(1);
        pars.Param[0] = new EncoderParameter(Encoder.ColorDepth, format.Colors);
      } else if (format is PngImageFormat)
      {
        codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Png.Guid);
        pars = null;//new EncoderParameters(0);
      } else if (format is GifImageFormat)
      {
        codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Gif.Guid);
        pars = null;//new EncoderParameters(0);
      } else//default is JPEG
      {
        var jpeg = format as JpegImageFormat;
        codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
        pars = new EncoderParameters(1);
        pars.Param[0] = new EncoderParameter(Encoder.Quality, jpeg?.Quality ?? 80L);
      }


      return ( codec: codec, pars: pars );
    }
  }
}
