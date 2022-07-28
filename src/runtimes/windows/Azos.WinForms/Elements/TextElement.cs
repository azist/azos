/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

namespace Azos.WinForms.Elements
{
  public abstract class TextElement : Element
  {
    #region .ctor

    public TextElement(ElementHostControl host)
      : base(host)
    {
    }

    #endregion


    #region Private Fields

    private string m_Text;

    #endregion


    #region Properties

    public string Text
    {
      get { return m_Text ?? string.Empty; }
      set
      {
        m_Text = value;
        OnTextChanged(EventArgs.Empty);
        Invalidate();
      }
    }

    #endregion


    #region Events

    public event EventHandler TextChanged;
    #endregion

    #region Public


    #endregion


    #region Protected

    protected virtual void OnTextChanged(EventArgs e)
    {
      if (TextChanged != null) TextChanged(this, e);
    }

    #endregion


    #region Private Utils


    #endregion

  }

}
