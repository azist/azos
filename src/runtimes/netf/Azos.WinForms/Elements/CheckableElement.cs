using System;

namespace Azos.WinForms.Elements
{
  public abstract class CheckableElement : Element
  {
    #region .ctor

    public CheckableElement(ElementHostControl host)
      : base(host)
    {
    }

    #endregion


    #region Private Fields

     private bool m_Checked;

    #endregion


    #region Properties

     public bool Checked
     {
      get { return m_Checked;}
      set
      {
        m_Checked = value;
        OnCheckedChanged(EventArgs.Empty);
        Invalidate();
      }
     }

    #endregion


    #region Events

     public event EventHandler CheckedChanged;
    #endregion

    #region Public


    #endregion


    #region Protected

      protected virtual void OnCheckedChanged(EventArgs e)
      {
        if (CheckedChanged!=null) CheckedChanged(this, e);
      }

    #endregion


    #region Private Utils


    #endregion

  }

}
