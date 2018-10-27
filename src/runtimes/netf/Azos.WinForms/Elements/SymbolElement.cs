/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Drawing;
using System.Drawing.Drawing2D;

namespace Azos.WinForms.Elements
{

  public enum SymbolType {
           Circle = 0,
           Square,
           Diamond,
           TriangleUp,
           TriangleDown,
           TriangleLeft,
           TriangleRight,
           None
         }

  /// <summary>
  /// Symbol elements represent simple glyphs like circle, triangle etc..
  /// This element is currently not themed
  /// </summary>
  public class SymbolElement : Element
  {
    #region .ctor

    public SymbolElement(ElementHostControl host)
      : base(host)
    {
    }

    #endregion


    #region Private Fields

      private SymbolType m_SymbolType;
      private Color m_PenColor = Color.Gray;
      private float m_PenWidth = 1f;

      private Color m_BrushColor = Color.Red;
      private float m_BrushGradientAngle = 45f;

    #endregion


    #region Properties

    public SymbolType SymbolType
    {
      get { return m_SymbolType; }
      set
      {
        m_SymbolType = value;
        Invalidate();
      }
    }

    public Color PenColor
    {
      get { return m_PenColor; }
      set
      {
        m_PenColor = value;
        Invalidate();
      }
    }

    public float PenWidth
    {
      get { return m_PenWidth; }
      set
      {
        m_PenWidth = value;
        Invalidate();
      }
    }

    public Color BrushColor
    {
      get { return m_BrushColor; }
      set
      {
        m_BrushColor = value;
        Invalidate();
      }
    }

    public float BrushGradientAngle
    {
      get { return m_BrushGradientAngle; }
      set
      {
        m_BrushGradientAngle = value;
        Invalidate();
      }
    }


    #endregion


    #region Events

    #endregion

    #region Public


    #endregion


    #region Protected
      protected internal override void Paint(System.Drawing.Graphics gr)
      {
         Point midPoint = new Point(Region.Left + Region.Width / 2,
                                    Region.Top + Region.Height / 2);
         int dim; //min dimension, cant span more than that
         dim = Region.Width;
         if (Region.Height < dim) dim = Region.Height;

         Rectangle rect = new Rectangle(midPoint.X - dim / 2,
                                        midPoint.Y - dim / 2,
                                        dim,
                                        dim);

         gr.SmoothingMode = SmoothingMode.AntiAlias;

         using( LinearGradientBrush brush = new LinearGradientBrush(rect, Color.White, m_BrushColor, m_BrushGradientAngle) )
         {
           using ( Pen pen = new Pen(m_PenColor, m_PenWidth) )
           {


                 switch(m_SymbolType)
                 {
                   case SymbolType.Circle:
                   {
                     gr.FillEllipse(brush, rect);
                     gr.DrawEllipse(pen, rect);
                     break;
                   }

                   case SymbolType.Square:
                   {
                     gr.FillRectangle(brush, rect);
                     gr.DrawRectangle(pen, rect);
                     break;
                   }

                   case SymbolType.Diamond:
                   {
                     Point [] vtx = new Point[4];
                     vtx[0] = new Point(midPoint.X, rect.Top); //north
                     vtx[1] = new Point(rect.Right, midPoint.Y); //east
                     vtx[2] = new Point(midPoint.X, rect.Bottom); //south
                     vtx[3] = new Point(rect.Left, midPoint.Y); //west
                     gr.FillPolygon(brush, vtx);
                     gr.DrawPolygon(pen, vtx);
                     break;
                   }

                   case SymbolType.TriangleUp:
                   {
                     Point[] vtx = new Point[3];
                     vtx[0] = new Point(midPoint.X, rect.Top); //north
                     vtx[1] = new Point(rect.Right, midPoint.Y); //east
                     vtx[2] = new Point(rect.Left, midPoint.Y); //west
                     gr.FillPolygon(brush, vtx);
                     gr.DrawPolygon(pen, vtx);
                     break;
                   }


                   case SymbolType.TriangleDown:
                   {
                     Point[] vtx = new Point[3];
                     vtx[0] = new Point(midPoint.X, rect.Bottom); //south
                     vtx[1] = new Point(rect.Right, midPoint.Y); //east
                     vtx[2] = new Point(rect.Left, midPoint.Y); //west
                     gr.FillPolygon(brush, vtx);
                     gr.DrawPolygon(pen, vtx);
                     break;
                   }


                   case SymbolType.TriangleLeft:
                   {
                     Point[] vtx = new Point[3];
                     vtx[0] = new Point(midPoint.X, rect.Top); //north
                     vtx[1] = new Point(midPoint.X, rect.Bottom); //south
                     vtx[2] = new Point(rect.Left, midPoint.Y); //west
                     gr.FillPolygon(brush, vtx);
                     gr.DrawPolygon(pen, vtx);
                     break;
                   }


                   case SymbolType.TriangleRight:
                   {
                     Point[] vtx = new Point[3];
                     vtx[0] = new Point(midPoint.X, rect.Top); //north
                     vtx[1] = new Point(midPoint.X, rect.Bottom); //south
                     vtx[2] = new Point(rect.Right, midPoint.Y); //east
                     gr.FillPolygon(brush, vtx);
                     gr.DrawPolygon(pen, vtx);
                     break;
                   }

                 } //switch

           }//using pen
         }//using brush
      }

    #endregion


    #region Private Utils


    #endregion

  }

}
