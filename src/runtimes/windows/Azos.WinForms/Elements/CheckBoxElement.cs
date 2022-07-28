/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System.Windows.Forms;

namespace Azos.WinForms.Elements
{
  public class CheckBoxElement : CheckableElement
  {
    #region .ctor

    public CheckBoxElement(ElementHostControl host)
      : base(host)
    {
    }

    #endregion


    #region Private Fields


    #endregion


    #region Properties


    #endregion


    #region Public


    #endregion


    #region Protected

    protected internal override void Paint(System.Drawing.Graphics gr)
    {
      base.Paint(gr);
      //BaseApplication.Theme.PartRenderer.CheckBox(gr, Region, MouseIsOver, FieldControlContext, Checked);
    }

    protected internal override void OnMouseClick(MouseEventArgs e)
    {
      Checked = ! Checked;
      base.OnMouseClick(e);
    }

    #endregion


    #region Private Utils


    #endregion

  }
}
