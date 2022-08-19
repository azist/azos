/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Drawing;

using Azos.WinForms.Elements;

namespace Azos.WinForms.Controls.GridKit
{

  /// <summary>
  /// Represents a comment hover element for grid
  /// </summary>
  public sealed class CommentElement : TextElement
  {
      #region .ctor

       internal CommentElement(CellView host) : base(host)
       {
       }

     #endregion

     #region Protected
       protected internal override void Paint(System.Drawing.Graphics gr)
       {
          var reg = Region;
          reg.Inflate(-2,-2);

           gr.FillRectangle(Brushes.Beige, reg);

           gr.DrawLine(Pens.Gray, reg.X, reg.Y, reg.X, reg.Bottom);
           gr.DrawLine(Pens.Gray, reg.Left, reg.Y, reg.Right, reg.Y);

           using(var pen = new Pen(Color.Black))
           {
             pen.Width = 2f;
             gr.DrawLine(pen, reg.Right, reg.Y, reg.Right, reg.Bottom);
             gr.DrawLine(pen, reg.Left, reg.Bottom, reg.Right, reg.Bottom);
           }

           using (StringFormat fmt = new StringFormat())
           {
             fmt.Alignment = StringAlignment.Near;

             var treg = reg;
             treg.Inflate(-2, -2);

             gr.DrawString(Text, m_Host.Font, Brushes.Black, treg, fmt);
           }
       }

     #endregion
  }

}
