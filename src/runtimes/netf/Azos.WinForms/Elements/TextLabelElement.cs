/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using System.Drawing;

namespace Azos.WinForms.Elements
{
  public class TextLabelElement : TextElement
  {
    #region .ctor

    public TextLabelElement(ElementHostControl host)
      : base(host)
    {
    }

    #endregion


    #region Private Fields
      private StringAlignment m_Alignment = StringAlignment.Near;
      private bool m_IsHyperlink;
    #endregion


    #region Properties

     public StringAlignment Alignment
     {
      get { return m_Alignment;}
      set
      {
        m_Alignment = value;
        Invalidate();
      }
     }

     /// <summary>
     /// Indicates whether this label element is a hyperlink and should raise OnHyperlink event
     /// </summary>
     public bool IsHyperlink
     {
       get { return m_IsHyperlink; }
       set
       {
         m_IsHyperlink = value;
       }
     }

    #endregion


    #region Events
      public EventHandler Hyperlink;
    #endregion

    #region Public


    #endregion


    #region Protected

      protected internal override void Paint(System.Drawing.Graphics gr)
      {
        base.Paint(gr);

        //BaseApplication.Theme.PartRenderer.LabelText(gr,
        //                          Region,
        //                          m_IsHyperlink ? MouseIsOver : false,
        //                          Host.Font,
        //                          FieldControlContext,
        //                          Text,
        //                          m_Alignment);
      }

      protected internal override void OnMouseEnter(EventArgs e)
      {
        base.OnMouseEnter(e);

        if (m_IsHyperlink)
          Invalidate();
      }

      protected internal override void OnMouseLeave(EventArgs e)
      {
        base.OnMouseLeave(e);

        if (m_IsHyperlink)
          Invalidate();
      }

      protected internal override void OnMouseClick(System.Windows.Forms.MouseEventArgs e)
      {
        base.OnMouseClick(e);

        if (m_IsHyperlink)
             OnHyperlink(EventArgs.Empty);
      }

      protected virtual void OnHyperlink(EventArgs e)
      {
        if (Hyperlink!=null)
                 Hyperlink(this, e);
      }


    #endregion


    #region Private Utils


    #endregion

  }

}
