using System.IO;
using System.Drawing;

using Azos.Graphics;
using Azos.Platform.Abstraction.Graphics;

namespace Azos.Platform.Abstraction.NetFramework.Graphics
{
  public sealed class NetGraphics : IPALGraphics
  {
    /// <summary>
    /// This GDI+ implementation allows to create assets outside of GDI+.Graphics scope
    /// </summary>
    public bool CanvasOwnsAssets => false;

    public IPALImage CreateImage(string fileName)
    {
      var nimg = System.Drawing.Image.FromFile(fileName);
      return new NetImage(nimg);
    }

    public IPALImage CreateImage(byte[] data)
    {
      using(var ms = new MemoryStream(data))
      {
       var nimg = System.Drawing.Image.FromStream(ms);
       return new NetImage(nimg);
      }
    }

    public IPALImage CreateImage(Stream stream)
    {
      var nimg = System.Drawing.Image.FromStream(stream);
      return new NetImage(nimg);
    }

    public IPALImage CreateImage(Size size, Size resolution, ImagePixelFormat pixFormat)
    {
      return new NetImage(size, resolution, pixFormat);
    }

  }
}
