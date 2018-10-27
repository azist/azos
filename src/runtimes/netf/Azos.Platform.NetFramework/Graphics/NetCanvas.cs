/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Drawing;
using NGR = System.Drawing.Graphics;

using Azos.Graphics;
using Azos.Platform.Abstraction.Graphics;

namespace Azos.Platform.Abstraction.NetFramework.Graphics
{
  /// <summary>
  /// Implements Canvas using .NET framework GDI+ Graphics object
  /// </summary>
  public sealed class NetCanvas : DisposableObject, IPALCanvas
  {


    internal NetCanvas(NGR graphics)
    {
      m_Graphics = graphics;
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Graphics);
    }

    private NGR m_Graphics;


    public InterpolationMode Interpolation
    {
      get => xlator.xlat(m_Graphics.InterpolationMode);
      set => m_Graphics.InterpolationMode = xlator.xlat(value);
    }

    public IPALCanvasPen CreatePen(Color color, float width, PenDashStyle style)
    {
      return new NetPen(color, width, style);
    }

    public IPALCanvasBrush CreateSolidBrush(Color color)
    {
      return new NetBrush(color);
    }

    public IPALCanvasFont CreateFont(string name, float size, FontStyling style, MeasureUnit unit)
    {
      return new NetFont(name, size, style, unit);
    }

    public void Clear(Color color) => m_Graphics.Clear(color);


    public void DrawRectangle(IPALCanvasPen pen, Rectangle rect) => m_Graphics.DrawRectangle(((NetPen)pen).GDIPen, rect);
    public void DrawRectangle(IPALCanvasPen pen, RectangleF rect) => m_Graphics.DrawRectangle(((NetPen)pen).GDIPen, rect.X, rect.Y, rect.Width, rect.Height);

    public void FillRectangle(IPALCanvasBrush brush, Rectangle rect) => m_Graphics.FillRectangle(((NetBrush)brush).GDIBrush, rect);
    public void FillRectangle(IPALCanvasBrush brush, RectangleF rect) => m_Graphics.FillRectangle(((NetBrush)brush).GDIBrush, rect);

    public void DrawLine(IPALCanvasPen pen, Point p1, Point p2) => m_Graphics.DrawLine(((NetPen)pen).GDIPen, p1, p2);
    public void DrawLine(IPALCanvasPen pen, PointF p1, PointF p2) => m_Graphics.DrawLine(((NetPen)pen).GDIPen, p1, p2);

    public void DrawEllipse(IPALCanvasPen pen, Rectangle rect) => m_Graphics.DrawEllipse(((NetPen)pen).GDIPen, rect);
    public void DrawEllipse(IPALCanvasPen pen, RectangleF rect) => m_Graphics.DrawEllipse(((NetPen)pen).GDIPen, rect);


    public void DrawImageUnscaled(IPALImage image, Point p) => m_Graphics.DrawImageUnscaled(((NetImage)image).Bitmap, p);
    //GDI does not expose DrawImageUnscaled with PointF, hence (int) cast below:
    public void DrawImageUnscaled(IPALImage image, PointF p) => m_Graphics.DrawImageUnscaled(((NetImage)image).Bitmap, new Point((int)p.X, (int)p.Y));

    public void DrawImage(IPALImage image, Rectangle rect) => m_Graphics.DrawImage(((NetImage)image).Bitmap, rect);
    public void DrawImage(IPALImage image, RectangleF rect) => m_Graphics.DrawImage(((NetImage)image).Bitmap, rect);
    public void DrawImage(IPALImage image, Rectangle src, Rectangle dest) => m_Graphics.DrawImage(((NetImage)image).Bitmap, src, dest, GraphicsUnit.Pixel);
    public void DrawImage(IPALImage image, RectangleF src, RectangleF dest) => m_Graphics.DrawImage(((NetImage)image).Bitmap, src, dest, GraphicsUnit.Pixel);

    public SizeF MeasureString(IPALCanvasFont font, string text, SizeF? bounds)
    {
      return  bounds.HasValue ?
         m_Graphics.MeasureString(text,((NetFont)font).GDIFont, bounds.Value) :
         m_Graphics.MeasureString(text,((NetFont)font).GDIFont);
    }

    public void DrawString(IPALCanvasFont font,  IPALCanvasBrush brush, string text, PointF p)
    {
      m_Graphics.DrawString(text,((NetFont)font).GDIFont, ((NetBrush)brush).GDIBrush, p);
    }
  }
}
