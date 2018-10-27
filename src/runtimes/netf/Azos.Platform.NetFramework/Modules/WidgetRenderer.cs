/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Drawing;
using System.Drawing.Drawing2D;


using Azos.Apps;
using Azos.Security.CAPTCHA;

namespace Azos.Platform.Abstraction.NetFramework.Modules
{
  /// <summary>
  /// Performs various platform-specific rendering tasks (such as rendering of PuzzleKeypad)
  /// </summary>
  public sealed class WidgetRenderer : ModuleBase, IPuzzleKeypadRenderer
  {
    public WidgetRenderer(ModuleBase parent) : base(parent) { }

    public override bool IsHardcodedModule => false;

    /// <summary> Renders default image of the keypad suitable for user entry (i.e. touch or mouse clicks) </summary>
    Azos.Graphics.Image IPuzzleKeypadRenderer.RenderDefaultPuzzleKeypad(PuzzleKeypad keypad, Color? bgColor, bool showRects)
    {
      var gdiImage = drawGDIPuzzleKeypadImage(keypad, bgColor, showRects);
      var handle = new Graphics.NetImage(gdiImage);
      var img = new Azos.Graphics.Image( handle );

      return img;
    }

    private Image drawGDIPuzzleKeypadImage(PuzzleKeypad keypad, Color? bgColor = null, bool showRects = false)
    {
      var sz = keypad.Size;
      var result = new Bitmap(sz.Width + PuzzleKeypad.DEFAULT_RENDER_OFFSET_X * 2, sz.Height + PuzzleKeypad.DEFAULT_RENDER_OFFSET_Y * 2);
      using (var gr = System.Drawing.Graphics.FromImage(result))
      {
        if (bgColor.HasValue)
          gr.Clear(bgColor.Value);

        gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

        var boxPen = new Pen(Color.FromArgb(240, 240, 240), 1.0f);

        var boxPen2 = new Pen(Color.FromArgb(200, 200, 200), 1.3f);
        boxPen2.DashStyle = DashStyle.Dot;

        var shadowPen = new Pen(Color.FromArgb(255, 220, 220, 220), 1.0f);

        LinearGradientBrush paperBrush = null;
        Font font = null;
        LinearGradientBrush fntBrush = null;
        LinearGradientBrush fntBadBrush = null;

        try
        {
          foreach (var box in keypad.Boxes)
          {
            if (paperBrush == null)
            {
              paperBrush = new LinearGradientBrush(box.Rect, Color.FromArgb(240, 240, 240), Color.FromArgb(255, 255, 255), LinearGradientMode.Vertical);
              paperBrush.WrapMode = WrapMode.TileFlipXY;
            }

            if (fntBrush == null)
            {
              fntBrush = new LinearGradientBrush(box.Rect, Color.FromArgb(240, 240, 250), Color.FromArgb(100, 80, 150), LinearGradientMode.ForwardDiagonal);
              fntBrush.WrapMode = WrapMode.TileFlipXY;

              fntBadBrush = new LinearGradientBrush(box.Rect, Color.FromArgb(240, 240, 250), Color.FromArgb(200, 180, 255), LinearGradientMode.Vertical);
              fntBadBrush.WrapMode = WrapMode.TileFlipXY;
            }

            if (font == null)
            {
              font = new Font("Courier New", box.Rect.Height * 0.65f, FontStyle.Bold, GraphicsUnit.Pixel);
            }


            gr.ResetTransform();
            gr.TranslateTransform(PuzzleKeypad.DEFAULT_RENDER_OFFSET_X, PuzzleKeypad.DEFAULT_RENDER_OFFSET_Y);

            if (showRects)
              gr.DrawRectangle(Pens.Red, box.Rect);

            var rnd = App.Random;

            gr.RotateTransform((float)(-3d + (6d * rnd.NextRandomDouble)));

            var pTL = new Point(box.Rect.Left + rnd.NextScaledRandomInteger(-6, +6),
                                 box.Rect.Top + rnd.NextScaledRandomInteger(-6, +6));

            var pTR = new Point(box.Rect.Right + rnd.NextScaledRandomInteger(-6, +6),
                                 box.Rect.Top  + rnd.NextScaledRandomInteger(-6, +6));

            var pBL = new Point(box.Rect.Left    + rnd.NextScaledRandomInteger(-6, +6),
                                 box.Rect.Bottom + rnd.NextScaledRandomInteger(-6, +6));

            var pBR = new Point(box.Rect.Right   + rnd.NextScaledRandomInteger(-6, +6),
                                 box.Rect.Bottom + rnd.NextScaledRandomInteger(-6, +6));

            var pa = new[] { pTL, pTR, pBR, pBL };
            gr.FillPolygon(paperBrush, pa);
            gr.DrawPolygon(boxPen, pa);

            //gr.FillRectangle(paperBrush, box.Rect);
            //gr.DrawRectangle(boxPen, box.Rect);

            //var distortedBRX = box.Rect.Right + ExternalRandomGenerator.Instance.NextScaledRandomInteger(-10, +4);
            //var distortedBRY = box.Rect.Bottom + ExternalRandomGenerator.Instance.NextScaledRandomInteger(-10, +8);

            gr.DrawLine(shadowPen, pTR, pBR);

            gr.DrawLine(boxPen2, pBL.X + 1, pBL.Y, pBR.X - 1, pBR.Y);
            gr.DrawLine(boxPen2, pBL.X, pBL.Y + 1, pBR.X - 2, pBR.Y + 1);


            var tsz = gr.MeasureString(box.Char.ToString(), font);
            var pnt = new PointF((box.Rect.Left + box.Rect.Width / 2f) - tsz.Width / 2f,
                                  (box.Rect.Top + box.Rect.Height / 2f) - tsz.Height / 2f);


            if (rnd.NextScaledRandomInteger(0, 100) > 40)
            {
              var bpnt = pnt;
              bpnt.X += rnd.NextScaledRandomInteger(-2, 4);
              bpnt.Y += rnd.NextScaledRandomInteger(-2, 4);
              gr.DrawString(box.Char.ToString(), font, fntBadBrush, bpnt);

              for (var i = 0; i < rnd.NextScaledRandomInteger(8, 75); i++)
              {
                gr.FillRectangle(fntBadBrush, new Rectangle(box.Rect.Left + rnd.NextScaledRandomInteger(0, box.Rect.Width - 4),
                                                            box.Rect.Top + rnd.NextScaledRandomInteger(0, box.Rect.Height - 4),
                                                            rnd.NextScaledRandomInteger(1, 4),
                                                            rnd.NextScaledRandomInteger(1, 4)
                                                            ));

              }
            }

            gr.DrawString(box.Char.ToString(), font, fntBrush, pnt);
          }
        }
        finally
        {
          if (boxPen != null) boxPen.Dispose();
          if (boxPen2 != null) boxPen2.Dispose();
          if (shadowPen != null) shadowPen.Dispose();
          if (paperBrush != null) paperBrush.Dispose();
          if (font != null) font.Dispose();
          if (fntBrush != null)
          {
            fntBrush.Dispose();
            fntBadBrush.Dispose();
          }
        }

      }
      return result;
    }
  }
}
