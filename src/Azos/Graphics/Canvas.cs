/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Azos.Platform.Abstraction;
using Azos.Platform.Abstraction.Graphics;

namespace Azos.Graphics
{
  /// <summary>
  /// Represents a 2d drawing surface
  /// </summary>
  public sealed partial class Canvas : DisposableObject
  {
    /// <summary>
    /// Returns true when all assets (such as brushes, pens, fonts) are owned by the canvas instance and
    /// they can not be used/persisted beyond the scope of this canvas lifecycle.
    /// Some libraries allow to create objects and cache them for subsequent use, whereas others (e.g. Windows classic GDI)
    /// mandate that all graphics handles belong to the particular canvas instance and get invalidated when that instance
    /// gets releases via Dispose()
    /// </summary>
    public static bool OwnsAssets =>  PlatformAbstractionLayer.Graphics.CanvasOwnsAssets;


    internal Canvas(IPALCanvas handle){ m_Handle = handle;}

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Handle);
    }

    private IPALCanvas m_Handle;

    /// <summary>
    /// Defines how pixel colors are approximated/interpolated during image resize, rotate and other operations
    /// which distort 1:1 pixel mappings
    /// </summary>
    public InterpolationMode Interpolation
    {
      get { EnsureObjectNotDisposed(); return m_Handle.Interpolation;  }
      set { EnsureObjectNotDisposed(); m_Handle.Interpolation = value; }
    }


    /// <summary>Draws image of the original size at the specified location </summary>
    public void DrawImageUnscaled(Image image, int x, int y) => DrawImageUnscaled(image, new Point(x, y));
    /// <summary>Draws image of the original size at the specified location </summary>
    public void DrawImageUnscaled(Image image, Point p)
    {
      EnsureObjectNotDisposed();
      m_Handle.DrawImageUnscaled(image.Handle, p);
    }

    /// <summary>Draws image of the original size at the specified location </summary>
    public void DrawImageUnscaled(Image image, float x, float y) => DrawImageUnscaled(image, new PointF(x, y));
    /// <summary>Draws image of the original size at the specified location </summary>
    public void DrawImageUnscaled(Image image, PointF p)
    {
      EnsureObjectNotDisposed();
      m_Handle.DrawImageUnscaled(image.Handle, p);
    }

                  private void checkDrawImage(Image image)
                  {
                    if (image==null)
                      throw new GraphicsException(StringConsts.ARGUMENT_ERROR+"{0}.{1}(image=null)".Args(nameof(Canvas), nameof(DrawImage)));
                    EnsureObjectNotDisposed();
                  }

    public void DrawImage(Image image, int x, int y, int w, int h) => DrawImage(image, new Rectangle(x, y, w, h));
    public void DrawImage(Image image, Rectangle rect)
    {
      checkDrawImage(image);
      m_Handle.DrawImage(image.Handle, rect);
    }

    public void DrawImage(Image image, float x, float y, float w, float h) => DrawImage(image, new RectangleF(x, y, w, h));
    public void DrawImage(Image image, RectangleF rect)
    {
      checkDrawImage(image);
      m_Handle.DrawImage(image.Handle, rect);
    }

    public void DrawImage(Image image, Rectangle src, Rectangle dest)
    {
      checkDrawImage(image);
      m_Handle.DrawImage(image.Handle, src, dest);
    }

    public void DrawImage(Image image, RectangleF src, RectangleF dest)
    {
      checkDrawImage(image);
      m_Handle.DrawImage(image.Handle, src, dest);
    }

    public void Clear(Color color)
    {
      EnsureObjectNotDisposed();
      m_Handle.Clear(color);
    }

                  private void checkFillRectangle(Brush brush)
                  {
                    if (brush==null)
                    throw new GraphicsException(StringConsts.ARGUMENT_ERROR+"{0}.{1}(brush=null)".Args(nameof(Canvas), nameof(FillRectangle)));
                    EnsureObjectNotDisposed();
                  }

    public void FillRectangle(Canvas.Brush brush, int x, int y, int w, int h) => FillRectangle(brush, new Rectangle(x, y, w, h));
    public void FillRectangle(Canvas.Brush brush, Point p, Size s) => FillRectangle(brush, new Rectangle(p, s));
    public void FillRectangle(Canvas.Brush brush, Rectangle rect)
    {
      checkFillRectangle(brush);
      m_Handle.FillRectangle(brush.Handle, rect);
    }

    public void FillRectangle(Canvas.Brush brush, float x, float y, float w, float h) => FillRectangle(brush, new RectangleF(x, y, w, h));
    public void FillRectangle(Canvas.Brush brush, PointF p, SizeF s) => FillRectangle(brush, new RectangleF(p, s));
    public void FillRectangle(Canvas.Brush brush, RectangleF rect)
    {
      checkFillRectangle(brush);
      m_Handle.FillRectangle(brush.Handle, rect);
    }


                  private void checkDrawRectangle(Pen pen)
                  {
                    if (pen==null)
                    throw new GraphicsException(StringConsts.ARGUMENT_ERROR+"{0}.{1}(pen=null)".Args(nameof(Canvas), nameof(DrawRectangle)));
                    EnsureObjectNotDisposed();
                  }


    public void DrawRectangle(Canvas.Pen pen, int x, int y, int w, int h) => DrawRectangle(pen, new Rectangle(x, y, w, h));
    public void DrawRectangle(Canvas.Pen pen, Rectangle rect)
    {
      checkDrawRectangle(pen);
      m_Handle.DrawRectangle(pen.Handle, rect);
    }

    public void DrawRectangle(Canvas.Pen pen, float x, float y, float w, float h) => DrawRectangle(pen, new RectangleF(x, y, w, h));
    public void DrawRectangle(Canvas.Pen pen, RectangleF rect)
    {
      checkDrawRectangle(pen);
      m_Handle.DrawRectangle(pen.Handle, rect);
    }


                  private void checkDrawLine(Pen pen)
                  {
                    if (pen==null)
                      throw new GraphicsException(StringConsts.ARGUMENT_ERROR+"{0}.{1}(pen=null)".Args(nameof(Canvas), nameof(DrawLine)));
                    EnsureObjectNotDisposed();
                  }


    public void DrawLine(Canvas.Pen pen, int x1, int y1, int x2, int y2) => DrawLine(pen, new Point(x1, y1), new Point(x2, y2));
    public void DrawLine(Canvas.Pen pen, Point p1, Point p2)
    {
      checkDrawLine(pen);
      m_Handle.DrawLine(pen.Handle, p1, p2);
    }

    public void DrawLine(Canvas.Pen pen, float x1, float y1, float x2, float y2) => DrawLine(pen, new PointF(x1, y1), new PointF(x2, y2));
    public void DrawLine(Canvas.Pen pen, PointF p1, PointF p2)
    {
      checkDrawLine(pen);
      m_Handle.DrawLine(pen.Handle, p1, p2);
    }
                  private void checkDrawEllipse(Pen pen)
                  {
                    if (pen==null)
                      throw new GraphicsException(StringConsts.ARGUMENT_ERROR+"{0}.{1}(pen=null)".Args(nameof(Canvas), nameof(DrawEllipse)));
                    EnsureObjectNotDisposed();
                  }

    public void DrawEllipse(Canvas.Pen pen, int x, int y, int w, int h) => DrawEllipse(pen, new Rectangle(x, y, w, h));
    public void DrawEllipse(Canvas.Pen pen, Rectangle rect)
    {
      checkDrawEllipse(pen);
      m_Handle.DrawEllipse(pen.Handle, rect);
    }

    public void DrawEllipse(Canvas.Pen pen, float x, float y, float w, float h) => DrawEllipse(pen, new RectangleF(x, y, w, h));
    public void DrawEllipse(Canvas.Pen pen, RectangleF rect)
    {
      checkDrawEllipse(pen);
      m_Handle.DrawEllipse(pen.Handle, rect);
    }


    /// <summary>
    /// Measures the size of text rendered in the font within the optional bounds
    /// </summary>
    public SizeF MeasureString(Canvas.Font font, string text, SizeF? bounds)
    {
      if (font==null || text==null)
       throw new GraphicsException(StringConsts.ARGUMENT_ERROR+"{0}.{1}(font=null)".Args(nameof(Canvas), nameof(MeasureString)));
      EnsureObjectNotDisposed();
      return m_Handle.MeasureString(font.Handle, text, bounds);
    }

    public void DrawString(Canvas.Font font, Canvas.Brush brush, string text, float x, float y) => DrawString(font, brush, text, new PointF(x, y));
    public void DrawString(Canvas.Font font, Canvas.Brush brush, string text, PointF p)
    {
      if (font==null || brush==null || text==null)
       throw new GraphicsException(StringConsts.ARGUMENT_ERROR+"{0}.{1}(font|brush|text=null)".Args(nameof(Canvas), nameof(DrawString)));
      EnsureObjectNotDisposed();
      m_Handle.DrawString(font.Handle, brush.Handle, text, p);
    }


    public Pen CreatePen(Color color, float width, PenDashStyle style)
    {
      EnsureObjectNotDisposed();
      var pen = m_Handle.CreatePen(color, width, style);
      return new Canvas.Pen(pen);
    }

    public Brush CreateSolidBrush(Color color)
    {
      EnsureObjectNotDisposed();
      var brush = m_Handle.CreateSolidBrush(color);
      return new Canvas.Brush(brush);
    }

    public Font CreateFont(string name, float size, FontStyling style, MeasureUnit unit)
    {
      EnsureObjectNotDisposed();
      var font = m_Handle.CreateFont(name, size, style, unit);
      return new Canvas.Font(font);
    }


  }
}
