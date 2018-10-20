
using System;
using System.Windows.Forms;

namespace Azos.WinForms.Elements
{
  /// <summary>
  /// Represents an elemnt that may be pressed down by the mouse. Mostly used as a button base
  /// </summary>
  public abstract class PressableElement : Element
  {
    #region .ctor

    public PressableElement(ElementHostControl host)
      : base(host)
    {
    }

    #endregion


    #region Private Fields
       private bool m_Pressed;
    #endregion


    #region Properties
      /// <summary>
      /// Indicates whether an element is pressed
      /// </summary>
      public bool Pressed
      {
        get { return m_Pressed; }
      }
    #endregion


    #region Events


    #endregion


    #region Public

    #endregion


    #region Protected

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
      if (!m_Pressed)
      {
        m_Pressed = true;
        Invalidate();
      }
      base.OnMouseDown(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
      if (m_Pressed)
      {
        m_Pressed = false;
        Invalidate();
      }
      base.OnMouseUp(e);
    }


    protected internal override void OnMouseEnter(EventArgs e)
    {
      base.OnMouseEnter(e);
      Invalidate();
    }


    protected internal override void OnMouseLeave(EventArgs e)
    {
      m_Pressed = false;
      base.OnMouseLeave(e);
      Invalidate();
    }


    #endregion


    #region Private Utils



    #endregion

  }

}
