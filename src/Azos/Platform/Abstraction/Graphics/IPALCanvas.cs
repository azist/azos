/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.IO;
using System.Drawing;

using Azos.Graphics;

namespace Azos.Platform.Abstraction.Graphics
{

  public interface IPALCanvasAsset : IDisposable { }

  public interface IPALCanvasBrush : IPALCanvasAsset
  {
    Color Color{ get;}
  }

  public interface IPALCanvasPen   : IPALCanvasAsset
  {
    Color Color{ get;}
    float Width{ get;}
    PenDashStyle DashStyle{ get;}
  }

  public interface IPALCanvasFont  : IPALCanvasAsset
  {
    string Name{ get;}
    float Size { get;}
    FontStyling Style{ get;}
    MeasureUnit Unit{ get;}
  }

  public interface IPALCanvas : IDisposable
  {
    InterpolationMode Interpolation{ get; set;}


    IPALCanvasPen CreatePen(Color color, float width, PenDashStyle style);
    IPALCanvasBrush CreateSolidBrush(Color color);
    IPALCanvasFont CreateFont(string name, float size, FontStyling style, MeasureUnit unit);

    void Clear(Color color);
    void FillRectangle(IPALCanvasBrush brush, Rectangle rect);
    void FillRectangle(IPALCanvasBrush brush, RectangleF rect);

    void DrawRectangle(IPALCanvasPen pen, Rectangle rect);
    void DrawRectangle(IPALCanvasPen pen, RectangleF rect);

    void DrawLine(IPALCanvasPen pen, Point p1, Point p2);
    void DrawLine(IPALCanvasPen pen, PointF p1, PointF p2);

    void DrawEllipse(IPALCanvasPen pen, Rectangle rect);
    void DrawEllipse(IPALCanvasPen pen, RectangleF rect);

    void DrawImageUnscaled(IPALImage image, Point p);
    void DrawImageUnscaled(IPALImage image, PointF p);
    void DrawImage(IPALImage image, Rectangle rect);
    void DrawImage(IPALImage image, RectangleF rect);
    void DrawImage(IPALImage image, Rectangle src, Rectangle dest);
    void DrawImage(IPALImage image, RectangleF src, RectangleF dest);

    SizeF MeasureString(IPALCanvasFont font, string text, SizeF? bounds);
    void DrawString(IPALCanvasFont font,  IPALCanvasBrush brush, string text, PointF p);
  }
}
