/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
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
